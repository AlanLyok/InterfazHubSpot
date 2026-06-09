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

        private static void MarcarModificado(ProcesosSpertaHubSpot p)
        {
            p.ObjectState = Constants.Object_Modified;
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
                .OrderBy(x => x.FechaCreacion)
                .Take(maxItems)
                .ToList();

            return lista.ConvertAll(ProjectarMuestra);
        }

        private static ColaIntegracionPendienteMuestra ProjectarMuestra(ProcesosSpertaHubSpot x)
        {
            return new ColaIntegracionPendienteMuestra
            {
                ProcesoId = x.ProcesoId,
                EmpresaId = x.EmpresaId,
                Destino = x.Destino,
                TipoEntidad = x.TipoEntidad,
                TipoOperacion = x.TipoOperacion,
                Identificador = x.Identificador,
                Intentos = x.Intentos,
                FechaCreacion = x.FechaCreacion,
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
                    .OrderBy(x => x.FechaCreacion)
                    .Take(maxItems)
                    .ToList();

                var ahora = DateTime.Now;
                foreach (var p in candidatos)
                {
                    p.Estado = IntegracionColaEstados.EnProceso;
                    p.FechaInicioProceso = ahora;
                    p.Intentos = p.Intentos + 1;
                    MarcarModificado(p);
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
            p.FechaFinProceso = DateTime.Now;
            p.MensajeUltimoError = null;
            MarcarModificado(p);
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
            p.FechaFinProceso = DateTime.Now;
            p.MensajeUltimoError = mensaje != null && mensaje.Length > 8000 ? mensaje.Substring(0, 8000) : mensaje;
            MarcarModificado(p);
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
            p.FechaInicioProceso = null;
            p.FechaFinProceso = null;
            MarcarModificado(p);
            repo.SaveEntity(p);
            uow.SaveChanges();
        }

        /// <summary>Incrementa <see cref="ProcesosSpertaHubSpot.Intentos"/> en la fila reclamada (HTTP reintentable fallido).</summary>
        public void IncrementarIntentos(long procesoId)
        {
            var uow = CreateUow();
            var repo = uow.Repository<ProcesosSpertaHubSpot>();
            var p = repo.Queryable().FirstOrDefault(x => x.ProcesoId == procesoId);
            if (p == null)
                return;
            p.Intentos = p.Intentos + 1;
            MarcarModificado(p);
            repo.SaveEntity(p);
            uow.SaveChanges();
        }
    }

    /// <summary>Proyección segura para vistas de depuración sobre <see cref="ProcesosSpertaHubSpot"/> pendiente.</summary>
    public sealed class ColaIntegracionPendienteMuestra
    {
        public long ProcesoId { get; set; }

        public int? EmpresaId { get; set; }

        public string Destino { get; set; }

        public string TipoEntidad { get; set; }

        public string TipoOperacion { get; set; }

        public int Identificador { get; set; }

        public int Intentos { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}
