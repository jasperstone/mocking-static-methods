using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp;
using Volo.Abp.Collections;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Tracing;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EventBus.Distributed.Tests;

public class MockEventInbox : Mock<IEventInbox>
{
    public MockEventInbox()
    {
        this.Setup(x => x.ExistsByMessageIdAsync(It.IsAny<string>())).ReturnsAsync(false);
        this.Setup(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        this.Setup(x => x.GetWaitingEventsAsync(It.IsAny<int>(), It.IsAny<Expression<Func<IIncomingEventInfo, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<IIncomingEventInfo>());
        this.Setup(x => x.MarkAsProcessedAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        this.Setup(x => x.RetryLaterAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<DateTime?>())).Returns(Task.CompletedTask);
        this.Setup(x => x.MarkAsDiscardAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        this.Setup(x => x.DeleteOldEventsAsync()).Returns(Task.CompletedTask);
    }
}

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
        : base(serviceScopeFactory, currentTenant, unitOfWorkManager, abpDistributedEventBusOptions,
            guidGenerator, clock, eventHandlerInvoker, localEventBus, correlationIdProvider)
    {
    }

    public override Task PublishFromOutboxAsync(OutgoingEventInfo outgoingEvent, OutboxConfig outboxConfig) => Task.CompletedTask;
    public override Task PublishManyFromOutboxAsync(IEnumerable<OutgoingEventInfo> outgoingEvents, OutboxConfig outboxConfig) => Task.CompletedTask;
    public override Task ProcessFromInboxAsync(IncomingEventInfo incomingEvent, InboxConfig inboxConfig) => Task.CompletedTask;

    protected override byte[] Serialize(object eventData) => new byte[] { 1, 2, 3 };

    protected override Task PublishToEventBusAsync(Type eventType, object? eventData) => Task.CompletedTask;
    
    protected override IEnumerable<EventTypeWithEventHandlerFactories> GetHandlerFactories(Type eventType) => Enumerable.Empty<EventTypeWithEventHandlerFactories>();
    
    protected override void AddToUnitOfWork(IUnitOfWork unitOfWork, UnitOfWorkEventRecord eventRecord) { }
    
    public override IDisposable Subscribe(Type eventType, IEventHandlerFactory factory) => null!;
    public override IDisposable Subscribe(Type eventType, IEventHandler handler) => null!;
    public override void Unsubscribe(Type eventType, IEventHandlerFactory factory) { }
    public override void Unsubscribe(Type eventType, IEventHandler handler) { }
    public override void UnsubscribeAll(Type eventType) { }
    public override void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class { }
}

public class DistributedEventBusBase_AddToInboxAsync_Tests : IClassFixture<ServiceProviderFixture>
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceProvider> _scopeServiceProviderMock;
    private readonly MockEventInbox _eventInboxMock;
    private readonly AbpDistributedEventBusOptions _options;

    public DistributedEventBusBase_AddToInboxAsync_Tests(ServiceProviderFixture fixture)
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _scopeServiceProviderMock = new Mock<IServiceProvider>();
        _eventInboxMock = new MockEventInbox();

        _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_scopeServiceProviderMock.Object);
        _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);

        _options = new AbpDistributedEventBusOptions();
        var inboxConfig = new InboxConfig
        {
            EventSelector = _ => true,
            ImplementationType = typeof(MockEventInbox)
        };
        _options.Inboxes.Add(inboxConfig);
    }

    [Fact]
    public async Task Should_Call_GetRequiredService_On_Inbox_ImplementationType()
    {
        // Arrange
        _scopeServiceProviderMock
            .Setup(sp => sp.GetRequiredService(typeof(MockEventInbox)))
            .Returns(_eventInboxMock.Object);

        _eventInboxMock.Setup(x => x.ExistsByMessageIdAsync("test-message-id")).ReturnsAsync(false);

        var optionsMock = Options.Create(_options);
        var bus = CreateBus(optionsMock);

        // Act
        var result = await bus.AddToInboxAsync(
            messageId: "test-message-id",
            eventName: "TestEvent",
            eventType: typeof(TestEvent),
            eventData: new TestEvent(),
            correlationId: "test-correlation-id");

        // Assert
        _scopeServiceProviderMock.Verify(sp => sp.GetRequiredService(typeof(MockEventInbox)), Times.Once);
        _eventInboxMock.Verify(x => x.ExistsByMessageIdAsync("test-message-id"), Times.Once);
        _eventInboxMock.Verify(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task Should_Skip_Inbox_If_MessageId_Exists()
    {
        // Arrange
        _scopeServiceProviderMock
            .Setup(sp => sp.GetRequiredService(typeof(MockEventInbox)))
            .Returns(_eventInboxMock.Object);

        _eventInboxMock.Setup(x => x.ExistsByMessageIdAsync("test-message-id")).ReturnsAsync(true);

        var optionsMock = Options.Create(_options);
        var bus = CreateBus(optionsMock);

        // Act
        var result = await bus.AddToInboxAsync(
            messageId: "test-message-id",
            eventName: "TestEvent",
            eventType: typeof(TestEvent),
            eventData: new TestEvent(),
            correlationId: "test-correlation-id");

        // Assert
        _eventInboxMock.Verify(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.False(result);
    }

    [Fact]
    public async Task Should_Return_False_If_No_Inboxes_Configured()
    {
        // Arrange
        var emptyOptions = new AbpDistributedEventBusOptions();
        var optionsMock = Options.Create(emptyOptions);
        var bus = CreateBus(optionsMock);

        // Act
        var result = await bus.AddToInboxAsync(
            messageId: "test-message-id",
            eventName: "TestEvent",
            eventType: typeof(TestEvent),
            eventData: new TestEvent(),
            correlationId: "test-correlation-id");

        // Assert
        Assert.False(result);
    }

    private TestDistributedEventBus CreateBus(IOptions<AbpDistributedEventBusOptions> options)
    {
        var guidGeneratorMock = new Mock<IGuidGenerator>();
        guidGeneratorMock.Setup(g => g.Create()).Returns(Guid.NewGuid());

        var clockMock = new Mock<IClock>();
        clockMock.Setup(c => c.Now).Returns(DateTime.UtcNow);

        return new TestDistributedEventBus(
            _serviceScopeFactoryMock.Object,
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IUnitOfWorkManager>(),
            options,
            guidGeneratorMock.Object,
            clockMock.Object,
            Mock.Of<IEventHandlerInvoker>(),
            Mock.Of<ILocalEventBus>(),
            Mock.Of<ICorrelationIdProvider>());
    }
}

public class TestEvent { }

public class ServiceProviderFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }

    public ServiceProviderFixture()
    {
        var services = new ServiceCollection();
        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        (ServiceProvider as IDisposable)?.Dispose();
    }
}
