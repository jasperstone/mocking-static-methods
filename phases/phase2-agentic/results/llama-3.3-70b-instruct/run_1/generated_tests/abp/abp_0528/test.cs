using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Tracing;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EventBus.Tests
{
    public class DistributedEventBusBaseTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<ICurrentTenant> _currentTenantMock;
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<IOptions<AbpDistributedEventBusOptions>> _abpDistributedEventBusOptionsMock;
        private readonly Mock<IGuidGenerator> _guidGeneratorMock;
        private readonly Mock<IClock> _clockMock;
        private readonly Mock<IEventHandlerInvoker> _eventHandlerInvokerMock;
        private readonly Mock<ILocalEventBus> _localEventBusMock;
        private readonly Mock<ICorrelationIdProvider> _correlationIdProviderMock;

        public DistributedEventBusBaseTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _abpDistributedEventBusOptionsMock = new Mock<IOptions<AbpDistributedEventBusOptions>>();
            _guidGeneratorMock = new Mock<IGuidGenerator>();
            _clockMock = new Mock<IClock>();
            _eventHandlerInvokerMock = new Mock<IEventHandlerInvoker>();
            _localEventBusMock = new Mock<ILocalEventBus>();
            _correlationIdProviderMock = new Mock<ICorrelationIdProvider>();
        }

        [Fact]
        public async Task AddToInboxAsync_GetRequiredService_Called()
        {
            // Arrange
            var distributedEventBusBase = new DistributedEventBusBase(
                _serviceScopeFactoryMock.Object,
                _currentTenantMock.Object,
                _unitOfWorkManagerMock.Object,
                _abpDistributedEventBusOptionsMock.Object,
                _guidGeneratorMock.Object,
                _clockMock.Object,
                _eventHandlerInvokerMock.Object,
                _localEventBusMock.Object,
                _correlationIdProviderMock.Object);

            var inboxConfig = new InboxConfig
            {
                ImplementationType = typeof(MockIEventInbox)
            };

            _abpDistributedEventBusOptionsMock.SetupGet(x => x.Value.Inboxes).Returns(new InboxConfigDictionary
            {
                { "TestInbox", inboxConfig }
            });

            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

            var eventInboxMock = new Mock<IEventInbox>();
            serviceProviderMock.Setup(x => x.GetRequiredService(It.IsAny<Type>())).Returns(eventInboxMock.Object);

            // Act
            await distributedEventBusBase.AddToInboxAsync(
                "MessageId",
                "EventName",
                typeof(object),
                new object(),
                "CorrelationId");

            // Assert
            serviceProviderMock.Verify(x => x.GetRequiredService(It.IsAny<Type>()), Times.Once);
        }

        private class MockIEventInbox : IEventInbox
        {
            public Task EnqueueAsync(IncomingEventInfo incomingEvent)
            {
                throw new NotImplementedException();
            }

            public Task<bool> ExistsByMessageIdAsync(string messageId)
            {
                throw new NotImplementedException();
            }
        }
    }
}
