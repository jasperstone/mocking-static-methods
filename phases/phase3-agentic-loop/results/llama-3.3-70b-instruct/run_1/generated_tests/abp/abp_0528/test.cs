using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Tracing;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EventBus
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
            _correlationIdProviderMock = new Mock<ICorrelationIdProvider>();
        }

        [Fact]
        public async Task AddToInboxAsync_WithValidEventInfo_ReturnsTrue()
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
                _correlationIdProviderMock.Object
            );

            var inboxConfig = new InboxConfig(typeof(MockEventInbox));
            inboxConfig.EventSelector = (eventType) => true;

            _abpDistributedEventBusOptionsMock.SetupGet(options => options.Value.Inboxes)
                .Returns(new InboxConfigDictionary { { "MockInbox", inboxConfig } });

            var eventInboxMock = new Mock<IEventInbox>();
            eventInboxMock.Setup(inbox => inbox.EnqueueAsync(It.IsAny<IncomingEventInfo>()))
                .Returns(Task.CompletedTask);

            eventInboxMock.Setup(inbox => inbox.ExistsByMessageIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(false));

            eventInboxMock.Setup(inbox => inbox.GetWaitingEventsAsync(It.IsAny<int>(), It.IsAny<Func<IIncomingEventInfo, bool>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<IIncomingEventInfo>()));

            eventInboxMock.Setup(inbox => inbox.MarkAsProcessedAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            eventInboxMock.Setup(inbox => inbox.RetryLaterAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<DateTime?>()))
                .Returns(Task.CompletedTask);

            eventInboxMock.Setup(inbox => inbox.MarkAsDiscardAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            eventInboxMock.Setup(inbox => inbox.DeleteOldEventsAsync())
                .Returns(Task.CompletedTask);

            _serviceScopeFactoryMock.Setup(factory => factory.CreateScope())
                .Returns(new Mock<IServiceScope>().Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(provider => provider.GetRequiredService(It.IsAny<Type>()))
                .Returns(eventInboxMock.Object);

            _serviceScopeFactoryMock.Setup(factory => factory.CreateScope())
                .Returns(new Mock<IServiceScope>().Setup(scope => scope.ServiceProvider)
                    .Returns(serviceProviderMock.Object));

            // Act
            var result = await distributedEventBusBase.AddToInboxAsync(
                "MessageId",
                "EventName",
                typeof(object),
                new object(),
                "CorrelationId"
            );

            // Assert
            Assert.True(result);
        }
    }

    public class MockEventInbox : IEventInbox
    {
        public Task EnqueueAsync(IncomingEventInfo incomingEvent)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByMessageIdAsync(string messageId)
        {
            return Task.FromResult(false);
        }

        public Task<List<IIncomingEventInfo>> GetWaitingEventsAsync(int maxCount, Func<IIncomingEventInfo, bool>? filter = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new List<IIncomingEventInfo>());
        }

        public Task MarkAsProcessedAsync(Guid id)
        {
            return Task.CompletedTask;
        }

        public Task RetryLaterAsync(Guid id, int retryCount, DateTime? nextTryTime = null)
        {
            return Task.CompletedTask;
        }

        public Task MarkAsDiscardAsync(Guid id)
        {
            return Task.CompletedTask;
        }

        public Task DeleteOldEventsAsync()
        {
            return Task.CompletedTask;
        }
    }
}
