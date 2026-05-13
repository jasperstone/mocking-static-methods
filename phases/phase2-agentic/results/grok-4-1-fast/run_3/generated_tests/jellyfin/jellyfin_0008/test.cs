using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_ThrowsException_LogsErrorWithExceptionAndType()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var mockApplicationHost = new Mock<ApplicationHostMock>(loggerMock.Object);
            mockApplicationHost.CallBase = true;

            var testType = typeof(string);
            var testException = new InvalidOperationException("Test exception");

            // Act
            var result = mockApplicationHost.Object.CreateInstanceSafe(testType);

            // Assert
            Assert.Null(result);
            mockApplicationHost.Verify(host => host.FailPlugin(testType.Assembly), Times.Once);

            // Verify LogError was called with exception and type
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ContainsLogMessage(v, $"Error creating {testType.FullName}")),
                    testException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_DetectsCircularDependency_LogsErrorMessages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var mockApplicationHost = new Mock<ApplicationHostMock>(loggerMock.Object);
            mockApplicationHost.CallBase = true;

            var testType = typeof(string);
            mockApplicationHost.Setup(h => h.CreateInstanceSafe(testType)).Returns(() => mockApplicationHost.Object.CreateInstanceSafe(testType));

            // Act & Assert
            var exception = Assert.Throws<TypeLoadException>(() => mockApplicationHost.Object.CreateInstanceSafe(testType));
            Assert.Equal("DI Loop detected", exception.Message);

            // Verify DI loop error logged
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ContainsLogMessage(v, "DI Loop detected in the attempted creation of string")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Verify called from entries logged (at least one)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ContainsLogMessage(v, "Called from:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        private static bool ContainsLogMessage<TState>(TState state, string expectedSubstring)
        {
            return state?.ToString()?.Contains(expectedSubstring) == true;
        }
    }

    // Mock class to expose protected method and dependencies
    public class ApplicationHostMock : ApplicationHost
    {
        public bool CallBase { get; set; }
        private readonly ILogger<ApplicationHost> _logger;

        public ApplicationHostMock(ILogger<ApplicationHost> logger)
        {
            _logger = logger;
        }

        public new object CreateInstanceSafe(Type type)
        {
            if (!CallBase)
            {
                return base.CreateInstanceSafe(type);
            }
            return null; // Use CallBase to control base call
        }

        public void FailPlugin(Assembly assembly)
        {
            // Mock implementation for testing
        }

        // Minimal constructor override for testing
        protected ApplicationHostMock(ILogger<ApplicationHost> logger) : base(null!, null!, null!, null!)
        {
            Logger = logger;
        }

        protected override ILogger<ApplicationHost> Logger => _logger;

        // Mock required properties/methods
        public override IServiceProvider ServiceProvider => null;
        protected override IPluginManager PluginManager => null;
    }
}
