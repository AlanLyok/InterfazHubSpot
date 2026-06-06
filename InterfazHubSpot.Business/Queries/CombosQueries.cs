using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using InterfazHubSpot.Entities;
using Mastersoft.Framework.Interfaces;

namespace InterfazHubSpot.Business
{
    public class CombosQueries
    {
        //--------------------------------------------------
        //  Variables Privadas
        //--------------------------------------------------

        private IUnitOfWorkAsync mobjUnitOfWork;

        //--------------------------------------------------
        //  Constructor
        //--------------------------------------------------

        public CombosQueries(IUnitOfWorkAsync oUnitOfWork)
        {
            mobjUnitOfWork = oUnitOfWork;
        }

        //--------------------------------------------------
        //  Metodos Publicos
        //--------------------------------------------------

        public async Task<List<EmpresasCombo>> GetEmpresasComboAsync()
        {
            return await mobjUnitOfWork.Repository<Empresas>()
                                       .Queryable()
                                       .Where(x => x.CodEmpre > 0)
                                       .OrderBy(x => x.CodEmpre)
                                       .Select(x => new EmpresasCombo()
                                       {
                                           CodEmpre = x.CodEmpre,
                                           Descrip = x.Descrip
                                       })
                                       .ToListAsync();
        }


        public async Task<List<PerfilesWeb>> GetPerfilesWebComboAsync(int intEmpresaId)
        {
            intEmpresaId = mobjUnitOfWork.GetMulti("PerfilesWeb", intEmpresaId);

            var result = await mobjUnitOfWork.Repository<PerfilesWeb>()
                                             .Queryable()
                                             .Where(x => x.EmpresaId == intEmpresaId)
                                             .OrderBy(x => x.Descripcion)
                                             .ToListAsync();

            result.Insert(0, new PerfilesWeb()
            {
                Id = 0,
                Descripcion = "Acceso Total",
                EmpresaId = intEmpresaId,
                ObjectState = 0
            });

            return result;
        }
    }
}





