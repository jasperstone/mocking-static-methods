using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Xunit;

namespace Volo.Abp.EventBus.Tests
{
    public class DistributedEventBusBaseTests
    {
        [Fact]
        public async Task AddToInboxAsync_GetRequiredServiceCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var eventInboxMock = new Mock<IEventInbox>();
            var inboxConfig = new InboxConfig
            {
                ImplementationType = typeof(MockEventInbox)
            };

            serviceProviderMock
                .Setup(x => x.GetRequiredService(It.IsAny<Type>()))
                .Returns(eventInboxMock.Object);

            var distributedEventBusBase = new DistributedEventBusBaseMock(
                serviceProviderMock.Object,
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IUnitOfWorkManager>(),
                new AbpDistributedEventBusOptions(),
                Mock.Of<IGuidGenerator>(),
                Mock.Of<IClock>(),
                Mock.Of<IEventHandlerInvoker>(),
                Mock.Of<ILocalEventBus>(),
                Mock.Of<ICorrelationIdProvider>()
            );

            distributedEventBusBase.AbpDistributedEventBusOptions.Inboxes.Add("TestInbox", inboxConfig);

            // Act
            await distributedEventBusBase.AddToInboxAsync(
                "MessageId",
                "EventName",
                typeof(object),
                new object(),
                "CorrelationId"
            );

            // Assert
            serviceProviderMock.Verify(
                x => x.GetRequiredService(It.IsAny<Type>()),
                Times.Once
            );
        }

        private class DistributedEventBusBaseMock : DistributedEventBusBase
        {
            public DistributedEventBusBaseMock(
                IServiceProvider serviceProvider,
                ICurrentTenant currentTenant,
                IUnitOfWorkManager unitOfWorkManager,
                IOptions<AbpDistributedEventBusOptions> abpDistributedEventBusOptions,
                IGuidGenerator guidGenerator,
                IClock clock,
                IEventHandlerInvoker eventHandlerInvoker,
                ILocalEventBus localEventBus,
                ICorrelationIdProvider correlationIdProvider
            ) : base(
                serviceProvider,
                currentTenant,
                unitOfWorkManager,
                abpDistributedEventBusOptions,
                guidGenerator,
                clock,
                eventHandlerInvoker,
                localEventBus,
                correlationIdProvider
            )
            {
            }

            protected override byte[] Serialize(object eventData)
            {
                return new byte[0];
            }
        }

        private class MockEventInbox : IEventInbox
        {
            public Task EnqueueAsync(IncomingEventInfo incomingEventInfo)
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
