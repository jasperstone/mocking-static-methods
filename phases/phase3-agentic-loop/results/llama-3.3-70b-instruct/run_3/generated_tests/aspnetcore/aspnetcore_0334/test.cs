using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Tests
{
    public class SignInManagerTests
    {
        [Fact]
        public async Task Constructor_GetService_IPasskeyHandler()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IPasskeyHandler<IdentityUser>>(Mock.Of<IPasskeyHandler<IdentityUser>>())
                .BuildServiceProvider();

            var userManager = new UserManager<IdentityUser>(Mock.Of<IUserStore<IdentityUser>>(), 
                Mock.Of<IOptions<IdentityOptions>>(), 
                Mock.Of<IPasswordHasher<IdentityUser>>(), 
                new List<IUserValidator<IdentityUser>>(), 
                new List<IPasswordValidator<IdentityUser>>(), 
                Mock.Of<ILookupNormalizer>(), 
                new IdentityErrorDescriber(), 
                serviceProvider, 
                Mock.Of<ILogger<UserManager<IdentityUser>>>());

            // Act
            var signInManager = new SignInManager<IdentityUser>(userManager, 
                Mock.Of<IHttpContextAccessor>(), 
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(), 
                Mock.Of<IOptions<IdentityOptions>>(), 
                Mock.Of<ILogger<SignInManager<IdentityUser>>>(), 
                Mock.Of<IAuthenticationSchemeProvider>(), 
                Mock.Of<IUserConfirmation<IdentityUser>>());

            // Assert
            Assert.NotNull(((dynamic)signInManager)._passkeyHandler);
        }
    }
}
