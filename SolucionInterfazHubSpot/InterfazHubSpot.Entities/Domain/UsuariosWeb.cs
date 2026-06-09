using System;
using System.Collections.Generic;

using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Entities
{
    public class ResultLogin
    {
        public string NombreCompleto { get; set; }
        public string Iniciales { get; set; }
        public string Email { get; set; }

        public string MensError { get; set; }

        public List<EmpresasCombo> Empresas { get; set; }

        public ResultLogin()
        {
            this.NombreCompleto = "";
            this.Iniciales = "";
            this.Email = "";

            this.MensError = "";

            this.Empresas = new List<EmpresasCombo>();
        }
    }


    public class ParUsuariosWeb
    {
        public string usuario { get; set; }
        public string password { get; set; }
        public int EmpresaId { get; set; }

        public ParUsuariosWeb()
        {
            this.usuario = "";
            this.password = "";
            this.EmpresaId = 0;
        }
    }


    public class ResultUsuariosWeb
    {
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }

        public int UsuarioId { get; set; }
        public string Usuario { get; set; }
        public string NombreCompleto { get; set; }
        public string Iniciales { get; set; }
        public string Email { get; set; }

        public int PerfilId { get; set; }
        public bool EsMSAdnin { get; set; }
        public bool EsSuperUser { get; set; }

        public string HeaderColor { get; set; }
        public Nullable<bool> ModoOscuro { get; set; }
        public string KendoThemeClaro { get; set; }
        public string KendoThemeOscuro { get; set; }
        public string ImagenFondo { get; set; }
        public string ColorEtiquetas { get; set; }

        public bool UsaDobleFactor { get; set; }
        public bool MuestraQR { get; set; }
        public string QRCode { get; set; }

        public string MensError { get; set; }

        public ResultUsuariosWeb()
        {
            this.EmpresaId = 0;
            this.Empresa = "";

            this.UsuarioId = 0;
            this.Usuario = "";
            this.NombreCompleto = "";
            this.Iniciales = "";
            this.Email = "";

            this.PerfilId = 0;
            this.EsMSAdnin = false;
            this.EsSuperUser = false;

            this.HeaderColor = "";
            this.ModoOscuro = false;
            this.KendoThemeClaro = "";
            this.KendoThemeOscuro = "";
            this.ImagenFondo = "";
            this.ColorEtiquetas = "";

            this.UsaDobleFactor = false;
            this.MuestraQR = false;
            this.QRCode = "";

            this.MensError = "";
        }
    }


    public class ParamEstilo
    {
        public string color { get; set; }
        public string hover { get; set; }
        public string modooscuro { get; set; }
        public string estiloclaro { get; set; }
        public string estilooscuro { get; set; }
        public string imagenfondo { get; set; }
        public string coloretiquetas { get; set; }
    }


    public class ResultEstilo : EntityErrors
    {
        public string color { get; set; }
        public bool modooscuro { get; set; }
        public string estiloclaro { get; set; }
        public string estilooscuro { get; set; }
        public string imagenfondo { get; set; }
        public string coloretiquetas { get; set; }

        public List<string> fondos { get; set; }

        public ResultEstilo()
        {
            this.color = "#0078d4";
            this.modooscuro = false;
            this.estiloclaro = "bootstrap";
            this.estilooscuro = "black";
            this.imagenfondo = "";
            this.coloretiquetas = "";

            this.fondos = new List<string>();
        }
    }


    public class ParamLogin
    {
        public string email { get; set; }
        public string password { get; set; }
    }


    public class ParamPin
    {
        public string usuario { get; set; }
        public string pin { get; set; }

        public ParamPin()
        {
            this.usuario = "";
            this.pin = "";
        }
    }
}
