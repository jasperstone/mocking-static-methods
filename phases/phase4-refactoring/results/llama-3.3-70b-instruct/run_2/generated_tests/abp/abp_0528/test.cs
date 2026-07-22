using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq.Expressions;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Xunit;

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

            var eventInbox = serviceProvider.GetService<IEventInbox>();

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

        [Fact]
        public async Task AddToInboxAsync_EventInboxDoesNotExist_EventNotAddedToInbox()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
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

            // Act
            var result = await distributedEventBusBase.AddToInboxAsync(
                "messageId",
                "eventName",
                typeof(object),
                new object(),
                "correlationId"
            );

            // Assert
            Assert.False(result);
        }

        private class MockEventInbox : IEventInbox
        {
            public Task<bool> ExistsByMessageIdAsync(string messageId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(false);
            }

            public Task EnqueueAsync(IncomingEventInfo incomingEventInfo, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task<List<IIncomingEventInfo>> GetWaitingEventsAsync(
                int maxCount,
                Expression<Func<IIncomingEventInfo, bool>>? filter = null,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new List<IIncomingEventInfo>());
            }

            public Task MarkAsProcessedAsync(Guid id, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task RetryLaterAsync(
                Guid id,
                int retryCount,
                DateTime? nextRetryTime = null,
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task MarkAsDiscardAsync(Guid id, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task DeleteOldEventsAsync(DateTime? olderThan = null, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }
    }
}
