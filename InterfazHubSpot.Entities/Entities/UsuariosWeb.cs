using System.ComponentModel.DataAnnotations.Schema;
using Mastersoft.Framework.Interfaces;

namespace InterfazHubSpot.Entities
{
    public partial class UsuariosWeb : Entity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? EmpresaId { get; set; }
        public string Usuario { get; set; }
        public string NombreCompleto { get; set; }
        public string EMail { get; set; }
        public string Hash { get; set; }
        public string Salt { get; set; }
        public bool? UsaDobleFactor { get; set; }
        public bool? MuestraQR { get; set; }
        public int? PerfilId { get; set; }
        public string HeaderColor { get; set; }
        public bool? ModoOscuro { get; set; }
        public string KendoThemeClaro { get; set; }
        public string KendoThemeOscuro { get; set; }
        public string ImagenFondo { get; set; }
        public string ColorEtiquetas { get; set; }

        [NotMapped]
        public string Password { get; set; }

        public UsuariosWeb()
        {
            Id = 0;
            EmpresaId = null;
            Usuario = "";
            NombreCompleto = "";
            EMail = "";
            Hash = "";
            Salt = "";
            PerfilId = null;
            Password = "";
            HeaderColor = "";
            ModoOscuro = false;
            KendoThemeClaro = "";
            KendoThemeOscuro = "";
            ImagenFondo = "";
            ColorEtiquetas = "";
        }
    }
}
