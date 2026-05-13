using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Volo.Abp.AspNetCore.SignalR.Authentication;
using Volo.Abp.Security.Claims;

public class AbpAuthenticationHubFilterTests
{
    [Fact]
    public async Task InvokeMethodAsync_CallsGetRequiredServiceAndHandlesDynamicClaimsPrincipalAsync()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var currentPrincipalAccessorMock = new Mock<ICurrentPrincipalAccessor>();
        var claimsPrincipalFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
        var optionsMock = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
        var signalROptionsMock = new Mock<IOptions<AbpSignalROptions>>();

        optionsMock.Setup(o => o.Value.IsDynamicClaimsEnabled).Returns(true);
        signalROptionsMock.Setup(o => o.Value.CheckDynamicClaimsInterval).Returns(TimeSpan.FromMinutes(1));

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>())
            .Returns(currentPrincipalAccessorMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
            .Returns(optionsMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
            .Returns(signalROptionsMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
            .Returns(claimsPrincipalFactoryMock.Object);

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "TestUser") }, "TestAuthType"));
        var hubContextMock = new Mock<HubCallerContext>();
        hubContextMock.Setup(h => h.User).Returns(claimsPrincipal);

        var invocationContext = new HubInvocationContext
        {
            Context = hubContextMock.Object,
            ServiceProvider = serviceProviderMock.Object
        };

        var filter = new AbpAuthenticationHubFilter();

        // Act
        await filter.InvokeMethodAsync(invocationContext, async ctx => await Task.FromResult<object?>(null));

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>(), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>(), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>(), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>(), Times.Once);
        claimsPrincipalFactoryMock.Verify(f => f.CreateDynamicAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
    }
}
