using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines
{
    internal class ReseedFolderFlagTests
    {
        [Fact]
        public async Task PerformAsync_LogsSkipped_WhenRerunGuardFlagIsTrue()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = true;
            var loggerMock = new Mock<IStartupLogger>();
            var providerMock = new Mock<object>();
            var pathsMock = new Mock<object>();

            var routine = (ReseedFolderFlag)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ReseedFolderFlag));
            // Use reflection to set private readonly fields
            var loggerField = typeof(ReseedFolderFlag).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var providerField = typeof(ReseedFolderFlag).GetField("_provider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pathsField = typeof(ReseedFolderFlag).GetField("_paths", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            loggerField.SetValue(routine, loggerMock.Object);
            providerField.SetValue(routine, providerMock.Object);
            pathsField.SetValue(routine, pathsMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Migration is skipped because it does not apply."), Times.Once);
        }
    }

    // Minimal interface to mock logger calls
    public interface IStartupLogger
    {
        void LogInformation(string message);
        void LogInformation(string message, params object[] args);
        void LogError(string message, params object[] args);
    }
}
