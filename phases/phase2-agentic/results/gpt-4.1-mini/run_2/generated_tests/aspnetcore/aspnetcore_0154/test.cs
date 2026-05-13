using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using Moq;

namespace Microsoft.AspNetCore.Hosting.Tests
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

            services.AddSingleton(typeof(ILogger<WebHost>), loggerMock.Object);

            // Add IConfiguration to avoid null reference in Build
            var config = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(config);

            // Setup a minimal IServiceProvider to return our services
            builder.ConfigureServices(s =>
            {
                s.AddSingleton(typeof(ILogger<WebHost>), loggerMock.Object);
                s.AddSingleton<IConfiguration>(config);
            });

            // Act
            var host = builder.Build();

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"The assembly {duplicateAssemblyName} was specified multiple times")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
