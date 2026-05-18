using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;

namespace UserManagerTests
{
    public class AddLoginAsyncTests
    {
        [Fact]
        public async Task AddLoginAsync_WhenExistingUser_ReturnsFailedResultAndLogsDebug()
        {
            // Arrange
            var user = new object() as class;
            var loginInfo = new UserLoginInfo("provider", "key", "display");
            var storeMock = new Mock<IUserStore<object>>();
            var loggerMock = new Mock<ILogger<UserManager<object>>>();
            var userManager = new UserManager<object>(
                storeMock.Object,
                Options.Create(new IdentityOptions()),
                new PasswordHasher<object>(),
                new IUserValidator<object>[0],
                new IPasswordValidator<object>[0],
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                loggerMock.Object);

            // Setup FindByLoginAsync to return a user (simulate existing login)
            storeMock.Setup(s => s.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .ReturnsAsync(new object());

            // Act
            var result = await userManager.AddLoginAsync(user, loginInfo);

            // Assert
            Assert.False(result.Succeeded);
            storeMock.Verify(s => s.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, default), Times.Once);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AddLogin for user failed because it was already associated with another user.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
