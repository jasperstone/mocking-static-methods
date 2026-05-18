using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Hosting
{
    public class WebHostBuilderTests
    {
        [Fact]
        public void Build_LogsWarning_WhenDuplicateHostingStartupAssemblies()
        {
            // Arrange
            var builder = new WebHostBuilder();

            // Create a mock WebHostOptions that returns duplicate assemblies
            var optionsMock = new Mock<WebHostOptions>();
            var duplicateAssemblyName = "DuplicateAssembly";
            var assemblies = new List<string> { duplicateAssemblyName, duplicateAssemblyName };
            optionsMock.Setup(o => o.GetFinalHostingStartupAssemblies()).Returns(assemblies);

            // Set the private _options field to our mock
            var optionsField = typeof(WebHostBuilder).GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance);
            optionsField.SetValue(builder, optionsMock.Object);

            // Create a mock logger that is enabled for Warning level
            var loggerMock = new Mock<ILogger<WebHost>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);

            // Create a service collection and register the mock logger and IConfiguration
            var services = new ServiceCollection();
            services.AddSingleton(loggerMock.Object);
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // We need to inject this service provider into the WebHost instance created inside Build.
            // Since we cannot override WebHost creation, we will hook into the IServiceProvider returned by BuildCommonServices.
            // But BuildCommonServices is private, so we cannot override it.
            // Instead, we will replace the IServiceProvider returned by host.Services.GetRequiredService<ILogger<WebHost>> to return our mock logger.
            // This is done by registering the mock logger in the service provider.

            // Act
            var host = builder.Build();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains(duplicateAssemblyName))), Times.Once);
        }
    }
}
