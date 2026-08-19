using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using Emby.Server.Implementations;
using Emby.Server.Implementations.Plugins;
using Microsoft.Extensions.Configuration;
using MediaBrowser.Common.Configuration;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_DetectsDILoop_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationHost>>();
            var mockPluginManager = new Mock<PluginManager>();
            var applicationHost = new Mock<ApplicationHost>(Mock.Of<IServerApplicationPaths>(), Mock.Of<ILoggerFactory>(), Mock.Of<IStartupOptions>(), Mock.Of<IConfiguration>())
                .SetupProperty(x => x.Logger, mockLogger.Object)
                .SetupProperty(x => x._pluginManager, mockPluginManager.Object)
                .Object;

            var type = typeof(ApplicationHost);

            // Act
            try
            {
                applicationHost.CreateInstanceSafe(type);
            }
            catch (TypeLoadException)
            {
                // Expected exception
            }

            // Assert
            mockLogger.Verify(
                x => x.LogError("DI Loop detected in the attempted creation of {Type}", type.FullName),
                Times.Once);

            mockLogger.Verify(
                x => x.LogError("Called from: {TypeName}", type.FullName),
                Times.Once);

            mockPluginManager.Verify(
                x => x.FailPlugin(type.Assembly),
                Times.Once);
        }
    }
}
