using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Xunit;

namespace Volo.Abp.EventBus.Tests
{
    public class DistributedEventBusBaseTests
    {
        [Fact]
        public async Task AddToInboxAsync_GetRequiredServiceCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var eventInboxMock = new Mock<IEventInbox>();
            serviceProviderMock.Setup(p => p.GetRequiredService(It.IsAny<Type>())).Returns(eventInboxMock.Object);

            var distributedEventBusBase = new DistributedEventBusBaseMock(serviceProviderMock.Object);
            var inboxConfig = new InboxConfig { ImplementationType = typeof(IEventInbox) };

            // Act
            await distributedEventBusBase.AddToInboxAsync(null, "EventName", typeof(object), new object(), null);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService(It.IsAny<Type>()), Times.Once);
        }

        private class DistributedEventBusBaseMock : DistributedEventBusBase
        {
            public DistributedEventBusBaseMock(IServiceProvider serviceProvider) 
                : base(new Mock<IServiceScopeFactory>().Object, 
                       new Mock<ICurrentTenant>().Object, 
                       new Mock<IUnitOfWorkManager>().Object, 
                       new Mock<IOptions<AbpDistributedEventBusOptions>>().Object, 
                       new Mock<IGuidGenerator>().Object, 
                       new Mock<IClock>().Object, 
                       new Mock<IEventHandlerInvoker>().Object, 
                       new Mock<ILocalEventBus>().Object, 
                       new Mock<ICorrelationIdProvider>().Object)
            {
                ServiceScopeFactory = new Mock<IServiceScopeFactory>();
                ServiceScopeFactory.Setup(f => f.CreateScope()).Returns(new Mock<IServiceScope>().Object);
                var serviceScope = ServiceScopeFactory.Object.CreateScope();
                serviceScope.SetupGet(s => s.ServiceProvider).Returns(serviceProvider);
            }

            protected override byte[] Serialize(object eventData)
            {
                throw new NotImplementedException();
            }
        }
    }
}
