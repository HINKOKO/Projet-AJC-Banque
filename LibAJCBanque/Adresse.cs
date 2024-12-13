using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace LibAJCBanque
{
    public class Adresse
    {
        private int id;
        private string libelle;
        private string complement;
        private string ville;
        private string codePostal;

        [Key]
        public int Id { get => id; set => id = value; }
        [Required,MaxLength(50)]
        public string Libelle { get => libelle; set => libelle = value; }
        [AllowNull,MaxLength(50)]
        public string Complement { get => complement; set => complement = value; }
        [Required, MaxLength(50)]
        public string Ville { get => ville; set => ville = value; }
        [Required, MinLength(5), MaxLength(5), StringLength(5)]
        public string CodePostal { get => codePostal; set => codePostal = value; }
    }
}
