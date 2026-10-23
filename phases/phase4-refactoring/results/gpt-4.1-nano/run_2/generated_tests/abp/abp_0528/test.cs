using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Volo.Abp.EventBus.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Volo.Abp.EventBus.Tests
{
    public class DistributedEventBusBaseTests
    {
        private class DummyDistributedEventBus : DistributedEventBusBase
        {
            public bool SerializeCalled { get; private set; }
            public bool OnAddToOutboxAsyncCalled { get; private set; }
            public bool TriggerDistributedEventReceivedAsyncCalled { get; private set; }
            public bool TriggerHandlersAsyncCalled { get; private set; }
            public bool PublishToEventBusAsyncCalled { get; private set; }
            public bool AddToOutboxAsyncResult { get; set; } = true;
            public bool AddToInboxAsyncResult { get; set; } = true;

            public DummyDistributedEventBus(
                IServiceScopeFactory serviceScopeFactory,
                ICurrentTenant currentTenant,
                IUnitOfWorkManager unitOfWorkManager,
                IOptions<AbpDistributedEventBusOptions> options,
                IGuidGenerator guidGenerator,
                IClock clock,
                IEventHandlerInvoker eventHandlerInvoker,
                ILocalEventBus localEventBus,
                ICorrelationIdProvider correlationIdProvider)
                : base(serviceScopeFactory, currentTenant, unitOfWorkManager, options, guidGenerator, clock, eventHandlerInvoker, localEventBus, correlationIdProvider)
            {
            }

            protected override byte[] Serialize(object eventData)
            {
                SerializeCalled = true;
                return new byte[0];
            }

            protected override Task OnAddToOutboxAsync(string eventName, Type eventType, object eventData)
            {
                OnAddToOutboxAsyncCalled = true;
                return Task.CompletedTask;
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

            protected override Task TriggerDistributedEventReceivedAsync(DistributedEventReceived distributedEvent)
            {
                TriggerDistributedEventReceivedAsyncCalled = true;
                return Task.CompletedTask;
            }

            protected override Task TriggerHandlersAsync(Type eventType, object eventData, List<Exception> exceptions = null, InboxConfig inboxConfig = null)
            {
                TriggerHandlersAsyncCalled = true;
                return Task.CompletedTask;
            }

            public override Task PublishToEventBusAsync(Type eventType, object eventData)
            {
                PublishToEventBusAsyncCalled = true;
                return Task.CompletedTask;
            }

            public override Task<bool> AddToOutboxAsync(Type eventType, object eventData)
            {
                return Task.FromResult(AddToOutboxAsyncResult);
            }

            public override Task<bool> AddToInboxAsync(string messageId, string eventName, Type eventType, object eventData, string correlationId)
            {
                return Task.FromResult(AddToInboxAsyncResult);
            }
        }

        [Fact]
        public async Task AddToInboxAsync_Should_Call_GetRequiredService_And_Enqueue()
        {
            // Arrange
            var options = new AbpDistributedEventBusOptions
            {
                Inboxes = new Dictionary<string, InboxConfig>
                {
                    {
                        "inbox1", new InboxConfig
                        {
                            ImplementationType = typeof(IEventInbox),
                            EventSelector = null
                        }
                    }
                }
            };

            var mockInbox = new Mock<IEventInbox>();
            mockInbox.Setup(x => x.ExistsByMessageIdAsync(It.IsAny<string>())).ReturnsAsync(false);
            mockInbox.Setup(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>())).Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService(typeof(IEventInbox))).Returns(mockInbox.Object);

            var mockScope = new Mock<IServiceScope>();
            mockScope.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
            mockUnitOfWorkManager.Setup(x => x.Current).Returns(mockUnitOfWork.Object);

            var mockGuidGenerator = new Mock<IGuidGenerator>();
            mockGuidGenerator.Setup(x => x.Create()).Returns(Guid.NewGuid());

            var mockClock = new Mock<IClock>();
            mockClock.Setup(x => x.Now).Returns(DateTime.UtcNow);

            var mockLocalEventBus = new Mock<ILocalEventBus>();
            var mockCorrelationIdProvider = new Mock<ICorrelationIdProvider>();
            mockCorrelationIdProvider.Setup(x => x.Get()).Returns("corr-id");

            var optionsWrapper = Options.Create(options);

            var testBus = new DummyDistributedEventBus(
                mockScopeFactory.Object,
                null,
                mockUnitOfWorkManager.Object,
                optionsWrapper,
                mockGuidGenerator.Object,
                mockClock.Object,
                null,
                mockLocalEventBus.Object,
                mockCorrelationIdProvider.Object
            );

            // Act
            var result = await testBus.AddToInboxAsync(
                messageId: "msg-123",
                eventName: "TestEvent",
                eventType: typeof(string),
                eventData: "data",
                correlationId: "corr-123"
            );

            // Assert
            Assert.True(result);
            mockInbox.Verify(x => x.ExistsByMessageIdAsync("msg-123"), Times.Once);
            mockInbox.Verify(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Once);
        }
    }
}
