using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class UserManagerTests
{
    [Fact]
    public async Task ChangePasswordAsync_LogsDebugMessage_WhenPasswordChangeFails()
    {
        // Arrange
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        var passwordHasherMock = new Mock<IPasswordHasher<IdentityUser>>();
        var userValidatorsMock = new Mock<IUserValidator<IdentityUser>>();
        var passwordValidatorsMock = new Mock<IPasswordValidator<IdentityUser>>();
        var keyNormalizerMock = new Mock<ILookupNormalizer>();
        var errorDescriberMock = new Mock<IdentityErrorDescriber>();
        var loggerMock = new Mock<ILogger<UserManager<IdentityUser>>>();

        var userManager = new UserManager<IdentityUser>(
            userStoreMock.Object,
            Mock.Of<IOptions<IdentityOptions>>(),
            passwordHasherMock.Object,
            new[] { userValidatorsMock.Object },
            new[] { passwordValidatorsMock.Object },
            keyNormalizerMock.Object,
            errorDescriberMock.Object,
            Mock.Of<IServiceProvider>(),
            loggerMock.Object);

        var user = new IdentityUser { Id = "1", UserName = "testuser" };
        userStoreMock.Setup(s => s.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        passwordHasherMock.Setup(p => p.VerifyHashedPassword(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(PasswordVerificationResult.Failed);

        // Act
        var result = await userManager.ChangePasswordAsync(user, "currentPassword", "newPassword");

        // Assert
        loggerMock.Verify(
            l => l.LogDebug(
                It.Is<Microsoft.Extensions.Logging.LogLevel>(l => l == Microsoft.Extensions.Logging.LogLevel.Debug),
                It.Is<string>(s => s.Contains("Change password failed for user.")),
                It.IsAny<object[]>(),
                It.IsAny<Exception>()),
            Times.Once);
    }
}
