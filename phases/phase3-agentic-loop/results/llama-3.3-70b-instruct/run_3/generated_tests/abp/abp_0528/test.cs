using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Tracing;
using Xunit;

namespace Volo.Abp.EventBus.Distributed.Tests
{
    public class DistributedEventBusBaseTests
    {
        [Fact]
        public async Task AddToInboxAsync_ValidEvent_EventAddedToInbox()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IGuidGenerator, DefaultGuidGenerator>()
                .AddSingleton<IClock, Clock>()
                .AddSingleton<IOptions<AbpDistributedEventBusOptions>, AbpDistributedEventBusOptions>()
                .AddSingleton<ILocalEventBus, LocalEventBus>()
                .AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>()
                .BuildServiceProvider();

            var distributedEventBusBase = new DistributedEventBusBase(
                Mock.Of<IServiceScopeFactory>(),
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IUnitOfWorkManager>(),
                serviceProvider.GetService<IOptions<AbpDistributedEventBusOptions>>(),
                serviceProvider.GetService<IGuidGenerator>(),
                serviceProvider.GetService<IClock>(),
                Mock.Of<IEventHandlerInvoker>(),
                serviceProvider.GetService<ILocalEventBus>(),
                serviceProvider.GetService<ICorrelationIdProvider>()
            );

            var eventInbox = Mock.Of<IEventInbox>();
            var inboxConfig = new InboxConfig(typeof(MyEvent), eventInbox);

            distributedEventBusBase.AbpDistributedEventBusOptions.Inboxes.Add(typeof(MyEvent), inboxConfig);

            // Act
            var result = await distributedEventBusBase.AddToInboxAsync(
                "messageId",
                "eventName",
                typeof(MyEvent),
                new MyEvent(),
                "correlationId"
            );

            // Assert
            Assert.True(result);
        }

        private class MyEvent
        {
        }
    }
}
