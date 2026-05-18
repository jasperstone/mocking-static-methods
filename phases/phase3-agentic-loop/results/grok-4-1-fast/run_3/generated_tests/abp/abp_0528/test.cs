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

namespace Volo.Abp.EventBus.Tests.Distributed;

public class DistributedEventBusBase_AddToInboxAsync_Tests
{
    [Fact]
    public async Task ShouldCallGetRequiredService_OnScopeServiceProvider()
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
            .Setup(x => x.GetRequiredService(typeof(TestEventInbox)))
            .Returns(eventInboxMock.Object);

        eventInboxMock
            .Setup(x => x.ExistsByMessageIdAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        eventInboxMock
            .Setup(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>()))
            .Returns(Task.CompletedTask);

        var options = new AbpDistributedEventBusOptions
        {
            Inboxes = new InboxConfigDictionary
            {
                ["test"] = new InboxConfig(typeof(TestEventInbox), null)
            }
        };
        var optionsMock = new Mock<IOptions<AbpDistributedEventBusOptions>>();
        optionsMock.Setup(x => x.Value).Returns(options);

        var testBus = new TestDistributedEventBus(
            serviceScopeFactoryMock.Object,
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
        var result = await testBus.AddToInboxAsync(
            messageId: "test-message-id",
            eventName: "TestEvent",
            eventType: typeof(string),
            eventData: "test data",
            correlationId: "test-correlation-id"
        );

        // Assert
        serviceProviderMock.Verify(x => x.GetRequiredService(typeof(TestEventInbox)), Times.Once);
        eventInboxMock.Verify(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task ShouldNotCallGetRequiredService_WhenNoInboxesConfigured()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        var emptyOptions = new AbpDistributedEventBusOptions();
        var optionsMock = new Mock<IOptions<AbpDistributedEventBusOptions>>();
        optionsMock.Setup(x => x.Value).Returns(emptyOptions);

        var testBus = new TestDistributedEventBus(
            serviceScopeFactoryMock.Object,
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
        var result = await testBus.AddToInboxAsync(
            messageId: "test-message-id",
            eventName: "TestEvent",
            eventType: typeof(string),
            eventData: "test data",
            correlationId: "test-correlation-id"
        );

        // Assert
        serviceScopeFactoryMock.Verify(x => x.CreateScope(), Times.Never);
        Assert.False(result);
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

        protected override byte[] Serialize(object eventData)
        {
            return System.Text.Encoding.UTF8.GetBytes(eventData?.ToString() ?? "");
        }

        protected override Task PublishToEventBusAsync(Type eventType, object? eventData)
        {
            return Task.CompletedTask;
        }

        protected override IEnumerable<EventTypeWithEventHandlerFactories> GetHandlerFactories(Type eventType)
        {
            return Enumerable.Empty<EventTypeWithEventHandlerFactories>();
        }

        public override IDisposable Subscribe(Type eventType, IEventHandlerFactory factory)
        {
            return NullDisposable.Instance;
        }

        public override IDisposable Subscribe(Type eventType, IEventHandler handler)
        {
            return NullDisposable.Instance;
        }

        public override void Unsubscribe(Type eventType, IEventHandlerFactory factory)
        {
        }

        public override void Unsubscribe(Type eventType, IEventHandler handler)
        {
        }

        public override void UnsubscribeAll(Type eventType)
        {
        }

        protected override void AddToUnitOfWork(IUnitOfWork unitOfWork, UnitOfWorkEventRecord eventRecord)
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
    }

    private class TestEventInbox : MockOfIEventInbox
    {
    }

    private class MockOfIEventInbox : IEventInbox
    {
        public virtual Task EnqueueAsync(IncomingEventInfo incomingEventInfo) => Task.CompletedTask;
        public virtual Task<bool> ExistsByMessageIdAsync(string messageId) => Task.FromResult(false);
        public virtual Task<List<IIncomingEventInfo>> GetWaitingEventsAsync(int maxResultCount, Expression<Func<IIncomingEventInfo, bool>>? predicate, CancellationToken cancellationToken = default) => Task.FromResult(new List<IIncomingEventInfo>());
        public virtual Task MarkAsProcessedAsync(Guid eventId) => Task.CompletedTask;
        public virtual Task RetryLaterAsync(Guid eventId, int maxRetryCount, DateTime? targetDate) => Task.CompletedTask;
    }

    private class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();
        public void Dispose() { }
    }
}
