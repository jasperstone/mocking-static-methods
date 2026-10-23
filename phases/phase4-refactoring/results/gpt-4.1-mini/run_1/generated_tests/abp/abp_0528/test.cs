using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Xunit;

namespace Volo.Abp.EventBus.Tests.Distributed;

public class DistributedEventBusBaseTests
{
    private class TestDistributedEventBus : DistributedEventBusBase
    {
        public TestDistributedEventBus(
            IServiceScopeFactory serviceScopeFactory,
            IGuidGenerator guidGenerator,
            IClock clock)
            : base(
                serviceScopeFactory,
                null!,
                null!,
                new Microsoft.Extensions.Options.OptionsWrapper<AbpDistributedEventBusOptions>(new AbpDistributedEventBusOptions()),
                guidGenerator,
                clock,
                null!,
                null!,
                null!)
        {
        }

        protected override byte[] Serialize(object eventData)
        {
            return System.Text.Encoding.UTF8.GetBytes(eventData.ToString() ?? "");
        }

        public Task<bool> CallAddToInboxAsync(
            string? messageId,
            string eventName,
            Type eventType,
            object eventData,
            string? correlationId)
        {
            return AddToInboxAsync(messageId, eventName, eventType, eventData, correlationId);
        }

        public override IDisposable Subscribe(Type eventType, IEventHandlerFactory factory)
        {
            throw new NotImplementedException();
        }

        public override void Unsubscribe<TEvent>(Func<TEvent, Task> action)
        {
            throw new NotImplementedException();
        }

        public override void Unsubscribe(Type eventType, IEventHandler handler)
        {
            throw new NotImplementedException();
        }

        public override void Unsubscribe(Type eventType, IEventHandlerFactory factory)
        {
            throw new NotImplementedException();
        }

        public override void UnsubscribeAll(Type eventType)
        {
            throw new NotImplementedException();
        }

        protected override Task PublishFromOutboxAsync(OutgoingEventInfo outgoingEvent, OutboxConfig outboxConfig)
        {
            throw new NotImplementedException();
        }

        protected override Task PublishManyFromOutboxAsync(IEnumerable<OutgoingEventInfo> outgoingEvents, OutboxConfig outboxConfig)
        {
            throw new NotImplementedException();
        }

        public override Task ProcessFromInboxAsync(IncomingEventInfo incomingEvent, InboxConfig inboxConfig)
        {
            throw new NotImplementedException();
        }

        protected override Task PublishToEventBusAsync(Type eventType, object eventData)
        {
            throw new NotImplementedException();
        }

        protected override void AddToUnitOfWork(IUnitOfWork unitOfWork, UnitOfWorkEventRecord eventRecord)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<EventTypeWithEventHandlerFactories> GetHandlerFactories(Type eventType)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public async Task AddToInboxAsync_ShouldReturnFalse_WhenNoInboxesConfigured()
    {
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var guidGeneratorMock = new Mock<IGuidGenerator>();
        var clockMock = new Mock<IClock>();

        var bus = new TestDistributedEventBus(serviceScopeFactoryMock.Object, guidGeneratorMock.Object, clockMock.Object);

        // No inboxes configured
        var result = await bus.CallAddToInboxAsync("msg1", "TestEvent", typeof(string), "data", "corr1");

        Assert.False(result);
    }

    [Fact]
    public async Task AddToInboxAsync_ShouldCallEventInboxMethods_WhenInboxConfiguredAndMessageIdNotExists()
    {
        var inboxConfig = new InboxConfig
        {
            ImplementationType = typeof(IEventInbox),
            EventSelector = (type) => true
        };

        var eventInboxMock = new Mock<IEventInbox>();
        eventInboxMock.Setup(x => x.ExistsByMessageIdAsync(It.IsAny<string>())).ReturnsAsync(false);
        eventInboxMock.Setup(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>())).Returns(Task.CompletedTask);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetRequiredService(inboxConfig.ImplementationType)).Returns(eventInboxMock.Object);

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.SetupGet(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

        var guidGeneratorMock = new Mock<IGuidGenerator>();
        guidGeneratorMock.Setup(x => x.Create()).Returns(Guid.NewGuid());

        var clockMock = new Mock<IClock>();
        clockMock.Setup(x => x.Now).Returns(DateTimeOffset.UtcNow);

        var bus = new TestDistributedEventBus(serviceScopeFactoryMock.Object, guidGeneratorMock.Object, clockMock.Object);
        bus.AbpDistributedEventBusOptions.Inboxes["test"] = inboxConfig;

        var result = await bus.CallAddToInboxAsync("msg1", "TestEvent", typeof(string), "data", "corr1");

        Assert.True(result);
        eventInboxMock.Verify(x => x.ExistsByMessageIdAsync("msg1"), Times.Once);
        eventInboxMock.Verify(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Once);
    }

    [Fact]
    public async Task AddToInboxAsync_ShouldSkipEnqueue_WhenMessageIdExists()
    {
        var inboxConfig = new InboxConfig
        {
            ImplementationType = typeof(IEventInbox),
            EventSelector = (type) => true
        };

        var eventInboxMock = new Mock<IEventInbox>();
        eventInboxMock.Setup(x => x.ExistsByMessageIdAsync(It.IsAny<string>())).ReturnsAsync(true);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetRequiredService(inboxConfig.ImplementationType)).Returns(eventInboxMock.Object);

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.SetupGet(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

        var guidGeneratorMock = new Mock<IGuidGenerator>();
        guidGeneratorMock.Setup(x => x.Create()).Returns(Guid.NewGuid());

        var clockMock = new Mock<IClock>();
        clockMock.Setup(x => x.Now).Returns(DateTimeOffset.UtcNow);

        var bus = new TestDistributedEventBus(serviceScopeFactoryMock.Object, guidGeneratorMock.Object, clockMock.Object);
        bus.AbpDistributedEventBusOptions.Inboxes["test"] = inboxConfig;

        var result = await bus.CallAddToInboxAsync("msg1", "TestEvent", typeof(string), "data", "corr1");

        Assert.False(result);
        eventInboxMock.Verify(x => x.ExistsByMessageIdAsync("msg1"), Times.Once);
        eventInboxMock.Verify(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Never);
    }
}
