using System.Configuration;
using ServeurAJCBanque.Models;
using ServeurAJCBanque.Services;
using ServeurAJCBanque.Managers;
using ServeurAJCBanque.MockBank;
using ServeurAJCBanque.Authentication;
using System.Globalization;
using ServeurAJCBanque.Repository;
using ServeurAJCBanque.Internal;
using ServeurAJCBanque.Internal;

public class Program
{
    //private static List<Transaction> allTxs = null;
    static string csvPath = ConfigurationManager.AppSettings["CsvFilePath"];
    static string jsonPath = ConfigurationManager.AppSettings["JSONFilePath"];
    static string logPath = ConfigurationManager.AppSettings["logFilePath"];
    static string authConnectionString = ConfigurationManager.ConnectionStrings["dbAuth"].ConnectionString;

    // 10 transactions dragged back from server by default
    static int txFlow = 10;
    static bool jsonAvailable = false;


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
            isAuthenticated = Auth.AuthenticateUser(authConnectionString);
            if (!isAuthenticated)
                Console.WriteLine("Accès refusé - verifiez vos identifiants.");

        } while (isAuthenticated == false);


        // encapsulate the transactions management in a dedicated Manager
        var dbConnectionString = ConfigurationManager.ConnectionStrings["dbTransactions"].ConnectionString;
        var transactionRepository = new TransactionRepository(dbConnectionString);
        var txManager = new TransactionManager(transactionRepository);
        var txPuller = new TransactionPuller(csvPath);
        var bankGenerator = new BankGenerator();


        var allTxs = txManager.LoadTransactions();

        // Abonnement aux événements pour déclencher les délégués correspondants
        txManager.TransactionsExported += (sender, e) =>
        {
            // todo database logic
            Console.WriteLine("Export terminé !\n\tJSON généré et prêt pour traitement par client.");
            File.WriteAllText(csvPath, string.Empty);
        };

        txManager.JsonAvailable += (sender, e) =>
        {
            jsonAvailable = !jsonAvailable;
        };

        txPuller.CSVModif += (sender, e) =>
        {
            Console.WriteLine("Fichier CSV -> nouvelles transactions !");
        };

        transactionRepository.TransactionsArchived += (sender, e) =>
        {
            Console.WriteLine("ARCHIVAGE\n \tTable Transactions nettoyée\n\tTransactions invalides envoyées au fichier Log.");
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

            Console.WriteLine("\t\t Choix menu : ");
            choice = Console.ReadKey(true);

            switch (choice.Key)
            {
                case ConsoleKey.Q:
                    Console.WriteLine("\n\t\tA bientôt sur le serveur de la Steeve-Bank !\n");
                    break;
                case ConsoleKey.NumPad1:
                    txPuller.RecupTransactions(bankGenerator, txFlow);
                    txPuller.StopWatching();
                    break;
                case ConsoleKey.NumPad2:
                    txManager.TraiterTransactions(csvPath);
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
            Console.ReadKey(true);
            Console.Clear();
        } while (choice.Key != ConsoleKey.Q);
    }
}
