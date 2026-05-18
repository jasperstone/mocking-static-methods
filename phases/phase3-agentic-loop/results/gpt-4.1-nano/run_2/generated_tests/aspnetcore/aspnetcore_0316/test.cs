using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace IdentityApiTests
{
    public class IdentityApiEndpointRouteBuilderExtensionsTests
    {
        [Fact]
        public void MapIdentityApi_CallsGetRequiredService_ForTimeProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var timeProviderMock = new Mock<TimeProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<BearerTokenOptions>>();
            var emailSenderMock = new Mock<IEmailSender<object>>();
            var linkGeneratorMock = new Mock<LinkGenerator>();
            var userManagerMock = new Mock<UserManager<object>>();
            var signInManagerMock = new Mock<SignInManager<object>>();

            services.AddSingleton(timeProviderMock.Object);
            services.AddSingleton(optionsMonitorMock.Object);
            services.AddSingleton(emailSenderMock.Object);
            services.AddSingleton(linkGeneratorMock.Object);
            services.AddTransient(_ => userManagerMock.Object);
            services.AddTransient(_ => signInManagerMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<TimeProvider>())
                .Returns(() => timeProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>())
                .Returns(() => optionsMonitorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IEmailSender<object>>())
                .Returns(() => emailSenderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<LinkGenerator>())
                .Returns(() => linkGeneratorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<UserManager<object>>())
                .Returns(() => userManagerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<SignInManager<object>>())
                .Returns(() => signInManagerMock.Object);

            // Act
            var endpointsMock = new Mock<IEndpointRouteBuilder>();
            endpointsMock.Setup(e => e.ServiceProvider).Returns(serviceProviderMock.Object);
            var resultBuilder = IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<object>(endpointsMock.Object);

            // Assert
            // Verify that GetRequiredService<TimeProvider> was called
            serviceProviderMock.Verify(sp => sp.GetRequiredService<TimeProvider>(), Times.Once);
            // Verify that GetRequiredService<IOptionsMonitor<BearerTokenOptions>> was called
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>(), Times.Once);
            // Verify that GetRequiredService<IEmailSender<object>> was called
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IEmailSender<object>>(), Times.Once);
            // Verify that GetRequiredService<LinkGenerator> was called
            serviceProviderMock.Verify(sp => sp.GetRequiredService<LinkGenerator>(), Times.Once);
        }
    }
}
