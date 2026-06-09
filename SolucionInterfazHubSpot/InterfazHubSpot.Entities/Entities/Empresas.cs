
using System;
using System.ComponentModel.DataAnnotations.Schema;

using Mastersoft.Framework.Interfaces;

namespace InterfazHubSpot.Entities
{
    public partial class Empresas : Entity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CodEmpre { get; set; }

        public string Descrip { get; set; }
        public string Direccion { get; set; }
        public string Localidad { get; set; }
        public Nullable<int> Provinciaid { get; set; }
        public Nullable<int> CondFiscalID { get; set; }
        public string Cuit { get; set; }
        public string NroIngBrutos { get; set; }
        public string Demo { get; set; }
        public string Telefono { get; set; }
        public string Mail { get; set; }
        public byte[] ImagenLogo { get; set; }
        public string ContentTypeLogo { get; set; }
        public string NombreArchLogo { get; set; }

        [NotMapped]
        public bool UsaLogo { get; set; }

        public Empresas()
        {
            this.CodEmpre = 0;
            this.Descrip = "";
            this.Direccion = "";
            this.Localidad = "";
            this.Provinciaid = null;
            this.CondFiscalID = null;
            this.Cuit = "";
            this.NroIngBrutos = "";
            this.Demo = "";
            this.Telefono = "";
            this.Mail = "";
            this.ContentTypeLogo = "";
            this.NombreArchLogo = "";
            this.UsaLogo = false;
        }
    }


}




