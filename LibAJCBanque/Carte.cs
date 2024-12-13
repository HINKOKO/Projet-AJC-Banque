using System.ComponentModel.DataAnnotations;

namespace LibAJCBanque
{
    public class Carte
    {
        private string numeroCarte;
        private Compte compte;

        [Key, MinLength(16), MaxLength(16), StringLength(16)]
        public string NumeroCarte { get => numeroCarte; set => numeroCarte = value; }
        public Compte Compte { get => compte; set => compte = value; }

    }
}
