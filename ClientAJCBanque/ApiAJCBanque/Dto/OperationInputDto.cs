using LibAJCBanque;

namespace ApiAJCBanque.Dto
{
    public class OperationInputDto
    {
        /* Structure du fichier Json 
        {
            "NumeroCarte": "4974018502239490",
            "Montant": 881,
            "TypeOp": 0,
            "DateOperation": "2024-09-25T15:06:24",
            "Devise": "USD",
            "IsValid": true,
            "Rate": 0.9522
        }*/

        public string NumeroCarte { get; set; }
        public decimal Montant { get; set; }
        public TypeOperation TypeOp { get; set; }
        public DateTime DateOperation { get; set; }
        public string Devise { get; set; }
        public bool IsValid { get; set; }
        public decimal? Rate { get; set; }
    }

}
