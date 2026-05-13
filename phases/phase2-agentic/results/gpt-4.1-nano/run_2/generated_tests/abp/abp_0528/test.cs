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
            public bool AddToOutboxAsyncResult { get; set; }

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

            public override Task<bool> AddToOutboxAsync(Type eventType, object eventData)
            {
                return Task.FromResult(AddToOutboxAsyncResult);
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
        }

        [Fact]
        public async Task AddToOutboxAsync_Should_Call_OnAddToOutboxAsync_And_GetRequiredService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var unitOfWorkMock = new Mock<IUnitOfWorkManager>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var serviceProviderInScopeMock = new Mock<IServiceProvider>();
            var options = Options.Create(new AbpDistributedEventBusOptions());
            var guidGeneratorMock = new Mock<IGuidGenerator>();
            var clockMock = new Mock<IClock>();
            var localEventBusMock = new Mock<ILocalEventBus>();
            var correlationIdProviderMock = new Mock<ICorrelationIdProvider>();

            var implementationType = typeof(MockService);
            var serviceInstanceMock = new Mock<IEventOutbox>();
            var eventOutboxInstance = serviceInstanceMock.Object;

            // Setup service provider to return the mock IEventOutbox
            serviceProviderMock.Setup(sp => sp.GetRequiredService(implementationType))
                .Returns(eventOutboxInstance);

            // Setup scope to return the service provider
            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderInScopeMock.Object);
            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

            // Setup unit of work to return the service provider
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
            unitOfWork.SetupGet(u => u).Returns(unitOfWorkMock.Object);

            var eventBus = new DummyDistributedEventBus(
                serviceScopeFactoryMock.Object,
                null,
                unitOfWorkMock.Object,
                options,
                guidGeneratorMock.Object,
                clockMock.Object,
                null,
                localEventBusMock.Object,
                correlationIdProviderMock.Object);

            // Set current unit of work
            var currentUow = unitOfWork.Object;
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            unitOfWorkManagerMock.Setup(m => m.Current).Returns(currentUow);

            // Act
            var result = await eventBus.AddToOutboxAsync(typeof(MockService), new object());

            // Assert
            Assert.True(result);
            Assert.True(eventBus.OnAddToOutboxAsyncCalled);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(implementationType), Times.Once);
        }

        private class MockService : IEventOutbox
        {
            public Task EnqueueAsync(OutgoingEventInfo outgoingEventInfo)
            {
                return Task.CompletedTask;
            }

            public Task<bool> ExistsByMessageIdAsync(string messageId)
            {
                return Task.FromResult(false);
            }
        }
    }
}
