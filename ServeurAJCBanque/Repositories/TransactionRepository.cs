using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ServeurAJCBanque.Models;
using ServeurAJCBanque.Repositories;

namespace ServeurAJCBanque.Repository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly string connectionString;
        // Evenement sur L'archivage des transactions
        public event EventHandler TransactionsArchived;

        public TransactionRepository(string dbConnectionString)
        {
            if (string.IsNullOrWhiteSpace(dbConnectionString))
                throw new ArgumentException("La chaîne de connexion est vide ou nulle.", nameof(dbConnectionString));
            connectionString = dbConnectionString;
        }

        public void ArchiveValidTransactions()
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


        public List<Transaction> GetAllTransactions()
        {
            var allTxs = new List<Transaction>();
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
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'insertion des transactions : {ex.Message}");
                }
                return true;
            }
        }
    }
}
