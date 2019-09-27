using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.ComponentModel;

/*
 * A. 데이터 테이블 추가시 해야할 것 (툴 사용 시 기본 세팅)
 *   1. JDataTable.cs 에 각 항목 추가
 *      ㄴ Constants 클래스에 항목 추가
 *      ㄴ m_dataTableNameList 에 항목 추가
 *      ㄴ GetDefaultDataString 에 기본 세팅 추가
 *      ㄴ StringToTable 에 항목 추가
 *      ㄴ TableToString 에 항목 추가
 *   2. JItemData.cs 참고하여 데이터 프로퍼티 클래스 추가 (설명을 포함하여 메뉴얼을 대체한다.)
 *   
 * B. 데이터 테이블 칼럼 추가 시 해야할 것
 *   1. JItemData.cs 에 해당하는 파일에 관련 변수와 프로퍼티 추가 (설명 및 기본값 포함)
 *   2. 같은 파일 내 Clone 함수 데이터 복사 로직 추가
 */

namespace DataTool.DataClass
{
    static class Constants
    {
        public const string None = "None";
        public const string Item = "Item";
    }

    public static class JDataTableFunc
    {
        //지원하는 데이터 테이블 목록
        private static string[] m_dataTableNameList = {
            Constants.Item
        };
        public static string[] GetDataTableList()
        {
            return m_dataTableNameList;
        }
        public static string GetDefaultDataString(string dataName)
        {
            switch(dataName)
            {
                case Constants.Item:
                    JNormalDataTable<JItemData> table = new JNormalDataTable<JItemData>();
                    table.DataName = Constants.Item;
                    table.AddNewData(null, null);
                    return TableToString<JNormalDataTable<JItemData>>(table);
                default:
                    break;
            }
            return String.Empty;
        }

        public static IDataTable StringToTable(string dataName, string dataStr)
        {
            switch (dataName)
            {
                case Constants.Item:
                    return JDataTableFunc.StringToTable<JNormalDataTable<JItemData>>(dataStr);
                default:
                    break;
            }
            return null;
        }

        public static string TableToString(string dataName, IDataTable table)
        {
            switch (dataName)
            {
                case Constants.Item:
                    return JDataTableFunc.TableToString<JNormalDataTable<JItemData>>((JNormalDataTable<JItemData>)table);
                default:
                    break;
            }
            return String.Empty;            
        }

        //두 함수를 통해 데이터 in, out 형태를 정할 수 있다.
        private static TTable StringToTable<TTable>(string data) where TTable : IDataTable
        {
            return JsonConvert.DeserializeObject<TTable>(data);
        }
        private static string TableToString<TTable>(TTable table) where TTable : IDataTable
        {
            return JsonConvert.SerializeObject(table);
        }
    }
    public class JData : IEquatable<JData>
    {
        protected int m_classID = 0;
        protected string m_className = Constants.None;
        protected string m_name = Constants.None;

        [Category("ID")]
        [DisplayName("ClassID")]
        [Description("숫자 키값")]
        public int ClassID
        {
            set
            {
                if(PropertyChanged != null)
                {
                    JData beforeData = this.Clone();
                    m_classID = value;
                    NotifyPropertyChanged(beforeData);
                    return;
                }
                m_classID = value;
            }
            get { return m_classID; }
        }

        [Category("ID")]
        [DisplayName("ClassName")]
        [Description("문자열 키값")]
        public string ClassName
        {
            set
            {
                if (PropertyChanged != null)
                {
                    JData beforeData = this.Clone();
                    m_className = value;
                    NotifyPropertyChanged(beforeData);
                    return;
                }
                m_className = value;
            }
            get { return m_className; }
        }

        [DisplayName("Name")]
        [Description("로컬 네임")]
        public string Name
        {
            set
            {
                if (PropertyChanged != null)
                {
                    JData beforeData = this.Clone();
                    m_name = value;
                    NotifyPropertyChanged(beforeData);
                    return;
                }
                m_name = value;
            }
            get { return m_name; }
        }

