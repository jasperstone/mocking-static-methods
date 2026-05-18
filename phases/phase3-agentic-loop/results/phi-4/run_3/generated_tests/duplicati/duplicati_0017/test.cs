using System;
using Xunit;
using Moq;
using Duplicati.Server;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

public class ConnectionTests
{
    [Fact]
    public void SignalSettingsChanged_CallsUpdatePowerModeProviderOnLiveControls()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var liveControlsMock = new Mock<LiveControls>(null);

        // Setup the mock to return the LiveControls instance when GetRequiredService is called
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<LiveControls>())
            .Returns(liveControlsMock.Object);

        // Mock IDbConnection and IDbCommand
        var dbConnectionMock = new Mock<IDbConnection>();
        var dbCommandMock = new Mock<IDbCommand>();
        dbConnectionMock.Setup(c => c.CreateCommand()).Returns(dbCommandMock.Object);

        // Create an instance of the class containing SignalSettingsChanged
        var connection = new Connection(
            dbConnectionMock.Object,
            false, // disableFieldEncryption
            null, // key
            "", // dataFolder
            () => { } // startOrStopUsageReporter
        )
        {
            ServiceProvider = serviceProviderMock.Object
        };

        // Act
        connection.SignalSettingsChanged();

        // Assert
        liveControlsMock.Verify(lc => lc.UpdatePowerModeProvider(), Times.Once);
    }
}
