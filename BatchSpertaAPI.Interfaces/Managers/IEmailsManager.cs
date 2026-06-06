using System.Collections.Generic;

namespace BatchSpertaAPI.Interfaces.Managers
{
    public interface IEmailsManager
    {
        void GrabarEmailErroresProcesamiento(
            string entidad,
            IEnumerable<string> errores = null);
    }
}
