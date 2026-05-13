using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Tests
{
    public class UserManagerTests
    {
        [Fact]
        public async Task ChangePasswordCoreAsync_LogsDebugMessage_WhenPasswordChangeFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UserManager<MockUser>>>();
            var userManager = new UserManager<MockUser>(null, null, null, null, null, null, null, null, loggerMock.Object);

            var user = new MockUser { Id = "1" };
            var currentPassword = "currentPassword";
            var newPassword = "newPassword";

            // Act
            var result = await userManager.ChangePasswordCoreAsync(user, currentPassword, newPassword);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Change password failed for user.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
            Assert.False(result.Succeeded);
        }
    }

    public class MockUser : IdentityUser
    {
    }
}
