using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class UserManagerTests
{
    [Fact]
    public async Task AddLoginAsync_ExistingUser_LogsDebugMessage()
    {
        // Arrange
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        var optionsAccessorMock = new Mock<IOptions<IdentityOptions>>();
        var passwordHasherMock = new Mock<IPasswordHasher<IdentityUser>>();
        var userValidators = new List<IUserValidator<IdentityUser>>();
        var passwordValidators = new List<IPasswordValidator<IdentityUser>>();
        var keyNormalizerMock = new Mock<ILookupNormalizer>();
        var errorDescriberMock = new Mock<IdentityErrorDescriber>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<UserManager<IdentityUser>>>();

        var userManager = new UserManager<IdentityUser>(
            userStoreMock.Object,
            optionsAccessorMock.Object,
            passwordHasherMock.Object,
            userValidators,
            passwordValidators,
            keyNormalizerMock.Object,
            errorDescriberMock.Object,
            serviceProviderMock.Object,
            loggerMock.Object);

        var user = new IdentityUser { UserName = "testuser" };
        var login = new UserLoginInfo("testprovider", "testkey", "testdisplay");

        userStoreMock.Setup(x => x.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        errorDescriberMock.Setup(x => x.LoginAlreadyAssociated())
            .Returns(new IdentityError { Code = "LoginAlreadyAssociated", Description = "Login already associated with another user." });

        // Act
        var result = await userManager.AddLoginAsync(user, login);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AddLogin for user failed because it was already associated with another user.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "LoginAlreadyAssociated");
    }
}
