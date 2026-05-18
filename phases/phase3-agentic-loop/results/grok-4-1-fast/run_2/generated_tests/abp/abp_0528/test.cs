using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp;
using Volo.Abp.Collections;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Tracing;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EventBus.Tests.Distributed;

public class DistributedEventBusBaseTests
{
    [Fact]
    public async Task AddToInboxAsync_ShouldCallGetRequiredService_WhenInboxesConfigured()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var eventInboxMock = new Mock<IEventInbox>();

        serviceScopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(scopeMock.Object);

        scopeMock
            .Setup(x => x.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        serviceProviderMock
            .Setup(x => x.GetRequiredService(typeof(MockEventInbox)))
            .Returns(eventInboxMock.Object);

        eventInboxMock
            .Setup(x => x.ExistsByMessageIdAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        eventInboxMock
            .Setup(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>()))
            .Returns(Task.CompletedTask);

        var options = new AbpDistributedEventBusOptions();
        options.Inboxes.Add("testInbox", new InboxConfig
        {
            ImplementationType = typeof(MockEventInbox),
            EventSelector = null
        });

        var distributedEventBus = new TestDistributedEventBus(
            serviceScopeFactoryMock.Object,
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IUnitOfWorkManager>(),
            Options.Create(options),
            Mock.Of<IGuidGenerator>(),
            Mock.Of<IClock>(),
            Mock.Of<IEventHandlerInvoker>(),
            Mock.Of<ILocalEventBus>(),
            Mock.Of<ICorrelationIdProvider>()
        );

        // Act
        var result = await distributedEventBus.AddToInboxAsync(
            messageId: "test-message-id",
            eventName: "TestEvent",
            eventType: typeof(string),
            eventData: "test data",
            correlationId: "test-correlation-id"
        );

        // Assert
        serviceProviderMock.Verify(x => x.GetRequiredService(typeof(MockEventInbox)), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task AddToInboxAsync_ShouldNotCallGetRequiredService_WhenNoInboxesConfigured()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var options = new AbpDistributedEventBusOptions();

        var distributedEventBus = new TestDistributedEventBus(
            serviceScopeFactoryMock.Object,
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IUnitOfWorkManager>(),
            Options.Create(options),
            Mock.Of<IGuidGenerator>(),
            Mock.Of<IClock>(),
            Mock.Of<IEventHandlerInvoker>(),
            Mock.Of<ILocalEventBus>(),
            Mock.Of<ICorrelationIdProvider>()
        );

        // Act
        var result = await distributedEventBus.AddToInboxAsync(
            messageId: "test-message-id",
            eventName: "TestEvent",
            eventType: typeof(string),
            eventData: "test data",
            correlationId: "test-correlation-id"
        );

        // Assert
        Assert.False(result);
    }
}

// Mock interface matching expected usage
public interface IEventInbox
{
    Task<bool> ExistsByMessageIdAsync(string messageId);
    Task EnqueueAsync(IncomingEventInfo eventInfo);
}

public class MockEventInbox : IEventInbox
{
    public Task<bool> ExistsByMessageIdAsync(string messageId) => Task.FromResult(false);
    public Task EnqueueAsync(IncomingEventInfo eventInfo) => Task.CompletedTask;
}

// Test implementation of abstract class with correct access modifiers
public class TestDistributedEventBus : DistributedEventBusBase
{
    public TestDistributedEventBus(
        IServiceScopeFactory serviceScopeFactory,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        IOptions<AbpDistributedEventBusOptions> abpDistributedEventBusOptions,
        IGuidGenerator guidGenerator,
        IClock clock,
        IEventHandlerInvoker eventHandlerInvoker,
        ILocalEventBus localEventBus,
        ICorrelationIdProvider correlationIdProvider)
        : base(serviceScopeFactory, currentTenant, unitOfWorkManager, abpDistributedEventBusOptions, guidGenerator, clock, eventHandlerInvoker, localEventBus, correlationIdProvider)
    {
    }

    protected override byte[] Serialize(object eventData)
    {
        return System.Text.Encoding.UTF8.GetBytes(eventData?.ToString() ?? "");
    }

    public override async Task PublishFromOutboxAsync(OutgoingEventInfo outgoingEvent, OutboxConfig outboxConfig)
    {
        await Task.CompletedTask;
    }

    public override async Task PublishManyFromOutboxAsync(IEnumerable<OutgoingEventInfo> outgoingEvents, OutboxConfig outboxConfig)
    {
        await Task.CompletedTask;
    }

    public override async Task ProcessFromInboxAsync(IncomingEventInfo incomingEvent, InboxConfig inboxConfig)
    {
        await Task.CompletedTask;
    }

    // Implement protected abstract methods from EventBusBase with correct access
    protected override List<IEventHandlerFactory> GetHandlerFactories(Type eventType) => new List<IEventHandlerFactory>();
    protected override void AddToUnitOfWork(IUnitOfWork unitOfWork, UnitOfWorkEventRecord eventRecord) { }
    protected override Task PublishToEventBusAsync(Type eventType, object eventData) => Task.CompletedTask;
    
    // Implement public abstract methods from EventBusBase
    public override IDisposable Subscribe(Type eventType, IEventHandlerFactory handlerFactory) => new MockDisposable();
    public override void Unsubscribe(Type eventType, IEventHandlerFactory handlerFactory) { }
    public override IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class => new MockDisposable();
    public override void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class { }
    public override void UnsubscribeAll(Type eventType) { }

    // Expose protected method for testing
    public new Task<bool> AddToInboxAsync(string? messageId, string eventName, Type eventType, object eventData, string? correlationId)
    {
        return base.AddToInboxAsync(messageId, eventName, eventType, eventData, correlationId);
    }
}

public class MockDisposable : IDisposable
{
    public void Dispose() { }
}
