using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EventBus.Tests
{
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

            public override Task PublishFromOutboxAsync(OutgoingEventInfo outgoingEvent, OutboxConfig outboxConfig)
            {
                throw new NotImplementedException();
            }

            public override Task PublishManyFromOutboxAsync(IEnumerable<OutgoingEventInfo> outgoingEvents, OutboxConfig outboxConfig)
            {
                throw new NotImplementedException();
            }

            public override Task ProcessFromInboxAsync(IncomingEventInfo incomingEvent, InboxConfig inboxConfig)
            {
                throw new NotImplementedException();
            }

            public override IDisposable Subscribe(Type eventType, IEventHandlerFactory factory)
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
            var guidGenerator = new Mock<IGuidGenerator>();
            var clock = new Mock<IClock>();
            var serviceScopeFactory = new Mock<IServiceScopeFactory>();

            var bus = new TestDistributedEventBus(serviceScopeFactory.Object, guidGenerator.Object, clock.Object);

            // No inboxes configured
            var result = await bus.AddToInboxAsync("msg1", "EventName", typeof(string), "data", "correlationId");

            Assert.False(result);
        }

        [Fact]
        public async Task AddToInboxAsync_ShouldCallEnqueueAsync_WhenInboxConfiguredAndMessageIdNotExists()
        {
            var inboxConfig = new InboxConfig
            {
                ImplementationType = typeof(IEventInbox),
                EventSelector = (type) => true
            };

            var guidGenerator = new Mock<IGuidGenerator>();
            guidGenerator.Setup(g => g.Create()).Returns(Guid.NewGuid());

            var clock = new Mock<IClock>();
            clock.Setup(c => c.Now).Returns(DateTime.Now);

            var eventInboxMock = new Mock<IEventInbox>();
            eventInboxMock.Setup(e => e.ExistsByMessageIdAsync(It.IsAny<string>())).ReturnsAsync(false);
            eventInboxMock.Setup(e => e.EnqueueAsync(It.IsAny<IncomingEventInfo>())).Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(inboxConfig.ImplementationType))
                .Returns(eventInboxMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

            var bus = new TestDistributedEventBus(serviceScopeFactoryMock.Object, guidGenerator.Object, clock.Object);
            // Inject the options with inbox config
            bus.AbpDistributedEventBusOptions.Inboxes.Clear();
            bus.AbpDistributedEventBusOptions.Inboxes["test"] = inboxConfig;

            var result = await bus.AddToInboxAsync("msg1", "EventName", typeof(string), "data", "correlationId");

            Assert.True(result);
            eventInboxMock.Verify(e => e.ExistsByMessageIdAsync("msg1"), Times.Once);
            eventInboxMock.Verify(e => e.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Once);
        }

        [Fact]
        public async Task AddToInboxAsync_ShouldSkipEnqueue_WhenMessageIdExists()
        {
            var inboxConfig = new InboxConfig
            {
                ImplementationType = typeof(IEventInbox),
                EventSelector = (type) => true
            };

            var guidGenerator = new Mock<IGuidGenerator>();
            guidGenerator.Setup(g => g.Create()).Returns(Guid.NewGuid());

            var clock = new Mock<IClock>();
            clock.Setup(c => c.Now).Returns(DateTime.Now);

            var eventInboxMock = new Mock<IEventInbox>();
            eventInboxMock.Setup(e => e.ExistsByMessageIdAsync(It.IsAny<string>())).ReturnsAsync(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(inboxConfig.ImplementationType))
                .Returns(eventInboxMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

            var bus = new TestDistributedEventBus(serviceScopeFactoryMock.Object, guidGenerator.Object, clock.Object);
            // Inject the options with inbox config
            bus.AbpDistributedEventBusOptions.Inboxes.Clear();
            bus.AbpDistributedEventBusOptions.Inboxes["test"] = inboxConfig;

            var result = await bus.AddToInboxAsync("msg1", "EventName", typeof(string), "data", "correlationId");

            Assert.False(result);
            eventInboxMock.Verify(e => e.ExistsByMessageIdAsync("msg1"), Times.Once);
            eventInboxMock.Verify(e => e.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Never);
        }
    }
}
