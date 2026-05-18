using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Identity.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.AspNetCore.Identity;

public class UserManagerTests
{
    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordDoesNotMatch_LogsDebugChangePasswordFailed()
    {
        // Arrange
        var store = new Mock<IUserStore<IdentityUser>>();
        var passwordStore = new Mock<IPasswordStore<IdentityUser>>();
        store.Setup(s => s.GetPasswordStore()).Returns(passwordStore.Object);

        var passwordHasher = new Mock<IPasswordHasher<IdentityUser>>();
        var userValidators = Array.Empty<IUserValidator<IdentityUser>>();
        var passwordValidators = Array.Empty<IPasswordValidator<IdentityUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errorDescriber = new IdentityErrorDescriber();

        var logger = new Mock<ILogger<UserManager<IdentityUser>>>();
        var options = new IdentityOptions();
        var optionsAccessor = Options.Create(options);

        var userManager = new UserManager<IdentityUser>(
            store.Object,
            optionsAccessor,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errorDescriber,
            null,
            logger.Object);

        var user = new IdentityUser { Id = "1" };
        const string currentPassword = "wrong-password";
        const string newPassword = "new-password";

        // Simulate VerifyPasswordAsync returning Failed
        passwordStore.Setup(p => p.GetUserPasswordAsync(user, default))
                     .ReturnsAsync((string?)null);
        passwordHasher.Setup(h => h.VerifyHashedPassword(user, It.IsAny<string>(), currentPassword))
                      .Returns(PasswordVerificationResult.Failed);

        // Act
        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Single(result.Errors);
        Assert.Equal("Password mismatch.", result.Errors[0].Description);

        logger.Verify(
            l => l.LogDebug(
                It.Is<EventId>(eid => eid.Id == 2 && eid.Name == "ChangePasswordFailed"),
                "Change password failed for user."),
            Times.Once);
    }
}
