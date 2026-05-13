using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Xunit;

namespace Volo.Abp.EventBus.Distributed.Tests
{
    public class DistributedEventBusBaseTests
    {
        private class TestDistributedEventBus : DistributedEventBusBase
        {
            public TestDistributedEventBus(
                IServiceScopeFactory serviceScopeFactory,
                IGuidGenerator guidGenerator,
                IClock clock)
                : base(
                    serviceScopeFactory,
                    null!,
                    null!,
                    new Microsoft.Extensions.Options.OptionsWrapper<AbpDistributedEventBusOptions>(new AbpDistributedEventBusOptions()),
                    guidGenerator,
                    clock,
                    null!,
                    null!,
                    null!)
            {
            }

            protected override byte[] Serialize(object eventData)
            {
                return System.Text.Encoding.UTF8.GetBytes(eventData.ToString() ?? "");
            }

            public Task<bool> CallAddToInboxAsync(
                string? messageId,
                string eventName,
                Type eventType,
                object eventData,
                string? correlationId)
            {
                return AddToInboxAsync(messageId, eventName, eventType, eventData, correlationId);
            }
        }

        [Fact]
        public async Task AddToInboxAsync_ShouldReturnFalse_WhenNoInboxesConfigured()
        {
            // Arrange
            var options = new AbpDistributedEventBusOptions();
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            var mockGuidGenerator = new Mock<IGuidGenerator>();
            var mockClock = new Mock<IClock>();

            var bus = new TestDistributedEventBus(mockScopeFactory.Object, mockGuidGenerator.Object, mockClock.Object);

            // Act
            var result = await bus.CallAddToInboxAsync("msg1", "TestEvent", typeof(string), "data", "corr1");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddToInboxAsync_ShouldCallGetRequiredServiceAndEnqueue_WhenInboxConfiguredAndMessageIdNotExists()
        {
            // Arrange
            var inboxConfigType = typeof(MockEventInbox);
            var inboxConfig = new InboxConfig(inboxConfigType, eventSelector: (type) => true);

            var options = new AbpDistributedEventBusOptions();
            options.Inboxes.Add("default", inboxConfig);

            var mockEventInbox = new Mock<IEventInbox>();
            mockEventInbox.Setup(x => x.ExistsByMessageIdAsync(It.IsAny<string>())).ReturnsAsync(false);
            mockEventInbox.Setup(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>())).Returns(Task.CompletedTask);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetRequiredService(inboxConfigType)).Returns(mockEventInbox.Object);

            var mockScope = new Mock<IServiceScope>();
            mockScope.SetupGet(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            var mockGuidGenerator = new Mock<IGuidGenerator>();
            mockGuidGenerator.Setup(x => x.Create()).Returns(Guid.NewGuid());

            var mockClock = new Mock<IClock>();
            mockClock.Setup(x => x.Now).Returns(DateTime.Now);

            var bus = new TestDistributedEventBus(mockScopeFactory.Object, mockGuidGenerator.Object, mockClock.Object);
            // Inject options with inbox config
            typeof(DistributedEventBusBase).GetProperty("AbpDistributedEventBusOptions")!.SetValue(bus, options);

            // Act
            var result = await bus.CallAddToInboxAsync("msg1", "TestEvent", typeof(string), "data", "corr1");

            // Assert
            Assert.True(result);
            mockServiceProvider.Verify(x => x.GetRequiredService(inboxConfigType), Times.Once);
            mockEventInbox.Verify(x => x.ExistsByMessageIdAsync("msg1"), Times.Once);
            mockEventInbox.Verify(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Once);
        }

        [Fact]
        public async Task AddToInboxAsync_ShouldSkipEnqueue_WhenMessageIdExists()
        {
            // Arrange
            var inboxConfigType = typeof(MockEventInbox);
            var inboxConfig = new InboxConfig(inboxConfigType, eventSelector: (type) => true);

            var options = new AbpDistributedEventBusOptions();
            options.Inboxes.Add("default", inboxConfig);

            var mockEventInbox = new Mock<IEventInbox>();
            mockEventInbox.Setup(x => x.ExistsByMessageIdAsync(It.IsAny<string>())).ReturnsAsync(true);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetRequiredService(inboxConfigType)).Returns(mockEventInbox.Object);

            var mockScope = new Mock<IServiceScope>();
            mockScope.SetupGet(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

            var mockGuidGenerator = new Mock<IGuidGenerator>();
            mockGuidGenerator.Setup(x => x.Create()).Returns(Guid.NewGuid());

            var mockClock = new Mock<IClock>();
            mockClock.Setup(x => x.Now).Returns(DateTime.Now);

            var bus = new TestDistributedEventBus(mockScopeFactory.Object, mockGuidGenerator.Object, mockClock.Object);
            // Inject options with inbox config
            typeof(DistributedEventBusBase).GetProperty("AbpDistributedEventBusOptions")!.SetValue(bus, options);

            // Act
            var result = await bus.CallAddToInboxAsync("msg1", "TestEvent", typeof(string), "data", "corr1");

            // Assert
            Assert.False(result);
            mockServiceProvider.Verify(x => x.GetRequiredService(inboxConfigType), Times.Once);
            mockEventInbox.Verify(x => x.ExistsByMessageIdAsync("msg1"), Times.Once);
            mockEventInbox.Verify(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Never);
        }

        // Dummy classes to satisfy types
        private class MockEventInbox : IEventInbox
        {
            public Task<bool> ExistsByMessageIdAsync(string messageId) => Task.FromResult(false);
            public Task EnqueueAsync(IncomingEventInfo incomingEventInfo) => Task.CompletedTask;
        }

        private class InboxConfig
        {
            public Type ImplementationType { get; }
            public Func<Type, bool>? EventSelector { get; }

            public InboxConfig(Type implementationType, Func<Type, bool>? eventSelector)
            {
                ImplementationType = implementationType;
                EventSelector = eventSelector;
            }
        }
    }
}
