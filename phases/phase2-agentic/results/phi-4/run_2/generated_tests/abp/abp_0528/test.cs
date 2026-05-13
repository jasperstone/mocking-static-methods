using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Xunit;

public class DistributedEventBusBaseTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IEventInbox> _eventInboxMock;
    private readonly Mock<IGuidGenerator> _guidGeneratorMock;
    private readonly Mock<IClock> _clockMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly DistributedEventBusBase _distributedEventBusBase;

    public DistributedEventBusBaseTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _eventInboxMock = new Mock<IEventInbox>();
        _guidGeneratorMock = new Mock<IGuidGenerator>();
        _clockMock = new Mock<IClock>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        var options = new AbpDistributedEventBusOptions
        {
            Inboxes = new Dictionary<string, InboxConfig>
            {
                { "TestInbox", new InboxConfig { ImplementationType = typeof(Mock<IEventInbox>).AssemblyQualifiedName } }
            }
        };

        _serviceProviderMock
            .Setup(sp => sp.GetRequiredService(typeof(Mock<IEventInbox>)))
            .Returns(_eventInboxMock.Object);

        _serviceScopeFactoryMock
            .Setup(sf => sf.CreateScope())
            .Returns(new Mock<IServiceScope>().Object);

        _distributedEventBusBase = new Mock<DistributedEventBusBase>(
            _serviceScopeFactoryMock.Object,
            null,
            null,
            new Mock<IOptions<AbpDistributedEventBusOptions>>().Object,
            _guidGeneratorMock.Object,
            _clockMock.Object,
            null,
            null,
            null
        ).Object;

        ((Mock<DistributedEventBusBase>)_distributedEventBusBase).SetupGet(db => db.AbpDistributedEventBusOptions).Returns(options);
    }

    [Fact]
    public async Task AddToInboxAsync_WhenInboxConfigured_AddsToInbox()
    {
        // Arrange
        _guidGeneratorMock.Setup(g => g.Create()).Returns(Guid.NewGuid().ToString());
        _clockMock.Setup(c => c.Now).Returns(DateTime.UtcNow);

        // Act
        var result = await _distributedEventBusBase.AddToInboxAsync("messageId", "eventName", typeof(object), new object(), "correlationId");

        // Assert
        _serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(Mock<IEventInbox>)), Times.Once);
        _eventInboxMock.Verify(ei => ei.EnqueueAsync(It.IsAny<IncomingEventInfo>()), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task AddToInboxAsync_WhenNoInboxesConfigured_DoesNotAddToInbox()
    {
        // Arrange
        var options = new AbpDistributedEventBusOptions
        {
            Inboxes = new Dictionary<string, InboxConfig>()
        };

        ((Mock<DistributedEventBusBase>)_distributedEventBusBase).SetupGet(db => db.AbpDistributedEventBusOptions).Returns(options);

        // Act
        var result = await _distributedEventBusBase.AddToInboxAsync("messageId", "eventName", typeof(object), new object(), "correlationId");

        // Assert
        _serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(Mock<IEventInbox>)), Times.Never);
        Assert.False(result);
    }
}
