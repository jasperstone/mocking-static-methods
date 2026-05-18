using System;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpResponseJsonExtensionsTests
    {
        [Fact]
        public async Task WriteAsJsonAsync_ShouldUseDefaultSerializerOptions_WhenOptionsNotProvided()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<JsonOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new JsonOptions());
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(optionsMock.Object);
            httpContext.RequestServices = serviceProviderMock.Object;

            var response = httpContext.Response;
            var value = new { Name = "Test" };
            var contentType = "application/json";

            // Act
            await response.WriteAsJsonAsync(value, contentType: contentType);

            // Assert
            Assert.Equal(contentType, response.ContentType);
        }
    }
}
