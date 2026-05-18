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
        private Mock<IOptions<AbpDistributedEventBusOptions>> _optionsMock;

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
            _optionsMock = new Mock<IOptions<AbpDistributedEventBusOptions>>();

            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);
        }

        private DummyDistributedEventBus CreateBus()
        {
            var options = new AbpDistributedEventBusOptions();
            _optionsMock.Setup(o => o.Value).Returns(options);
            return new DummyDistributedEventBus(
                _serviceScopeFactoryMock.Object,
                _currentTenantMock.Object,
                _unitOfWorkManagerMock.Object,
                _optionsMock,
                _guidGeneratorMock.Object,
                _clockMock.Object,
                _eventHandlerInvokerMock.Object,
                _localEventBusMock.Object,
                _correlationIdProviderMock.Object);
        }

        [Fact]
        public async Task AddToOutboxAsync_Should_Call_GetRequiredService_And_Execute()
        {
            // Arrange
            var bus = CreateBus();

            var eventType = typeof(string);
            var eventData = "test data";

            var outboxConfig = new OutboxConfig
            {
                ImplementationType = typeof(MockEventOutbox).AssemblyQualifiedName,
                Selector = null
            };

            var options = new AbpDistributedEventBusOptions();
            options.Outboxes.Add("test", outboxConfig);
            _optionsMock.Setup(o => o.Value).Returns(options);

            var unitOfWork = new Mock<IUnitOfWork>();
            var serviceProvider = new Mock<IServiceProvider>();
            var eventOutboxMock = new Mock<IEventOutbox>();
            var eventOutboxInstance = eventOutboxMock.Object;

            // Setup service provider to return mock IEventOutbox
            serviceProvider.Setup(sp => sp.GetRequiredService(It.IsAny<Type>()))
                .Returns(eventOutboxInstance);

            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProvider.Object);
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(_unitOfWorkMock.Object);
            _unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProvider.Object);

            // Act
            var result = await bus.InvokePrivateMethod<bool>("AddToOutboxAsync", eventType, eventData);

            // Assert
            Assert.True(result);
            _serviceProviderMock.Verify(sp => sp.GetRequiredService(It.Is<Type>(t => t == typeof(MockEventOutbox))), Times.Once);
        }
    }

    // Helper extension to invoke private methods for testing
    public static class ReflectionExtensions
    {
        public static async Task<T> InvokePrivateMethod<T>(this object obj, string methodName, params object[] args)
        {
            var method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method.Invoke(obj, args);
            if (result is Task<T> task)
            {
                return await task;
            }
            else if (result is Task taskResult)
            {
                await taskResult;
                return default(T);
            }
            else
            {
                return (T)result;
            }
        }
    }

    // Dummy implementations for dependencies
    public class MockEventOutbox : IEventOutbox
    {
        public Task EnqueueAsync(OutgoingEventInfo outgoingEvent)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByMessageIdAsync(string messageId)
        {
            return Task.FromResult(false);
        }
    }
}
