
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfazHubSpot.Entities;
using Mastersoft.Framework.DataRepository;
using Mastersoft.Framework.Interfaces;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Interfaces
{
    public interface IUsuariosWebManager
    {
        void Inicializar(MSContext oContexto);

        void Inicializar(MSContext oContexto, IUnitOfWorkAsync oUnitOfWork);

        ResultUsuariosWeb ValidarLogin(ParUsuariosWeb oParam);

        bool ValidarPIN(ParamPin oParam);

        Task<DatosIniAbmUsuariosWeb> TraerDatosInicialesAsync();

        Task<ResultIniUsuariosWeb> TraerTodoUsuariosWebAsync();

        Task<UsuariosWeb> TraerUsuariosWebAsync(int intId);

        Task<EntityErrors> GrabarUsuariosWebAsync(UsuariosWeb oUsuariosWeb);

        Task<EntityErrors> EliminarUsuariosWebAsync(int intId);

        UsuariosWeb NuevoUsuariosWeb();
    }
}




