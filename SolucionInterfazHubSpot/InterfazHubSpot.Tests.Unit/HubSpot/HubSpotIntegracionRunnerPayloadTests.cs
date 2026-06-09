using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using InterfazHubSpot.Business.HubSpot;
using InterfazHubSpot.Business.Integration.Dtos;
using Newtonsoft.Json.Linq;
using Xunit;

namespace InterfazHubSpot.Tests.Unit.HubSpot
{
    /// <summary>
    /// Verifica los métodos privados de mapeo en <see cref="HubSpotIntegracionRunner"/>
    /// usando reflexión. No se requiere DB ni token: solo se ejercitan métodos de transformación.
    /// </summary>
    public sealed class HubSpotIntegracionRunnerPayloadTests
    {
        // ---------------------------------------------------------------
        // Obtener instancia de HubSpotIntegracionRunner sin ejecutar el ctor real
        // (el ctor real requiere MSContext y registra pasos).
        // _hubCfg se setea via reflexión para que BuildCompanyProperties funcione.
        // ---------------------------------------------------------------
        private static HubSpotIntegracionRunner CreateUninitializedRunner()
        {
            var runner = (HubSpotIntegracionRunner)FormatterServices.GetUninitializedObject(typeof(HubSpotIntegracionRunner));
            // Setear _hubCfg con una instancia por defecto (leerá app settings que incluyen default "mastersoft_id_")
            var hubCfgField = typeof(HubSpotIntegracionRunner)
                .GetField("_hubCfg", BindingFlags.NonPublic | BindingFlags.Instance);
            if (hubCfgField != null)
            {
                // HubSpotConfiguration ctor lee ConfigurationManager pero tiene defaults seguros
                var cfg = (HubSpotConfiguration)FormatterServices.GetUninitializedObject(typeof(HubSpotConfiguration));

                // Setear las propiedades requeridas por BuildCompanyProperties via sus backing fields
                SetPrivateField(cfg, "<PropertyMastersoftId>k__BackingField", "mastersoft_id_");
                SetPrivateField(cfg, "<PropertyManejoCuentaCorriente>k__BackingField", "manejo_cuenta_corriente");
                SetPrivateField(cfg, "<BaseUrl>k__BackingField", "https://api.hubapi.com");
                SetPrivateField(cfg, "<PrivateAppToken>k__BackingField", "test-token");
                SetPrivateField(cfg, "<DelayMsBetweenCalls>k__BackingField", 0);
                SetPrivateField(cfg, "<CuentaCorrientePageSize>k__BackingField", 500);

                hubCfgField.SetValue(runner, cfg);
            }

            return runner;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private static JObject InvokeBuildCompanyProperties(HubSpotIntegracionRunner runner, ClienteDatosDto dto, int pk, string codigo)
        {
            var method = typeof(HubSpotIntegracionRunner).GetMethod(
                "BuildCompanyProperties",
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (JObject)method.Invoke(runner, new object[] { dto, pk, codigo });
        }

        private static JObject InvokeBuildContactProperties(ContactoDto c)
        {
            var method = typeof(HubSpotIntegracionRunner).GetMethod(
                "BuildContactProperties",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (JObject)method.Invoke(null, new object[] { c });
        }

        private static string InvokeCoalesce(params string[] valores)
        {
            var method = typeof(HubSpotIntegracionRunner).GetMethod(
                "Coalesce",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (string)method.Invoke(null, new object[] { valores });
        }

        // ---------------------------------------------------------------
        // Coalesce
        // ---------------------------------------------------------------

        [Fact]
        public void Coalesce_TodosNullOEspacios_DevuelveNull()
        {
            var result = InvokeCoalesce(null, "  ", string.Empty);
            Assert.Null(result);
        }

        [Fact]
        public void Coalesce_PrimerNoVacio_DevuelveTrimmed()
        {
            var result = InvokeCoalesce(null, "  Acme Corp  ", "segundo");
            Assert.Equal("Acme Corp", result);
        }

        [Fact]
        public void Coalesce_SegundoElemento_CuandoPrimeroEsNull()
        {
            var result = InvokeCoalesce(null, "segundo");
            Assert.Equal("segundo", result);
        }

        // ---------------------------------------------------------------
        // BuildContactProperties
        // ---------------------------------------------------------------

        [Fact]
        public void BuildContactProperties_Completa_MapeaTodosCampos()
        {
            var c = new ContactoDto
            {
                ApellidoYNombre = "García, Juan",
                CorreoElectronico = "juan@test.com",
                Telefono = "011-4444-5555",
                SectorId = "VENTAS",
            };

            var result = InvokeBuildContactProperties(c);

            Assert.Equal("García, Juan", (string)result["firstname"]);
            Assert.Equal("juan@test.com", (string)result["email"]);
            Assert.Equal("011-4444-5555", (string)result["phone"]);
            Assert.Equal("VENTAS", (string)result["sector"]);
        }

        [Fact]
        public void BuildContactProperties_CamposNull_DevuelveCadenasVaciasNoNull()
        {
            var c = new ContactoDto { ApellidoYNombre = null, CorreoElectronico = null, Telefono = null, SectorId = null };

            var result = InvokeBuildContactProperties(c);

            Assert.Equal(string.Empty, (string)result["firstname"]);
            Assert.Equal(string.Empty, (string)result["email"]);
            Assert.Equal(string.Empty, (string)result["phone"]);
            Assert.Equal(string.Empty, (string)result["sector"]);
        }

        // ---------------------------------------------------------------
        // BuildCompanyProperties
        // ---------------------------------------------------------------

        private static ClienteDatosDto BuildMinimalDto()
        {
            return new ClienteDatosDto
            {
                RazonSocial = "Empresa SA",
                ApellidoYNombre = "Apellido",
                Contacto = "Contacto",
                NumeroDocumento = "30-12345678-9",
                Calle = "San Martin",
                Puerta = "100",
                Localidad = "Buenos Aires",
                CodigoPostal = "1000",
                CodigoProvinciaCliente = "BA",
                CodigoPais = "AR",
                ZonaId = "Z1",
                VendedorId = "V01",
                ResponsableCuentaId = "R01",
                ListaPreciosId = "LP1",
                CondicionVentaId = "CV1",
                DiasParaDeuda = "30",
                LimiteCredito = "50000",
                CategoriaClienteId = "A",
                ManejoCuentaCorriente = "Cuenta Corriente al 01/01/2026. Deuda: $0",
            };
        }

        [Fact]
        public void BuildCompanyProperties_RazonSocialPresente_UsaRazonSocialComoName()
        {
            var runner = CreateUninitializedRunner();
            var dto = BuildMinimalDto();
            dto.RazonSocial = "Mi Empresa SA";
            dto.ApellidoYNombre = "Apellido Corp";

            var result = InvokeBuildCompanyProperties(runner, dto, 1, "C001");

            Assert.Equal("Mi Empresa SA", (string)result["name"]);
        }

        [Fact]
        public void BuildCompanyProperties_SinRazonSocial_UsaApellidoYNombreComoName()
        {
            var runner = CreateUninitializedRunner();
            var dto = BuildMinimalDto();
            dto.RazonSocial = null;
            dto.ApellidoYNombre = "Apellido Y Nombre";

            var result = InvokeBuildCompanyProperties(runner, dto, 1, "C001");

            Assert.Equal("Apellido Y Nombre", (string)result["name"]);
        }

        [Fact]
        public void BuildCompanyProperties_SinRazonNiApellido_UsaContactoComoName()
        {
            var runner = CreateUninitializedRunner();
            var dto = BuildMinimalDto();
            dto.RazonSocial = null;
            dto.ApellidoYNombre = null;
            dto.Contacto = "El Contacto";

            var result = InvokeBuildCompanyProperties(runner, dto, 1, "C001");

            Assert.Equal("El Contacto", (string)result["name"]);
        }

        [Fact]
        public void BuildCompanyProperties_MapeaMastersoftIdConNombrePropiedadConfigurado()
        {
            var runner = CreateUninitializedRunner();
            var dto = BuildMinimalDto();

            var result = InvokeBuildCompanyProperties(runner, dto, 999, "C999");

            // _hubCfg fue creado con PropertyMastersoftId = "mastersoft_id_" en CreateUninitializedRunner
            Assert.Equal("999", (string)result["mastersoft_id_"]);
        }

        [Fact]
        public void BuildCompanyProperties_DireccionesEntregaMasDeTres_SoloMapeaTres()
        {
            var runner = CreateUninitializedRunner();
            var dto = BuildMinimalDto();
            dto.ListaDireccionEntregas = new List<DireccionEntregaDto>
            {
                new DireccionEntregaDto { Domicilio = "Calle 1", CodigoPostal = "1000", Localidad = "Ciudad 1", ProvinciaId = "BA", Pais = "Argentina" },
                new DireccionEntregaDto { Domicilio = "Calle 2", CodigoPostal = "2000", Localidad = "Ciudad 2", ProvinciaId = "CBA", Pais = "Argentina" },
                new DireccionEntregaDto { Domicilio = "Calle 3", CodigoPostal = "3000", Localidad = "Ciudad 3", ProvinciaId = "SFE", Pais = "Argentina" },
                new DireccionEntregaDto { Domicilio = "Calle 4", CodigoPostal = "4000", Localidad = "Ciudad 4", ProvinciaId = "MZA", Pais = "Argentina" },
            };

            var result = InvokeBuildCompanyProperties(runner, dto, 1, "C001");

            // Las 3 primeras deben estar mapeadas
            Assert.NotNull(result["direccion_1_domicilio"]);
            Assert.NotNull(result["direccion_2_domicilio"]);
            Assert.NotNull(result["direccion_3_domicilio"]);

            // La cuarta NO debe estar
            Assert.Null(result.Property("direccion_4_domicilio"));
        }

        [Fact]
        public void BuildCompanyProperties_MapeaManejoCuentaCorriente()
        {
            var runner = CreateUninitializedRunner();
            var dto = BuildMinimalDto();

            var result = InvokeBuildCompanyProperties(runner, dto, 1, "C001");

            Assert.Equal("Cuenta Corriente al 01/01/2026. Deuda: $0", (string)result["manejo_cuenta_corriente"]);
        }

        [Fact]
        public void BuildCompanyProperties_MapeaCalleAPropertyAddress()
        {
            var runner = CreateUninitializedRunner();
            var dto = BuildMinimalDto();
            dto.Calle = "Av. Corrientes 1234";

            var result = InvokeBuildCompanyProperties(runner, dto, 1, "C001");

            Assert.Equal("Av. Corrientes 1234", (string)result["address"]);
            Assert.False(result.ContainsKey("adress"));
        }

        [Fact]
        public void BuildCompanyProperties_MapeaCodigoPaisAPropertyCountryLowercase()
        {
            var runner = CreateUninitializedRunner();
            var dto = BuildMinimalDto();
            dto.CodigoPais = "Argentina";

            var result = InvokeBuildCompanyProperties(runner, dto, 1, "C001");

            Assert.Equal("Argentina", (string)result["country"]);
            Assert.False(result.ContainsKey("Country"));
        }

        [Fact]
        public void BuildCompanyProperties_TodosLosNombresDePropiedad_EstanEnLowercase()
        {
            var runner = CreateUninitializedRunner();
            var dto = BuildMinimalDto();
            dto.ListaDireccionEntregas = new List<DireccionEntregaDto>
            {
                new DireccionEntregaDto { Domicilio = "Calle 1", CodigoPostal = "1000", Localidad = "Ciudad 1", ProvinciaId = "BA", Pais = "Argentina" },
            };

            var result = InvokeBuildCompanyProperties(runner, dto, 1, "C001");

            foreach (var prop in result.Properties())
            {
                Assert.True(
                    prop.Name == prop.Name.ToLowerInvariant(),
                    "Propiedad HubSpot debe estar en lowercase: '" + prop.Name + "'");
                Assert.True(prop.Name.Length < 100, "Propiedad HubSpot debe tener menos de 100 caracteres: '" + prop.Name + "'");
            }
        }

        [Fact]
        public void BuildCompanyProperties_MapeaDireccionPais()
        {
            var runner = CreateUninitializedRunner();
            var dto = BuildMinimalDto();
            dto.ListaDireccionEntregas = new List<DireccionEntregaDto>
            {
                new DireccionEntregaDto { Domicilio = "Calle 1", CodigoPostal = "1000", Localidad = "Ciudad 1", ProvinciaId = "BA", Pais = "Argentina" },
            };

            var result = InvokeBuildCompanyProperties(runner, dto, 1, "C001");

            Assert.Equal("Argentina", (string)result["direccion_1_pais"]);
        }

        [Fact]
        public void BuildCompanyProperties_SinDirecciones_NoLanza()
        {
            var runner = CreateUninitializedRunner();
            var dto = BuildMinimalDto();
            dto.ListaDireccionEntregas = null;

            var ex = Record.Exception(() => InvokeBuildCompanyProperties(runner, dto, 1, "C001"));

            Assert.Null(ex);
        }

        [Fact]
        public void BuildCompanyProperties_CamposNullEnDto_DevuelveCadenasVaciasNoNull()
        {
            var runner = CreateUninitializedRunner();
            var dto = new ClienteDatosDto
            {
                RazonSocial = null,
                ApellidoYNombre = null,
                Contacto = null,
                NumeroDocumento = null,
                Calle = null,
                Puerta = null,
            };

            var result = InvokeBuildCompanyProperties(runner, dto, 1, null);

            // Verificar que no hay nulos JSON (todos string.Empty o tienen valor)
            foreach (var prop in result.Properties())
            {
                Assert.NotEqual(JTokenType.Null, prop.Value.Type);
            }
        }
    }
}
