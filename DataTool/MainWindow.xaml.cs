using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using System.IO;
using DataTool.DataClass;

namespace DataTool
{
    public partial class MainWindow : Window
    {
        IDataTable m_table;
        JDataCommandList m_commandList = new JDataCommandList();
        string m_tableName = string.Empty;
        string CurrrentTableName
        {
            set
            {
                m_tableName = value;
                this.Title = GetTitleString(value, this.CurrentFilePath);
            }
            get { return m_tableName; }
        }

        string m_filePath = string.Empty;
        string CurrentFilePath
        {
            set
            {
                m_filePath = value;
                this.Title = GetTitleString(this.CurrrentTableName, value);
            }
            get { return m_filePath; }
        }

        public MainWindow()
        {
            InitializeComponent();
            m_commandList.AddCallback = new UndoRedoCallback(AddDataInTable);
            m_commandList.RemoveCallback = new UndoRedoCallback(RemoveDataInTable);
            m_commandList.ChangeCallback = new UndoRedoCallback(ChangeDataInTable);
        }
        private string GetTitleString(string dataName, string path)
        {
            string result = "DataTool";

            if (dataName != String.Empty)
            {
                result += $" <{dataName}>";
            }
            if (path != String.Empty)
            {
                result += $" [{path}]";
            }
            if (m_commandList.CanSave())
            {
                result += " *";
            }
            return result;
        }
        private void Button_Del(object sender, RoutedEventArgs e)
        {
            if (m_table == null)
                return;
            m_table.RemoveData(this.classNameList.SelectedIndex, delegate (JData data)
            {
                m_commandList.Add(JDataCommandType.Property_Remove, data.Clone(), null, this.classNameList.SelectedIndex);
                this.Title = GetTitleString(this.CurrrentTableName, this.CurrentFilePath);
                this.classNameList.Items.Remove(data.ToStringKey());
                this.propertyGrid1.SelectedObject = null;
            });
        }
        private void Button_Add(object sender, RoutedEventArgs e)
        {
            if (m_table == null)
                return;
            m_table.AddCopyNewData(new JDataPropertyChanged(Property_Key_Changed), delegate (JData data)
            {
                m_commandList.Add(JDataCommandType.Property_Add, null, data.Clone(), m_table.GetIndex(data));
                this.Title = GetTitleString(this.CurrrentTableName, this.CurrentFilePath);
                this.classNameList.Items.Add(data.ToStringKey());
            });
        }
        private void MenuItem_New(object sender, RoutedEventArgs e)
        {
            DialogNewFile dialog = new DialogNewFile();
            var dataNameList = JDataTableFunc.GetDataTableList();
            foreach (string dataName in dataNameList)
            {
                dialog.AddComboBoxItem(dataName);
            }
            dialog.Left = this.Left + dialog.Width / 2;
            dialog.Top = this.Top + dialog.Height / 2;
            dialog.ResizeMode = ResizeMode.NoResize;
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                var dataName = dialog.GetComboBoxItem();
                this.CurrrentTableName = dataName;
                this.classNameList.Items.Clear();
                this.propertyGrid1.SelectedObject = null;
                if (!InitTable(this.CurrrentTableName, JDataTableFunc.GetDefaultDataString(dataName)))
                {
                    System.Windows.MessageBox.Show("init table fail");
                    return;
                }
                this.CurrentFilePath = String.Empty;
            }
        }
        private void MenuItem_Open(object sender, RoutedEventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
                openFileDialog.Filter = "json files (*.json)|*.json|All files (*.*)|*,*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                filePath = openFileDialog.FileName;
                var fileStream = openFileDialog.OpenFile();
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    fileContent = reader.ReadToEnd();
                }
            }
            if (fileContent == string.Empty)
            {
                System.Windows.MessageBox.Show("read fail");
                return;
            }
            this.CurrrentTableName = GetTableName(fileContent);
            this.classNameList.Items.Clear();
            this.propertyGrid1.SelectedObject = null;
            if (!InitTable(this.CurrrentTableName, fileContent))
            {
                System.Windows.MessageBox.Show("init table fail");
                return;
            }
            this.CurrentFilePath = filePath;
        }
        private void MenuItem_Save(object sender, RoutedEventArgs e)
        {
            if (this.CurrentFilePath == String.Empty)
            {
                MenuItem_SaveAs(sender, e);
                return;
            }
            SaveTable(this.CurrrentTableName, this.CurrentFilePath);
        }
        private void MenuItem_SaveAs(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            saveFileDialog.Filter = "json files (*.json)|*.json|All files (*.*)|*,*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveTable(this.CurrrentTableName, saveFileDialog.FileName);
                this.CurrentFilePath = saveFileDialog.FileName;
            }
        }
        private void MenuItem_Del(object sender, RoutedEventArgs e)
        {
            Button_Del(sender, e);
        }
        private void MenuItem_Undo(object sender, RoutedEventArgs e)
        {
            m_commandList.Undo();
        }
        private void MenuItem_Redo(object sender, RoutedEventArgs e)
        {
            m_commandList.Redo();
        }
        private void ClassNameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = this.classNameList.SelectedIndex;
            if (index == -1)
                return;
            this.propertyGrid1.SelectedObject = m_table.FindData(index);
        }
        #region CallBackFunc
        //키값 변경시
        void Property_Key_Changed(JData beforeData, JData afterData)
        {
            m_commandList.Add(JDataCommandType.Property_Changed, beforeData, afterData.Clone(), m_table.GetIndex(afterData));
            this.Title = GetTitleString(this.CurrrentTableName, this.CurrentFilePath);
            int index = this.classNameList.SelectedIndex;
            if (index == -1)
            {
                System.Windows.MessageBox.Show(String.Format("listbox 항목 수정 실패 (SelectedIndex = -1)"));
                return;
            }
            this.classNameList.Items[index] = afterData.ToStringKey();
            this.classNameList.SelectedIndex = index;
        }

        public void ChangeDataInTable(int index, JData data)
        {
            m_table.ChangeData(index, data);
            var changedData = m_table.FindData(index);
            this.classNameList.Items[index] = changedData.ToStringKey();
            this.classNameList.SelectedIndex = index;
            this.propertyGrid1.SelectedObject = changedData;
            this.Title = GetTitleString(this.CurrrentTableName, this.CurrentFilePath);
        }

        public void RemoveDataInTable(int index, JData data)
        {
            m_table.RemoveData(index, null);
            if (this.classNameList.SelectedIndex == index)
            {
                this.propertyGrid1.SelectedObject = null;
            }
            this.classNameList.Items.RemoveAt(index);
            this.Title = GetTitleString(this.CurrrentTableName, this.CurrentFilePath);
        }

        public void AddDataInTable(int index, JData data)
        {
            m_table.AddData(index, data, delegate (JData d)
            {
                this.classNameList.Items.Insert(index, d.ToStringKey());
            });
            this.Title = GetTitleString(this.CurrrentTableName, this.CurrentFilePath);
        }
        #endregion

        private void SaveTable(string dataName, string path)
        {
            string result = JDataTableFunc.TableToString(dataName, m_table);
            if (result == String.Empty)
            {
                System.Windows.MessageBox.Show(String.Format("저장에 실패하였습니다."));
                return;
            }
            File.WriteAllText(path, result);
            m_commandList.Save();
            this.Title = GetTitleString(this.CurrrentTableName, this.CurrentFilePath);
            System.Windows.MessageBox.Show(String.Format("{0} 파일에 저장하였습니다.", path));
        }

        private bool InitTable(string dataName, string dataStr)
        {
            m_table = JDataTableFunc.StringToTable(dataName, dataStr);
            if (m_table == null)
            {
                System.Windows.MessageBox.Show(String.Format("{0} 테이블은 현재 지원하지 않습니다.", dataName));
                return false;
            }
            m_table.ForeachCallbackData(delegate (JData data)
            {
                data.AddPropChangedHandler(new JDataPropertyChanged(Property_Key_Changed));
            });
            m_table.ForeachCallbackData(delegate (JData data) 
            {
                this.classNameList.Items.Add(data.ToStringKey());
            });
            m_commandList.Clear();
            return true;
        }
        
        private string GetTableName(string jsonStr)
        {
            JObject jo = JObject.Parse(jsonStr);
            return jo["DataName"].ToString();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_CanExecute_Save(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_commandList.CanSave();
        }

        private void CommandBinding_CanExecute_SaveAs(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_CanExecute_Undo(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_commandList.UndoCount > 0;
        }

        private void CommandBinding_CanExecute_Redo(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_commandList.RedoCount > 0;
        }

        private void CommandBinding_CanExecute_Del(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.classNameList.SelectedIndex > -1;
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!m_commandList.CanSave())
                return;
            MessageBoxResult result = System.Windows.MessageBox.Show("변경된 내용이 있습니다. 저장하시겠습니까 ?", "저장 확인", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                MenuItem_Save(sender, null);
                return;
            }
            else if (result == MessageBoxResult.No)
            {
                return;
            }
            e.Cancel = true;
        }
    }
}
