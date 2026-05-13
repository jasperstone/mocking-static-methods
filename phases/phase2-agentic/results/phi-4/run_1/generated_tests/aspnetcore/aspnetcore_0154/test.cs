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
            var mockLogger = new Mock<ILogger<WebHost>>();
            var options = new WebHostOptions
            {
                HostingStartupAssemblies = new List<string> { "Assembly1", "Assembly2", "Assembly1" }
            };

            // Act
            var assemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var assemblyName in options.GetFinalHostingStartupAssemblies())
            {
                if (!assemblyNames.Add(assemblyName) && mockLogger.Object.IsEnabled(LogLevel.Warning))
                {
                    mockLogger.Object.LogWarning($"The assembly {assemblyName} was specified multiple times. Hosting startup assemblies should only be specified once.");
                }
            }

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("Assembly1") && s.Contains("specified multiple times"))),
                Times.Once);
        }
    }
}
