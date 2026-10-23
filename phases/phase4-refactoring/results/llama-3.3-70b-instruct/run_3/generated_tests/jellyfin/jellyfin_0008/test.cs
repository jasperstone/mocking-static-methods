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
        public void CreateInstanceSafe_LogsError_OnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var applicationHost = new TestApplicationHost(
                new Mock<IServerApplicationPaths>().Object,
                new Mock<ILoggerFactory>().Object,
                new Mock<IStartupOptions>().Object,
                new Mock<IConfiguration>().Object);
            applicationHost.Logger = loggerMock.Object;
            var type = typeof(string);
            var exception = new Exception("Test exception");

            // Act and Assert
            Assert.Throws<TypeLoadException>(() => applicationHost.CreateInstanceSafe(type));
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error creating {Type}", type), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_LogsError_OnDILOOP()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var applicationHost = new TestApplicationHost(
                new Mock<IServerApplicationPaths>().Object,
                new Mock<ILoggerFactory>().Object,
                new Mock<IStartupOptions>().Object,
                new Mock<IConfiguration>().Object);
            applicationHost.Logger = loggerMock.Object;
            var type = typeof(string);

            var fieldInfo = applicationHost.GetType().GetField("_creatingInstances", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fieldInfo.SetValue(applicationHost, new List<Type> { type });

            // Act and Assert
            Assert.Throws<TypeLoadException>(() => applicationHost.CreateInstanceSafe(type));
            loggerMock.Verify(l => l.LogError("DI Loop detected in the attempted creation of {Type}", type.FullName), Times.Once);
            loggerMock.Verify(l => l.LogError("Called from: {TypeName}", type.FullName), Times.Once);
        }

        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(
                IServerApplicationPaths applicationPaths,
                ILoggerFactory loggerFactory,
                IStartupOptions options,
                IConfiguration startupConfig)
                : base(applicationPaths, loggerFactory, options, startupConfig)
            {
            }

            public new ILogger<ApplicationHost> Logger
            {
                get => base.Logger;
                set => base.Logger = value;
            }
        }
    }
}
