using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Local;
using System.Linq.Expressions;

namespace Volo.Abp.EventBus.Tests
{
    public class DistributedEventBusBaseTests
    {
        [Fact]
        public async Task AddToInboxAsync_EventInboxExists_EventAddedToInbox()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IEventInbox, MockEventInbox>()
                .BuildServiceProvider();

            var distributedEventBusBase = new DistributedEventBusBase(
                Mock.Of<IServiceScopeFactory>(),
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IUnitOfWorkManager>(),
                new AbpDistributedEventBusOptions(),
                Mock.Of<IGuidGenerator>(),
                Mock.Of<IClock>(),
                Mock.Of<IEventHandlerInvoker>(),
                Mock.Of<ILocalEventBus>(),
                Mock.Of<ICorrelationIdProvider>()
            );

            distributedEventBusBase.AbpDistributedEventBusOptions.Inboxes.Add("inbox1", new InboxConfig
            {
                ImplementationType = typeof(MockEventInbox)
            });

            // Act
            var result = await distributedEventBusBase.AddToInboxAsync(
                "messageId",
                "eventName",
                typeof(object),
                new object(),
                "correlationId"
            );

            // Assert
            Assert.True(result);
        }

        private class MockEventInbox : IEventInbox
        {
            public Task<bool> ExistsByMessageIdAsync(string messageId)
            {
                return Task.FromResult(false);
            }

            public Task EnqueueAsync(IncomingEventInfo incomingEventInfo)
            {
                return Task.CompletedTask;
            }

            public async Task<IEnumerable<IIncomingEventInfo>> GetWaitingEventsAsync(int maxCount, Expression<Func<IIncomingEventInfo, bool>>? filter = null, System.Threading.CancellationToken cancellationToken = default)
            {
                await Task.CompletedTask;
                return Enumerable.Empty<IIncomingEventInfo>();
            }

            public Task MarkAsProcessedAsync(Guid eventId)
            {
                return Task.CompletedTask;
            }

            public Task RetryLaterAsync(Guid eventId, int retryCount, DateTime? nextTryDate = null)
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
