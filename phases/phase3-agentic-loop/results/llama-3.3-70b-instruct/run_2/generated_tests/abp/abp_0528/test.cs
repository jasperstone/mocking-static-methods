using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Xunit;

namespace Volo.Abp.EventBus.Distributed.Tests
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
        public async Task AddToInboxAsync_EventInboxExists_EventAddedToInbox()
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
                _correlationIdProviderMock.Object
            );

            var inboxConfig = new InboxConfig(typeof(MockEventInbox));
            _abpDistributedEventBusOptionsMock.SetupGet(x => x.Value.Inboxes).Returns(new InboxConfigDictionary { { "MockEventInbox", inboxConfig } });

            var eventInboxMock = new Mock<IEventInbox>();
            eventInboxMock.Setup(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>())).Returns(Task.CompletedTask);
            eventInboxMock.Setup(x => x.ExistsByMessageIdAsync(It.IsAny<string>())).Returns(Task.FromResult(false));

            _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(new Mock<IServiceScope>().SetupGet(x => x.ServiceProvider).Returns(new Mock<IServiceProvider>().Setup(x => x.GetRequiredService(It.IsAny<Type>())).Returns(eventInboxMock.Object));

            // Act
            var result = await distributedEventBusBase.AddToInboxAsync("messageId", "eventName", typeof(object), new object(), "correlationId");

            // Assert
            Assert.True(result);
        }

        private class MockEventInbox : IEventInbox
        {
            public Task EnqueueAsync(IncomingEventInfo incomingEvent)
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
