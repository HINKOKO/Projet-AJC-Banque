using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibAJCBanque;

namespace AppAJCBanque.DTO
{
    public class OperationXML
    {
        public string NumeroCompte { get; set; }
        public DateTime DateOperation { get; set; }
        public string TypeOperation { get; set; }
        public decimal Montant { get; set; }
        public string NumeroCarteMasque { get; set; }
    }
}
