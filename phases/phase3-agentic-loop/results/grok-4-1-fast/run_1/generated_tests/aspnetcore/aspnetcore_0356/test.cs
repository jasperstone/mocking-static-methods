using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Tests;

public class UserManagerChangePasswordTests
{
    private class TestUser : IdentityUser { }

    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordDoesNotMatch_LogsChangePasswordFailedDebug()
    {
        // Arrange
        var store = new Mock<IUserStore<TestUser>>();
        var passwordStore = new Mock<IPasswordStore<TestUser>>();
        var hasher = new Mock<IPasswordHasher<TestUser>>();
        var logger = new Mock<ILogger<UserManager<TestUser>>>();
        var services = new Mock<IServiceProvider>();
        var options = Options.Create(new IdentityOptions());
        var normalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var userValidator = new Mock<IUserValidator<TestUser>>();
        var passwordValidator = new Mock<IPasswordValidator<TestUser>>();

        store.Setup(s => s.GetPasswordStore()).Returns(passwordStore.Object);
        passwordStore.Setup(s => s.VerifyPasswordAsync(It.IsAny<TestUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PasswordVerificationResult.Failed);

        var userManager = new UserManager<TestUser>(
            store.Object,
            options,
            hasher.Object,
            new[] { userValidator.Object },
            new[] { passwordValidator.Object },
            normalizer.Object,
            errors.Object,
            services.Object,
            logger.Object);

        var user = new TestUser { Id = "testuser" };
        var currentPassword = "wrong";
        var newPassword = "new";

        // Act
        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        // Assert
        Assert.False(result.Succeeded);
        logger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.Is<EventId>(eid => eid.Id == 2),
                "Change password failed for user.",
                It.IsAny<object[]>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
