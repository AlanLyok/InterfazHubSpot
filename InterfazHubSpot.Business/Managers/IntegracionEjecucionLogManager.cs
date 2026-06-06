using System;
using BatchSpertaAPI.Entities;
using BatchSpertaAPI.Mapping.Context;
using Mastersoft.Framework.DataRepository;
using Mastersoft.Framework.Standard;

namespace BatchSpertaAPI.Business.Managers
{
    public sealed class IntegracionEjecucionLogManager
    {
        private readonly MSContext _ctx;

        public IntegracionEjecucionLogManager(MSContext ctx)
        {
            _ctx = ctx;
        }

        public void Registrar(long? procesoId, string destino, int? clienteId, string fase, bool exito, string detalle)
        {
            var uow = new UnitOfWork(_ctx, new MSGestionContext(_ctx));
            uow.Repository<IntegracionEjecucionLog>().Insert(new IntegracionEjecucionLog
            {
                ProcesoId = procesoId,
                Destino = destino ?? string.Empty,
                ClienteId = clienteId,
                Fase = fase ?? string.Empty,
                Exito = exito,
                Detalle = detalle != null && detalle.Length > 120000 ? detalle.Substring(0, 120000) : detalle,
                FechaUtc = DateTime.UtcNow,
            });
            uow.SaveChanges();
        }
    }
}
