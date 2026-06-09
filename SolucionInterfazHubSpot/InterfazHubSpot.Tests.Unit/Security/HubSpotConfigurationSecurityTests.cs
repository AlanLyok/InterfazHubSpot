using System;
using System.Reflection;
using System.Runtime.Serialization;
using InterfazHubSpot.Business.HubSpot;
using Xunit;

namespace InterfazHubSpot.Tests.Unit.Security
{
    [Collection("ConfigurationAppSettings")]
    public sealed class HubSpotConfigurationSecurityTests
    {
        [Fact, Trait("Category", "Security")]
        public void ValidarToken_SinTokenNiMock_LanzaInvalidOperationException()
        {
            var cfg = CreateConfiguration(useMock: false, token: string.Empty);

            var ex = Assert.Throws<TargetInvocationException>(() => InvokeValidarToken(cfg));
            Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Contains("PrivateAppToken", ex.InnerException.Message);
        }

        [Fact, Trait("Category", "Security")]
        public void ValidarToken_ConMockActivo_NoLanzaAunqueTokenVacio()
        {
            var cfg = CreateConfiguration(useMock: true, token: string.Empty);
            InvokeValidarToken(cfg);
        }

        [Fact, Trait("Category", "Security")]
        public void TienePrivateAppToken_TokenWhitespace_False()
        {
            var cfg = CreateConfiguration(useMock: false, token: "   ");
            Assert.False(GetTienePrivateAppToken(cfg));
        }

        [Fact, Trait("Category", "Security")]
        public void TienePrivateAppToken_TokenPresente_True()
        {
            var cfg = CreateConfiguration(useMock: false, token: "pat-test");
            Assert.True(GetTienePrivateAppToken(cfg));
        }

        [Fact, Trait("Category", "Security")]
        public void ParseInt_ValorInvalido_UsaFallback()
        {
            var result = InvokeParseInt("no-es-numero", 42);
            Assert.Equal(42, result);
        }

        [Fact, Trait("Category", "Security")]
        public void ParseBoolTrue_Si_InterpretaTrue()
        {
            Assert.True(InvokeParseBoolTrue("true", false));
            Assert.True(InvokeParseBoolTrue("1", false));
            Assert.True(InvokeParseBoolTrue("yes", false));
        }

        private static int InvokeParseInt(string s, int fallback)
        {
            var method = typeof(HubSpotConfiguration).GetMethod(
                "ParseInt",
                BindingFlags.Static | BindingFlags.NonPublic);
            return (int)method.Invoke(null, new object[] { s, fallback });
        }

        private static bool InvokeParseBoolTrue(string s, bool fallback)
        {
            var method = typeof(HubSpotConfiguration).GetMethod(
                "ParseBoolTrue",
                BindingFlags.Static | BindingFlags.NonPublic);
            return (bool)method.Invoke(null, new object[] { s, fallback });
        }

        private static object CreateConfiguration(bool useMock, string token)
        {
            var cfg = FormatterServices.GetUninitializedObject(typeof(HubSpotConfiguration));
            SetBackingField(cfg, "UseDevelopmentMock", useMock);
            SetBackingField(cfg, "PrivateAppToken", token ?? string.Empty);
            SetBackingField(cfg, "BaseUrl", "https://api.hubapi.com");
            return cfg;
        }

        private static void InvokeValidarToken(object cfg)
        {
            var method = typeof(HubSpotConfiguration).GetMethod(
                "ValidarToken",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            method.Invoke(cfg, null);
        }

        private static bool GetTienePrivateAppToken(object cfg)
        {
            var prop = typeof(HubSpotConfiguration).GetProperty(
                "TienePrivateAppToken",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return (bool)prop.GetValue(cfg, null);
        }

        private static void SetBackingField(object target, string propertyName, object value)
        {
            var field = target.GetType().GetField(
                $"<{propertyName}>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new InvalidOperationException("Backing field no encontrado: " + propertyName);
            }

            field.SetValue(target, value);
        }
    }
}