        protected JDataPropertyChanged PropertyChanged;
        public void AddPropChangedHandler(JDataPropertyChanged handler)
        {
            PropertyChanged = handler;
        }
        protected void NotifyPropertyChanged(JData beforeData)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(beforeData, this);
            }
        }
        public string ToStringKey()
        {
            return string.Format("{0}. {1} ({2})", m_classID, m_className, m_name);
        }

        //notify 를 피하기 위해 m_ 변수로 직접 변경해줌.
        public virtual JData Clone()
        {
            JData data = new JData();
            data.m_classID = this.ClassID;
            data.m_className = this.ClassName;
            data.m_name = this.Name;
            return data;
        }
        public virtual JData CloneNewData()
        {
            JData data = new JData();
            data.m_classID = this.ClassID + 1;
            data.m_className = this.ClassName + "_t";
            data.m_name = this.Name;
            return data;
        }

        public bool Equals(JData other)
        {
            if (other == null)
                return false;
            if (this.ClassID != other.ClassID)
                return false;
            if (this.ClassName != other.ClassName)
                return false;
            return true;
        }
    }
    public delegate void JDataPropertyChanged(JData beforeData, JData afterData);
    public delegate void ForeachCallback(JData data);
    public delegate void AddNewDataCallback(JData key);
    public delegate void RemoveDataCallback(JData key);
    //각 데이터 테이블에서 툴에 적용하기 위해 구현해줘야할 함수들
    public interface IDataTable
    {
        int GetIndex(JData data);
        JData FindData(int index);
        void ChangeData(int index, JData data);
        void RemoveData(int index, RemoveDataCallback callback);
        void AddData(int index, JData data, AddNewDataCallback callback);
        void AddNewData(JDataPropertyChanged handler, AddNewDataCallback callback);
        void AddCopyNewData(JDataPropertyChanged handler, AddNewDataCallback callback);
        void ForeachCallbackData(ForeachCallback callback);
    }

    public class JNormalDataTable<TData> : IDataTable
        where TData : JData, new()
    {
        public string DataName { get; set; } = Constants.None;
        public List<TData> Data { get; set; } = new List<TData>();

        public void AddCopyNewData(JDataPropertyChanged handler, AddNewDataCallback callback)
        {
            if (this.Data.Count == 0)
            {
                AddNewData(handler, callback);
                return;
            }
            TData data = (TData)this.Data.Last().CloneNewData();
            this.Data.Add(data);
            if (callback != null)
            {
                callback(data);
            }
        }
        public void AddNewData(JDataPropertyChanged handler, AddNewDataCallback callback)
        {            
            TData data = new TData();
            if(handler != null)
            {
                data.AddPropChangedHandler(handler);
            }
            this.Data.Add(data);
            if(callback != null)
            {
                callback(data);
            }
        }
        public JData FindData(int index)
        {
            return this.Data[index];
        }
        public void ForeachCallbackData(ForeachCallback callback)
        {
            foreach (var d in Data)
            {
                callback(d);
            }
        }
        public int GetIndex(JData data)
        {
            return this.Data.IndexOf((TData)data);
        }
        public void ChangeData(int index, JData data)
        {
            if (index < 0)
                return;
            this.Data.RemoveAt(index);
            this.Data.Insert(index, (TData)data.Clone());
        }
        public void RemoveData(int index, RemoveDataCallback callback)
        {
            if (index < 0)
                return;
            if (callback != null)
            {
                callback(this.Data[index]);
            }
            this.Data.RemoveAt(index);
        }
        public void AddData(int index, JData data, AddNewDataCallback callback)
        {
            TData cloneData = (TData)data.Clone();
            this.Data.Insert(index, cloneData);
            if (callback != null)
            {
                callback(cloneData);
            }
        }
    }
}
