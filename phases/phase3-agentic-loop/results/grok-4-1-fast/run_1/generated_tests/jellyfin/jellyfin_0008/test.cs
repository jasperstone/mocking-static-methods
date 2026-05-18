using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_ThrowsException_LogsErrorWithException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManagerMock>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns((object)null);

            var testHost = new TestApplicationHost(
                loggerMock.Object, 
                pluginManagerMock.Object, 
                serviceProviderMock.Object);

            var problematicType = typeof(TestProblematicType);

            // Act
            var result = testHost.CreateInstanceSafe(problematicType);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error creating TestProblematicType")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            pluginManagerMock.Verify(pm => pm.FailPlugin(problematicType.Assembly), Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_DetectsCircularDependency_LogsErrorsAndThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var pluginManagerMock = new Mock<PluginManagerMock>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns((object)null);

            var testHost = new TestApplicationHost(
                loggerMock.Object, 
                pluginManagerMock.Object, 
                serviceProviderMock.Object);

            var circularType = typeof(CircularDependencyType);

            // Act & Assert
            var exception = Assert.Throws<TypeLoadException>(() => testHost.CreateInstanceSafe(circularType));
            Assert.Equal("DI Loop detected", exception.Message);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected") || v.ToString().Contains("Called from")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeast(2));

            pluginManagerMock.Verify(pm => pm.FailPlugin(circularType.Assembly), Times.Once);
        }

        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(
                ILogger<ApplicationHost> logger, 
                PluginManagerMock pluginManager, 
                IServiceProvider serviceProvider)
                : base(Mock.Of<IServerApplicationPaths>(), 
                       Mock.Of<ILoggerFactory>(), 
                       Mock.Of<IStartupOptions>(), 
                       Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>())
            {
                // Use reflection to set private fields
                typeof(ApplicationHost).GetField("_pluginManager", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .SetValue(this, pluginManager);
                
                GetType().GetProperty("ServiceProvider", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?
                    .SetValue(this, serviceProvider);
            }

            public object CreateInstanceSafe(Type type) => base.CreateInstanceSafe(type);

            protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal() => Enumerable.Empty<Assembly>();
        }

        private class PluginManagerMock
        {
            public virtual void FailPlugin(Assembly assembly) { }
        }

        private class TestProblematicType
        {
            public TestProblematicType()
            {
                throw new InvalidOperationException("Simulated creation error");
            }
        }

        private class CircularDependencyType
        {
        }
    }
}
