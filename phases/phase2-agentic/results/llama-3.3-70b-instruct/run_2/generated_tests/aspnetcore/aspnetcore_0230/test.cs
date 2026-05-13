using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpRequestJsonExtensionsTests
    {
        [Fact]
        public async Task ResolveSerializerOptions_GetServiceCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.RequestServices).Returns(serviceProviderMock.Object);

            var jsonOptionsMock = new Mock<IOptions<JsonOptions>>();
            jsonOptionsMock.Setup(j => j.Value).Returns(new JsonOptions());

            serviceProviderMock.Setup(s => s.GetService(typeof(IOptions<JsonOptions>))).Returns(jsonOptionsMock.Object);

            // Act
            var serializerOptions = HttpRequestJsonExtensions.ResolveSerializerOptions(httpContextMock.Object);

            // Assert
            serviceProviderMock.Verify(s => s.GetService(typeof(IOptions<JsonOptions>)), Times.Once);
        }

        [Fact]
        public async Task ResolveSerializerOptions_DefaultSerializerOptionsReturned_WhenGetServiceReturnsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.RequestServices).Returns(serviceProviderMock.Object);

            serviceProviderMock.Setup(s => s.GetService(typeof(IOptions<JsonOptions>))).Returns(null);

            // Act
            var serializerOptions = HttpRequestJsonExtensions.ResolveSerializerOptions(httpContextMock.Object);

            // Assert
            Assert.Same(JsonOptions.DefaultSerializerOptions, serializerOptions);
        }

        [Fact]
        public async Task ResolveSerializerOptions_DefaultSerializerOptionsReturned_WhenGetServiceReturnsNullAndValueIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.RequestServices).Returns(serviceProviderMock.Object);

            var jsonOptionsMock = new Mock<IOptions<JsonOptions>>();
            jsonOptionsMock.Setup(j => j.Value).Returns((JsonOptions)null);

            serviceProviderMock.Setup(s => s.GetService(typeof(IOptions<JsonOptions>))).Returns(jsonOptionsMock.Object);

            // Act
            var serializerOptions = HttpRequestJsonExtensions.ResolveSerializerOptions(httpContextMock.Object);

            // Assert
            Assert.Same(JsonOptions.DefaultSerializerOptions, serializerOptions);
        }
    }
}
