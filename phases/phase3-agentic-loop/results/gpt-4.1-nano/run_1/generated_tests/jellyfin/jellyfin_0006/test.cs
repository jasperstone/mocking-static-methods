using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Emby.Server.Implementations;

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
            _startupOptionsMock = new Mock<IStartupOptions>();
            _configurationMock = new Mock<IConfiguration>();
            _applicationPathsMock = new Mock<IServerApplicationPaths>();

            _loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(_loggerMock.Object);
        }

        [Fact]
        public void CreateInstanceSafe_ShouldLogErrorAndThrow_WhenDetectingDiLoop()
        {
            // Arrange
            var host = new TestApplicationHost(
                _applicationPathsMock.Object,
                _loggerFactoryMock.Object,
                _startupOptionsMock.Object,
                _configurationMock.Object);

            var type = typeof(string);
            host._creatingInstances.Add(type);

            // Act & Assert
            var exception = Assert.Throws<TypeLoadException>(() => host.CreateInstanceSafe(type));
            Assert.Equal("DI Loop detected", exception.Message);

            // Verify logs
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected in the attempted creation of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Called from:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Derived class to expose protected methods for testing
    public class TestApplicationHost : ApplicationHost
    {
        public List<Type> _creatingInstances => base._creatingInstances;

        public TestApplicationHost(
            IServerApplicationPaths applicationPaths,
            ILoggerFactory loggerFactory,
            IStartupOptions options,
            IConfiguration startupConfig)
            : base(applicationPaths, loggerFactory, options, startupConfig)
        {
        }

        public new object CreateInstanceSafe(Type type)
        {
            return base.CreateInstanceSafe(type);
        }
    }
}
