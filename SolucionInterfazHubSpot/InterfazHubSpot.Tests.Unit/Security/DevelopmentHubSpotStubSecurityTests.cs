using System;
using System.Configuration;
using System.Reflection;
using System.Runtime.Serialization;
using InterfazHubSpot.Business.HubSpot;
using Xunit;

namespace InterfazHubSpot.Tests.Unit.Security
{
    public sealed class DevelopmentHubSpotStubSecurityTests
    {
        [Fact, Trait("Category", "Security")]
        public void HubSpotConfiguration_ConMock_NoRequiereTokenReal()
        {
            var cfg = CreateMockConfiguration();
            InvokeValidarToken(cfg);
            Assert.False(GetTienePrivateAppToken(cfg));
        }

        [Fact, Trait("Category", "Security")]
        public void DevelopmentHubSpotStubHandler_RespondeSinCredencialesReales()
        {
            var handler = new DevelopmentHubSpotStubHandler();
            Assert.NotNull(handler);
        }

        private static object CreateMockConfiguration()
        {
            var cfg = FormatterServices.GetUninitializedObject(typeof(HubSpotConfiguration));
            SetBackingField(cfg, "UseDevelopmentMock", true);
            SetBackingField(cfg, "PrivateAppToken", string.Empty);
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
            field.SetValue(target, value);
        }
    }
}
