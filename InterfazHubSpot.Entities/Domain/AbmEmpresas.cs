using System.Collections.Generic;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Entities
{
    public class DatosIniAbmEmpresas
    {
        public List<ProvinciasCombo> Provincias { get; set; }
        public List<CondFiscalCombo> CondFiscal { get; set; }
    }

    public partial class Empresas : IEntityKeyValid
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
            return oErrorMessages.Count == 0;
        }
    }




    public class ResultIniEmpresas
    {
        public List<EmpresasIni> Empresas { get; set; }
    }


    public class EmpresasIni
    {
        public int CodEmpre { get; set; }
        public string Descrip { get; set; }
    }

}



