using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace LibAJCBanque
{
    public abstract class Client
    {
        private int identifiant;
        private string nom;
        private string mail;
        private Adresse adresse;

        private Regex regexMail = new Regex("");

        [Key,JsonPropertyOrder(1)]
        public int Identifiant { get => identifiant; set => identifiant = value; }
        
        [Required, JsonPropertyOrder(2)]
        [MaxLength(50)]
        public string Nom { get => nom; set => nom = value; }
        
        [Required, JsonPropertyOrder(4)]
        [MaxLength(50)]
        //[RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$",
        //    ErrorMessage = "Veuillez entrer une adresse email valide.")]
        public string Mail { get => mail; set => mail = value; }
        
        [JsonPropertyOrder(8)]
        public Adresse Adresse { get => adresse; set => adresse = value; }
    }
}
