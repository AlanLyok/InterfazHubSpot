using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml;
using BatchSpertaAPI.Core;
using BatchSpertaAPI.Security;
using Mastersoft.Framework.Standard;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace BatchSpertaAPI.Providers
{
    public class OAuthProvider : OAuthAuthorizationServerProvider
    {
        //--------------------------------------------------
        //  Variables Privadas
        //--------------------------------------------------

        /// <summary>Coincide con SpertaAPI: prefijo de cadena / claims para la base de gestión (tablas ERP incl. cola).</summary>
        private const string DefaultCNPrefix = "MSGestion";

        //--------------------------------------------------
        //  Metodos Publicos
        //--------------------------------------------------

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                var userName = context.UserName;
                var password = context.Password;
                var companyId = context.Request.Headers["CompanyId"];
                var status = context.Request.Headers["Status"];

                if (status == null)
                {
                    SpertaFwkUsuarioAuthResult authResult;
                    SpertaFwkUsuarioAuthFailureKind failureKind;
                    if (!SpertaFwkUsuarioAuthenticator.TryAuthenticate(userName, password, companyId, out authResult, out failureKind))
                    {
                        context.SetError("invalid_grant", DescribeAuthFailure(failureKind));
                        return;
                    }

                    var empresaIdStr = authResult.EmpresaId.ToString();
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Sid, authResult.UsuarioId.ToString()),
                        new Claim(ClaimTypes.Name, authResult.Usuario),
                        new Claim("urn:Custom:CNPrefix", DefaultCNPrefix),
                        new Claim("urn:Custom:CompanyId", empresaIdStr),
                        new Claim("urn:Custom:ProfileId", authResult.PerfilId.ToString()),
                        new Claim("urn:Custom:EsSuperUser", "N"),
                        new Claim("urn:Custom:EsMSAdnin", "N")
                    };

                    ClaimsIdentity oAuthIdentity = new ClaimsIdentity(claims, Startup.OAuthOptions.AuthenticationType);

                    var ticket = new AuthenticationTicket(oAuthIdentity, null);

                    context.Validated(ticket);
                }
                else
                {
                    var strXml = Util.DecryptString(status);

                    var xmlDoc = new XmlDocument();

                    xmlDoc.LoadXml(strXml);

                    var nodoRaiz = xmlDoc.DocumentElement;

                    var usuarioId = nodoRaiz.GetAttribute("UsuarioId");
                    var usuario = nodoRaiz.GetAttribute("Usuario");
                    var empresaId = nodoRaiz.GetAttribute("EmpresaId");
                    var perfilId = nodoRaiz.GetAttribute("PerfilId");
                    var esSuperUser = nodoRaiz.GetAttribute("EsSuperUser");
                    var esMSAdnin = nodoRaiz.GetAttribute("EsMSAdnin");

                    var claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.Sid, usuarioId),
                            new Claim(ClaimTypes.Name, usuario),
                            new Claim("urn:Custom:CNPrefix", DefaultCNPrefix),
                            new Claim("urn:Custom:CompanyId", empresaId),
                            new Claim("urn:Custom:ProfileId", perfilId),
                            new Claim("urn:Custom:EsSuperUser", esSuperUser),
                            new Claim("urn:Custom:EsMSAdnin", esMSAdnin)
                        };

                    ClaimsIdentity oAuthIdentity = new ClaimsIdentity(claims, Startup.OAuthOptions.AuthenticationType);

                    var ticket = new AuthenticationTicket(oAuthIdentity, null);

                    context.Validated(ticket);
                }
            });
        }

        private static string DescribeAuthFailure(SpertaFwkUsuarioAuthFailureKind failureKind)
        {
            switch (failureKind)
            {
                case SpertaFwkUsuarioAuthFailureKind.CompanyIdRequiredOrInvalid:
                    return "Debe enviar la cabecera HTTP CompanyId con el identificador de empresa ERP (entero mayor que cero). No es un identificador de sistemas externos (p. ej. CRM).";
                case SpertaFwkUsuarioAuthFailureKind.CompanyMismatchFixedUser:
                    return "La empresa indicada no corresponde al usuario.";
                case SpertaFwkUsuarioAuthFailureKind.PerfilRequiredForSharedUser:
                    return "El usuario no tiene un perfil válido para operación multiempresa.";
                case SpertaFwkUsuarioAuthFailureKind.SharedUserTablasCompartidasMismatch:
                    return "No se pudo validar el contexto multiempresa del usuario.";
                case SpertaFwkUsuarioAuthFailureKind.CompanyNotInSeguridadPorEmpresa:
                    return "No tiene permiso para operar en la empresa indicada.";
                case SpertaFwkUsuarioAuthFailureKind.CompanyNotFoundInEmpresas:
                    return "La empresa indicada no existe en el sistema.";
                case SpertaFwkUsuarioAuthFailureKind.ConnectionMisconfigured:
                    return "Error de configuración del servidor de autenticación.";
                case SpertaFwkUsuarioAuthFailureKind.InvalidCredentialsOrUserDisabled:
                case SpertaFwkUsuarioAuthFailureKind.UnexpectedError:
                default:
                    return "Usuario o contraseña incorrecta.";
            }
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            if (context.ClientId == null)
                context.Validated();

            return Task.FromResult<object>(null);
        }


        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

    }
}


