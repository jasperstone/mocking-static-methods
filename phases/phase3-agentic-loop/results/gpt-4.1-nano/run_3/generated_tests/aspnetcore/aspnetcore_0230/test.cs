using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace HttpExtensionsTests
{
    public class HttpRequestJsonExtensionsTests
    {
        [Fact]
        public async Task ReadFromJsonAsync_WithServiceProvider_ReturnsExpectedResult()
        {
            // Arrange
            var json = "{\"Name\":\"Test\"}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Body = stream;
            request.ContentType = "application/json";

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions<JsonOptions>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            context.RequestServices = serviceProvider;

            // Act
            var result = await request.ReadFromJsonAsync<TestClass>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
        }

        private class TestClass
        {
            public string Name { get; set; }
        }
    }
}
