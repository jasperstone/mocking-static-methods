using Xunit;
using Moq;
using System;
using Duplicati.Server;
using Duplicati.Library.Interface;
using Duplicati.Server.Database;

namespace Duplicati.Tests
{
    public class LiveControlsTests
    {
        [Fact]
        public void UpdatePowerModeProvider_CallsGetRequiredService()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLiveControls = new Mock<LiveControls>(new Connection());

            mockServiceProvider.Setup(sp => sp.GetRequiredService<LiveControls>()).Returns(mockLiveControls.Object);

            var connection = new Connection
            {
                ServiceProvider = mockServiceProvider.Object
            };

            // Act
            connection.SignalSettingsChanged();

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<LiveControls>(), Times.Once);
        }
    }
}
