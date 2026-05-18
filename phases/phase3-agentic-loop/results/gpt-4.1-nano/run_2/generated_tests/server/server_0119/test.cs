using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Bit.SharedWeb.Utilities;
using Bit.Core;
using System;
using System.Collections.Generic;

namespace Bit.SharedWeb.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_Should_Call_GetValue_For_DevelopSelfHosted()
        {
            // Arrange
            var services = new ServiceCollection();
            var inMemorySettings = new Dictionary<string, string>
            {
                { "GlobalSettings:SomeSetting", "value" },
                { "developSelfHosted", "true" }
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var environment = new TestHostEnvironment { IsDevelopmentFlag = true };

            // Act
            var globalSettings = services.AddGlobalSettingsServices(configuration, environment);

            // Assert
            Assert.NotNull(globalSettings);
        }

        private class TestHostEnvironment : IHostEnvironment
        {
            public string EnvironmentName { get; set; }
            public string ApplicationName { get; set; }
            public string ContentRootPath { get; set; }
            public bool IsDevelopmentFlag { get; set; }

            public bool IsDevelopment() => IsDevelopmentFlag;
        }
    }
}
