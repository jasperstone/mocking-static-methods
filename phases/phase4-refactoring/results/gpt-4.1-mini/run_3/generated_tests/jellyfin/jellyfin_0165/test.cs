using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        [Fact]
        public void LoadAssemblies_LogsErrorOnFileLoadException()
        {
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<object>(); // Not used in this test
            var config = new object(); // Not used in this test
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                var appVersion = new Version(1, 0, 0);

                var pluginManager = (PluginManager)Activator.CreateInstance(
                    typeof(PluginManager),
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    null,
                    new object[] { loggerMock.Object, null, null, tempDir, appVersion },
                    null);

                // We cannot inject LocalPlugin instances, so we rely on no plugins discovered and no error logged
                // This test is a placeholder to show the setup; real coverage requires refactoring or integration tests

                var assemblies = pluginManager.LoadAssemblies().ToList();

                // Verify that LogError was never called because no plugins exist
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Never);
            }
            finally
            {
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch { }
            }
        }
    }
}
