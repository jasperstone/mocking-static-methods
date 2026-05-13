using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Microsoft.Extensions.DependencyInjection;

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
            .Setup(s => s.GetRequiredService(It.IsAny<Type>()))
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

        var inboxConfig = new InboxConfig
        {
            ImplementationType = typeof(IEventInbox),
            EventSelector = _ => true
        };

        _optionsMock
            .SetupGet(o => o.Inboxes)
            .Returns(new Dictionary<string, InboxConfig> { { "TestInbox", inboxConfig } });

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
            .Setup(d => d.ServiceScopeFactory.CreateScope())
            .Returns(new Mock<IServiceScope>().Object);

        distributedEventBus
            .SetupGet(d => d.AbpDistributedEventBusOptions)
            .Returns(_optionsMock.Object);

        distributedEventBus
            .Setup(d => d.Serialize(It.IsAny<object>()))
            .Returns(new byte[0]);

        _guidGeneratorMock
            .Setup(g => g.Create())
            .Returns(Guid.NewGuid());

        _clockMock
            .Setup(c => c.Now)
            .Returns(DateTime.UtcNow);

        // Act
        var result = await distributedEventBus.Object.AddToInboxAsync(messageId, eventName, eventType, eventData, correlationId);

        // Assert
        Assert.True(result);
        _eventInboxMock.Verify(e => e.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Once);
    }

    [Fact]
    public async Task AddToInboxAsync_WithNoInboxes_ReturnsFalse()
    {
        // Arrange
        var eventType = typeof(string);
        var eventData = "test event";
        var messageId = Guid.NewGuid().ToString();
        var eventName = "TestEvent";
        var correlationId = Guid.NewGuid().ToString();

        _optionsMock
            .SetupGet(o => o.Inboxes)
            .Returns(new Dictionary<string, InboxConfig>());

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
            .SetupGet(d => d.AbpDistributedEventBusOptions)
            .Returns(_optionsMock.Object);

        // Act
        var result = await distributedEventBus.Object.AddToInboxAsync(messageId, eventName, eventType, eventData, correlationId);

        // Assert
        Assert.False(result);
    }
}
