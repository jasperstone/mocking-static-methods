using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class UserManagerTests
{
    [Fact]
    public async Task ChangePasswordAsync_LogsDebugOnFailure()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserManager<IdentityUser>>>();
        var mockUserStore = new Mock<IUserStore<IdentityUser>>();
        var mockPasswordHasher = new Mock<IPasswordHasher<IdentityUser>>();
        var mockUserValidator = new Mock<IUserValidator<IdentityUser>>();
        var mockPasswordValidator = new Mock<IPasswordValidator<IdentityUser>>();
        var mockKeyNormalizer = new Mock<ILookupNormalizer>();
        var mockErrorDescriber = new Mock<IdentityErrorDescriber>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        var userManager = new UserManager<IdentityUser>(
            mockUserStore.Object,
            Mock.Of<IOptions<IdentityOptions>>(),
            mockPasswordHasher.Object,
            new[] { mockUserValidator.Object },
            new[] { mockPasswordValidator.Object },
            mockKeyNormalizer.Object,
            mockErrorDescriber.Object,
            mockServiceProvider.Object,
            mockLogger.Object
        );

        var user = new IdentityUser { Id = "1", UserName = "testuser" };
        var currentPassword = "currentPassword";
        var newPassword = "newPassword";

        mockUserStore.Setup(s => s.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        mockPasswordHasher.Setup(p => p.VerifyHashedPassword(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(PasswordVerificationResult.Failed);

        // Act
        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        // Assert
        mockLogger.Verify(
            l => l.LogDebug(
                It.IsAny<LogLevel>(),
                It.Is<string>(s => s.Contains("Change password failed for user.")),
                It.IsAny<object[]>(),
                It.IsAny<Exception>()
            ),
            Times.Once
        );

        Assert.False(result.Succeeded);
    }
}
