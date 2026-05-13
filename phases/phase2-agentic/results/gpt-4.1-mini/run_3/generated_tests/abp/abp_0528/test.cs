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
                ICurrentTenant currentTenant,
                IUnitOfWorkManager unitOfWorkManager,
                IGuidGenerator guidGenerator,
                IClock clock)
                : base(
                    serviceScopeFactory,
                    currentTenant,
                    unitOfWorkManager,
                    new Microsoft.Extensions.Options.OptionsWrapper<AbpDistributedEventBusOptions>(new AbpDistributedEventBusOptions()),
                    guidGenerator,
                    clock,
                    Mock.Of<IEventHandlerInvoker>(),
                    Mock.Of<ILocalEventBus>(),
                    Mock.Of<ICorrelationIdProvider>())
            {
            }

            protected override byte[] Serialize(object eventData)
            {
                return System.Text.Encoding.UTF8.GetBytes(eventData.ToString() ?? string.Empty);
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

            public void SetInboxOptions(AbpDistributedEventBusOptions options)
            {
                AbpDistributedEventBusOptions.Inboxes.Clear();
                foreach (var kvp in options.Inboxes)
                {
                    AbpDistributedEventBusOptions.Inboxes.Add(kvp.Key, kvp.Value);
                }
            }
        }

        private interface IEventInbox
        {
            Task<bool> ExistsByMessageIdAsync(string messageId);
            Task EnqueueAsync(IncomingEventInfo incomingEventInfo);
        }

        [Fact]
        public async Task AddToInboxAsync_ShouldReturnFalse_WhenNoInboxesConfigured()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var guidGeneratorMock = new Mock<IGuidGenerator>();
            var clockMock = new Mock<IClock>();

            var bus = new TestDistributedEventBus(
                serviceScopeFactoryMock.Object,
                currentTenantMock.Object,
                unitOfWorkManagerMock.Object,
                guidGeneratorMock.Object,
                clockMock.Object);

            // No inboxes configured
            bus.SetInboxOptions(new AbpDistributedEventBusOptions());

            // Act
            var result = await bus.CallAddToInboxAsync("msg1", "TestEvent", typeof(string), "data", "corr1");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddToInboxAsync_ShouldCallGetRequiredServiceAndEnqueue_WhenInboxConfiguredAndMessageIdNotExists()
        {
            // Arrange
            var inboxConfigKey = "TestInbox";
            var inboxImplementationType = typeof(IEventInbox);

            var inboxConfig = new InboxConfig
            {
                ImplementationType = inboxImplementationType,
                EventSelector = (type) => true
            };

            var options = new AbpDistributedEventBusOptions();
            options.Inboxes.Add(inboxConfigKey, inboxConfig);

            var eventInboxMock = new Mock<IEventInbox>();
            eventInboxMock.Setup(x => x.ExistsByMessageIdAsync(It.IsAny<string>())).ReturnsAsync(false);
            eventInboxMock.Setup(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>())).Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(inboxImplementationType)).Returns(eventInboxMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

            var currentTenantMock = new Mock<ICurrentTenant>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var guidGeneratorMock = new Mock<IGuidGenerator>();
            guidGeneratorMock.Setup(g => g.Create()).Returns(Guid.NewGuid());
            var clockMock = new Mock<IClock>();
            clockMock.Setup(c => c.Now).Returns(DateTime.Now);

            var bus = new TestDistributedEventBus(
                serviceScopeFactoryMock.Object,
                currentTenantMock.Object,
                unitOfWorkManagerMock.Object,
                guidGeneratorMock.Object,
                clockMock.Object);

            bus.SetInboxOptions(options);

            // Act
            var result = await bus.CallAddToInboxAsync("messageId1", "TestEvent", typeof(string), "eventData", "correlationId1");

            // Assert
            Assert.True(result);
            eventInboxMock.Verify(x => x.ExistsByMessageIdAsync("messageId1"), Times.Once);
            eventInboxMock.Verify(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(inboxImplementationType), Times.Once);
        }

        [Fact]
        public async Task AddToInboxAsync_ShouldSkipEnqueue_WhenMessageIdExists()
        {
            // Arrange
            var inboxConfigKey = "TestInbox";
            var inboxImplementationType = typeof(IEventInbox);

            var inboxConfig = new InboxConfig
            {
                ImplementationType = inboxImplementationType,
                EventSelector = (type) => true
            };

            var options = new AbpDistributedEventBusOptions();
            options.Inboxes.Add(inboxConfigKey, inboxConfig);

            var eventInboxMock = new Mock<IEventInbox>();
            eventInboxMock.Setup(x => x.ExistsByMessageIdAsync(It.IsAny<string>())).ReturnsAsync(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(inboxImplementationType)).Returns(eventInboxMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

            var currentTenantMock = new Mock<ICurrentTenant>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var guidGeneratorMock = new Mock<IGuidGenerator>();
            guidGeneratorMock.Setup(g => g.Create()).Returns(Guid.NewGuid());
            var clockMock = new Mock<IClock>();
            clockMock.Setup(c => c.Now).Returns(DateTime.Now);

            var bus = new TestDistributedEventBus(
                serviceScopeFactoryMock.Object,
                currentTenantMock.Object,
                unitOfWorkManagerMock.Object,
                guidGeneratorMock.Object,
                clockMock.Object);

            bus.SetInboxOptions(options);

            // Act
            var result = await bus.CallAddToInboxAsync("messageId1", "TestEvent", typeof(string), "eventData", "correlationId1");

            // Assert
            Assert.False(result);
            eventInboxMock.Verify(x => x.ExistsByMessageIdAsync("messageId1"), Times.Once);
            eventInboxMock.Verify(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Never);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(inboxImplementationType), Times.Once);
        }
    }
}
