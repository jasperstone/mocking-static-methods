using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Correlation;
using Xunit;

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
    public async Task AddToInboxAsync_ShouldReturnTrue_WhenEventInboxExistsAndMessageIdIsNotInUse()
    {
        // Arrange
        var eventType = typeof(string);
        var eventName = "TestEvent";
        var eventData = "TestData";
        var messageId = "TestMessageId";
        var correlationId = "TestCorrelationId";

        _optionsMock
            .SetupGet(o => o.Inboxes)
            .Returns(new Dictionary<string, InboxConfig>
            {
                { "TestInbox", new InboxConfig("TestInbox", typeof(IEventInbox), null) }
            });

        _eventInboxMock
            .Setup(e => e.ExistsByMessageIdAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var distributedEventBusBase = new Mock<DistributedEventBusBase>(
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

        distributedEventBusBase
            .SetupGet(d => d.ServiceScopeFactory)
            .Returns(Mock.Of<IServiceScopeFactory>(f =>
                f.CreateScope().ServiceProvider == _serviceProviderMock.Object));

        distributedEventBusBase
            .SetupGet(d => d.AbpDistributedEventBusOptions)
            .Returns(_optionsMock.Object);

        distributedEventBusBase
            .Setup(d => d.Serialize(It.IsAny<object>()))
            .Returns(new byte[0]);

        // Act
        var result = await distributedEventBusBase.Object.AddToInboxAsync(messageId, eventName, eventType, eventData, correlationId);

        // Assert
        Assert.True(result);
        _serviceProviderMock.Verify(s => s.GetRequiredService(typeof(IEventInbox)), Times.Once);
    }

    [Fact]
    public async Task AddToInboxAsync_ShouldReturnFalse_WhenNoInboxesConfigured()
    {
        // Arrange
        var eventType = typeof(string);
        var eventName = "TestEvent";
        var eventData = "TestData";
        var messageId = "TestMessageId";
        var correlationId = "TestCorrelationId";

        _optionsMock
            .SetupGet(o => o.Inboxes)
            .Returns(new Dictionary<string, InboxConfig>());

        var distributedEventBusBase = new Mock<DistributedEventBusBase>(
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

        distributedEventBusBase
            .SetupGet(d => d.ServiceScopeFactory)
            .Returns(Mock.Of<IServiceScopeFactory>(f =>
                f.CreateScope().ServiceProvider == _serviceProviderMock.Object));

        distributedEventBusBase
            .SetupGet(d => d.AbpDistributedEventBusOptions)
            .Returns(_optionsMock.Object);

        distributedEventBusBase
            .Setup(d => d.Serialize(It.IsAny<object>()))
            .Returns(new byte[0]);

        // Act
        var result = await distributedEventBusBase.Object.AddToInboxAsync(messageId, eventName, eventType, eventData, correlationId);

        // Assert
        Assert.False(result);
        _serviceProviderMock.Verify(s => s.GetRequiredService(typeof(IEventInbox)), Times.Never);
    }
}
