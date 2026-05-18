using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Timing;
using Volo.Abp.Tracing;
using Volo.Abp.EventBus.Distributed;

public class DistributedEventBusBaseTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IEventInbox> _eventInboxMock;
    private readonly Mock<IGuidGenerator> _guidGeneratorMock;
    private readonly Mock<IClock> _clockMock;
    private readonly Mock<AbpDistributedEventBusOptions> _optionsMock;

    public DistributedEventBusBaseTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _eventInboxMock = new Mock<IEventInbox>();
        _guidGeneratorMock = new Mock<IGuidGenerator>();
        _clockMock = new Mock<IClock>();
        _optionsMock = new Mock<AbpDistributedEventBusOptions>();

        _serviceProviderMock
            .Setup(s => s.GetService(It.IsAny<Type>()))
            .Returns(_eventInboxMock.Object);
    }

    [Fact]
    public async Task AddToInboxAsync_WithValidConfig_AddsToInbox()
    {
        // Arrange
        var eventType = typeof(string);
        var eventData = "test event";
        var messageId = Guid.NewGuid().ToString();
        var eventName = "TestEvent";
        var correlationId = Guid.NewGuid().ToString();

        var inboxConfig = new InboxConfig("TestInbox", typeof(IEventInbox), _ => true);

        _optionsMock
            .SetupGet(o => o.Inboxes)
            .Returns(new InboxConfigDictionary { { "TestInbox", inboxConfig } });

        var distributedEventBus = new Mock<DistributedEventBusBase>(
            Mock.Of<IServiceScopeFactory>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<IOptions<AbpDistributedEventBusOptions>>(),
            _guidGeneratorMock.Object,
            _clockMock.Object,
            Mock.Of<IEventHandlerInvoker>(),
            Mock.Of<ILocalEventBus>(),
            Mock.Of<ICorrelationIdProvider>())
        {
            CallBase = true
        };

        distributedEventBus
            .SetupGet(db => db.ServiceScopeFactory)
            .Returns(Mock.Of<IServiceScopeFactory>(f => f.CreateScope().ServiceProvider == _serviceProviderMock.Object));

        distributedEventBus
            .SetupGet(db => db.AbpDistributedEventBusOptions)
            .Returns(_optionsMock.Object);

        distributedEventBus
            .Setup(db => db.Serialize(It.IsAny<object>()))
            .Returns(new byte[0]);

        _guidGeneratorMock
            .Setup(g => g.Create())
            .Returns(messageId);

        _clockMock
            .Setup(c => c.Now)
            .Returns(DateTime.UtcNow);

        _eventInboxMock
            .Setup(e => e.ExistsByMessageIdAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _eventInboxMock
            .Setup(e => e.EnqueueAsync(It.IsAny<IncomingEventInfo>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await distributedEventBus.Object.AddToInboxAsync(messageId, eventName, eventType, eventData, correlationId);

        // Assert
        Assert.True(result);
        _serviceProviderMock.Verify(s => s.GetService(It.IsAny<Type>()), Times.Once);
        _eventInboxMock.Verify(e => e.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Once);
    }

    [Fact]
    public async Task AddToInboxAsync_WithNoInboxes_ReturnsFalse()
    {
        // Arrange
        _optionsMock
            .SetupGet(o => o.Inboxes)
            .Returns(new InboxConfigDictionary());

        var distributedEventBus = new Mock<DistributedEventBusBase>(
            Mock.Of<IServiceScopeFactory>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<IOptions<AbpDistributedEventBusOptions>>(),
            Mock.Of<IGuidGenerator>(),
            Mock.Of<IClock>(),
            Mock.Of<IEventHandlerInvoker>(),
            Mock.Of<ILocalEventBus>(),
            Mock.Of<ICorrelationIdProvider>())
        {
            CallBase = true
        };

        distributedEventBus
            .SetupGet(db => db.AbpDistributedEventBusOptions)
            .Returns(_optionsMock.Object);

        // Act
        var result = await distributedEventBus.Object.AddToInboxAsync(null, "TestEvent", typeof(string), "test event", null);

        // Assert
        Assert.False(result);
        _serviceProviderMock.Verify(s => s.GetService(It.IsAny<Type>()), Times.Never);
    }
}
