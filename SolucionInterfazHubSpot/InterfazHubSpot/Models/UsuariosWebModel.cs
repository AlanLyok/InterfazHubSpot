using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InterfazHubSpot.Entities;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Models
{
    public class DatosIniAbmUsuariosWebModel
    {
        public List<MSErrorMessage> Errores { get; set; }
        public DatosIniAbmUsuariosWeb Datos { get; set; }
        public UsuariosWeb UsuariosWeb { get; set; }

        public DatosIniAbmUsuariosWebModel()
        {
            this.Errores = new List<MSErrorMessage>();
            this.Datos = new DatosIniAbmUsuariosWeb();
            this.UsuariosWeb = new UsuariosWeb();
        }
    }


    public class ResultIniUsuariosWebModel
    {
        public List<MSErrorMessage> Errores { get; set; }
        public List<UsuariosWebIni> Datos { get; set; }

        public ResultIniUsuariosWebModel()
        {
            this.Errores = new List<MSErrorMessage>();
            this.Datos = new List<UsuariosWebIni>();
        }
    }


    public class AbmUsuariosWebParam : IEntityValid
    {
        public int Id { get; set; }

        public bool Validate(List<ErrorMessage> oErrorMessages)
        {
            return oErrorMessages.Count == 0;
        }
    }


    public class AbmUsuariosWebResult
    {
        public List<MSErrorMessage> Errores { get; set; }
        public UsuariosWeb UsuariosWeb { get; set; }

        public AbmUsuariosWebResult()
        {
            this.Errores = new List<MSErrorMessage>();
            this.UsuariosWeb = new UsuariosWeb();
        }
    }


}


