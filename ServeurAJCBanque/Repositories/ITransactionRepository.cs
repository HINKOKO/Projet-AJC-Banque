using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServeurAJCBanque.Models;


namespace ServeurAJCBanque.Repositories
{
    public interface ITransactionRepository
    {
        List<Transaction> GetAllTransactions();
        bool InsertTransactions(List<Transaction> transactions);
        void ArchiveValidTransactions();
    }
}
