using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using Emby.Server.Implementations;
using Microsoft.Extensions.Configuration;

public class ApplicationHostTests
{
    private class DummyApplicationHost : ApplicationHost
    {
        public DummyApplicationHost(
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

    [Fact]
    public void CreateInstanceSafe_Should_LogError_When_DiLoopDetected()
    {
        // Arrange
        var mockApplicationPaths = new Mock<IServerApplicationPaths>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger<ApplicationHost>>();
        var mockDeviceLogger = new Mock<ILogger<DeviceId>>();
        mockLoggerFactory.Setup(x => x.CreateLogger<ApplicationHost>()).Returns(mockLogger.Object);
        mockLoggerFactory.Setup(x => x.CreateLogger<DeviceId>()).Returns(mockDeviceLogger.Object);
        var mockOptions = new Mock<IStartupOptions>();
        var inMemoryConfig = new ConfigurationBuilder().Build();

        var host = new DummyApplicationHost(
            mockApplicationPaths.Object,
            mockLoggerFactory.Object,
            mockOptions.Object,
            inMemoryConfig);

        // Setup _creatingInstances to simulate a DI loop
        var type = typeof(string);
        host._creatingInstances = new List<Type> { type };

        // Act
        var exceptionThrown = false;
        try
        {
            host.CreateInstanceSafe(type);
        }
        catch (TypeLoadException)
        {
            exceptionThrown = true;
        }

        // Assert
        Assert.True(exceptionThrown);
        mockLogger.Verify(
            x => x.LogError("DI Loop detected in the attempted creation of {Type}", type.FullName),
            Times.Once);
        mockLogger.Verify(
            x => x.LogError("Called from: {TypeName}", It.IsAny<string>()),
            Times.AtLeastOnce);
    }
}
