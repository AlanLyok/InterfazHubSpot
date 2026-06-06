using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using BatchSpertaAPI.Business;
using Mastersoft.Framework.DataRepository;
using Mastersoft.Framework.Standard;

namespace BatchSpertaAPI.Core
{
    public static class Util
    {
        //--------------------------------------------------
        //  Constantes Privadas
        //--------------------------------------------------

        private const string DefaultCNPrefix = "BatchSpertaAPI";

        //------------------------------------------------------------------------------------------------
        //  ObjectState
        //------------------------------------------------------------------------------------------------

        public const int Object_Added = 0;
        public const int Object_Deleted = 1;
        public const int Object_Modified = 2;
        public const int Object_Unchanged = 3;

        //------------------------------------------------------------------------------------------------
        //  Metodos Publicos
        //------------------------------------------------------------------------------------------------

        public static MSContext GetMSContext()
        {
            var oMSContext = new MSContext();

            oMSContext.CN = "";
            oMSContext.CNPrefix = DefaultCNPrefix;
            oMSContext.DBProvider = "SQLServer";
            oMSContext.EmpresaId = 0;
            oMSContext.UsuarioId = 0;
            oMSContext.Usuario = "";
            oMSContext.PerfilId = 0;
            oMSContext.EsSuperUser = false;
            oMSContext.EsMSAdmin = false;
            oMSContext.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            oMSContext.KendoTheme = "bootstrap";
            oMSContext.HeaderColor = "#0078d4";
            oMSContext.ImagenFondo = "FondoNumbit.png";
            oMSContext.ColorEtiquetas = "Blanco";
            oMSContext.RecordarPassword = false;

            if (HttpContext.Current.User.Identity.AuthenticationType == "Bearer")
            {
                var identity = (ClaimsIdentity)HttpContext.Current.User.Identity;

                IEnumerable<Claim> claims = identity.Claims;

                foreach (Claim cla in claims)
                {
                    //Debug.WriteLine(cla.Type + " - " + cla.Value);

                    if (cla.Type == ClaimTypes.Sid)
                    {
                        oMSContext.UsuarioId = int.Parse(cla.Value);
                    }
                    else if (cla.Type == ClaimTypes.Name)
                    {
                        oMSContext.Usuario = cla.Value;
                    }
                    else if (cla.Type == "urn:Custom:CNPrefix")
                    {
                        oMSContext.CNPrefix = cla.Value;
                    }
                    else if (cla.Type == "urn:Custom:TenantId")
                    {
                        oMSContext.TenantId = cla.Value;
                    }
                    else if (cla.Type == "urn:Custom:CntTenant")
                    {
                        oMSContext.UsaContextTenants = StrBooToBooleanDefTrue(cla.Value);
                    }
                    else if (cla.Type == "urn:Custom:TabTenant")
                    {
                        oMSContext.UsaTablaTenants = StrBooToBooleanDefTrue(cla.Value);
                    }
                    else if (cla.Type == "urn:Custom:SaveTenant")
                    {
                        oMSContext.SaveTenantIdByFwk = StrBooToBooleanDefFalse(cla.Value);
                    }
                    else if (cla.Type == "urn:Custom:CompanyId")
                    {
                        oMSContext.EmpresaId = int.Parse(cla.Value);
                    }
                    else if (cla.Type == "urn:Custom:CompanyName")
                    {
                        oMSContext.Empresa = cla.Value;
                    }
                    else if (cla.Type == "urn:Custom:EjercicioId")
                    {
                        oMSContext.EjercicioId = int.Parse(cla.Value);
                    }
                    else if (cla.Type == "urn:Custom:Ejercicio")
                    {
                        oMSContext.Ejercicio = cla.Value;
                    }
                    else if (cla.Type == "urn:Custom:ContableEjercicioId")
                    {
                        oMSContext.ContableEjercicioId = int.Parse(cla.Value);
                    }
                    else if (cla.Type == "urn:Custom:ContableEjercicio")
                    {
                        oMSContext.ContableEjercicio = cla.Value;
                    }
                    else if (cla.Type == "urn:Custom:ProfileId")
                    {
                        oMSContext.PerfilId = int.Parse(cla.Value);
                    }
                    else if (cla.Type == "urn:Custom:EsSuperUser")
                    {
                        oMSContext.EsSuperUser = (cla.Value == "S") ? true : false;
                    }
                    else if (cla.Type == "urn:Custom:EsMSAdnin")
                    {
                        oMSContext.EsMSAdmin = (cla.Value == "S") ? true : false;
                    }
                    else if (cla.Type == "urn:Custom:Tag")
                    {
                        oMSContext.Tag = cla.Value;
                    }
                    else if (cla.Type == "urn:Custom:Tag1")
                    {
                        oMSContext.Tag1 = cla.Value;
                    }
                    else if (cla.Type == "urn:Custom:Tag2")
                    {
                        oMSContext.Tag2 = cla.Value;
                    }
                    else if (cla.Type == "urn:Custom:Tag3")
                    {
                        oMSContext.Tag3 = cla.Value;
                    }
                    else if (cla.Type == "urn:Custom:Tag4")
                    {
                        oMSContext.Tag4 = cla.Value;
                    }
                    else if (cla.Type == "urn:Custom:Tag5")
                    {
                        oMSContext.Tag5 = cla.Value;
                    }
                }
            }
            else
            {
                var strIdentity = HttpContext.Current.User.Identity.Name;

                if (strIdentity.Trim().Length > 0)
                {
                    var strXml = DecryptString(strIdentity);

                    var xmlDoc = new XmlDocument();

                    xmlDoc.LoadXml(strXml);

                    var nodoRaiz = xmlDoc.DocumentElement;

                    oMSContext.TenantId = nodoRaiz.GetAttribute("TenantId");
                    oMSContext.UsaContextTenants = StrBooToBooleanDefTrue(nodoRaiz.GetAttribute("CntTenant"));
                    oMSContext.UsaTablaTenants = StrBooToBooleanDefTrue(nodoRaiz.GetAttribute("TabTenant"));
                    oMSContext.SaveTenantIdByFwk = StrBooToBooleanDefFalse(nodoRaiz.GetAttribute("SaveTenant"));
                    oMSContext.DBProvider = "SQLServer";
                    oMSContext.EmpresaId = int.Parse(nodoRaiz.GetAttribute("EmpresaId"));
                    oMSContext.Empresa = nodoRaiz.GetAttribute("Empresa");
                    oMSContext.EjercicioId = int.Parse(nodoRaiz.GetAttribute("EjercicioId"));
                    oMSContext.Ejercicio = nodoRaiz.GetAttribute("Ejercicio");
                    oMSContext.ContableEjercicioId = int.Parse(nodoRaiz.GetAttribute("ContableEjercicioId"));
                    oMSContext.ContableEjercicio = nodoRaiz.GetAttribute("ContableEjercicio");
                    oMSContext.UsuarioId = int.Parse(nodoRaiz.GetAttribute("UsuarioId"));
                    oMSContext.Usuario = nodoRaiz.GetAttribute("Usuario");
                    oMSContext.NombreCompleto = nodoRaiz.GetAttribute("NombreCompleto");
                    oMSContext.UsuarioEmail = nodoRaiz.GetAttribute("Email");
                    oMSContext.Iniciales = nodoRaiz.GetAttribute("Iniciales");
                    oMSContext.PerfilId = int.Parse(nodoRaiz.GetAttribute("PerfilId"));
                    oMSContext.EsSuperUser = (nodoRaiz.GetAttribute("EsSuperUser") == "S") ? true : false;
                    oMSContext.EsMSAdmin = (nodoRaiz.GetAttribute("EsMSAdnin") == "S") ? true : false;
                    oMSContext.EsSuperUser = (nodoRaiz.GetAttribute("EsSuperUser") == "S") ? true : false;
                    oMSContext.EsMSAdmin = (nodoRaiz.GetAttribute("EsMSAdnin") == "S") ? true : false;
                    oMSContext.KendoTheme = nodoRaiz.GetAttribute("KendoTheme");
                    oMSContext.HeaderColor = nodoRaiz.GetAttribute("HeaderColor");
                    oMSContext.ImagenFondo = nodoRaiz.GetAttribute("ImagenFondo");
                    oMSContext.ColorEtiquetas = nodoRaiz.GetAttribute("ColorEtiquetas");
                    oMSContext.RecordarPassword = (nodoRaiz.GetAttribute("Recordar") == "S");
                    oMSContext.Tag = nodoRaiz.GetAttribute("Tag");
                    oMSContext.Tag1 = nodoRaiz.GetAttribute("Tag1");
                    oMSContext.Tag2 = nodoRaiz.GetAttribute("Tag2");
                    oMSContext.Tag3 = nodoRaiz.GetAttribute("Tag3");
                    oMSContext.Tag4 = nodoRaiz.GetAttribute("Tag4");
                    oMSContext.Tag5 = nodoRaiz.GetAttribute("Tag5");
                }
            }

            return oMSContext;
        }


