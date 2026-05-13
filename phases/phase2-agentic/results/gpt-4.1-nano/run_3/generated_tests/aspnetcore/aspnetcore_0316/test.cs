using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace IdentityApiTests
{
    public class IdentityApiEndpointRouteBuilderExtensionsTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IEndpointRouteBuilder> _endpointsMock;
        private readonly Mock<IServiceScope> _scopeMock;
        private readonly Mock<TimeProvider> _timeProviderMock;
        private readonly Mock<IOptionsMonitor<BearerTokenOptions>> _bearerTokenOptionsMock;
        private readonly Mock<IEmailSender<object>> _emailSenderMock;
        private readonly Mock<LinkGenerator> _linkGeneratorMock;
        private readonly Mock<UserManager<object>> _userManagerMock;
        private readonly Mock<SignInManager<object>> _signInManagerMock;

        public IdentityApiEndpointRouteBuilderExtensionsTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _endpointsMock = new Mock<IEndpointRouteBuilder>();
            _scopeMock = new Mock<IServiceScope>();
            _timeProviderMock = new Mock<TimeProvider>();
            _bearerTokenOptionsMock = new Mock<IOptionsMonitor<BearerTokenOptions>>();
            _emailSenderMock = new Mock<IEmailSender<object>>();
            _linkGeneratorMock = new Mock<LinkGenerator>();
            _userManagerMock = new Mock<UserManager<object>>();
            _signInManagerMock = new Mock<SignInManager<object>>();

            // Setup ServiceProvider to return mocks
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<TimeProvider>())
                .Returns(_timeProviderMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>())
                .Returns(_bearerTokenOptionsMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IEmailSender<object>>())
                .Returns(_emailSenderMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<LinkGenerator>())
                .Returns(_linkGeneratorMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<UserManager<object>>())
                .Returns(_userManagerMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<SignInManager<object>>())
                .Returns(_signInManagerMock.Object);

            // Setup EndpointRouteBuilder to return a mock route group
            var routeGroupMock = new Mock<IEndpointRouteBuilder>();
            routeGroupMock.Setup(r => r.MapPost(It.IsAny<string>(), It.IsAny<Delegate>()))
                .Returns(Mock.Of<IEndpointConventionBuilder>());
            _endpointsMock.Setup(e => e.MapGroup(It.IsAny<string>()))
                .Returns(routeGroupMock.Object);
        }

        [Fact]
        public void MapIdentityApi_CallsGetRequiredService_ForTimeProvider()
        {
            // Arrange
            var endpoints = _endpointsMock.Object;

            // Act
            var result = IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<object>(endpoints);

            // Assert
            _serviceProviderMock.Verify(sp => sp.GetRequiredService<TimeProvider>(), Times.Once);
        }
    }
}
