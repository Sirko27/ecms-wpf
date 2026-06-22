using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_Core
{
    public class SnapshotManager
    {
        private readonly Stack<SystemSnapshot> _undoStack = new Stack<SystemSnapshot>();
        private readonly Stack<SystemSnapshot> _redoStack = new Stack<SystemSnapshot>();

        private const int MaxHistory = 25;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void Push(SystemSnapshot snapshot)
        {
            _undoStack.Push(snapshot);

            if (_undoStack.Count > MaxHistory)
                RebuildUndoStack();

            _redoStack.Clear();
        }

        public SystemSnapshot Undo(SystemSnapshot current)
        {
            if (!CanUndo) return current;
            _redoStack.Push(current);
            return _undoStack.Pop();
        }

        public SystemSnapshot Redo(SystemSnapshot current)
        {
            if (!CanRedo) return current;
            _undoStack.Push(current);
            return _redoStack.Pop();
        }

        private void RebuildUndoStack()
        {
            var items = _undoStack.ToList();
            items = items.Take(MaxHistory).ToList();
            _undoStack.Clear();

            foreach (var item in items.AsEnumerable().Reverse())
                _undoStack.Push(item);
        }
    }
}
