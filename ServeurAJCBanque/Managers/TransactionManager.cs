using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ServeurAJCBanque.Helpers;
using ServeurAJCBanque.MockBank;

namespace ServeurAJCBanque.Models
{
    public class TransactionManager
    {
        private readonly string connectionString;
        private static List<Transaction> allTxs = new List<Transaction>();

        public event EventHandler TransactionsExported;
        public event EventHandler TransactionsArchived;

        public TransactionManager(string dbConnectionString)
        {
            if (string.IsNullOrWhiteSpace(dbConnectionString))
            {
                throw new ArgumentException("La chaîne de connexion est vide ou nulle.", nameof(dbConnectionString));
            }
            connectionString = dbConnectionString;
            allTxs = LoadTransactions();
        }

        /* ========== CHARGEMENT DES TRANSACTIONS ========== */
        public List<Transaction> LoadTransactions()
        {
            allTxs.Clear();

            using (var dbConn = new SqlConnection(connectionString))
            {
                try
                {
                    dbConn.Open();
                    var cmd = dbConn.CreateCommand();
                    cmd.CommandText = "SELECT Id, NumeroCarte, Montant, TypeOp, DateOperation, Devise, IsValid FROM Transactions";
                    var rdr = cmd.ExecuteReader();

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
                    Console.WriteLine($"Erreur lors du chargement des transactions : {ex.Message}");
                }
            }
            return allTxs;
        }

