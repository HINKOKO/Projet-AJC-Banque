using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServeurAJCBanque.Models;

namespace ServeurAJCBanque.Managers
{
    /// <summary>
    /// Interface pour la logique de management des transactions
    /// </summary>
    public interface ITransactionManager
    {
        List<Transaction> LoadTransactions();
        List<Transaction> GetValidTransactions();
        List<Transaction> GetInvalidTransactions();
        Task ExportTransactionsAsync(string jsonPath, string logPath);
        Task ArchiveValidTransactions();
        void TraiterTransactions(string csvPath);
    }
}
