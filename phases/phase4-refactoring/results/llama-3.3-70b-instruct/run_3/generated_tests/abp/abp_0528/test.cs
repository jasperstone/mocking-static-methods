using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq.Expressions;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Local;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EventBus.Tests
{
    public class DistributedEventBusBaseTests
    {
        [Fact]
        public async Task AddToInboxAsync_WithValidEventInfo_ReturnsTrue()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IGuidGenerator, GuidGenerator>()
                .AddSingleton<IClock, Clock>()
                .AddSingleton<AbpDistributedEventBusOptions, AbpDistributedEventBusOptions>()
                .AddSingleton<ILocalEventBus, LocalEventBus>()
                .AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>()
                .BuildServiceProvider();

            var distributedEventBusBase = new DistributedEventBusBase(
                Mock.Of<IServiceScopeFactory>(),
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<IOptions<AbpDistributedEventBusOptions>>(),
                serviceProvider.GetService<IGuidGenerator>(),
                serviceProvider.GetService<IClock>(),
                Mock.Of<IEventHandlerInvoker>(),
                serviceProvider.GetService<ILocalEventBus>(),
                serviceProvider.GetService<ICorrelationIdProvider>()
            );

            var eventInbox = new Mock<IEventInbox>();
            eventInbox.Setup(e => e.EnqueueAsync(It.IsAny<IncomingEventInfo>())).Returns(Task.CompletedTask);
            eventInbox.Setup(e => e.ExistsByMessageIdAsync(It.IsAny<string>())).Returns(Task.FromResult(false));
            eventInbox.Setup(e => e.GetWaitingEventsAsync(It.IsAny<int>(), It.IsAny<Expression<Func<IIncomingEventInfo, bool>>>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new List<IIncomingEventInfo>()));
            eventInbox.Setup(e => e.MarkAsProcessedAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
            eventInbox.Setup(e => e.RetryLaterAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<DateTime?>())).Returns(Task.CompletedTask);
            eventInbox.Setup(e => e.MarkAsDiscardAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
            eventInbox.Setup(e => e.DeleteOldEventsAsync()).Returns(Task.CompletedTask);

            var inboxConfig = new InboxConfig
            {
                ImplementationType = typeof(MockEventInbox),
                EventSelector = (eventType) => true
            };

            distributedEventBusBase.AbpDistributedEventBusOptions.Inboxes.Add("TestInbox", inboxConfig);

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

            public Task<List<IIncomingEventInfo>> GetWaitingEventsAsync(int maxCount, Expression<Func<IIncomingEventInfo, bool>>? filter, CancellationToken cancellationToken)
            {
                return Task.FromResult(new List<IIncomingEventInfo>());
            }

            public Task MarkAsProcessedAsync(Guid eventId)
            {
                return Task.CompletedTask;
            }

            public Task RetryLaterAsync(Guid eventId, int retryCount, DateTime? nextRetryTime)
            {
                return Task.CompletedTask;
            }

            public Task MarkAsDiscardAsync(Guid eventId)
            {
                return Task.CompletedTask;
            }

            public Task DeleteOldEventsAsync()
            {
                return Task.CompletedTask;
            }
        }
    }
}
