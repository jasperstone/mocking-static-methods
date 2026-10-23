using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;
using Volo.Abp;
using Volo.Abp.Collections;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
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
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IEventInbox> _mockEventInbox;
    private readonly TestDistributedEventBus _eventBus;

    public DistributedEventBusBase_AddToInboxAsync_Tests()
    {
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockEventInbox = new Mock<IEventInbox>();

        _mockServiceScopeFactory
            .Setup(f => f.CreateScope())
            .Returns(_mockScope.Object);

        _mockScope
            .Setup(s => s.ServiceProvider)
            .Returns(_mockServiceProvider.Object);

        var options = new AbpDistributedEventBusOptions
        {
            Inboxes = new InboxConfigDictionary
            {
                ["test"] = new InboxConfig(
                    typeof(TestEventInbox),
                    eventType => true)
            }
        };

        _eventBus = new TestDistributedEventBus(
            _mockServiceScopeFactory.Object,
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IUnitOfWorkManager>(),
            Options.Create(options),
            Mock.Of<IGuidGenerator>(),
            Mock.Of<IClock>(),
            Mock.Of<IEventHandlerInvoker>(),
            Mock.Of<ILocalEventBus>(),
            Mock.Of<ICorrelationIdProvider>()
        );

        _mockServiceProvider
            .Setup(p => p.GetRequiredService(typeof(TestEventInbox)))
            .Returns(_mockEventInbox.Object);
    }

    [Fact]
    public async Task Should_Return_False_When_No_Inboxes_Configured()
    {
        var options = new AbpDistributedEventBusOptions();
        var eventBusNoInboxes = new TestDistributedEventBusNoInboxes(
            _mockServiceScopeFactory.Object,
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IUnitOfWorkManager>(),
            Options.Create(options),
            Mock.Of<IGuidGenerator>(),
            Mock.Of<IClock>(),
            Mock.Of<IEventHandlerInvoker>(),
            Mock.Of<ILocalEventBus>(),
            Mock.Of<ICorrelationIdProvider>()
        );

        var result = await eventBusNoInboxes.AddToInboxAsync("msg1", "TestEvent", typeof(object), new object(), "corr1");

        Assert.False(result);
    }

    [Fact]
    public async Task Should_Call_GetRequiredService_And_Enqueue_When_Inbox_Matches()
    {
        _mockEventInbox
            .Setup(x => x.ExistsByMessageIdAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockEventInbox
            .Setup(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>()))
            .Returns(Task.CompletedTask);

        var result = await _eventBus.AddToInboxAsync("msg1", "TestEvent", typeof(object), new object(), "corr1");

        Assert.True(result);
        _mockServiceProvider.Verify(p => p.GetRequiredService(typeof(TestEventInbox)), Times.Once);
        _mockEventInbox.Verify(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Once);
    }

    [Fact]
    public async Task Should_Skip_Inbox_When_MessageId_Exists()
    {
        _mockEventInbox
            .Setup(x => x.ExistsByMessageIdAsync("msg1"))
            .ReturnsAsync(true);

        var result = await _eventBus.AddToInboxAsync("msg1", "TestEvent", typeof(object), new object(), "corr1");

        Assert.False(result);
        _mockEventInbox.Verify(x => x.ExistsByMessageIdAsync("msg1"), Times.Once);
        _mockEventInbox.Verify(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Never);
    }

    [Fact]
    public async Task Should_Skip_Inbox_When_EventSelector_Does_Not_Match()
    {
        var optionsNoMatch = new AbpDistributedEventBusOptions
        {
            Inboxes = new InboxConfigDictionary
            {
                ["test"] = new InboxConfig(typeof(TestEventInbox), eventType => false)
            }
        };
        var eventBusNoMatch = new TestDistributedEventBusNoMatch(
            _mockServiceScopeFactory.Object,
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IUnitOfWorkManager>(),
            Options.Create(optionsNoMatch),
            Mock.Of<IGuidGenerator>(),
            Mock.Of<IClock>(),
            Mock.Of<IEventHandlerInvoker>(),
            Mock.Of<ILocalEventBus>(),
            Mock.Of<ICorrelationIdProvider>()
        );

        var result = await eventBusNoMatch.AddToInboxAsync("msg1", "TestEvent", typeof(object), new object(), "corr1");

        Assert.False(result);
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
    
    public override IDisposable? Subscribe(Type eventType, IEventHandlerFactory factory) => null!;
    public override void UnsubscribeAll(Type? eventType) { }
    public override void Unsubscribe(Type eventType, IEventHandlerFactory factory) { }
    public override void Unsubscribe(Type eventType, IEventHandler handler) { }
    public override void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class { }
    protected override void AddToUnitOfWork(IUnitOfWork unitOfWork, UnitOfWorkEventRecord eventRecord) { }
}

public class TestDistributedEventBusNoInboxes : TestDistributedEventBus
{
    public TestDistributedEventBusNoInboxes(
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
}

public class TestDistributedEventBusNoMatch : TestDistributedEventBus
{
    public TestDistributedEventBusNoMatch(
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
}

public class TestEventInbox : IEventInbox
{
    public virtual Task EnqueueAsync(IncomingEventInfo eventInfo) => Task.CompletedTask;
    public virtual Task<bool> ExistsByMessageIdAsync(string messageId) => Task.FromResult(false);
    public virtual Task<List<IIncomingEventInfo>> GetWaitingEventsAsync(int maxResultCount, Expression<Func<IIncomingEventInfo, bool>>? predicate = null, CancellationToken cancellationToken = default) 
        => Task.FromResult(new List<IIncomingEventInfo>());
    public virtual Task MarkAsProcessedAsync(Guid eventId) => Task.CompletedTask;
    public virtual Task RetryLaterAsync(Guid eventId, int maxRetryCount, DateTime? processAfter = null) => Task.CompletedTask;
    public virtual Task MarkAsDiscardAsync(Guid eventId) => Task.CompletedTask;
    public virtual Task DeleteOldEventsAsync() => Task.CompletedTask;
}
