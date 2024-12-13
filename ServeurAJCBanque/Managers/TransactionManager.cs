using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;
using ServeurAJCBanque.MockBank;
using ServeurAJCBanque.Services;
using ServeurAJCBanque.Models;
using ServeurAJCBanque.Repositories;
using System.Diagnostics;

namespace ServeurAJCBanque.Managers;

public class TransactionManager : ITransactionManager
{
    private readonly ITransactionRepository repository;
    private readonly ExchangeRate exchangeRateService;


    public event EventHandler TransactionsExported;

    public TransactionManager(ITransactionRepository repository)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.exchangeRateService = new ExchangeRate();
    }

    /* ========== CHARGEMENT DES TRANSACTIONS ========== */
    public List<Transaction> LoadTransactions()
    {
        return repository.GetAllTransactions();
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
    public List<Transaction> GetValidTransactions()
    {
        return LoadTransactions().Where(tx => tx.IsValid).ToList();
    }

    // GetInvalidTransactions - LINQ sur toutes transactions pour filtrer
    public List<Transaction> GetInvalidTransactions()
    {
        return LoadTransactions().Where(tx => !tx.IsValid).ToList();
    }

    public void TraiterTransactions(string csvPath)
    {
        try
        {
            var transactions = ParseCSV(csvPath);

            if (repository.InsertTransactions(transactions))
                Console.WriteLine($"{transactions.Count} transactions insérées en base.");
            else
                Console.WriteLine($"transactions -> echec insertions");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur analyse transactions -> " + ex.Message);
        }
    }



    /* ========== TRAITEMENT DU FICHIERS CSV ========== */
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
                Rate = vtx.Devise == "EUR" ? (decimal?)1 : await exchangeRateService.GetExchangeRateAsync(vtx.Devise)
            };
            txsToExport.Add(vtxExport);
        }

        string jsonData = JsonSerializer.Serialize(txsToExport, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(jsonPath, jsonData);


        // 2. Write invalid transactions to log
        var invalidTransactions = GetInvalidTransactions();
        var logData = string.Join(Environment.NewLine, invalidTransactions.Select(tx => tx.ToString()));
        await File.WriteAllTextAsync(logPath, logData);

        // 3. Clear all transactions ? 

        // 4. Notify listeners that the export is complete
        TransactionsExported?.Invoke(this, EventArgs.Empty);
    }

    /* ========== ARCHIVAGE DES TRANSACTIONS VALIDES ========== */

   
    public async Task ArchiveValidTransactions()
    {
        repository.ArchiveValidTransactions();
    }
}
