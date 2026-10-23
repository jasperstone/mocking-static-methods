using System;
using Duplicati.Server.Database;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class ConnectionTests
{
    [Fact]
    public void SignalSettingsChanged_CallsGetRequiredService()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLiveControls = new Mock<LiveControls>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService<LiveControls>()).Returns(mockLiveControls.Object);

        var connection = new Connection(
            Mock.Of<IDbConnection>(),
            false,
            null,
            "dataFolder",
            () => { }
        );
        connection.SetServiceProvider(mockServiceProvider.Object);

        // Act
        connection.TestSignalSettingsChanged();

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService<LiveControls>(), Times.Once);
        mockLiveControls.Verify(lc => lc.UpdatePowerModeProvider(), Times.Once);
    }
}
