using InterfazHubSpot.Entities;
using InterfazHubSpot.Mapping.Context;
using Mastersoft.Framework.DataRepository;
using Mastersoft.Framework.Interfaces;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Business
{
    public class ErroresManager
    {
        //--------------------------------------------------
        //  Variables Privadas
        //--------------------------------------------------

        private IUnitOfWorkAsync mobjUnitOfWork;

        //--------------------------------------------------
        //  Constructor
        //--------------------------------------------------

        public ErroresManager(MSContext oContexto)
        {
            mobjUnitOfWork = new UnitOfWork(oContexto, new MSGestionContext(oContexto));
        }

        //--------------------------------------------------
        //  Metodos Publicos
        //--------------------------------------------------

        public void Grabar(Errores oErrores)
        {
            mobjUnitOfWork.Repository<Errores>().Insert(oErrores);

            mobjUnitOfWork.SaveChanges();
        }

    }
}

