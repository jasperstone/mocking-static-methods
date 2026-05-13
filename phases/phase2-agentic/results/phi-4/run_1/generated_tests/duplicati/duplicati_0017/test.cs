using Xunit;
using Moq;
using Duplicati.Library.RestAPI;
using Microsoft.Extensions.DependencyInjection;

public class ConnectionTests
{
    [Fact]
    public void SignalSettingsChanged_CallsUpdatePowerModeProvider()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var liveControlsMock = new Mock<LiveControls>();

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<LiveControls>())
            .Returns(liveControlsMock.Object);

        var connection = new Connection(serviceProviderMock.Object);

        // Act
        connection.SignalSettingsChanged();

        // Assert
        liveControlsMock.Verify(lc => lc.UpdatePowerModeProvider(), Times.Once);
    }
}
