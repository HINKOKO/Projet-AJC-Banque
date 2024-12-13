using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibAJCBanque
{
    public enum Sexe:byte
    {
         M=1,F=2,A=3
    }
    public class ClientPar: Client
    {
        private Sexe sexe;
        private string prenom;
        private DateTime dateNaissance;
        private readonly string type = "Particulier";

        public ClientPar()
            : base(){}

        [JsonPropertyOrder(5)]
        public Sexe Sexe { get => sexe; set => sexe = value; }
        [MaxLength(50)]
        [JsonPropertyOrder(3)]
        public string Prenom { get => prenom; set => prenom = value; }
        [JsonPropertyOrder(6)]
        public DateTime DateNaissance { get => dateNaissance; set => dateNaissance = value; }
        [JsonPropertyOrder(7)]
        public string Type { get => type; }
    }
}
