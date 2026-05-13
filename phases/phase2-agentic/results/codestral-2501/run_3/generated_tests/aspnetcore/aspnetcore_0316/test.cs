using Xunit;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace IdentityApiEndpointRouteBuilderExtensionsTests
{
    public class IdentityApiEndpointRouteBuilderExtensionsTests
    {
        [Fact]
        public void MapIdentityApi_ShouldThrowArgumentNullException_WhenEndpointsIsNull()
        {
            // Arrange
            IEndpointRouteBuilder endpoints = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<IdentityUser>(endpoints));
        }

        [Fact]
        public void MapIdentityApi_ShouldGetRequiredServices_WhenEndpointsIsNotNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var endpointsMock = new Mock<IEndpointRouteBuilder>();
            endpointsMock.Setup(e => e.ServiceProvider).Returns(serviceProviderMock.Object);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<TimeProvider>()).Returns(new TimeProvider());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>()).Returns(Mock.Of<IOptionsMonitor<BearerTokenOptions>>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IEmailSender<IdentityUser>>()).Returns(Mock.Of<IEmailSender<IdentityUser>>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<LinkGenerator>()).Returns(Mock.Of<LinkGenerator>());

            // Act
            var result = IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<IdentityUser>(endpointsMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<TimeProvider>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IEmailSender<IdentityUser>>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<LinkGenerator>(), Times.Once);
        }

        [Fact]
        public async Task MapIdentityApi_RegisterEndpoint_ShouldThrowNotSupportedException_WhenUserManagerDoesNotSupportUserEmail()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var endpointsMock = new Mock<IEndpointRouteBuilder>();
            endpointsMock.Setup(e => e.ServiceProvider).Returns(serviceProviderMock.Object);

            var userManagerMock = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            userManagerMock.Setup(um => um.SupportsUserEmail).Returns(false);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<UserManager<IdentityUser>>()).Returns(userManagerMock.Object);

            var registerRequest = new RegisterRequest { Email = "test@example.com", Password = "Password123!" };
            var httpContext = new DefaultHttpContext();
            var routeGroupMock = new Mock<IEndpointRouteBuilder>();
            endpointsMock.Setup(e => e.MapGroup("")).Returns(routeGroupMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<IdentityUser>(endpointsMock.Object);
                var result = await routeGroupMock.Object.MapPost("/register", async ([FromBody] RegisterRequest registration, HttpContext context, [FromServices] IServiceProvider sp) =>
                {
                    var userManager = sp.GetRequiredService<UserManager<IdentityUser>>();
                    return TypedResults.Ok();
                })(registerRequest, httpContext, serviceProviderMock.Object);
            });
        }
    }
}
