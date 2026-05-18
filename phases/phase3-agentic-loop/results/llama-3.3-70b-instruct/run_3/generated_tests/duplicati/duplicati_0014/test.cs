using Xunit;
using Moq;
using System;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Server.Database;

namespace Duplicati.Server.Database.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_GetRequiredServiceCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var notificationUpdateServiceMock = new Mock<object>();
            var eventPollNotifyMock = new Mock<object>();
            var queueRunnerServiceMock = new Mock<object>();
            var liveControlsMock = new Mock<object>();

            serviceProviderMock.Setup(sp => sp.GetRequiredService(It.IsAny<Type>())).Returns((Type type) =>
            {
                if (type.Name == "INotificationUpdateService")
                    return notificationUpdateServiceMock.Object;
                else if (type.Name == "EventPollNotify")
                    return eventPollNotifyMock.Object;
                else if (type.Name == "IQueueRunnerService")
                    return queueRunnerServiceMock.Object;
                else if (type.Name == "LiveControls")
                    return liveControlsMock.Object;
                else
                    throw new Exception("Unknown type");
            });

            var connection = new Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProviderMock.Object);

            // Act
            var methodInfo = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Invoke(connection, null);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService(It.IsAny<Type>()), Times.Exactly(4));
        }
    }
}
