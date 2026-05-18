using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Emby.Server.Implementations;
using Microsoft.Extensions.Configuration;

namespace Emby.Tests
{
    public class ApplicationHostTests
    {
        private readonly Mock<ILogger<ApplicationHost>> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<IStartupOptions> _startupOptionsMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IServerApplicationPaths> _applicationPathsMock;

        public ApplicationHostTests()
        {
            _loggerMock = new Mock<ILogger<ApplicationHost>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(_loggerMock.Object);

            _startupOptionsMock = new Mock<IStartupOptions>();
            _configurationMock = new Mock<IConfiguration>();
            _applicationPathsMock = new Mock<IServerApplicationPaths>();
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

            protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal()
            {
                return Array.Empty<Assembly>();
            }

            public void CallCreateInstanceSafe(Type type)
            {
                base.CreateInstanceSafe(type);
            }

            // Override abstract members
            public override void Dispose() { }
        }

        [Fact]
        public void CreateInstanceSafe_ShouldLogError_WhenTypeIsInCreatingInstances()
        {
            // Arrange
            var type = typeof(string);
            var host = new TestApplicationHost(
                _applicationPathsMock.Object,
                _loggerFactoryMock.Object,
                _startupOptionsMock.Object,
                _configurationMock.Object);

            // Set the _creatingInstances list to simulate DI loop
            var field = typeof(ApplicationHost).GetField("_creatingInstances", BindingFlags.NonPublic | BindingFlags.Instance);
            var list = new List<Type> { type };
            field.SetValue(host, list);

            // Act
            host.CallCreateInstanceSafe(type);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected in the attempted creation of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
