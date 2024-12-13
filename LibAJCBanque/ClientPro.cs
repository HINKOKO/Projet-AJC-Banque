using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibAJCBanque
{
    public enum StatutJuridique : byte
    {
        SA = 1,
        SAS = 2,
        SARL = 3, 
        EURL = 4
    }
    public class ClientPro: Client
    {
        private string siret;
        private StatutJuridique statutJuridique;
        private Adresse adresseSiege;
        private readonly string type = "Professionnel";

        public ClientPro()
            : base() { }

        [Required,MinLength(14), MaxLength(14), StringLength(14)]
        [JsonPropertyOrder(10)]
        public string Siret { get => siret; set => siret = value; }
        
        [Required]
        [JsonPropertyOrder(11)]
        public StatutJuridique StatutJuridique { get => statutJuridique; set => statutJuridique = value; }
        
        [JsonPropertyOrder(9)]
        public Adresse AdresseSiege { get => adresseSiege; set => adresseSiege = value; }
        
        [Required,StringLength(20)]
        [JsonPropertyOrder(7)]
        public string Type { get => type; }
    }
}
