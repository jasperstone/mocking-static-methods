using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(ILoggerFactory loggerFactory)
                : base(new DummyPaths(), loggerFactory, null, null)
            {
                _creatingInstances = new List<Type>();
                Logger = loggerFactory.CreateLogger<ApplicationHost>();
            }

            public new List<Type> _creatingInstances;

            public new ILogger<ApplicationHost> Logger { get; set; }

            public object CallCreateInstanceSafe(Type type)
            {
                return base.CreateInstanceSafe(type);
            }
        }

        private class DummyPaths : IServerApplicationPaths
        {
            // Implement interface members as needed, or leave empty if not used
        }

        [Fact]
        public void CreateInstanceSafe_ShouldLogError_WhenDI_LoopDetected()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(loggerMock.Object);

            var host = new TestApplicationHost(loggerFactoryMock.Object);
            var type = typeof(string);
            host._creatingInstances.Add(type);

            // Act
            var exceptionThrown = false;
            try
            {
                host.CallCreateInstanceSafe(type);
            }
            catch (TypeLoadException)
            {
                exceptionThrown = true;
            }

            // Assert
            loggerMock.Verify(
                l => l.LogError("DI Loop detected in the attempted creation of {Type}", type.FullName),
                Times.Once);
            Assert.True(exceptionThrown, "Expected TypeLoadException to be thrown");
        }
    }
}
