using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            public bool TriggerHandlersAsyncCalled { get; private set; }
            public bool TriggerDistributedEventReceivedAsyncCalled { get; private set; }

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

            protected override async Task TriggerHandlersAsync(Type eventType, object eventData, List<Exception> exceptions = null, InboxConfig inboxConfig = null)
            {
                TriggerHandlersAsyncCalled = true;
                await Task.CompletedTask;
            }

            protected override async Task TriggerDistributedEventReceivedAsync(DistributedEventReceived distributedEvent)
            {
                TriggerDistributedEventReceivedAsyncCalled = true;
                await Task.CompletedTask;
            }
        }

        private Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private Mock<IServiceScope> _serviceScopeMock;
        private Mock<IServiceProvider> _serviceProviderMock;
        private Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<ILocalEventBus> _localEventBusMock;
        private Mock<ICorrelationIdProvider> _correlationIdProviderMock;
        private Mock<IClock> _clockMock;
        private Mock<IGuidGenerator> _guidGeneratorMock;
        private Mock<IEventHandlerInvoker> _eventHandlerInvokerMock;
        private Mock<ICurrentTenant> _currentTenantMock;
        private IOptions<AbpDistributedEventBusOptions> _options;

        public DistributedEventBusBaseTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _localEventBusMock = new Mock<ILocalEventBus>();
            _correlationIdProviderMock = new Mock<ICorrelationIdProvider>();
            _clockMock = new Mock<IClock>();
            _guidGeneratorMock = new Mock<IGuidGenerator>();
            _eventHandlerInvokerMock = new Mock<IEventHandlerInvoker>();
            _currentTenantMock = new Mock<ICurrentTenant>();

            _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
            _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);

            _unitOfWorkMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceScopeMock.Setup(x => x.Dispose());

            _unitOfWorkManagerMock.Setup(x => x.Current).Returns(_unitOfWorkMock.Object);

            _options = Options.Create(new AbpDistributedEventBusOptions());
        }

        [Fact]
        public async Task AddToOutboxAsync_Should_Call_OnAddToOutboxAsync_And_Enqueue()
        {
            // Arrange
            var eventType = typeof(string);
            var eventData = "test data";

            var outboxConfig = new OutboxConfig
            {
                ImplementationType = typeof(Mock<IEventOutbox>)
            };

            var options = new AbpDistributedEventBusOptions();
            options.Outboxes.Add("mock", outboxConfig);

            var bus = new DummyDistributedEventBus(
                _serviceScopeFactoryMock.Object,
                _currentTenantMock.Object,
                _unitOfWorkManagerMock.Object,
                _options,
                _guidGeneratorMock.Object,
                _clockMock.Object,
                _eventHandlerInvokerMock.Object,
                _localEventBusMock.Object,
                _correlationIdProviderMock.Object);

            var mockEventOutbox = new Mock<IEventOutbox>();
            _serviceProviderMock.Setup(sp => sp.GetRequiredService(outboxConfig.ImplementationType))
                .Returns(mockEventOutbox.Object);

            // Act
            var result = await bus.InvokePrivateMethod<bool>("AddToOutboxAsync", eventType, eventData);

            // Assert
            Assert.True(result);
            Assert.True(bus.OnAddToOutboxAsyncCalled);
            mockEventOutbox.Verify(x => x.EnqueueAsync(It.IsAny<OutgoingEventInfo>()), Times.Once);
        }

        [Fact]
        public async Task AddToInboxAsync_Should_Return_False_When_NoInboxes()
        {
            // Arrange
            var bus = new DummyDistributedEventBus(
                _serviceScopeFactoryMock.Object,
                _currentTenantMock.Object,
                _unitOfWorkManagerMock.Object,
                _options,
                _guidGeneratorMock.Object,
                _clockMock.Object,
                _eventHandlerInvokerMock.Object,
                _localEventBusMock.Object,
                _correlationIdProviderMock.Object);

            // Clear Inboxes
            var options = new AbpDistributedEventBusOptions();
            options.Inboxes.Clear();

            // Act
            var result = await bus.InvokePrivateMethod<bool>("AddToInboxAsync", "msgId", "eventName", typeof(string), "data", "corrId");

            // Assert
            Assert.False(result);
        }
    }
}
