using System.ComponentModel.DataAnnotations;

namespace LibAJCBanque
{
    public enum TypeOperation:byte
    {
        R=1,
        V=2,
        C=3
    }
    public class Operation
    {
        private int id;
        private Compte compte;
        private Carte carte;
        private DateTime dateO;
        private TypeOperation typeOperation;
        private decimal montant;

        [Key]
        public int Id { get => id; set => id = value; }
        [Required]
        public Compte Compte { get => compte; set => compte = value; }
        [Required]
        public DateTime DateO { get => dateO; set => dateO = value; }
        [Required]
        public TypeOperation TypeOperation { get => typeOperation; set => typeOperation = value; }
        [Required]
        public decimal Montant { get => montant; set => montant = value; }
        [Required]
        public Carte Carte { get => carte; set => carte = value; }
    }
}
