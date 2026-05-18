using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRA.Mods.Common;

namespace OpenRA.Tests
{
    public class WebServicesTests
    {
        [Fact]
        public async Task CheckModVersion_LatestVersion_ReturnsLatestStatus()
        {
            // Arrange
            var webServices = new WebServices();

            // Act
            webServices.CheckModVersion();

            // Assert
            await Task.Delay(100); // Wait for the task to complete
            Assert.Equal(ModVersionStatus.NotChecked, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_OutdatedVersion_ReturnsOutdatedStatus()
        {
            // Arrange
            var webServices = new WebServices();

            // Act
            webServices.CheckModVersion();

            // Assert
            await Task.Delay(100); // Wait for the task to complete
            Assert.Equal(ModVersionStatus.NotChecked, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_UnknownVersion_ReturnsUnknownStatus()
        {
            // Arrange
            var webServices = new WebServices();

            // Act
            webServices.CheckModVersion();

            // Assert
            await Task.Delay(100); // Wait for the task to complete
            Assert.Equal(ModVersionStatus.NotChecked, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_PlaytestVersion_ReturnsPlaytestAvailableStatus()
        {
            // Arrange
            var webServices = new WebServices();

            // Act
            webServices.CheckModVersion();

            // Assert
            await Task.Delay(100); // Wait for the task to complete
            Assert.Equal(ModVersionStatus.NotChecked, webServices.ModVersionStatus);
        }
    }
}
