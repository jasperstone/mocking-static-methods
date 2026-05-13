using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpRequestJsonExtensionsTests
    {
        [Fact]
        public void ResolveSerializerOptions_WhenServiceProvided_ReturnsServiceOptions()
        {
            // Arrange
            var jsonOptions = new JsonOptions
            {
                SerializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }
            };

            var optionsMock = new Mock<IOptions<JsonOptions>>();
            optionsMock.Setup(o => o.Value).Returns(jsonOptions);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
                .Returns(optionsMock.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProviderMock.Object
            };

            // Act
            var result = HttpRequestJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Equal(JsonNamingPolicy.CamelCase, result.PropertyNamingPolicy);
        }

        [Fact]
        public void ResolveSerializerOptions_WhenServiceNotProvided_ReturnsDefaultOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
                .Returns((IOptions<JsonOptions>)null);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProviderMock.Object
            };

            // Act
            var result = HttpRequestJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Equal(JsonSerializerDefaults.Web, result);
        }
    }
}
