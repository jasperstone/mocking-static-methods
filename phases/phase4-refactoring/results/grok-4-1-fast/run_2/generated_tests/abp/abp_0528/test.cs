using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Tracing;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EventBus.Tests.Distributed;

public class DistributedEventBusBase_AddToInboxAsync_Tests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IEventInbox> _eventInboxMock;
    private readonly TestDistributedEventBus _eventBus;

    public DistributedEventBusBase_AddToInboxAsync_Tests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _eventInboxMock = new Mock<IEventInbox>();

        _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);

        var options = new AbpDistributedEventBusOptions
        {
            Inboxes = new InboxConfigDictionary
            {
                ["test"] = new InboxConfig(
                    typeof(IEventInbox),
                    eventType => true
                )
            }
        };
        var optionsMock = new Mock<IOptions<AbpDistributedEventBusOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        _serviceProviderMock
            .Setup(sp => sp.GetRequiredService(typeof(IEventInbox)))
            .Returns(_eventInboxMock.Object);

        _eventBus = new TestDistributedEventBus(
            _serviceScopeFactoryMock.Object,
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IUnitOfWorkManager>(),
            optionsMock.Object,
            Mock.Of<IGuidGenerator>(),
            Mock.Of<IClock>(),
            Mock.Of<IEventHandlerInvoker>(),
            Mock.Of<ILocalEventBus>(),
            Mock.Of<ICorrelationIdProvider>()
        );
    }

    [Fact]
    public async Task Should_Return_False_When_No_Inboxes_Configured()
    {
        // Arrange
        var options = new AbpDistributedEventBusOptions();
        var optionsMock = new Mock<IOptions<AbpDistributedEventBusOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);
        
        var eventBus = new TestDistributedEventBus(
            _serviceScopeFactoryMock.Object,
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IUnitOfWorkManager>(),
            optionsMock.Object,
            Mock.Of<IGuidGenerator>(),
            Mock.Of<IClock>(),
            Mock.Of<IEventHandlerInvoker>(),
            Mock.Of<ILocalEventBus>(),
            Mock.Of<ICorrelationIdProvider>()
        );

        // Act
        var result = await eventBus.AddToInboxAsync("msg-id", "event", typeof(object), new object(), "corr-id");

        // Assert
        Assert.False(result);
        _serviceScopeFactoryMock.Verify(f => f.CreateScope(), Times.Never);
    }

    [Fact]
    public async Task Should_Call_GetRequiredService_On_ServiceScope_ServiceProvider()
    {
        // Act
        await _eventBus.AddToInboxAsync("msg-id", "event", typeof(object), new object(), "corr-id");

        // Assert
        _serviceScopeFactoryMock.Verify(f => f.CreateScope(), Times.Once);
        _serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IEventInbox)), Times.Once);
        _serviceScopeMock.Verify(s => s.Dispose(), Times.Once);
    }

    [Fact]
    public async Task Should_Skip_Inbox_When_MessageId_Exists()
    {
        // Arrange
        _eventInboxMock.Setup(e => e.ExistsByMessageIdAsync("msg-id")).ReturnsAsync(true);

        // Act
        var result = await _eventBus.AddToInboxAsync("msg-id", "event", typeof(object), new object(), "corr-id");

        // Assert
        Assert.True(result);
        _eventInboxMock.Verify(e => e.ExistsByMessageIdAsync("msg-id"), Times.Once);
        _eventInboxMock.Verify(e => e.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Never);
    }

    [Fact]
    public async Task Should_Enqueue_To_Matching_Inbox_And_Return_True()
    {
        // Arrange
        _eventInboxMock.Setup(e => e.ExistsByMessageIdAsync("msg-id")).ReturnsAsync(false);

        // Act
        var result = await _eventBus.AddToInboxAsync("msg-id", "event-name", typeof(MyEvent), new MyEvent(), "corr-id");

        // Assert
        Assert.True(result);
        _eventInboxMock.Verify(e => e.ExistsByMessageIdAsync("msg-id"), Times.Once);
        _eventInboxMock.Verify(e => e.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Once);
    }

    [Fact]
    public async Task Should_Return_False_When_EventSelector_Does_Not_Match()
    {
        // Arrange
        var options = new AbpDistributedEventBusOptions
        {
            Inboxes = new InboxConfigDictionary
            {
                ["test"] = new InboxConfig(
                    typeof(IEventInbox),
                    eventType => false
                )
            }
        };
        var optionsMock = new Mock<IOptions<AbpDistributedEventBusOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var eventBus = new TestDistributedEventBus(
            _serviceScopeFactoryMock.Object,
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IUnitOfWorkManager>(),
            optionsMock.Object,
            Mock.Of<IGuidGenerator>(),
            Mock.Of<IClock>(),
            Mock.Of<IEventHandlerInvoker>(),
            Mock.Of<ILocalEventBus>(),
            Mock.Of<ICorrelationIdProvider>()
        );

        // Act
        var result = await eventBus.AddToInboxAsync("msg-id", "event", typeof(object), new object(), "corr-id");

        // Assert
        Assert.False(result);
        _serviceProviderMock.Verify(sp => sp.GetRequiredService(It.IsAny<Type>()), Times.Never);
    }

    private class TestDistributedEventBus : DistributedEventBusBase
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

        public override Task PublishFromOutboxAsync(OutgoingEventInfo outgoingEvent, OutboxConfig outboxConfig)
        {
            return Task.CompletedTask;
        }

        public override Task PublishManyFromOutboxAsync(IEnumerable<OutgoingEventInfo> outgoingEvents, OutboxConfig outboxConfig)
        {
            return Task.CompletedTask;
        }

        public override Task ProcessFromInboxAsync(IncomingEventInfo incomingEvent, InboxConfig inboxConfig)
        {
            return Task.CompletedTask;
        }

        protected override byte[] Serialize(object eventData)
        {
            return new byte[] { 1, 2, 3 };
        }

        public override IDisposable Subscribe(Type eventType, IEventHandler handler)
        {
            return NullDisposable.Instance;
        }

        public override void Unsubscribe(Type eventType, IEventHandler handler)
        {
        }

        public override void UnsubscribeAll(Type eventType)
        {
        }

        public override IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class
        {
            return NullDisposable.Instance;
        }

        public override void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class
        {
        }

        public override void UnsubscribeAll<TEvent>() where TEvent : class
        {
        }
    }

    private class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();
        public void Dispose() { }
    }

    private class MyEvent { }
}
