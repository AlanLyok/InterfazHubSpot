using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchSpertaAPI.Entities
{
    //---------------------------------------------------------
    // Clases para proyectar resultados de combos,
    // cuando la entidad original tiene muchos campos
    //---------------------------------------------------------

    public class EmpresasCombo
    {
        public int CodEmpre { get; set; }
        public string Descrip { get; set; }
    }

    public class ProvinciasCombo
    {
        public int ProvinciaID { get; set; }
        public string Descripcion { get; set; }
    }

    public class CondFiscalCombo
    {
        public int CondFiscID { get; set; }
        public string Descripcion { get; set; }
    }

}





