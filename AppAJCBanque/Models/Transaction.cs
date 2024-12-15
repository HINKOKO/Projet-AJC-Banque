
namespace AppAJCBanque.Models
{

    public enum TypeOperation : byte
    {
        Retrait = 0,
        Facture = 1,
        Depot = 2,
        Invalid = 3,
    }

    public class Transaction
    {
        public int Id { get; set; } // Clé primaire

        public string NumeroCarte { get; set; } // 16 caractères

        public decimal Montant { get; set; }

        public TypeOperation TypeOp { get; set; } // Retrait, Facture, Dépôt

        public DateTime DateOperation { get; set; }


        public string Devise { get; set; }

        public bool IsValid { get; set; } // 1 pour valide, 0 pour invalide

        public Transaction()
        {
        }

        public Transaction(int id, string numeroCarte, decimal montant, TypeOperation typeOp, DateTime dateOperation, string devise, bool isValid)
        {
            Id = id;
            NumeroCarte = numeroCarte;
            Montant = montant;
            TypeOp = typeOp;
            DateOperation = dateOperation;
            Devise = devise;
            IsValid = isValid;
        }

        public override string ToString()
        {
            return $"Numéro carte: {NumeroCarte} | Montant: {Montant} {Devise} | Type: {TypeOp} | Date: {DateOperation} | Valide: {IsValid}";
        }
    }
}
