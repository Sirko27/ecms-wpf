using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_Core
{
    public class EventLogger
    {
        private static readonly EventLogger _instance = new EventLogger();

        public static EventLogger Instance => _instance;

        private readonly ObservableCollection<string> _entries = new ObservableCollection<string>();

        public IReadOnlyCollection<string> Entries => _entries;

        public event Action<string> EntryAdded;

        private EventLogger() { }

        public void Log(string deviceName, string message)
        {
            string entry = $"{DateTime.Now:HH:mm:ss} {deviceName}: {message}";
            _entries.Add(entry);
            EntryAdded?.Invoke(entry);
        }
    }
}
