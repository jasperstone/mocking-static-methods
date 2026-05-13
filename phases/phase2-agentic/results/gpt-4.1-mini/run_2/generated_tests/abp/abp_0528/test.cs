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
                return System.Text.Encoding.UTF8.GetBytes(eventData.ToString() ?? string.Empty);
            }
        }

        private interface ITestEventInbox : IEventInbox
        {
        }

        [Fact]
        public async Task AddToInboxAsync_Should_Call_GetRequiredService_And_Enqueue_When_MessageId_Not_Exists()
        {
            // Arrange
            var inboxConfigType = typeof(ITestEventInbox);

            var inboxConfig = new InboxConfig(inboxConfigType)
            {
                EventSelector = (type) => true
            };

            var options = new AbpDistributedEventBusOptions();
            options.Inboxes.Add("test", inboxConfig);

            var guid = Guid.NewGuid();
            var guidGeneratorMock = new Mock<IGuidGenerator>();
            guidGeneratorMock.Setup(g => g.Create()).Returns(guid);

            var clockMock = new Mock<IClock>();
            var now = DateTimeOffset.UtcNow;
            clockMock.Setup(c => c.Now).Returns(now);

            var eventInboxMock = new Mock<ITestEventInbox>();
            eventInboxMock.Setup(e => e.ExistsByMessageIdAsync(It.IsAny<string>())).ReturnsAsync(false);
            eventInboxMock.Setup(e => e.EnqueueAsync(It.IsAny<IncomingEventInfo>())).Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(inboxConfigType)).Returns(eventInboxMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

            var distributedEventBus = new TestDistributedEventBus(serviceScopeFactoryMock.Object, guidGeneratorMock.Object, clockMock.Object);
            distributedEventBus.AbpDistributedEventBusOptions.Inboxes.Clear();
            distributedEventBus.AbpDistributedEventBusOptions.Inboxes.Add("test", inboxConfig);

            var messageId = "message-1";
            var eventName = "TestEvent";
            var eventType = typeof(string);
            var eventData = "data";
            var correlationId = "correlation-1";

            // Act
            var result = await distributedEventBus.AddToInboxAsync(messageId, eventName, eventType, eventData, correlationId);

            // Assert
            Assert.True(result);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(inboxConfigType), Times.Once);
            eventInboxMock.Verify(e => e.ExistsByMessageIdAsync(messageId), Times.Once);
            eventInboxMock.Verify(e => e.EnqueueAsync(It.Is<IncomingEventInfo>(info =>
                info.MessageId == messageId &&
                info.EventName == eventName &&
                info.EventData != null)), Times.Once);
        }

        [Fact]
        public async Task AddToInboxAsync_Should_Skip_Enqueue_When_MessageId_Exists()
        {
            // Arrange
            var inboxConfigType = typeof(ITestEventInbox);

            var inboxConfig = new InboxConfig(inboxConfigType)
            {
                EventSelector = (type) => true
            };

            var options = new AbpDistributedEventBusOptions();
            options.Inboxes.Add("test", inboxConfig);

            var guidGeneratorMock = new Mock<IGuidGenerator>();
            guidGeneratorMock.Setup(g => g.Create()).Returns(Guid.NewGuid());

            var clockMock = new Mock<IClock>();
            clockMock.Setup(c => c.Now).Returns(DateTimeOffset.UtcNow);

            var eventInboxMock = new Mock<ITestEventInbox>();
            eventInboxMock.Setup(e => e.ExistsByMessageIdAsync(It.IsAny<string>())).ReturnsAsync(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(inboxConfigType)).Returns(eventInboxMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

            var distributedEventBus = new TestDistributedEventBus(serviceScopeFactoryMock.Object, guidGeneratorMock.Object, clockMock.Object);
            distributedEventBus.AbpDistributedEventBusOptions.Inboxes.Clear();
            distributedEventBus.AbpDistributedEventBusOptions.Inboxes.Add("test", inboxConfig);

            var messageId = "message-1";
            var eventName = "TestEvent";
            var eventType = typeof(string);
            var eventData = "data";
            var correlationId = "correlation-1";

            // Act
            var result = await distributedEventBus.AddToInboxAsync(messageId, eventName, eventType, eventData, correlationId);

            // Assert
            Assert.False(result);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(inboxConfigType), Times.Once);
            eventInboxMock.Verify(e => e.ExistsByMessageIdAsync(messageId), Times.Once);
            eventInboxMock.Verify(e => e.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Never);
        }

        [Fact]
        public async Task AddToInboxAsync_Should_Return_False_When_No_Inboxes_Configured()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var guidGeneratorMock = new Mock<IGuidGenerator>();
            var clockMock = new Mock<IClock>();

            var distributedEventBus = new TestDistributedEventBus(serviceScopeFactoryMock.Object, guidGeneratorMock.Object, clockMock.Object);

            // Act
            var result = await distributedEventBus.AddToInboxAsync("id", "event", typeof(string), "data", "correlation");

            // Assert
            Assert.False(result);
        }
    }
}
