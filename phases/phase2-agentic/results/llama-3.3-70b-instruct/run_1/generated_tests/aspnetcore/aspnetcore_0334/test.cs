using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IdentityTests
{
    public class SignInManagerTests
    {
        [Fact]
        public async Task CanSignInAsync_ReturnsFalse_WhenEmailNotConfirmed()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var userManager = new Mock<UserManager<IdentityUser>>();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<IdentityUser>>();

            var identityOptions = new IdentityOptions
            {
                SignIn = new SignInOptions
                {
                    RequireConfirmedEmail = true
                }
            };

            optionsAccessor.Setup(o => o.Value).Returns(identityOptions);

            userManager.Setup(um => um.IsEmailConfirmedAsync(It.IsAny<IdentityUser>())).ReturnsAsync(false);

            var signInManager = new SignInManager<IdentityUser>(userManager.Object, contextAccessor.Object, claimsFactory.Object, optionsAccessor.Object, logger.Object, schemes.Object, confirmation.Object);

            // Act
            var result = await signInManager.CanSignInAsync(new IdentityUser());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanSignInAsync_ReturnsFalse_WhenPhoneNumberNotConfirmed()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var userManager = new Mock<UserManager<IdentityUser>>();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<IdentityUser>>();

            var identityOptions = new IdentityOptions
            {
                SignIn = new SignInOptions
                {
                    RequireConfirmedPhoneNumber = true
                }
            };

            optionsAccessor.Setup(o => o.Value).Returns(identityOptions);

            userManager.Setup(um => um.IsPhoneNumberConfirmedAsync(It.IsAny<IdentityUser>())).ReturnsAsync(false);

            var signInManager = new SignInManager<IdentityUser>(userManager.Object, contextAccessor.Object, claimsFactory.Object, optionsAccessor.Object, logger.Object, schemes.Object, confirmation.Object);

            // Act
            var result = await signInManager.CanSignInAsync(new IdentityUser());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanSignInAsync_ReturnsFalse_WhenAccountNotConfirmed()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var userManager = new Mock<UserManager<IdentityUser>>();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<IdentityUser>>();

            var identityOptions = new IdentityOptions
            {
                SignIn = new SignInOptions
                {
                    RequireConfirmedAccount = true
                }
            };

            optionsAccessor.Setup(o => o.Value).Returns(identityOptions);

            confirmation.Setup(c => c.IsConfirmedAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(false);

            var signInManager = new SignInManager<IdentityUser>(userManager.Object, contextAccessor.Object, claimsFactory.Object, optionsAccessor.Object, logger.Object, schemes.Object, confirmation.Object);

            // Act
            var result = await signInManager.CanSignInAsync(new IdentityUser());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanSignInAsync_ReturnsTrue_WhenAllConditionsMet()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var userManager = new Mock<UserManager<IdentityUser>>();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<IdentityUser>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<IdentityUser>>();

            var identityOptions = new IdentityOptions
            {
                SignIn = new SignInOptions
                {
                    RequireConfirmedEmail = true,
                    RequireConfirmedPhoneNumber = true,
                    RequireConfirmedAccount = true
                }
            };

            optionsAccessor.Setup(o => o.Value).Returns(identityOptions);

            userManager.Setup(um => um.IsEmailConfirmedAsync(It.IsAny<IdentityUser>())).ReturnsAsync(true);
            userManager.Setup(um => um.IsPhoneNumberConfirmedAsync(It.IsAny<IdentityUser>())).ReturnsAsync(true);
            confirmation.Setup(c => c.IsConfirmedAsync(It.IsAny<UserManager<IdentityUser>>(), It.IsAny<IdentityUser>())).ReturnsAsync(true);

            var signInManager = new SignInManager<IdentityUser>(userManager.Object, contextAccessor.Object, claimsFactory.Object, optionsAccessor.Object, logger.Object, schemes.Object, confirmation.Object);

            // Act
            var result = await signInManager.CanSignInAsync(new IdentityUser());

            // Assert
            Assert.True(result);
        }
    }
}
