using Microsoft.AspNetCore.SignalR;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Security.Claims;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Tests
{
    public class AbpAuthenticationHubFilterTests
    {
        [Fact]
        public async Task InvokeMethodAsync_ValidClaimsPrincipal_DoesNotAbort()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "TestUser") }));
            var serviceProviderMock = new Mock<IServiceProvider>();
            var hubCallerContext = new DefaultHubCallerContext(new HubConnectionContext(claimsPrincipal));
            var hubInvocationContext = new HubInvocationContext(hubCallerContext, serviceProviderMock.Object, null, null, null);

            var abpAuthenticationHubFilter = new AbpAuthenticationHubFilter();

            // Act
            await abpAuthenticationHubFilter.InvokeMethodAsync(hubInvocationContext, async (context) => null);

            // Assert
            Assert.False(hubCallerContext.Aborted);
        }

        [Fact]
        public async Task InvokeMethodAsync_InvalidClaimsPrincipal_Aborts()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            var serviceProviderMock = new Mock<IServiceProvider>();
            var hubCallerContext = new DefaultHubCallerContext(new HubConnectionContext(claimsPrincipal));
            var hubInvocationContext = new HubInvocationContext(hubCallerContext, serviceProviderMock.Object, null, null, null);

            var abpAuthenticationHubFilter = new AbpAuthenticationHubFilter();

            // Act
            await abpAuthenticationHubFilter.InvokeMethodAsync(hubInvocationContext, async (context) => null);

            // Assert
            Assert.True(hubCallerContext.Aborted);
        }
    }
}
