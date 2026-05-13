using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Test
{
    public class UserManagerLoggerTests
    {
        private class TestUser { }

        [Fact]
        public async Task AddLoginCoreAsync_LogsDebug_WhenLoginAlreadyAssociated()
        {
            // Arrange
            var user = new TestUser();
            var loginInfo = new UserLoginInfo("provider", "key", "display");

            var storeMock = new Mock<IUserStore<TestUser>>();
            var loginStoreMock = storeMock.As<IUserLoginStore<TestUser>>();
            var loggerMock = new Mock<ILogger<UserManager<TestUser>>>();

            var optionsMock = new Mock<Microsoft.Extensions.Options.IOptions<IdentityOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new IdentityOptions());

            var passwordHasherMock = new Mock<IPasswordHasher<TestUser>>();
            var userValidators = Array.Empty<IUserValidator<TestUser>>();
            var passwordValidators = Array.Empty<IPasswordValidator<TestUser>>();
            var keyNormalizerMock = new Mock<ILookupNormalizer>();
            var errorDescriber = new IdentityErrorDescriber();
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup FindByLoginAsync to return a user, simulating login already associated
            var userManager = new UserManager<TestUser>(
                storeMock.Object,
                optionsMock.Object,
                passwordHasherMock.Object,
                userValidators,
                passwordValidators,
                keyNormalizerMock.Object,
                errorDescriber,
                serviceProviderMock.Object,
                loggerMock.Object);

            // Setup FindByLoginAsync to return a user (simulate existing user)
            var userManagerMock = new Mock<UserManager<TestUser>>(
                storeMock.Object,
                optionsMock.Object,
                passwordHasherMock.Object,
                userValidators,
                passwordValidators,
                keyNormalizerMock.Object,
                errorDescriber,
                serviceProviderMock.Object,
                loggerMock.Object)
            { CallBase = true };

            userManagerMock.Setup(um => um.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey))
                .ReturnsAsync(new TestUser());

            // Act
            var result = await userManagerMock.Object.AddLoginAsync(user, loginInfo);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, e => e.Code == nameof(IdentityErrorDescriber.LoginAlreadyAssociated));

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
