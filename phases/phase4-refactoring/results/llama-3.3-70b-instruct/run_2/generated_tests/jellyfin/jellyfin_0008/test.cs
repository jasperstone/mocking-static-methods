using Emby.Server.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Reflection;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var applicationHost = new TestApplicationHost(loggerMock.Object);
            var type = typeof(string);
            var exception = new Exception("Test exception");

            // Act and Assert
            try
            {
                applicationHost.CreateInstanceSafe(type);
            }
            catch (TypeLoadException)
            {
                loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error creating {Type}", type), Times.Once);
            }
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorOnDILoop()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var applicationHost = new TestApplicationHost(loggerMock.Object);
            var type = typeof(string);

            var creatingInstancesField = applicationHost.GetType().GetField("_creatingInstances", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            creatingInstancesField.SetValue(applicationHost, new List<Type> { type });

            // Act and Assert
            try
            {
                applicationHost.CreateInstanceSafe(type);
            }
            catch (TypeLoadException)
            {
                loggerMock.Verify(l => l.LogError("DI Loop detected in the attempted creation of {Type}", type.FullName), Times.Once);
            }
        }

        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(ILogger<ApplicationHost> logger) 
                : base(new Mock<IServerApplicationPaths>().Object, new Mock<ILoggerFactory>().Object, new Mock<IStartupOptions>().Object, new Mock<IConfiguration>().Object)
            {
                Logger = logger;
            }

            public new object CreateInstanceSafe(Type type)
            {
                try
                {
                    return base.CreateInstanceSafe(type);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error creating {Type}", type);
                    throw;
                }
            }

            protected override Type[] GetAssembliesWithPartsInternal()
            {
                return new Type[0];
            }
        }
    }
}
