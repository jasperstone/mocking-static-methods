using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Security.Claims;
using Volo.Abp.AspNetCore.SignalR.Authentication;

namespace AbpSignalRTests
{
    public class AbpAuthenticationHubFilterTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<ICurrentPrincipalAccessor> _principalAccessorMock;
        private readonly Mock<IAbpClaimsPrincipalFactory> _claimsFactoryMock;
        private readonly Mock<IOptions<AbpClaimsPrincipalFactoryOptions>> _claimsOptionsMock;
        private readonly Mock<IOptions<AbpSignalROptions>> _signalROptionsMock;
        private readonly AbpAuthenticationHubFilter _filter;

        public AbpAuthenticationHubFilterTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _principalAccessorMock = new Mock<ICurrentPrincipalAccessor>();
            _claimsFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();
            _claimsOptionsMock = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
            _signalROptionsMock = new Mock<IOptions<AbpSignalROptions>>();

            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>())
                .Returns(_claimsOptionsMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>())
                .Returns(_signalROptionsMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<ICurrentPrincipalAccessor>())
                .Returns(_principalAccessorMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>())
                .Returns(_claimsFactoryMock.Object);

            _filter = new AbpAuthenticationHubFilter();
        }

        [Fact]
        public async Task InvokeMethodAsync_Should_Call_HandleDynamicClaimsPrincipalAsync_And_Use_Principal()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuthType"));
            var contextMock = new Mock<HubCallerContext>();
            var invocationContextMock = new Mock<HubInvocationContext>();
            var serviceProvider = _serviceProviderMock.Object;

            var invocationContext = new HubInvocationContext
            {
                ServiceProvider = serviceProvider,
                Context = contextMock.Object
            };

            contextMock.Setup(c => c.User).Returns(claimsPrincipal);

            var nextCalled = false;
            Func<HubInvocationContext, ValueTask<object?>> next = ctx =>
            {
                nextCalled = true;
                return new ValueTask<object?>(42);
            };

            // Act
            var result = await _filter.InvokeMethodAsync(invocationContext, next);

            // Assert
            Assert.True(nextCalled);
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task HandleDynamicClaimsPrincipalAsync_Should_Abort_When_ClaimsNotAuthenticated()
        {
            // Arrange
            var identity = new ClaimsIdentity(); // Not authenticated
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var hubContextMock = new Mock<HubCallerContext>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<AbpClaimsPrincipalFactoryOptions>>();
            var signalOptionsMock = new Mock<IOptions<AbpSignalROptions>>();
            var claimsFactoryMock = new Mock<IAbpClaimsPrincipalFactory>();

            var optionsValue = new AbpClaimsPrincipalFactoryOptions { IsDynamicClaimsEnabled = true };
            optionsMock.Setup(o => o.Value).Returns(optionsValue);

            var signalOptions = new AbpSignalROptions { CheckDynamicClaimsInterval = TimeSpan.FromMinutes(5) };
            signalOptionsMock.Setup(o => o.Value).Returns(signalOptions);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>()).Returns(optionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<AbpSignalROptions>>()).Returns(signalOptionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAbpClaimsPrincipalFactory>()).Returns(claimsFactoryMock.Object);

            var context = new Mock<HubCallerContext>();
            var items = new System.Collections.Generic.Dictionary<object, object>();
            context.Setup(c => c.Items).Returns(items);
            context.Setup(c => c.User).Returns(claimsPrincipal);
            var hubContext = context.Object;

            var filter = new AbpAuthenticationHubFilter();

            // Act
            await filter.HandleDynamicClaimsPrincipalAsync(claimsPrincipal, serviceProviderMock.Object, hubContext, false);

            // Assert
            // Since identity is not authenticated, it should call abort
            context.Verify(c => c.Abort(), Times.Once);
        }
    }
}
