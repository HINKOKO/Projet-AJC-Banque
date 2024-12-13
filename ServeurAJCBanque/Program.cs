using System.Configuration;
using ServeurAJCBanque.Models;
using ServeurAJCBanque.Helpers;
using ServeurAJCBanque.MockBank;
using ServeurAJCBanque.Authentication;
using System.Globalization;


public class Program
{
    //private static List<Transaction> allTxs = null;
    static string csvPath = ConfigurationManager.AppSettings["CsvFilePath"];
    static string jsonPath = ConfigurationManager.AppSettings["JSONFilePath"];
    static string logPath = ConfigurationManager.AppSettings["logFilePath"];
    // 10 transactions dragged back from server by default
    static int txFlow = 10;


    private static async Task Main(string[] args)
    {
        //// Au demarage console - demander identifiants
        ///
        Styles styles = new Styles();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(styles.SteeveGreet);
        Console.WriteLine("\n\t\tLa Steeve-Bank vous demande de vous identifer\n");
        Console.ResetColor();

        bool isAuthenticated = false;
        do
        {
            isAuthenticated = Auth.AuthenticateUser();
            if (!isAuthenticated)
                Console.WriteLine("Accès refusé - verifiez vos identifiants.");

        } while (isAuthenticated == false);


        // encapsulate the transactions management in a dedicated Manager
        var dbConnectionString = ConfigurationManager.ConnectionStrings["dbTransactions"].ConnectionString;
        var txManager = new TransactionManager(dbConnectionString);
        var allTxs = txManager.LoadTransactions();

        txManager.TransactionsExported += (sender, e) =>
        {
            // todo database logic
            Console.WriteLine("Export terminé !\n\tJSON généré et prêt pour traitement par client.");
            File.WriteAllText(csvPath, string.Empty);
        };

        txManager.TransactionsArchived += (sender, e) =>
        {
            Console.WriteLine("ARCHIVAGE\n \ttable Transactions nettoyée\n\tTransactions invalides envoyées au fichier Log.");
        };

        // Affichage du menu
        ConsoleKeyInfo choice;
        do
        {
            Console.Write("\n\t\t  $$$----------------------------------------------$$$\n");
            Console.Write("\t\t  $$$ Bienvenue sur le serveur de la Steeve-Bank   $$$\n");
            Console.WriteLine("\t\t  $$$----------------------------------------------$$$\n");
            Console.WriteLine("\t\t| [1] - Recupérer dernieres transactions               |");
            Console.WriteLine("\t\t| [2] - Traiter les transactions                       |");
            Console.WriteLine("\t\t| [3] - Afficher les transactions non exportées        |");
            Console.WriteLine("\t\t| [4] - Export des transactions en JSON                |");
            Console.WriteLine("\t\t| [Q] - Exit menu                                      |");
            Console.WriteLine("\t\t ------------------------------------------------------ ");

            Console.WriteLine("\t\tEnter your choice : ");
            choice = Console.ReadKey(true);

            switch (choice.Key)
            {
                case ConsoleKey.Q:
                    Console.WriteLine("\n\t\tA bientôt sur le serveur de la Steeve-Bank !\n");
                    break;
                case ConsoleKey.NumPad1:
                    recupTransactions();
                    break;
                case ConsoleKey.NumPad2:
                    txManager.traiterTransactions(csvPath);
                    break;
                case ConsoleKey.NumPad3:
                    txManager.afficherTransactions(allTxs);
                    break;
                case ConsoleKey.NumPad4:
                    await txManager.ExportTransactionsAsync(jsonPath, logPath);
                    await txManager.ArchiveValidTransactions();
                    break;
                default:
                    Console.WriteLine("Option Inconnue !");
                    break;
            }
            Console.WriteLine("\n press any key to continue...");
            Console.ReadKey(true);
            Console.Clear();
        } while (choice.Key != ConsoleKey.Q);
    }


    /* ========== RECUPERATION DES DERNIERES TRANSACTIONS ===================== */
    // recupTransactions() et SaveToCsv() - charge les dernières transactions de la Steeve-Bank
    // et les sauvegarde en CSV pour vérifications et traitement
    private static void recupTransactions()
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
            BankGenerator bankGenerator = new BankGenerator();
            var transactions = BankGenerator.GenererTransactions(txFlow);
            SaveToCsv(transactions, csvPath);
            Console.WriteLine($"\ntransactions récupérées avec succès - retrouvez-les dans le fichier {GetRelativeCsvPath(csvPath)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur ->  " + ex.Message);
        }
    }


    private static void SaveToCsv(List<Transaction> transactions, string csvPath)
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
    /* ========== END OF RECUPERATION DES TRANSACTIONS ===================== */


   
       


}
