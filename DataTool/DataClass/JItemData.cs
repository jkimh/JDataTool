using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DataTool.DataClass
{
    public class JItemData : JData
    {
        private string m_desc = Constants.None;
        private int m_atk = 0;
        private int m_limitLv = 0;
        private string m_setGroup = Constants.None;

        #region properties
        
        [DisplayName("Desc")]
        [Description("아이템 설명")]
        public string Desc
        {
            set
            {
                if (PropertyChanged != null)
                {
                    JData beforeData = this.Clone();
                    m_desc = value;
                    NotifyPropertyChanged(beforeData);
                    return;
                }
                m_desc = value;
            }
            get { return m_desc; }
        }
        [DisplayName("Atk")]
        [Description("공격력")]
        public int Atk
        {
            set
            {
                if (PropertyChanged != null)
                {
                    JData beforeData = this.Clone();
                    m_atk = value;
                    NotifyPropertyChanged(beforeData);
                    return;
                }
                m_atk = value;
            }
            get { return m_atk; }
        }
        [DisplayName("LimitLv")]
        [Description("레벨 제한")]
        public int LimitLv
        {
            set
            {
                if (PropertyChanged != null)
                {
                    JData beforeData = this.Clone();
                    m_limitLv = value;
                    NotifyPropertyChanged(beforeData);
                    return;
                }
                m_limitLv = value;
            }
            get { return m_limitLv; }
        }
        [DisplayName("SetGroup")]
        [Description("세트 아이템 그룹 (datatable_setitemgroup.json 의 ClassName과 동일해야 한다.)")]
        public string SetGroup
        {
            set
            {
                if (PropertyChanged != null)
                {
                    JData beforeData = this.Clone();
                    m_setGroup = value;
                    NotifyPropertyChanged(beforeData);
                    return;
                }
                m_setGroup = value;
            }
            get { return m_setGroup; }
        }
        #endregion

        public override JData Clone()
        {
            JItemData ret = new JItemData();
            ret.m_classID = this.ClassID;
            ret.m_className = this.ClassName;
            ret.m_name = this.Name;
            ret.m_desc = this.Desc;
            ret.m_atk = this.Atk;
            ret.m_limitLv = this.LimitLv;
            ret.m_setGroup = this.SetGroup;
            ret.PropertyChanged = this.PropertyChanged;
            return ret;
        }
        public override JData CloneNewData()
        {
            JItemData ret = new JItemData();
            ret.m_classID = this.ClassID + 1;
            ret.m_className = this.ClassName + "_t";
            ret.m_name = this.Name;
            ret.m_desc = this.Desc;
            ret.m_atk = this.Atk;
            ret.m_limitLv = this.LimitLv;
            ret.m_setGroup = this.SetGroup;
            ret.PropertyChanged = this.PropertyChanged;
            return ret;
        }
    }
}
