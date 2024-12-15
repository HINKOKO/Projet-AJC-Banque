using System;
using System.IO;
using System.Text.Json;

namespace AppAJCBanque.Services
{
    public class FileWatcherService : IDisposable
    {
        private readonly FileSystemWatcher _fileWatcher;
        private readonly string _path;
        private readonly string _filter;

        public event Action<List<Models.Transaction>> TransactionsUpdated;

        public FileWatcherService(string path, string filter = "*.json")
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

            _path = path;
            _filter = filter;

            _fileWatcher = new FileSystemWatcher(_path)
            {
                NotifyFilter = NotifyFilters.Attributes
                               | NotifyFilters.CreationTime
                               | NotifyFilters.DirectoryName
                               | NotifyFilters.FileName
                               | NotifyFilters.LastAccess
                               | NotifyFilters.LastWrite
                               | NotifyFilters.Security
                               | NotifyFilters.Size,
                Filter = _filter,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            // Subscribe to events
            _fileWatcher.Changed += OnChanged;
            _fileWatcher.Created += OnCreated;
            _fileWatcher.Deleted += OnDeleted;
            _fileWatcher.Renamed += OnRenamed;
            _fileWatcher.Error += OnError;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                Console.WriteLine($"File changed: {e.FullPath}");
                TriggerUpdate(e.FullPath);
            }
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File created: {e.FullPath}");
            TriggerUpdate(e.FullPath);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File deleted: {e.FullPath}");
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"File renamed from {e.OldFullPath} to {e.FullPath}");
            TriggerUpdate(e.FullPath);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"File watcher error: {e.GetException()?.Message}");
        }

        private void TriggerUpdate(string filePath)
        {
            try
            {
                // Example: Load and parse transactions
                var transactions = LoadTransactionsFromFile(filePath);
                TransactionsUpdated?.Invoke(transactions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
            }
        }

        private List<Models.Transaction> LoadTransactionsFromFile(string filePath)
        {
            // Implementation to read and parse the file
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<Models.Transaction>>(json) ?? new List<Models.Transaction>();
        }

        public void Dispose()
        {
            _fileWatcher.Dispose();
        }
    }
}
