using System.Net.Http;
using System.Threading.Tasks;

namespace InterfazHubSpot.Business.Integration
{
    /// <summary>Cliente HTTP mínimo alineado a SpertaAPI <c>api/v100</c> (token OAuth, health, clientes, pedidos/comprobantes).</summary>
    public interface ISpertaApiClient
    {
        /// <summary>Usa el <see cref="HttpClient"/> indicado o el cliente por defecto del proceso.</summary>
        HttpClient Http { get; }

        Task<string> GetHealthAsync();

        Task<string> PostClientesGrabarAsync(string jsonBody);

        Task<string> PostPedidosGrabarAsync(string jsonBody);

        /// <summary>POST opcional: requiere <c>SpertaAPIReciboRelativePath</c> en configuración.</summary>
        Task<string> PostReciboAsync(string jsonBody);

        /// <summary>GET envelope estándar con datos de cliente para CRM (<c>/sperta/integraciones/clientes/{id}</c>).</summary>
        Task<string> GetIntegracionesClienteAsync(int clienteId);

        /// <summary>Página JSON HubSpot CC (<c>/sperta/integraciones/hubspot/cuenta-corriente-clientes</c>).</summary>
        Task<string> GetIntegracionesHubSpotCuentaCorrienteAsync(int cursor, int pageSize);
    }
}
