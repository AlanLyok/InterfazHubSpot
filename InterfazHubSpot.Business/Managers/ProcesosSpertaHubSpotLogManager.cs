using System;
using InterfazHubSpot.Entities;
using InterfazHubSpot.Mapping.Context;
using Mastersoft.Framework.DataRepository;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Business.Managers
{
    public sealed class ProcesosSpertaHubSpotLogManager
    {
        private readonly MSContext _ctx;

        public ProcesosSpertaHubSpotLogManager(MSContext ctx)
        {
            _ctx = ctx;
        }

        public void Registrar(long? procesoId, string destino, int? identificador, string fase, bool exito, string detalle)
        {
            var uow = new UnitOfWork(_ctx, new MSGestionContext(_ctx));
            uow.Repository<ProcesosSpertaHubSpotLog>().Insert(new ProcesosSpertaHubSpotLog
            {
                ProcesoId = procesoId,
                Destino = destino ?? string.Empty,
                Identificador = identificador,
                Fase = fase ?? string.Empty,
                Exito = exito,
                Detalle = detalle != null && detalle.Length > 120000 ? detalle.Substring(0, 120000) : detalle,
                FechaGrabacion = DateTime.Now,
            });
            uow.SaveChanges();
        }
    }
}
