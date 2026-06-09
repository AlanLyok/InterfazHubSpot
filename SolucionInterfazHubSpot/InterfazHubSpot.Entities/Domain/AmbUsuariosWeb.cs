
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfazHubSpot.Entities;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Entities
{
    public class DatosIniAbmUsuariosWeb
    {
        public List<PerfilesWeb> PerfilesWeb { get; set; }
        public List<EmpresasCombo> Empresas { get; set; }
    }

    public partial class UsuariosWeb : IEntityKeyValid
    {
        //--------------------------------------------------------------------------------
        //   Implementacion de IEntityValid
        //--------------------------------------------------------------------------------

        public bool ValidateKey(List<ErrorMessage> oErrorMessages)
        {
            return oErrorMessages.Count == 0;
        }


        public bool Validate(List<ErrorMessage> oErrorMessages)
        {
            if (String.IsNullOrWhiteSpace(this.Usuario))
            {
                oErrorMessages.Add(new ErrorMessage("El campo 'Usuario' no debe estar vacio", "Usuario"));
            }

            if (String.IsNullOrWhiteSpace(this.NombreCompleto))
            {
                oErrorMessages.Add(new ErrorMessage("El campo 'Nombre Completo' no debe estar vacio", "NombreCompleto"));
            }

            if (this.Id == 0 && String.IsNullOrWhiteSpace(this.Password))
            {
                oErrorMessages.Add(new ErrorMessage("El campo 'Contraseña' no debe estar vacio", "Password"));
            }

            if (this.PerfilId == null)
            {
                oErrorMessages.Add(new ErrorMessage("El campo 'Perfil' debe estar informado", "PerfilId"));
            }

            return oErrorMessages.Count == 0;
        }
    }




    public class ResultIniUsuariosWeb
    {
        public List<UsuariosWebIni> UsuariosWeb { get; set; }
    }


    public class UsuariosWebIni
    {
        public int Id { get; set; }
        public string Usuario { get; set; }
        public string NombreCompleto { get; set; }
        public string PwDescripcion { get; set; }
    }

}



