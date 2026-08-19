using System;
using Duplicati.Server;
using Duplicati.Server.Database;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class ConnectionTests
{
    private class TestableConnection : Connection
    {
        public TestableConnection(IDbConnection connection, bool disableFieldEncryption, EncryptedFieldHelper.KeyInstance? key, string dataFolder, Action startOrStopUsageReporter)
            : base(connection, disableFieldEncryption, key, dataFolder, startOrStopUsageReporter)
        {
        }

        public new void SignalSettingsChanged()
        {
            base.SignalSettingsChanged();
        }
    }

    [Fact]
    public void SignalSettingsChanged_ShouldCallUpdatePowerModeProvider()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLiveControls = new Mock<LiveControls>();

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService<LiveControls>())
            .Returns(mockLiveControls.Object);

        var connection = new TestableConnection(
            connection: null,
            disableFieldEncryption: false,
            key: null,
            dataFolder: "",
            startOrStopUsageReporter: () => { }
        );

        connection.SetServiceProvider(mockServiceProvider.Object);

        // Act
        connection.SignalSettingsChanged();

        // Assert
        mockLiveControls.Verify(lc => lc.UpdatePowerModeProvider(), Times.Once);
    }
}
