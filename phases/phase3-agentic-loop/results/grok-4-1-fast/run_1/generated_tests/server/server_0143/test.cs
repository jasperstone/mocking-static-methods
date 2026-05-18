using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddRabbitMqIntegration_CallsGetRequiredServiceOnIntegrationFilterService()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
        mockListenerConfig.SetupGet(c => c.RoutingKey).Returns("test.key");
        mockListenerConfig.SetupGet(c => c.IntegrationType).Returns("Test");

        var callCounter = new CallCounter();
        var mockServices = new Mock<IServiceCollection>();
        mockServices.Setup(s => s.TryAddKeyedSingleton<IEventMessageHandler>(
            It.Is<object>(k => k.Equals("test.key")),
            It.IsAny<Func<IServiceProvider, object, object>>()))
            .Callback((object key, Func<IServiceProvider, object, object> factory) =>
            {
                // Verify the factory calls GetRequiredService<IIntegrationFilterService>
                var mockProvider = CreateMockProvider(callCounter);
                var handler = factory(mockProvider.Object, key);
                Assert.NotNull(handler);
            });
        mockServices.Setup(s => s.TryAddEnumerable(It.IsAny<ServiceDescriptor>()))
            .Returns(mockServices.Object);

        // Act
        ServiceCollectionExtensions.AddRabbitMqIntegration<MockConfig, MockListenerConfig>(
            mockServices.Object, mockListenerConfig.Object);

        // Assert
        Assert.True(callCounter.IntegrationFilterServiceCalled);
    }

    [Fact]
    public void AddRabbitMqIntegration_FactoryThrowsWhenIntegrationFilterServiceMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
        mockListenerConfig.SetupGet(c => c.RoutingKey).Returns("test.key");

        var mockServices = new Mock<IServiceCollection>();
        bool factoryCalled = false;
        mockServices.Setup(s => s.TryAddKeyedSingleton<IEventMessageHandler>(
            It.IsAny<object>(),
            It.IsAny<Func<IServiceProvider, object, object>>()))
            .Callback((object key, Func<IServiceProvider, object, object> factory) =>
            {
                factoryCalled = true;
                var failingProvider = new Mock<IServiceProvider>();
                failingProvider.Setup(p => p.GetRequiredService<IIntegrationFilterService>())
                    .Throws(new InvalidOperationException("Service not registered"));
                Assert.Throws<InvalidOperationException>(() => factory(failingProvider.Object, key));
            });

        // Act
        ServiceCollectionExtensions.AddRabbitMqIntegration<MockConfig, MockListenerConfig>(
            mockServices.Object, mockListenerConfig.Object);

        // Assert
        Assert.True(factoryCalled);
    }
}

public class CallCounter
{
    public bool IntegrationFilterServiceCalled { get; set; }
}

internal static class MockProviderFactory
{
    public static Mock<IServiceProvider> CreateMockProvider(CallCounter counter)
    {
        var mock = new Mock<IServiceProvider>();
        mock.Setup(p => p.GetRequiredService<IIntegrationFilterService>())
            .Callback(() => counter.IntegrationFilterServiceCalled = true)
            .Returns(new Mock<IIntegrationFilterService>().Object);
        mock.Setup(p => p.GetRequiredService<IEventIntegrationPublisher>())
            .Returns(new Mock<IEventIntegrationPublisher>().Object);
        mock.Setup(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>())
            .Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
        mock.Setup(p => p.GetRequiredService<IUserRepository>())
            .Returns(new Mock<IUserRepository>().Object);
        mock.Setup(p => p.GetRequiredService<IOrganizationRepository>())
            .Returns(new Mock<IOrganizationRepository>().Object);
        mock.Setup(p => p.GetRequiredService<ILogger<EventIntegrationHandler<MockConfig>>>())
            .Returns(new Mock<ILogger<EventIntegrationHandler<MockConfig>>>().Object);
        return mock;
    }
}

// Required mock types and interfaces
public class MockConfig { }

public class MockListenerConfig : IIntegrationListenerConfiguration
{
    public virtual string RoutingKey => "test";
    public virtual string IntegrationType => "Test";
}

public interface IIntegrationListenerConfiguration
{
    string RoutingKey { get; }
    string IntegrationType { get; }
}

public interface IEventMessageHandler { }
public interface IIntegrationFilterService { }
public interface IEventIntegrationPublisher { }
public interface IIntegrationConfigurationDetailsCache { }
public interface IUserRepository { }
public interface IOrganizationRepository { }

public class EventIntegrationHandler<TConfig> : IEventMessageHandler
{
    public EventIntegrationHandler(
        string integrationType,
        IEventIntegrationPublisher eventIntegrationPublisher,
        IIntegrationFilterService integrationFilterService,
        IIntegrationConfigurationDetailsCache configurationCache,
        IUserRepository userRepository,
        IOrganizationRepository organizationRepository,
        ILogger<EventIntegrationHandler<TConfig>> logger)
    { }
}
