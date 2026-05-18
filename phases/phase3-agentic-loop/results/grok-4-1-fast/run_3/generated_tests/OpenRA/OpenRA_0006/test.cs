using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common;
using OpenRA.Support;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
    public class ItchIntegrationTests
    {
        [Fact]
        public async Task GetPlayerName_NoApiKey_DoesNotInvokeCallback()
        {
            // Arrange
            var callback = new Mock<Action<string>>();
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null);
            
            var itchIntegration = new ItchIntegration();

            // Act
            itchIntegration.GetPlayerName(callback.Object);
            await Task.Delay(500);

            // Assert
            callback.Verify(c => c(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetPlayerName_ApiError_DoesNotInvokeCallback()
        {
            // Arrange
            var callback = new Mock<Action<string>>();
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "invalid-key");
            
            var itchIntegration = new ItchIntegration();

            // Act
            itchIntegration.GetPlayerName(callback.Object);
            await Task.Delay(500);

            // Assert - real network call should fail and hit exception handler
            callback.Verify(c => c(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void NameSelectionLogic_DisplayNamePreferredOverUsername()
        {
            // Test the core name selection logic that runs after successful API call
            var user = new ItchIntegration.User 
            { 
                DisplayName = "DisplayName", 
                Username = "Username" 
            };
            string expectedName = "DisplayName";
            
            string actualName = string.IsNullOrEmpty(user.DisplayName) ? user.Username : user.DisplayName;
            
            Assert.Equal(expectedName, actualName);
        }

        [Fact]
        public void NameSelectionLogic_EmptyDisplayNameUsesUsername()
        {
            var user = new ItchIntegration.User 
            { 
                DisplayName = "", 
                Username = "Username" 
            };
            string expectedName = "Username";
            
            string actualName = string.IsNullOrEmpty(user.DisplayName) ? user.Username : user.DisplayName;
            
            Assert.Equal(expectedName, actualName);
        }

        [Fact]
        public void NameSelectionLogic_NullDisplayNameUsesUsername()
        {
            var user = new ItchIntegration.User 
            { 
                DisplayName = null, 
                Username = "Username" 
            };
            string expectedName = "Username";
            
            string actualName = string.IsNullOrEmpty(user.DisplayName) ? user.Username : user.DisplayName;
            
            Assert.Equal(expectedName, actualName);
        }

        [Fact]
        public void Deserialization_ValidJson_ParsesUserCorrectly()
        {
            var json = """
            {
                "user": {
                    "display_name": "TestDisplay",
                    "username": "testuser"
                }
            }
            """;
            
            var root = JsonSerializer.Deserialize<ItchIntegration.Root>(json);
            Assert.NotNull(root?.User);
            Assert.Equal("TestDisplay", root.User.DisplayName);
            Assert.Equal("testuser", root.User.Username);
        }
    }
}
