using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Tests
{
    public class WebHostBuilderTests
    {
        [Fact]
        public void LogWarning_ShouldLogWarning_WhenDuplicateAssembliesSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<WebHost>>();
            var options = new Mock<WebHostOptions>();
            options.Setup(o => o.GetFinalHostingStartupAssemblies())
                   .Returns(new List<string> { "Assembly1", "Assembly2", "Assembly1" });

            // Act
            var assemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var assemblyName in options.Object.GetFinalHostingStartupAssemblies())
            {
                if (!assemblyNames.Add(assemblyName) && loggerMock.Object.IsEnabled(LogLevel.Warning))
                {
                    loggerMock.Object.LogWarning($"The assembly {assemblyName} was specified multiple times. Hosting startup assemblies should only be specified once.");
                }
            }

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("Assembly1") && s.Contains("specified multiple times"))),
                Times.Once);
        }
    }
}
