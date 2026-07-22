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
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Local;
using Volo.Abp.EventBus.Local.Events;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Volo.Abp.Tracing;
using Xunit;

namespace Volo.Abp.EventBus.Tests.Distributed;

public class DistributedEventBusBase_AddToInboxAsync_Tests
{
    [Fact]
    public async Task ShouldCallGetRequiredService_WhenInboxesConfigured()
    {
        // Arrange
        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        var scope = new Mock<IServiceScope>();
        var serviceProvider = new Mock<IServiceProvider>();
        var eventInbox = new MockIEventInbox();
        
        scope.Setup(s => s.ServiceProvider).Returns(serviceProvider.Object);
        serviceScopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);
        serviceProvider.Setup(sp => sp.GetRequiredService(typeof(MockIEventInbox))).Returns(eventInbox);
        
        var options = new AbpDistributedEventBusOptions();
        options.Inboxes.Add("test", new InboxConfig(typeof(MockIEventInbox), eventType => true));
        var optionsMock = new Mock<IOptions<AbpDistributedEventBusOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var eventBus = new TestDistributedEventBus(
            serviceScopeFactory.Object,
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
        var result = await eventBus.AddToInboxAsync("test-id", "TestEvent", typeof(TestEvent), new TestEvent(), null);

        // Assert
        serviceScopeFactory.Verify(f => f.CreateScope(), Times.Once());
        serviceProvider.Verify(sp => sp.GetRequiredService(typeof(MockIEventInbox)), Times.Once());
        scope.Verify(s => s.Dispose(), Times.Once());
        Assert.True(result);
    }

    [Fact]
    public async Task ShouldNotCallGetRequiredService_WhenNoInboxesConfigured()
    {
        // Arrange
        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        var serviceProvider = new Mock<IServiceProvider>();
        var options = new AbpDistributedEventBusOptions();
        var optionsMock = new Mock<IOptions<AbpDistributedEventBusOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var eventBus = new TestDistributedEventBus(
            serviceScopeFactory.Object,
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
        var result = await eventBus.AddToInboxAsync("test-id", "TestEvent", typeof(TestEvent), new TestEvent(), null);

        // Assert
        serviceScopeFactory.Verify(f => f.CreateScope(), Times.Never());
        Assert.False(result);
    }
}

public class TestEvent { }

public class MockIEventInbox : IEventInbox
{
    public virtual Task<bool> ExistsByMessageIdAsync(string messageId) => Task.FromResult(false);
    public virtual Task EnqueueAsync(IncomingEventInfo eventInfo) => Task.CompletedTask;
    public virtual Task<List<IIncomingEventInfo>> GetWaitingEventsAsync(int maxCount, Expression<Func<IIncomingEventInfo, bool>>? selector = null, CancellationToken cancellationToken = default) 
        => Task.FromResult(new List<IIncomingEventInfo>());
    public virtual Task MarkAsProcessedAsync(Guid id) => Task.CompletedTask;
    public virtual Task RetryLaterAsync(Guid id, int retryCount, DateTime? targetDate = null) => Task.CompletedTask;
    public virtual Task MarkAsDiscardAsync(Guid id) => Task.CompletedTask;
    public virtual Task DeleteOldEventsAsync() => Task.CompletedTask;
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
    
    public override Task PublishToEventBusAsync(Type eventType, object eventData) => Task.CompletedTask;
    
    public override IDisposable Subscribe(Type eventType, IEventHandlerFactory factory) => Mock.Of<IDisposable>();
    public override void Unsubscribe(Type eventType, IEventHandlerFactory factory) { }
    public override void UnsubscribeAll(Type eventType) { }
    public override void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class { }
    
    public override void AddToUnitOfWork(IUnitOfWork unitOfWork, UnitOfWorkEventRecord eventRecord) { }
}
