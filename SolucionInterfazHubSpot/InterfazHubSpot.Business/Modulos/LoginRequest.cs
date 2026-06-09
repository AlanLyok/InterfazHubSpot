using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfazHubSpot.Business.Modulos
{
    public class LoginRequest
    {
        public string Usuario { get; set; }
        public string Password { get; set; }
        public string TenantID { get; set; }
        public int EmpresaId { get; set; }
        public string UrlToken { get; set; }
    }

    public class TokenResponse
    {
        public List<string> Errores { get; set; }
        public string Token { get; set; }
    }
}
