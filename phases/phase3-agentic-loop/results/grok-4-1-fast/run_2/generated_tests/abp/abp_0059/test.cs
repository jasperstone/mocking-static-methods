using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.AspNetCore.SignalR.Authentication;
using Xunit;

namespace Volo.Abp.AspNetCore.SignalR.Tests.Authentication;

public class AbpAuthenticationHubFilterTests
{
    [Fact]
    public async Task GetRequiredService_AbpSignalROptions_ShouldBeCalled_WhenConditionsMet()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var claimsOptionsMock = new Mock<IOptions<Volo.Abp.Identity.ClaimsAbpClaimsPrincipalFactoryOptions>>();
        var signalROptionsMock = new Mock<IOptions<AbpSignalROptions>>();
        
        claimsOptionsMock.Setup(o => o.Value).Returns(new Volo.Abp.Identity.ClaimsAbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true });
        signalROptionsMock.Setup(o => o.Value).Returns(new AbpSignalROptions());
        
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<Volo.Abp.Identity.ClaimsAbpClaimsPrincipalFactoryOptions>>())
                          .Returns(claimsOptionsMock.Object);
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
                          .Returns(signalROptionsMock.Object);

        var authenticatedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("type", "value") }, "Negotiate"));
        authenticatedPrincipal.Identity!.IsAuthenticated = true;
        
        var hubContextMock = new Mock<HubCallerContext>();
        hubContextMock.Setup(c => c.User).Returns(authenticatedPrincipal);

        var filter = new AbpAuthenticationHubFilter();
        var method = typeof(AbpAuthenticationHubFilter).GetMethod("HandleDynamicClaimsPrincipalAsync", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        
        // Act
        await (Task)method.Invoke(filter, new object?[] { authenticatedPrincipal, serviceProviderMock.Object, hubContextMock.Object, true });

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>(), Times.Once);
    }

    [Fact]
    public async Task GetRequiredService_AbpSignalROptions_ShouldNotBeCalled_WhenDynamicClaimsDisabled()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var claimsOptionsMock = new Mock<IOptions<Volo.Abp.Identity.ClaimsAbpClaimsPrincipalFactoryOptions>>();
        
        claimsOptionsMock.Setup(o => o.Value).Returns(new Volo.Abp.Identity.ClaimsAbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = false });
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<Volo.Abp.Identity.ClaimsAbpClaimsPrincipalFactoryOptions>>())
                          .Returns(claimsOptionsMock.Object);

        var authenticatedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("type", "value") }, "Negotiate"));
        authenticatedPrincipal.Identity!.IsAuthenticated = true;
        
        var hubContextMock = new Mock<HubCallerContext>();
        hubContextMock.Setup(c => c.User).Returns(authenticatedPrincipal);

        var filter = new AbpAuthenticationHubFilter();
        var method = typeof(AbpAuthenticationHubFilter).GetMethod("HandleDynamicClaimsPrincipalAsync", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        
        // Act
        await (Task)method.Invoke(filter, new object?[] { authenticatedPrincipal, serviceProviderMock.Object, hubContextMock.Object, true });

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>(), Times.Never);
    }

    [Fact]
    public async Task GetRequiredService_AbpSignalROptions_ShouldNotBeCalled_WhenUserNotAuthenticated()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var claimsOptionsMock = new Mock<IOptions<Volo.Abp.Identity.ClaimsAbpClaimsPrincipalFactoryOptions>>();
        
        claimsOptionsMock.Setup(o => o.Value).Returns(new Volo.Abp.Identity.ClaimsAbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true });
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<Volo.Abp.Identity.ClaimsAbpClaimsPrincipalFactoryOptions>>())
                          .Returns(claimsOptionsMock.Object);

        var unauthenticatedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { }));
        
        var hubContextMock = new Mock<HubCallerContext>();
        hubContextMock.Setup(c => c.User).Returns(unauthenticatedPrincipal);

        var filter = new AbpAuthenticationHubFilter();
        var method = typeof(AbpAuthenticationHubFilter).GetMethod("HandleDynamicClaimsPrincipalAsync", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        
        // Act
        await (Task)method.Invoke(filter, new object?[] { unauthenticatedPrincipal, serviceProviderMock.Object, hubContextMock.Object, true });

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>(), Times.Never);
    }
}
