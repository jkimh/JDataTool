using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTool.DataClass
{
    public delegate void UndoRedoCallback(int index, JData data);
    public enum JDataCommandType
    {
        Property_Changed,
        Property_Add,
        Property_Remove
    }
    public class JDataCommand
    {
        public int CommandID { get; set; }
        public JDataCommandType Type { get; set; }
        public JData BeforeData { get; set; }
        public JData AfterData { get; set; }
        public int ChangedIndex { get; set; }
        public JDataCommand(int commandID, JDataCommandType type, JData beforeData, JData afterData, int index)
        {
            CommandID = commandID;
            Type = type;
            BeforeData = beforeData;
            AfterData = afterData;
            ChangedIndex = index;
        }
    }
    public class JDataCommandList
    {
        Stack<JDataCommand> m_redoStack = new Stack<JDataCommand>();
        Stack<JDataCommand> m_undoStack = new Stack<JDataCommand>();
        public UndoRedoCallback AddCallback { get; set; }
        public UndoRedoCallback RemoveCallback { get; set; }
        public UndoRedoCallback ChangeCallback { get; set; }
        int m_newCommandID = 1;
        int m_savedCommandID = 0;
        int m_currendCommandID = 0;
        public int UndoCount
        {
            get { return m_undoStack.Count; }
        }

        public int RedoCount
        {
            get { return m_redoStack.Count; }
        }
        public JDataCommandList()
        {

        }
        public bool CanSave()
        {
            return m_currendCommandID != m_savedCommandID;
        }
        public void Save()
        {
            m_savedCommandID = m_currendCommandID;
        }
        public void Clear()
        {
            m_redoStack.Clear();
            m_undoStack.Clear();
        }

        public void Add(JDataCommandType type, JData beforeData, JData afterData, int index)
        {
            JDataCommand command = new JDataCommand(m_newCommandID++, type, beforeData, afterData, index);
            m_currendCommandID = command.CommandID;
            m_undoStack.Push(command);
            m_redoStack.Clear();
        }

        public void Redo()
        {
            JDataCommand command = m_redoStack.Pop();
            m_currendCommandID = command.CommandID;
            switch (command.Type)
            {
                case JDataCommandType.Property_Add:
                    AddCallback(command.ChangedIndex, command.AfterData);
                    break;
                case JDataCommandType.Property_Changed:
                    ChangeCallback(command.ChangedIndex, command.AfterData);
                    break;
                case JDataCommandType.Property_Remove:
                    RemoveCallback(command.ChangedIndex, command.BeforeData);
                    break;
                default:
                    break;
            }
            m_undoStack.Push(command);
        }
        public void Undo()
        {
            JDataCommand command = m_undoStack.Pop();
            m_currendCommandID = m_undoStack.Count != 0 ? m_undoStack.Last().CommandID : 0;
            switch (command.Type)
            {
                case JDataCommandType.Property_Add:
                    RemoveCallback(command.ChangedIndex, command.AfterData);
                    break;
                case JDataCommandType.Property_Changed:
                    ChangeCallback(command.ChangedIndex, command.BeforeData);
                    break;
                case JDataCommandType.Property_Remove:
                    AddCallback(command.ChangedIndex, command.BeforeData);
                    break;
                default:
                    break;
            }
            m_redoStack.Push(command);
        }
    }
}
