using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Volo.Abp.EventBus.Distributed;

namespace Volo.Abp.EventBus.Tests
{
    public class DistributedEventBusBaseTests
    {
        private class DummyDistributedEventBus : DistributedEventBusBase
        {
            public bool SerializeCalled { get; private set; }
            public bool OnAddToOutboxAsyncCalled { get; private set; }
            public bool AddToOutboxAsyncReturns { get; set; } = true;
            public bool AddToInboxAsyncReturns { get; set; } = true;
            public bool SerializeReturn { get; set; } = true;

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
                return new byte[] { 1, 2, 3 };
            }

            protected override Task OnAddToOutboxAsync(string eventName, Type eventType, object eventData)
            {
                OnAddToOutboxAsyncCalled = true;
                return Task.CompletedTask;
            }

            protected override Task<bool> AddToOutboxAsync(Type eventType, object eventData)
            {
                return Task.FromResult(AddToOutboxAsyncReturns);
            }

            protected override Task<bool> AddToInboxAsync(string? messageId, string eventName, Type eventType, object eventData, string? correlationId)
            {
                return Task.FromResult(AddToInboxAsyncReturns);
            }
        }

        private IServiceScopeFactory CreateServiceScopeFactoryWithService(Type implementationType, object serviceInstance)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(implementationType, provider => serviceInstance);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProvider);

            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
            return scopeFactoryMock.Object;
        }

        [Fact]
        public async Task AddToOutboxAsync_Should_Call_GetRequiredService_And_Enqueue()
        {
            // Arrange
            var mockEventOutbox = new Mock<IEventOutbox>();
            mockEventOutbox.Setup(e => e.EnqueueAsync(It.IsAny<OutgoingEventInfo>())).Returns(Task.CompletedTask);

            var implementationType = typeof(IEventOutbox);
            var serviceInstance = mockEventOutbox.Object;

            var serviceScopeFactory = CreateServiceScopeFactoryWithService(implementationType, serviceInstance);

            var options = new Mock<IOptions<AbpDistributedEventBusOptions>>();
            var optionsValue = new AbpDistributedEventBusOptions
            {
                Outboxes = new Dictionary<string, OutboxConfig>
                {
                    {
                        "TestOutbox", new OutboxConfig
                        {
                            ImplementationType = implementationType,
                            Selector = null
                        }
                    }
                }
            };
            options.Setup(o => o.Value).Returns(optionsValue);

            var mockGuidGenerator = new Mock<IGuidGenerator>();
            mockGuidGenerator.Setup(g => g.Create()).Returns(Guid.NewGuid());

            var mockClock = new Mock<IClock>();
            mockClock.Setup(c => c.Now).Returns(DateTime.UtcNow);

            var mockCurrentTenant = new Mock<ICurrentTenant>();
            var mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
            var mockEventHandlerInvoker = new Mock<IEventHandlerInvoker>();
            var mockLocalEventBus = new Mock<ILocalEventBus>();
            var mockCorrelationIdProvider = new Mock<ICorrelationIdProvider>();

            var uowMock = new Mock<IUnitOfWork>();
            uowMock.SetupGet(u => u.ServiceProvider).Returns(serviceProvider: new ServiceCollection()
                .AddTransient(implementationType, provider => serviceInstance)
                .BuildServiceProvider());

            mockUnitOfWorkManager.Setup(m => m.Current).Returns(uowMock.Object);

            var eventBus = new DummyDistributedEventBus(
                serviceScopeFactory,
                mockCurrentTenant.Object,
                mockUnitOfWorkManager.Object,
                options,
                mockGuidGenerator.Object,
                mockClock.Object,
                mockEventHandlerInvoker.Object,
                mockLocalEventBus.Object,
                mockCorrelationIdProvider.Object
            );

            // Act
            var result = await eventBus.InvokePrivateMethod<bool>("AddToOutboxAsync", typeof(Type), typeof(object))(typeof(string), new object[] { typeof(string), "TestEvent", typeof(string), new { Prop = "value" } });

            // Assert
            Assert.True(result);
            mockEventOutbox.Verify(e => e.EnqueueAsync(It.IsAny<OutgoingEventInfo>()), Times.Once);
            Assert.True(eventBus.SerializeCalled);
            Assert.True(eventBus.OnAddToOutboxAsyncCalled);
        }
    }
}
