using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Duplicati.Server.Database;

namespace Duplicati.Tests.Server.Database
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_CallsExpectedServiceMethods()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

            var mockNotificationUpdateService = new Mock<INotificationUpdateService>(MockBehavior.Strict);
            mockNotificationUpdateService.Setup(s => s.IncrementLastDataUpdateId()).Verifiable();

            var mockEventPollNotify = new Mock<EventPollNotify>(MockBehavior.Strict);
            mockEventPollNotify.Setup(s => s.SignalNewEvent()).Verifiable();
            mockEventPollNotify.Setup(s => s.SignalServerSettingsUpdated()).Verifiable();

            var mockQueueRunnerService = new Mock<IQueueRunnerService>(MockBehavior.Strict);
            var mockCurrentTask = new Mock<IQueueRunnerTask>(MockBehavior.Strict);
            mockCurrentTask.Setup(t => t.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            mockQueueRunnerService.Setup(q => q.GetCurrentTask()).Returns(mockCurrentTask.Object).Verifiable();

            var mockLiveControls = new Mock<LiveControls>(MockBehavior.Strict);
            mockLiveControls.Setup(l => l.UpdatePowerModeProvider()).Verifiable();

            // Setup the service provider to return the mocks in order
            mockServiceProvider.Setup(sp => sp.GetRequiredService<INotificationUpdateService>())
                .Returns(mockNotificationUpdateService.Object).Verifiable();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>())
                .Returns(mockEventPollNotify.Object).Verifiable();
            // The second call to GetRequiredService<EventPollNotify>() returns the same mock
            mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>())
                .Returns(mockEventPollNotify.Object).Verifiable();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IQueueRunnerService>())
                .Returns(mockQueueRunnerService.Object).Verifiable();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<LiveControls>())
                .Returns(mockLiveControls.Object).Verifiable();

            // Create a dummy IDbConnection and Action for constructor
            var mockDbConnection = new Mock<System.Data.IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand()).Returns(Mock.Of<System.Data.IDbCommand>());

            var connection = new Connection(mockDbConnection.Object, false, null, "dataFolder", () => { });

            // Set the service provider to the mock
            connection.SetServiceProvider(mockServiceProvider.Object);

            // Act
            // Use reflection to invoke the private method SignalSettingsChanged
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(connection, null);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<INotificationUpdateService>(), Times.Once);
            mockNotificationUpdateService.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);

            mockServiceProvider.Verify(sp => sp.GetRequiredService<EventPollNotify>(), Times.Exactly(3));
            mockEventPollNotify.Verify(s => s.SignalNewEvent(), Times.Once);
            mockEventPollNotify.Verify(s => s.SignalServerSettingsUpdated(), Times.Once);

            mockServiceProvider.Verify(sp => sp.GetRequiredService<IQueueRunnerService>(), Times.Once);
            mockQueueRunnerService.Verify(q => q.GetCurrentTask(), Times.Once);
            mockCurrentTask.Verify(t => t.UpdateThrottleSpeeds(It.IsAny<int>(), It.IsAny<int>()), Times.Once);

            mockServiceProvider.Verify(sp => sp.GetRequiredService<LiveControls>(), Times.Once);
            mockLiveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_WithoutServiceProvider_DoesNotThrow()
        {
            // Arrange
            var mockDbConnection = new Mock<System.Data.IDbConnection>();
            mockDbConnection.Setup(c => c.CreateCommand()).Returns(Mock.Of<System.Data.IDbCommand>());

            var connection = new Connection(mockDbConnection.Object, false, null, "dataFolder", () => { });

            // Act & Assert
            var method = typeof(Connection).GetMethod("SignalSettingsChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            var exception = Record.Exception(() => method.Invoke(connection, null));
            Assert.Null(exception);
        }
    }
}
