using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Tests
{
    public class WebHostBuilderTests
    {
        [Fact]
        public void Build_LogsWarningForDuplicateHostingStartupAssemblies()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WebHost>>();
            var optionsMock = new Mock<IOptions<WebHostOptions>>();
            var hostingStartupAssemblies = new List<string> { "Assembly1", "Assembly1", "Assembly2" };

            optionsMock.Setup(o => o.Value).Returns(new WebHostOptions
            {
                HostingStartupAssemblies = hostingStartupAssemblies
            });

            var webHostBuilder = new WebHostBuilder();
            webHostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<ILogger<WebHost>>(loggerMock.Object);
                services.AddSingleton<IOptions<WebHostOptions>>(optionsMock.Object);
            });

            // Act
            var host = webHostBuilder.Build();

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)((state, exception) => state.ToString().Contains("The assembly Assembly1 was specified multiple times."))), Times.Once);
        }
    }
}
