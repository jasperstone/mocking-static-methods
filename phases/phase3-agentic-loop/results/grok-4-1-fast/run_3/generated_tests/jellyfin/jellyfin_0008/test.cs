using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostLogTests
    {
        [Fact]
        public void CreateInstanceSafe_DetectsLoop_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var mockHost = new Mock<ApplicationHost>(
                Mock.Of<IServerApplicationPaths>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IStartupOptions>(),
                Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>()) { CallBase = true };

            // Set up private fields
            var creatingInstancesField = typeof(ApplicationHost).GetField("_creatingInstances", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            creatingInstancesField?.SetValue(mockHost.Object, new System.Collections.Generic.List<Type>());

            var pluginManagerField = typeof(ApplicationHost).GetField("_pluginManager", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            pluginManagerField?.SetValue(mockHost.Object, Mock.Of<MediaBrowser.Common.Plugins.IPluginManager>());

            // Set Logger via reflection
            var loggerProperty = typeof(ApplicationHost).GetProperty("Logger", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            loggerProperty?.SetValue(mockHost.Object, loggerMock.Object);

            var type = typeof(string);

            // Add the type to creatingInstances to trigger loop detection
            ((System.Collections.Generic.List<Type>)creatingInstancesField.GetValue(mockHost.Object)).Add(type);

            // Act & Assert
            var ex = Assert.Throws<TypeLoadException>(() => 
            {
                // Use reflection to call protected method
                var method = typeof(ApplicationHost).GetMethod("CreateInstanceSafe", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                method?.Invoke(mockHost.Object, new object[] { type });
            });

            Assert.Equal("DI Loop detected", ex.Message);

            // Verify LogError was called for the loop detection
            loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("DI Loop detected") && state.ToString()!.Contains(type.FullName)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_ThrowsException_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var mockHost = new Mock<ApplicationHost>(
                Mock.Of<IServerApplicationPaths>(),
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IStartupOptions>(),
                Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>()) { CallBase = true };

            // Set up private fields
            var creatingInstancesField = typeof(ApplicationHost).GetField("_creatingInstances", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            creatingInstancesField?.SetValue(mockHost.Object, new System.Collections.Generic.List<Type>());

            var pluginManagerField = typeof(ApplicationHost).GetField("_pluginManager", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            pluginManagerField?.SetValue(mockHost.Object, Mock.Of<MediaBrowser.Common.Plugins.IPluginManager>());

            // Set Logger via reflection
            var loggerProperty = typeof(ApplicationHost).GetProperty("Logger", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            loggerProperty?.SetValue(mockHost.Object, loggerMock.Object);

            // Mock ServiceProvider to throw on CreateInstance
            mockHost.Setup(x => x.ServiceProvider).Returns((IServiceProvider)null);

            var badType = typeof(Exception); // Type that will fail creation

            // Act
            object result = null;
            Exception caughtException = null;
            try
            {
                var method = typeof(ApplicationHost).GetMethod("CreateInstanceSafe", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                result = method?.Invoke(mockHost.Object, new object[] { badType });
            }
            catch (TargetInvocationException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.Null(result);
            Assert.NotNull(caughtException);

            // Verify LogError was called with exception and type info (line 329)
            loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Error creating") && state.ToString()!.Contains(badType.Name)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
