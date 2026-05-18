using System;
using System.Reflection;
using Duplicati.Server.Database;
using Duplicati.Library.Main;
using Duplicati.Library.RestAPI;
using Duplicati.Library.Encryption;
using Duplicati.Library.DynamicLoader;
using Duplicati.Library.Main.Database;
using Duplicati.WebserverCore.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Duplicati.Server;
using System.Data;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_ShouldCallGetRequiredServiceForAllDependencies()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockNotificationUpdateService = new Mock<INotificationUpdateService>();
            var mockEventPollNotify = new Mock<EventPollNotify>();
            var mockQueueRunnerService = new Mock<IQueueRunnerService>();
            var mockLiveControls = new Mock<LiveControls>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(INotificationUpdateService))).Returns(mockNotificationUpdateService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(EventPollNotify))).Returns(mockEventPollNotify.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IQueueRunnerService))).Returns(mockQueueRunnerService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(LiveControls))).Returns(mockLiveControls.Object);

            var connection = new Connection(
                Mock.Of<IDbConnection>(),
                false,
                null,
                string.Empty,
                () => { }
            );
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            var methodInfo = typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(connection, null);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(INotificationUpdateService)), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(EventPollNotify)), Times.Exactly(2));
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IQueueRunnerService)), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(LiveControls)), Times.Once);
        }
    }
}
