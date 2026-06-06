using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using InterfazHubSpot.Business.Integration;
using InterfazHubSpot.Entities;
using InterfazHubSpot.Mapping.Context;
using Mastersoft.Framework.DataRepository;
using Mastersoft.Framework.Interfaces;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Business.Managers
{
    /// <summary>Lectura y actualización atómica de la cola <c>dbo.ProcesosSpertaHubSpot</c>.</summary>
    public sealed class ProcesosSpertaHubSpotManager
    {
        private readonly MSContext _ctx;

        public ProcesosSpertaHubSpotManager(MSContext ctx)
        {
            _ctx = ctx;
        }

        private IUnitOfWorkAsync CreateUow()
        {
            return new UnitOfWork(_ctx, new MSGestionContext(_ctx));
        }

        /// <summary>Cantidad de filas en estado Pendiente para el destino.</summary>
        public int ContarPendientes(string destino)
        {
            var uow = CreateUow();
            var repo = uow.Repository<ProcesosSpertaHubSpot>();
            return repo.Queryable().Count(x => x.Destino == destino && x.Estado == IntegracionColaEstados.Pendiente);
        }

        /// <summary>Cantidad de filas en proceso (Ejecución previa incompleta o en curso paralelo).</summary>
        public int ContarEnProceso(string destino)
        {
            var uow = CreateUow();
            var repo = uow.Repository<ProcesosSpertaHubSpot>();
            return repo.Queryable().Count(x => x.Destino == destino && x.Estado == IntegracionColaEstados.EnProceso);
        }

        /// <summary>Vista muestra sólo lectura de filas pendientes (no reclama).</summary>
        public IList<ColaIntegracionPendienteMuestra> ListarMuestraPendientes(string destino, int maxItems)
        {
            if (maxItems <= 0)
                return new List<ColaIntegracionPendienteMuestra>();

            var uow = CreateUow();
            var repo = uow.Repository<ProcesosSpertaHubSpot>();
            var lista = repo.Queryable()
                .Where(x => x.Destino == destino && x.Estado == IntegracionColaEstados.Pendiente)
                .OrderBy(x => x.FechaCreacionUtc)
                .Take(maxItems)
                .ToList();

            return lista.ConvertAll(ProjectarMuestra);
        }

        private static ColaIntegracionPendienteMuestra ProjectarMuestra(ProcesosSpertaHubSpot x)
        {
            return new ColaIntegracionPendienteMuestra
            {
                ProcesoId = x.ProcesoId,
                TenantId = x.TenantId,
                EmpresaId = x.EmpresaId,
                Destino = x.Destino,
                TipoEntidad = x.TipoEntidad,
                TipoOperacion = x.TipoOperacion,
                Identificador = x.Identificador,
                Intentos = x.Intentos,
                FechaCreacionUtc = x.FechaCreacionUtc,
            };
        }

        /// <summary>Toma hasta <paramref name="maxItems"/> filas pendientes y las marca en proceso (aislamiento serializable).</summary>
        public IList<ProcesosSpertaHubSpot> ReclamarPendientes(string destino, int maxItems)
        {
            if (maxItems <= 0)
                return new List<ProcesosSpertaHubSpot>();

            using (var scope = new TransactionScope(
                       TransactionScopeOption.Required,
                       new TransactionOptions
                       {
                           IsolationLevel = IsolationLevel.Serializable,
                           Timeout = TransactionManager.MaximumTimeout,
                       }))
            {
                var uow = CreateUow();
                var repo = uow.Repository<ProcesosSpertaHubSpot>();
                var candidatos = repo.Queryable()
                    .Where(x => x.Destino == destino && x.Estado == IntegracionColaEstados.Pendiente)
                    .OrderBy(x => x.FechaCreacionUtc)
                    .Take(maxItems)
                    .ToList();

                var ahora = DateTime.UtcNow;
                foreach (var p in candidatos)
                {
                    p.Estado = IntegracionColaEstados.EnProceso;
                    p.FechaInicioProcesoUtc = ahora;
                    p.Intentos = p.Intentos + 1;
                    repo.SaveEntity(p);
                }

                uow.SaveChanges();
                scope.Complete();
                return candidatos;
            }
        }

        public void MarcarOk(long procesoId)
        {
            var uow = CreateUow();
            var repo = uow.Repository<ProcesosSpertaHubSpot>();
            var p = repo.Queryable().FirstOrDefault(x => x.ProcesoId == procesoId);
            if (p == null)
                return;
            p.Estado = IntegracionColaEstados.Ok;
            p.FechaFinProcesoUtc = DateTime.UtcNow;
            p.MensajeUltimoError = null;
            repo.SaveEntity(p);
            uow.SaveChanges();
        }

        public void MarcarError(long procesoId, string mensaje)
        {
            var uow = CreateUow();
            var repo = uow.Repository<ProcesosSpertaHubSpot>();
            var p = repo.Queryable().FirstOrDefault(x => x.ProcesoId == procesoId);
            if (p == null)
                return;
            p.Estado = IntegracionColaEstados.Error;
            p.FechaFinProcesoUtc = DateTime.UtcNow;
            p.MensajeUltimoError = mensaje != null && mensaje.Length > 8000 ? mensaje.Substring(0, 8000) : mensaje;
            repo.SaveEntity(p);
            uow.SaveChanges();
        }

        /// <summary>Devuelve filas en error o pendientes de reintento manual (opcional).</summary>
        public void ReponerEnCola(long procesoId)
        {
            var uow = CreateUow();
            var repo = uow.Repository<ProcesosSpertaHubSpot>();
            var p = repo.Queryable().FirstOrDefault(x => x.ProcesoId == procesoId);
            if (p == null)
                return;
            p.Estado = IntegracionColaEstados.Pendiente;
            p.FechaInicioProcesoUtc = null;
            p.FechaFinProcesoUtc = null;
            repo.SaveEntity(p);
            uow.SaveChanges();
        }
    }

    /// <summary>Proyección segura para vistas de depuración sobre <see cref="ProcesosSpertaHubSpot"/> pendiente.</summary>
    public sealed class ColaIntegracionPendienteMuestra
    {
        public long ProcesoId { get; set; }

        public string TenantId { get; set; }

        public int? EmpresaId { get; set; }

        public string Destino { get; set; }

        public string TipoEntidad { get; set; }

        public string TipoOperacion { get; set; }

        public int Identificador { get; set; }

        public int Intentos { get; set; }

        public DateTime FechaCreacionUtc { get; set; }
    }
}
