using System.Collections.Generic;

namespace InterfazHubSpot.Interfaces.Managers
{
    public interface IEmailsManager
    {
        void GrabarEmailErroresProcesamiento(
            string entidad,
            IEnumerable<string> errores = null);
    }
}
