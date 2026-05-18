using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Server.Database;

public class ConnectionTests
{
    [Fact]
    public void SignalSettingsChanged_ShouldCallUpdatePowerModeProvider()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var liveControlsMock = new Mock<LiveControls>();

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<LiveControls>())
            .Returns(liveControlsMock.Object);

        var connection = new Connection(null, false, null, "", null);
        connection.SetServiceProvider(serviceProviderMock.Object);

        // Act
        connection.SignalSettingsChanged();

        // Assert
        liveControlsMock.Verify(lc => lc.UpdatePowerModeProvider(), Times.Once);
    }
}
