using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Builder;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Hosting.Tests
{
    public class WebHostBuilderTests
    {
        [Fact]
        public void Build_LogsWarning_WhenDuplicateHostingStartupAssemblies()
        {
            // Arrange
            var builder = new WebHostBuilder();

            // Setup options to return duplicate assemblies
            var optionsMock = new Mock<WebHostOptions>();
            var duplicateAssemblyName = "DuplicateAssembly";
            var assemblies = new List<string> { duplicateAssemblyName, duplicateAssemblyName };
            optionsMock.Setup(o => o.GetFinalHostingStartupAssemblies()).Returns(assemblies);

            // Use reflection to set the private _options field
            var optionsField = typeof(WebHostBuilder).GetField("_options", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            optionsField.SetValue(builder, optionsMock.Object);

            // Setup a service collection to provide a mock ILogger<WebHost>
            var services = new ServiceCollection();

            var loggerMock = new Mock<ILogger<WebHost>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);

            services.AddSingleton(loggerMock.Object);

            // Add IConfiguration to avoid null reference in Build
            var config = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(config);

            // Add WebHost service to avoid null reference in Build
            services.AddSingleton<WebHost>(sp =>
            {
                var appServices = sp;
                var hostingServiceProvider = sp;
                var opts = optionsMock.Object;
                var conf = config;
                var errors = new List<Exception>();
                return new WebHost(appServices, hostingServiceProvider, opts, conf, errors);
            });

            // Configure the builder to use the above services
            builder.ConfigureServices(servicesCollection =>
            {
                foreach (var service in services)
                {
                    servicesCollection.Add(service);
                }
            });

            // Act
            var host = builder.Build();

            // Assert
            loggerMock.Verify(l => l.LogWarning(
                It.Is<string>(s => s.Contains(duplicateAssemblyName) && s.Contains("specified multiple times")),
                It.IsAny<object[]>()), Times.Once);
        }
    }
}
