using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;
using Volo.Abp.AspNetCore.SignalR.Authentication;
using Volo.Abp.Security;
using Microsoft.Extensions.DependencyInjection; // Added for GetRequiredService

public class AbpAuthenticationHubFilterTests
{
    [Fact]
    public async Task InvokeMethodAsync_CallsGetRequiredServiceAndCompletesSuccessfully()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var currentPrincipalAccessorMock = new Mock<ICurrentPrincipalAccessor>();
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "TestUser") }));
        var hubContextMock = new Mock<HubCallerContext>();
        hubContextMock.Setup(h => h.User).Returns(claimsPrincipal);

        var invocationContextMock = new Mock<HubInvocationContext>();
        invocationContextMock.Setup(i => i.ServiceProvider).Returns(serviceProviderMock.Object);
        invocationContextMock.Setup(i => i.Context).Returns(hubContextMock.Object);

        serviceProviderMock.Setup(s => s.GetRequiredService<ICurrentPrincipalAccessor>()).Returns(currentPrincipalAccessorMock.Object);

        var filter = new AbpAuthenticationHubFilter();

        // Act
        var result = await filter.InvokeMethodAsync(invocationContextMock.Object, async ctx => await Task.FromResult<object?>(null));

        // Assert
        serviceProviderMock.Verify(s => s.GetRequiredService<ICurrentPrincipalAccessor>(), Times.Once);
        Assert.NotNull(result);
    }
}
