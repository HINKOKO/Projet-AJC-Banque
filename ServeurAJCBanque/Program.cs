using System.Configuration;
using Microsoft.Data.SqlClient;
using ServeurAJCBanque.Models;
using ServeurAJCBanque.MockBank;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using ServeurAJCBanque.Authentication;
using ServeurAJCBanque.DbConnection;
using System.Globalization;

namespace ServeurAJCBanque.Helpers;


internal class Program
{
    private static List<Transaction> allTxs = null;
    //static string csvPath = @"C:\Users\piotr\OneDrive\Documents\Back_Up_Pizzonio\POEI\NET\sln_AJCBanque\ServeurAJCBanque\datas\transactions\nouvelles_transactions.csv";
    static string csvPath = ConfigurationManager.AppSettings["CsvFilePath"];
    private static void Main(string[] args)
    {
        // Au demarage console - demander identifiants
        Console.WriteLine("\n\t\tLa Steeve-Bank vous demande de vous identifer\n");
        bool isAuthenticated = false;
        do
        {
            isAuthenticated = Auth.AuthenticateUser();
            if (!isAuthenticated)
                Console.WriteLine("Accès refusé - verifiez vos identifiants.");

        } while (isAuthenticated == false);

        // si accès autoriser - load de toutes les transactions dans une liste
        loadTransactions();

        // Boucle sur Menu
        ConsoleKeyInfo choice;
        do
        {
            Console.WriteLine("\n\t\tBienvenue sur le serveur de la Steeve-Bank !\n");
            Console.WriteLine("\t\t| 1 - Recupérer dernieres transactions                 |");
            Console.WriteLine("\t\t| 2 - Traiter les transactions                         |");
            Console.WriteLine("\t\t| 3 - Afficher les transactions non exportées          |");
            Console.WriteLine("\t\t| 4 - Export des transactions en JSON                  |");
            Console.WriteLine("\t\t| Q - Exit menu                                        |");
            Console.WriteLine("\t\t ------------------------------------------------------ ");

            Console.WriteLine("\t\tEnter your choice : ");
            choice = Console.ReadKey();

            switch (choice.Key)
            {
                case ConsoleKey.Q:
                    Console.WriteLine("\n\t\tA bientôt sur le serveur de la Steeve-Bank !\n");
                    break;
                case ConsoleKey.NumPad1:
                    recupTransactions();
                    break;
                case ConsoleKey.NumPad2:
                    traiterTransactions();
                    break;
                case ConsoleKey.NumPad3:
                    afficherTransactions(allTxs);
                    break;
                case ConsoleKey.NumPad4:
                    exportJSON();
                    break;
                default:
                    Console.WriteLine("Option Inconnue !");
                    break;
            }
            Console.WriteLine("\n press any key to continue...");
            Console.ReadKey();
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
            var bankGenerator = new BankGenerator();
            var transactions = bankGenerator.GenererTransactions(10);
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



    /* ============================== VERIFICATION ET TRAITEMENT TRANSACTIONS ================== */
    public static void traiterTransactions()
    {
        try
        {
            var transactions = parseCSV(csvPath);

            if (InsertIntoDatabase(transactions))
                Console.WriteLine($"{transactions.Count} transactions insérées en base.");
            else
                Console.WriteLine($"transactions -> echec insertions");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur analyse transactions -> " + ex.Message);
        }
    }

    public static List<Transaction> parseCSV(string csvPath)
    {
        var transactions = new List<Transaction>();
        foreach (var line in File.ReadLines(csvPath))
        {
            var cols = line.Split(';');
            Console.WriteLine($"{cols}");
            if (cols.Length == 5)
            {
                Console.WriteLine("Luhn tchik check sur " + cols[0]);
                // Validation Luhn et TypeOp
                bool isValidCard = BankGenerator.LuhnAlgorithm(cols[0]); // Numéro valide si le check-digit est 0
                Console.WriteLine($"{isValidCard} sur {cols[0]}");
                TypeOperation typeOp = ParseOpType(cols[2]);
                bool isValidTypeOp = Enum.IsDefined(typeof(TypeOperation), typeOp) && typeOp != TypeOperation.Invalid;

                var tx = new Transaction
                {
                    NumeroCarte = cols[0],
                    Montant = decimal.Parse(cols[1], CultureInfo.InvariantCulture),
                    TypeOp = typeOp,
                    DateOperation = DateTime.ParseExact(cols[3], "yyyy-dd-MMTHH:mm:ss", CultureInfo.InvariantCulture),
                    Devise = cols[4],
                    IsValid = isValidCard && isValidTypeOp // Marquer valide si les deux conditions sont remplies
                };

                transactions.Add(tx);
            }
        }
        return transactions;
    }
    static TypeOperation ParseOpType(string typeOp)
    {
        // Convertit le type d'opération de string à l'enum
        return typeOp switch
        {
            "Retrait" => TypeOperation.Retrait,
            "Facture" => TypeOperation.Facture,
            "Depot" => TypeOperation.Depot,
            _ => TypeOperation.Invalid // Si l'opération est inconnue, on retourne Invalid
        };
    }
    static bool InsertIntoDatabase(List<Transaction> transactions)
    {
        // Si aucune transaction à insérer
        if (transactions.Count == 0)
        {
            Console.WriteLine("Aucune transaction à insérer.");
            return false;
        }
        string connectionString = ConfigurationManager.ConnectionStrings["dbTransactions"]?.ConnectionString;


        // Démarrer une transaction SQL pour garantir que toutes les insertions réussissent
        using (var dbConn = new SqlConnection(connectionString))
        {
            try
            {
                dbConn.Open();
                Console.WriteLine("dbconn state -> " + dbConn.State);

                using (var transaction = dbConn.BeginTransaction())
                {
                    foreach (var tx in transactions)
                    {
                        var cmd = dbConn.CreateCommand();
                        cmd.Transaction = transaction;
                        cmd.CommandText = @"
                                INSERT INTO Transactions (NumeroCarte, Montant, TypeOp, DateOperation, Devise, IsValid)
                                VALUES (@NumeroCarte, @Montant, @TypeOp, @DateOperation, @Devise, @IsValid)";

                        cmd.Parameters.Add(new SqlParameter("@NumeroCarte", tx.NumeroCarte));
                        cmd.Parameters.Add(new SqlParameter("@Montant", tx.Montant));
                        cmd.Parameters.Add(new SqlParameter("@TypeOp", (byte)tx.TypeOp));
                        cmd.Parameters.Add(new SqlParameter("@DateOperation", tx.DateOperation));
                        cmd.Parameters.Add(new SqlParameter("@Devise", tx.Devise));
                        cmd.Parameters.Add(new SqlParameter("@IsValid", tx.IsValid));

                        // Exécuter la commande d'insertion
                        cmd.ExecuteNonQuery();
                    }

                    // Commit de la transaction
                    transaction.Commit();
                    Console.WriteLine($"{transactions.Count} transactions insérées.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'insertion en base : {ex.Message}");
                return false;
            }
        }
    }

    // ======= END OF VERIFICATION ET INSERTION EN BASE =========



    // ======= AFFICHER TRANSACTIONS NON EXPORTEES ============
    public static List<Transaction> loadTransactions()
    {
        allTxs = new List<Transaction>();
        using (var dbConn = DatabaseConnection.Instance.GetConnection("dbTransactions"))
        {
            try
            {
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "SELECT Id, NumeroCarte, Montant, TypeOp, DateOperation, Devise, IsValid FROM Transactions";
                var rdr = cmd.ExecuteReader();

                // empty case
                if (!rdr.HasRows)
                {
                    Console.WriteLine("aucun transaction en base!");
                }

                while (rdr.Read())
                {
                    int id = Convert.ToInt32(rdr["Id"]);
                    string numerocarte = rdr["NumeroCarte"].ToString();
                    decimal montant = Convert.ToDecimal(rdr["Montant"]);
                    TypeOperation typeOp = (TypeOperation)Convert.ToInt32(rdr["TypeOp"]);
                    DateTime dateTime = Convert.ToDateTime(rdr["DateOperation"]);
                    string devise = rdr["Devise"].ToString();
                    bool isvalid = Convert.ToBoolean(rdr["IsValid"]);

                    var tx = new Transaction(id, numerocarte, montant, typeOp, dateTime, devise, isvalid);
                    allTxs.Add(tx);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors du chargement de vos transactions en base" + ex.Message + ex.HelpLink);
            }
            finally
            {
                dbConn.Close();
            }
        }
        return allTxs;
    }

    public static void afficherTransactions(List<Transaction> allTxs)
    {
        // Construire chaine de resultats
        StringBuilder sb = new StringBuilder();

        // Filtrer avec LINQ - grouper par valides puis invalides
        var validTxs = allTxs.Where(tx => tx.IsValid).ToList();
        var invalidTxs = allTxs.Where(tx => !tx.IsValid).ToList();

        sb.AppendLine($"\n------ {validTxs.Count} transactions valides non exportées en JSON ------\n");
        if (validTxs.Count > 0)
        {
            foreach (var tx in validTxs)
            {
                sb.AppendLine(tx.ToString());
                sb.AppendLine("\n");
            }
            sb.AppendLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n\n");
            Console.WriteLine(sb.ToString());
        }
        sb.Clear();

        sb.AppendLine($"\n------ {invalidTxs.Count} transactions invalides non exportées en LOG\n");
        if (invalidTxs.Count > 0)
        {
            foreach (var tx in invalidTxs)
            {
                sb.AppendLine(tx.ToString());
                sb.AppendLine("\n");
            }
            Console.WriteLine(sb.ToString());
        }

    }

    //======= EXPORT AU FORMAT JSON DES TRANSACTIONS VALIDES =========
    private static async Task exportJSON()
    {
        string exportJSONPath = ConfigurationManager.AppSettings["JSONFilePath"];
        string directory = Path.GetDirectoryName(exportJSONPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // LINQ sur la méthode pour filtrer directement les transactions valides
        var validTxs = allTxs.Where(tx => tx.IsValid).ToList();

        // Objets anonymes - pour ajout a la volée du taux si devise != EUR
        var txsToExport = new List<object>();

        foreach (var vtx in validTxs)
        {
            var txExport = new
            {
                vtx.NumeroCarte,
                vtx.Montant,
                vtx.TypeOp,
                vtx.DateOperation,
                vtx.Devise,
                vtx.IsValid,
                Rate = vtx.Devise.Equals("EUR") ? (decimal?)null : await GetExchangeRateAsync(vtx.Devise)
            };
            txsToExport.Add(txExport);

        }
        string jsonData = JsonSerializer.Serialize(txsToExport, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(jsonData);
        File.WriteAllText(exportJSONPath, jsonData);

        Console.WriteLine($"TX valides exportés vers {exportJSONPath}");
    }

    public static async Task<decimal> GetExchangeRateAsync(string devise)
    {
        string apiUrl = ConfigurationManager.AppSettings["ExchangeRateUrl"];
        string apiKey = ConfigurationManager.AppSettings["ExchangeRateApiKey"];

        using HttpClient client = new HttpClient();
        string requestUrl = $"{apiUrl}{apiKey}/latest/{devise}";
        Console.WriteLine($"on fetch vers -> {requestUrl}");

        try
        {
            var response = await client.GetAsync(requestUrl);
            string jsonResponse = await response.Content.ReadAsStringAsync();

            var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);
            if (jsonData.TryGetValue("conversion_rates", out var rates) && rates is JsonElement rateElement)
            {
                if (rateElement.TryGetProperty("EUR", out var rateValue))
                {
                    return rateValue.GetDecimal();
                }
                else
                {
                    throw new Exception($"Taux de change pour {devise} non trouvé");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur API taux de change -> " + ex.Message);
            throw;
        }
        return 1;
    }
}
