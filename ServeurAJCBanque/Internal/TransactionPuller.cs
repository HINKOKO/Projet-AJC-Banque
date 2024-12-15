using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServeurAJCBanque.MockBank;
using ServeurAJCBanque.Models;

namespace ServeurAJCBanque.Internal
{
    public class TransactionPuller
    {
        private readonly string _csvPath;
        private FileSystemWatcher _watcher;
        public event EventHandler CSVModif;

        public TransactionPuller(string csvPath)
        {
            _csvPath = csvPath;
            _watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(_csvPath),
                Filter = Path.GetFileName(_csvPath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };

            // Abonner l'événement de modification
            _watcher.Changed += OnCsvModifie;
            _watcher.Created += OnCsvModifie;

            // Démarrer la surveillance
            _watcher.EnableRaisingEvents = true;
        }

        public void RecupTransactions(BankGenerator bankGenerator, int txFlow)
        {
            static string GetRelativeCsvPath(string fullPath)
            {
                int startIndex = fullPath.IndexOf(@"datas\");
                if (startIndex >= 0)
                {
                    return fullPath.Substring(startIndex);
                }
                return fullPath;
            }

            try
            {
                var transactions = BankGenerator.GenererTransactions(txFlow);
                SaveToCsv(transactions, _csvPath);
                Console.WriteLine($"\ntransactions récupérées avec succès - retrouvez-les dans le fichier {GetRelativeCsvPath(_csvPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur ->  " + ex.Message);
            }


        }

        public void SaveToCsv(List<Transaction> transactions, string csvPath)
        {
            // si supprimé  - le recréer - sinon écrase le contenu précédant du fichier CSV.
            string directory = Path.GetDirectoryName(csvPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var writer = new StreamWriter(csvPath))
            {
                foreach (var tx in transactions)
                    writer.WriteLine($"{tx.NumeroCarte};{tx.Montant.ToString(CultureInfo.InvariantCulture)};{tx.TypeOp};{tx.DateOperation:yyyy-dd-MMTHH:mm:ss};{tx.Devise}");
            }
        }

        private void OnCsvModifie(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"\nLe fichier {e.Name} a été modifié.");

            // Déclencher l'événement CSVModifie
            CSVModif?.Invoke(this, EventArgs.Empty);

            // Proposer d'afficher les données du fichier
            ProposerAffichage();
        }

        private void ProposerAffichage()
        {
            Console.WriteLine("Afficher les nouvelles transactions a traiter ?  [Y/N]");
            var choix = Console.ReadKey(intercept: true).Key;

            if (choix == ConsoleKey.Y)
            {
                Console.WriteLine("Contenu du fichier : ");
                AfficherCSV();
            }
            else
            {
                Console.WriteLine("Abandon affichage.");
            }
        }

        // afficher les tx en CSV a traiter en base
        private void AfficherCSV()
        {
            try
            {
                if (!File.Exists(_csvPath))
                {
                    Console.WriteLine("Fichier introuvable.");
                    return;
                }
                string[] lignes = File.ReadAllLines(_csvPath);
                foreach (var ligne in lignes)
                {
                    Task.Delay(500).Wait();
                    Console.WriteLine(ligne);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la lecture du fichier : {ex.Message}");
            }
        }

        // Arreter le watcher
        public void StopWatching()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }

    }
}
