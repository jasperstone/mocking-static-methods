using Emby.Server.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_LogsError_OnDILoopDetection()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var applicationPaths = new Mock<IServerApplicationPaths>();
            var startupOptions = new Mock<IStartupOptions>();
            var startupConfig = new Mock<IConfiguration>();
            var pluginManager = new Mock<PluginManager>(logger, new TestApplicationHost(applicationPaths.Object, loggerFactory, startupOptions.Object, startupConfig.Object), startupConfig.Object, string.Empty, new Version(1, 0, 0, 0));

            var applicationHost = new TestApplicationHost(applicationPaths.Object, loggerFactory, startupOptions.Object, startupConfig.Object);
            applicationHost.GetType().GetProperty("_creatingInstances", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(applicationHost, new List<Type> { typeof(ApplicationHost) });

            // Act
            applicationHost.GetType().GetMethod("CreateInstanceSafe", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(applicationHost, new object[] { typeof(ApplicationHost) });

            // Assert
            loggerFactory.AssertLogged(logger, LogLevel.Error, "DI Loop detected in the attempted creation of ApplicationHost");
        }

        [Fact]
        public void CreateInstanceSafe_LogsError_OnException()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var applicationPaths = new Mock<IServerApplicationPaths>();
            var startupOptions = new Mock<IStartupOptions>();
            var startupConfig = new Mock<IConfiguration>();
            var pluginManager = new Mock<PluginManager>(logger, new TestApplicationHost(applicationPaths.Object, loggerFactory, startupOptions.Object, startupConfig.Object), startupConfig.Object, string.Empty, new Version(1, 0, 0, 0));

            var applicationHost = new TestApplicationHost(applicationPaths.Object, loggerFactory, startupOptions.Object, startupConfig.Object);

            // Act
            try
            {
                applicationHost.GetType().GetMethod("CreateInstanceSafe", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(applicationHost, new object[] { typeof(InvalidType) });
            }
            catch (Exception)
            {
            }

            // Assert
            loggerFactory.AssertLogged(logger, LogLevel.Error, "Error creating InvalidType");
        }
    }

    public class TestApplicationHost : ApplicationHost
    {
        public TestApplicationHost(
            IServerApplicationPaths applicationPaths,
            ILoggerFactory loggerFactory,
            IStartupOptions options,
            IConfiguration startupConfig)
            : base(applicationPaths, loggerFactory, options, startupConfig)
        {
        }
    }

    public class InvalidType
    {
    }
}
