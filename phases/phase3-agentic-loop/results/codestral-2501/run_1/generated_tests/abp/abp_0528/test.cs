using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EventBus.Distributed.Tests
{
    public class DistributedEventBusBaseTests
    {
        [Fact]
        public async Task AddToInboxAsync_Should_AddToInbox_WhenInboxConfigIsValid()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var abpDistributedEventBusOptionsMock = new Mock<IOptions<AbpDistributedEventBusOptions>>();
            var guidGeneratorMock = new Mock<IGuidGenerator>();
            var clockMock = new Mock<IClock>();
            var eventHandlerInvokerMock = new Mock<IEventHandlerInvoker>();
            var localEventBusMock = new Mock<ILocalEventBus>();
            var correlationIdProviderMock = new Mock<ICorrelationIdProvider>();
            var eventInboxMock = new Mock<IEventInbox>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IEventInbox))).Returns(eventInboxMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(ss => ss.ServiceProvider).Returns(serviceProviderMock.Object);

            serviceScopeFactoryMock.Setup(ssf => ssf.CreateScope()).Returns(serviceScopeMock.Object);

            var inboxConfig = new InboxConfig("TestInbox")
            {
                ImplementationType = typeof(IEventInbox),
                EventSelector = (Type eventType) => true
            };

            var options = new AbpDistributedEventBusOptions
            {
                Inboxes = new InboxConfigDictionary
                {
                    { "TestInbox", inboxConfig }
                }
            };

            abpDistributedEventBusOptionsMock.Setup(o => o.Value).Returns(options);

            var distributedEventBusBase = new Mock<DistributedEventBusBase>(
                serviceScopeFactoryMock.Object,
                currentTenantMock.Object,
                unitOfWorkManagerMock.Object,
                abpDistributedEventBusOptionsMock.Object,
                guidGeneratorMock.Object,
                clockMock.Object,
                eventHandlerInvokerMock.Object,
                localEventBusMock.Object,
                correlationIdProviderMock.Object
            ).Object;

            // Act
            var result = await distributedEventBusBase.AddToInboxAsync(
                "messageId",
                "eventName",
                typeof(string),
                "eventData",
                "correlationId");

            // Assert
            Assert.True(result);
            eventInboxMock.Verify(ei => ei.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Once);
        }
    }
}
