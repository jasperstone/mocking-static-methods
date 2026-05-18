using Xunit;
using Moq;
using Duplicati.Server.Database;
using Duplicati.Library.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_ShouldCallUpdatePowerModeProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var liveControlsMock = new Mock<LiveControls>(null);
            var liveControls = liveControlsMock.Object;

            serviceProviderMock.Setup(sp => sp.GetRequiredService<LiveControls>()).Returns(liveControls);

            var connection = new Connection(null, false, null, "", null)
            {
                m_serviceProvider = serviceProviderMock.Object
            };

            // Act
            connection.SignalSettingsChanged();

            // Assert
            liveControlsMock.Verify(lc => lc.UpdatePowerModeProvider(), Times.Once);
        }
    }
}
