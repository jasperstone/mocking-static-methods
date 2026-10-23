using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRA.Mods.Common;

namespace OpenRA.Mods.Common.Tests
{
    public class WebServicesTests
    {
        [Fact]
        public async Task CheckModVersion_InitialStatus_ReturnsNotChecked()
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
        public async Task CheckModVersion_LatestVersion_ReturnsLatestStatus()
        {
            // Arrange
            var webServices = new WebServices();

            // Act
            webServices.CheckModVersion();

            // Assert
            await Task.Delay(100); // Wait for the task to complete
            Assert.Equal(ModVersionStatus.Latest, webServices.ModVersionStatus);
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
            Assert.Equal(ModVersionStatus.Outdated, webServices.ModVersionStatus);
        }
    }
}
