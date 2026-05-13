using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Tracing;
using Xunit;

namespace Volo.Abp.EventBus.Tests.Distributed;

public class DistributedEventBusBaseTests
{
    [Fact]
    public async Task AddToInboxAsync_ShouldCallGetRequiredService_OnScopeServiceProvider()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var eventInboxMock = new Mock<IEventInbox>();

        serviceScopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(scopeMock.Object);

        scopeMock
            .Setup(x => x.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        serviceProviderMock
            .Setup(x => x.GetRequiredService(It.IsAny<Type>()))
            .Returns(eventInboxMock.Object);

        eventInboxMock
            .Setup(x => x.ExistsByMessageIdAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        eventInboxMock
            .Setup(x => x.EnqueueAsync(It.IsAny<IncomingEventInfo>()))
            .Returns(Task.CompletedTask);

        var optionsMock = new Mock<IOptions<AbpDistributedEventBusOptions>>();
        optionsMock.Setup(x => x.Value).Returns(new AbpDistributedEventBusOptions
        {
            Inboxes = new InboxConfigDictionary
            {
                ["test"] = new InboxConfig(
                    typeof(IEventInbox),
                    eventType => true)
            }
        });

        var guidGeneratorMock = new Mock<IGuidGenerator>();
        guidGeneratorMock.Setup(x => x.Create()).Returns(Guid.NewGuid());

        var clockMock = new Mock<IClock>();
        clockMock.Setup(x => x.Now).Returns(DateTime.UtcNow);

        var testBus = new TestDistributedEventBus(
            serviceScopeFactoryMock.Object,
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IUnitOfWorkManager>(),
            optionsMock.Object,
            guidGeneratorMock.Object,
            clockMock.Object,
            Mock.Of<IEventHandlerInvoker>(),
            Mock.Of<ILocalEventBus>(),
            Mock.Of<ICorrelationIdProvider>()
        );

        // Act
        var result = await testBus.AddToInboxAsync(
            "test-message-id",
            "TestEvent",
            typeof(object),
            new object(),
            "test-correlation-id");

        // Assert
        serviceProviderMock.Verify(x => x.GetRequiredService(It.IsAny<Type>()), Times.Once);
        Assert.True(result);
    }

    private class TestDistributedEventBus : DistributedEventBusBase
    {
        public TestDistributedEventBus(
            IServiceScopeFactory serviceScopeFactory,
            ICurrentTenant currentTenant,
            IUnitOfWorkManager unitOfWorkManager,
            IOptions<AbpDistributedEventBusOptions> abpDistributedEventBusOptions,
            IGuidGenerator guidGenerator,
            IClock clock,
            IEventHandlerInvoker eventHandlerInvoker,
            ILocalEventBus localEventBus,
            ICorrelationIdProvider correlationIdProvider)
            : base(serviceScopeFactory, currentTenant, unitOfWorkManager, abpDistributedEventBusOptions,
                guidGenerator, clock, eventHandlerInvoker, localEventBus, correlationIdProvider)
        {
        }

        public override Task PublishFromOutboxAsync(OutgoingEventInfo outgoingEvent, OutboxConfig outboxConfig)
        {
            throw new NotImplementedException();
        }

        public override Task PublishManyFromOutboxAsync(IEnumerable<OutgoingEventInfo> outgoingEvents, OutboxConfig outboxConfig)
        {
            throw new NotImplementedException();
        }

        public override Task ProcessFromInboxAsync(IncomingEventInfo incomingEvent, InboxConfig inboxConfig)
        {
            throw new NotImplementedException();
        }

        protected override byte[] Serialize(object eventData)
        {
            return new byte[] { 1, 2, 3 };
        }
    }
}