        public static string GetIdentityHash()
        {
            var strIdentity = HttpContext.Current.User.Identity.Name;

            string sHashWithSalt = DecryptString(strIdentity);

            byte[] saltedHashBytes = Encoding.UTF8.GetBytes(sHashWithSalt);

            var key = "lkszdjhaswdahgdjhug";

            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));

            byte[] hash = hmac.ComputeHash(saltedHashBytes);

            return Convert.ToBase64String(hash);
        }


        private static bool StrBooToBooleanDefTrue(string valor)
        {
            var result = true;

            if (valor.Trim().ToLower() == "false")
            {
                result = false;
            }

            return result;
        }


        private static bool StrBooToBooleanDefFalse(string valor)
        {
            var result = false;

            if (valor.Trim().ToLower() == "true")
            {
                result = true;
            }

            return result;
        }


        public static string GetUsuario()
        {
            var oMSContext = GetMSContext();

            return oMSContext.Usuario;
        }


        public static string GetEmail()
        {
            var oMSContext = GetMSContext();

            return oMSContext.UsuarioEmail;
        }


        public static string GetTheme()
        {
            var oMSContext = GetMSContext();

            var theme = oMSContext.KendoTheme ?? "";

            if (theme.Length == 0)
            {
                theme = "bootstrap";
            }

            return theme;
        }


        public static string GetImagenFondo(MSContext oMSContext)
        {
            var imagen = "";

            var imagenFondo = oMSContext.ImagenFondo ?? "";

            if (imagenFondo.Trim().Length > 0)
            {
                var url = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;

                if (url == "~/" ||
                    url == "~/Home" ||
                    url == "~/Account/Login")
                {
                    imagen = imagenFondo.Trim();
                }
            }

            return imagen;
        }


        public static string GetColorEtiquetas(MSContext oMSContext)
        {
            var color = "";

            var colorEtiquetas = oMSContext.ColorEtiquetas ?? "";

            if (colorEtiquetas.Trim().Length > 0)
            {
                var url = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;

                if (url == "~/" ||
                    url == "~/Home" ||
                    url == "~/Account/Login")
                {
                    color = colorEtiquetas.Trim();
                }

            }

            return color;
        }


        public static string GetPDFViewer()
        {
            var modo = ConfigurationManager.AppSettings["PDFViewer"];

            if (modo == null)
            {
                modo = "";
            }

            return modo;
        }

        public static List<MSErrorMessage> EntityErrorsToMSErrorMessage(EntityErrors entityErrors)
        {
            var result = new List<MSErrorMessage>();

            var hayDetailError = false;

            var hayEmptySource = false;

            foreach (ErrorMessage oError in entityErrors.ListaErrores)
            {
                if (oError.Source.IndexOf(".") > 0)
                {
                    hayDetailError = true;
                }
                else
                {
                    result.Add(new MSErrorMessage()
                    {
                        Message = oError.Message,
                        Source = oError.Source
                    });
                }

                if (oError.Source.Length == 0)
                {
                    hayEmptySource = true;
                }
            }

            if ((hayDetailError || result.Count > 0) && !hayEmptySource)
            {
                result.Add(new MSErrorMessage()
                {
                    Message = "Datos incorrectos, verifique el mensaje de error en cada campo",
                    Source = ""
                });
            }

            return result;
        }


        public static IEnumerable<MSErrorMessage> EntityErrorsToMSErrorMessage(EntityErrors entityErrors, string entityName)
        {
            var result = new List<MSErrorMessage>();

            foreach (ErrorMessage oError in entityErrors.ListaErrores)
            {
                if (oError.Source.StartsWith(entityName + "."))
                {
                    result.Add(new MSErrorMessage()
                    {
                        Message = oError.Message,
                        Source = oError.Source.Replace(".", "")
                    });
                }
            }

            return result;
        }


        public static string EntityErrorsToItemError(EntityErrors entityErrors, string entityName, int indice)
        {
            string msg = null;

            foreach (ErrorMessage oError in entityErrors.ListaErrores)
            {
                if (oError.Source.StartsWith(entityName + ".") && oError.Item == indice)
                {
                    if (msg == null)
                    {
                        msg = oError.Message;
                    }
                    else
                    {
                        msg += "<br />" + oError.Message;
                    }
                }
            }

            return msg;
        }

        public static int? ToNullInt(int nValor)
        {
            if (nValor == 0)
            {
                return null;
            }
            else
            {
                return nValor;
            }
        }


        public static int ToInt(object nValor)
        {
            int intValor = 0;

            if ((nValor != null))
            {
                if (!(nValor is DBNull))
                {
                    intValor = (int)nValor;
                }

            }

            return intValor;
        }


        public static string ToStr(object cValor)
        {
            string strCadena = "";

            if ((cValor != null))
            {
                if (!(cValor is DBNull))
                {
                    strCadena = (string)cValor;
                }

            }

            return strCadena;
        }


        private static byte[] GetPasswordBytes()
        {
            var key = "sadhgj6123hhdajdkqjnzqfjlka7Z23";

            var ba = Encoding.UTF8.GetBytes(key);

            return System.Security.Cryptography.SHA256.Create().ComputeHash(ba);
        }


        public static string EncryptData(string text)
        {
            return AES.Encrypt(text, GetPasswordBytes());
        }


        public static string DecryptString(string text)
        {
            return AES.Decrypt(text, GetPasswordBytes());
        }


        public static string GetDownloadKey(string key)
        {
            var enc = EncryptData(key);

            var res = System.Web.HttpUtility.UrlEncode(enc);

            return res;
        }


        public static string GetDownloadKeyById(int docId, int usuarioId)
        {
            var key = DateTime.Now.Ticks.ToString() + "|" + docId.ToString() + "|" + usuarioId.ToString();

            var enc = EncryptData(key);

            var res = System.Web.HttpUtility.UrlEncode(enc);

            return res;
        }


        public static string GetPathFiles()
        {
            return ConfigurationManager.AppSettings["PathFiles"];
        }


        public static string GetFullPathPDF(string strFile)
        {
            return Path.Combine(GetPathFiles(), strFile);
        }

        public static string Right(this string value, int length)
        {
            return value.Substring(value.Length - length);
        }

    }


    public static class RazorViewToString
    {
        public static string RenderRazorViewToString(this Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}





