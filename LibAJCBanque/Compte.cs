using System.ComponentModel.DataAnnotations;

namespace LibAJCBanque
{

    public class Compte
    {
        private string numeroCompte;
        private DateTime dateOuverture;
        private decimal solde;
        private Client client;

        [Key, MinLength(11), MaxLength(11), StringLength(11)]
        public string NumeroCompte { get => numeroCompte; set => numeroCompte = value; }
        public DateTime DateOuverture { get => dateOuverture; set => dateOuverture = value; }
        public decimal Solde { get => solde; set => solde = value; }
        public Client Client { get => client; set => client = value; }
    }
}