        public void afficherTransactions(List<Transaction> allTxs)
        {
            if (allTxs.Count == 0)
            {
                try
                {
                    allTxs = LoadTransactions();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Probleme au reload des transactions " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("vous n'avez traité aucune transaction pour le moment.");
                Console.WriteLine("Penser a les recupérer puis traiter via menu 2.");
            }
            // Construire chaine de resultats
            StringBuilder sb = new StringBuilder();

            // Filtrer avec LINQ - grouper par valides puis invalides
            var validTxs = GetValidTransactions();
            var invalidTxs = GetInvalidTransactions();

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

        // GetValidTransactions - LINQ sur toutes transactions pour filtrer
        public static List<Transaction> GetValidTransactions()
        {
            return allTxs.Where(tx => tx.IsValid).ToList();
        }

        // GetInvalidTransactions - LINQ sur toutes transactions pour filtrer
        public static List<Transaction> GetInvalidTransactions()
        {
            return allTxs.Where(tx => !tx.IsValid).ToList();
        }

        public void traiterTransactions(string csvPath)
        {
            try
            {
                var transactions = ParseCSV(csvPath);

                if (InsertTransactions(transactions))
                    Console.WriteLine($"{transactions.Count} transactions insérées en base.");
                else
                    Console.WriteLine($"transactions -> echec insertions");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur analyse transactions -> " + ex.Message);
            }
        }


        /* ========== INSERTION DES TRANSACTIONS ========== */
        public bool InsertTransactions(List<Transaction> transactions)
        {
            if (transactions.Count == 0)
            {
                Console.WriteLine("Aucune transaction à insérer.");
                return false;
            }

            using (var dbConn = new SqlConnection(connectionString))
            {
                try
                {
                    dbConn.Open();
                    using (var transaction = dbConn.BeginTransaction())
                    {
                        foreach (var tx in transactions)
                        {
                            var cmd = dbConn.CreateCommand();
                            cmd.Transaction = transaction;
                            cmd.CommandText = @"
                                INSERT INTO Transactions (NumeroCarte, Montant, TypeOp, DateOperation, Devise, IsValid)
                                VALUES (@NumeroCarte, @Montant, @TypeOp, @DateOperation, @Devise, @IsValid)";

                            cmd.Parameters.AddWithValue("@NumeroCarte", tx.NumeroCarte);
                            cmd.Parameters.AddWithValue("@Montant", tx.Montant);
                            cmd.Parameters.AddWithValue("@TypeOp", (byte)tx.TypeOp);
                            cmd.Parameters.AddWithValue("@DateOperation", tx.DateOperation);
                            cmd.Parameters.AddWithValue("@Devise", tx.Devise);
                            cmd.Parameters.AddWithValue("@IsValid", tx.IsValid);

                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        Console.WriteLine($"{transactions.Count} transactions insérées.");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'insertion des transactions : {ex.Message}");
                    return false;
                }
            }
        }

        /* ========== TRAITEMENT DES FICHIERS CSV ========== */
        public List<Transaction> ParseCSV(string csvPath)
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

        // parseur de type operation
        public static TypeOperation ParseOpType(string typeOp)
        {
            return typeOp switch
            {
                "Retrait" => TypeOperation.Retrait,
                "Facture" => TypeOperation.Facture,
                "Depot" => TypeOperation.Depot,
                _ => TypeOperation.Invalid
            };
        }



        /* =========== EXPORT EN JSON && CLEAN DE LA TABLE TRANSACTIONS ======= */
        public async Task ExportTransactionsAsync(string jsonPath, string logPath)
        {
            // 1. Export valid transactions to JSON
            var validTransactions = GetValidTransactions();

            string JSONdir = Path.GetDirectoryName(jsonPath);
            if (!Directory.Exists(JSONdir))
            {
                Directory.CreateDirectory(JSONdir);
            }

            string LOGdir = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(LOGdir))
            {
                Directory.CreateDirectory(LOGdir);
            }

            // Convert to JSON with exchange rates
            var txsToExport = new List<object>();
            foreach (var vtx in validTransactions)
            {
                var vtxExport = new
                {
                    vtx.NumeroCarte,
                    vtx.Montant,
                    vtx.TypeOp,
                    vtx.DateOperation,
                    vtx.Devise,
                    vtx.IsValid,
                    Rate = vtx.Devise == "EUR" ? (decimal?)1 : await GetExchangeRateAsync(vtx.Devise)
                };
                txsToExport.Add(vtxExport);
            }

            string jsonData = JsonSerializer.Serialize(txsToExport, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(jsonPath, jsonData);


            // 2. Write invalid transactions to log
            var invalidTransactions = GetInvalidTransactions();
            var logData = string.Join(Environment.NewLine, invalidTransactions.Select(tx => tx.ToString()));
            await File.WriteAllTextAsync(logPath, logData);

            // 3. Clear all transactions
            allTxs.Clear();

            // 4. Notify listeners that the export is complete
            TransactionsExported?.Invoke(this, EventArgs.Empty);
        }

        /* ========== ARCHIVAGE DES TRANSACTIONS VALIDES ========== */
        public async Task ArchiveValidTransactions()
        {
            using (var dbConn = new SqlConnection(connectionString))
            {
                try
                {
                    dbConn.Open();
                    var cmd = dbConn.CreateCommand();

                    // Insérer dans TransactionArchive
                    cmd.CommandText = @"
                        INSERT INTO TransactionsArchives (NumeroCarte, Montant, TypeOp, DateOperation, Devise, IsValid)
                        SELECT NumeroCarte, Montant, TypeOp, DateOperation, Devise, IsValid
                        FROM Transactions
                        WHERE IsValid = 1";
                    int rowsInserted = cmd.ExecuteNonQuery();
                    Console.WriteLine($"{rowsInserted} transactions archivées.");

                    // Supprimer les transactions valides de la table Transactions
                    cmd.CommandText = "DELETE FROM Transactions;";
                    int rowsDeleted = cmd.ExecuteNonQuery();
                    Console.WriteLine($"{rowsDeleted} transactions supprimées de la table principale.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'archivage : {ex.Message}");
                }
                TransactionsArchived?.Invoke(this, EventArgs.Empty);
            }
        }

        public static async Task<decimal> GetExchangeRateAsync(string devise)
        {
            string apiUrl = ConfigurationManager.AppSettings["ExchangeRateUrl"];
            string apiKey = ConfigurationManager.AppSettings["ExchangeRateApiKey"];

            using HttpClient client = new HttpClient();
            string requestUrl = $"{apiUrl}{apiKey}/latest/{devise}";

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
}
