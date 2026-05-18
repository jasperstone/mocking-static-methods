using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpResponseJsonExtensionsTests
    {
        [Fact]
        public async Task WriteAsJsonAsync_ResolvesSerializerOptionsFromDI()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions<JsonOptions>()
                .Configure<JsonOptions>(options =>
                {
                    options.SerializerOptions.PropertyNameCaseInsensitive = true;
                })
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var response = new DefaultHttpResponse(httpContext);
            var value = new { Foo = "bar" };

            // Act
            await response.WriteAsJsonAsync(value);

            // Assert
            var writer = new StringWriter();
            await response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("{\"Foo\":\"bar\"}"));
            response.BodyWriter.Complete();
            var body = await new StreamReader(response.Body).ReadToEndAsync();
            Assert.Equal("{\"Foo\":\"bar\"}", body);
        }

        [Fact]
        public async Task WriteAsJsonAsync_UsesDefaultSerializerOptionsWhenNotResolvedFromDI()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();

            var response = new DefaultHttpResponse(httpContext);
            var value = new { Foo = "bar" };

            // Act
            await response.WriteAsJsonAsync(value);

            // Assert
            var writer = new StringWriter();
            await response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("{\"foo\":\"bar\"}"));
            response.BodyWriter.Complete();
            var body = await new StreamReader(response.Body).ReadToEndAsync();
            Assert.Equal("{\"foo\":\"bar\"}", body);
        }

        [Fact]
        public async Task WriteAsJsonAsync_SerializesValueToJson()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();

            var response = new DefaultHttpResponse(httpContext);
            var value = new { Foo = "bar" };

            // Act
            await response.WriteAsJsonAsync(value);

            // Assert
            var writer = new StringWriter();
            await response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("{\"foo\":\"bar\"}"));
            response.BodyWriter.Complete();
            var body = await new StreamReader(response.Body).ReadToEndAsync();
            Assert.Equal("{\"foo\":\"bar\"}", body);
        }

        [Fact]
        public async Task WriteAsJsonAsync_SerializesValueToJson_WithContentType()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();

            var response = new DefaultHttpResponse(httpContext);
            var value = new { Foo = "bar" };

            // Act
            await response.WriteAsJsonAsync(value, contentType: "application/json");

            // Assert
            Assert.Equal("application/json", response.ContentType);
            var writer = new StringWriter();
            await response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("{\"foo\":\"bar\"}"));
            response.BodyWriter.Complete();
            var body = await new StreamReader(response.Body).ReadToEndAsync();
            Assert.Equal("{\"foo\":\"bar\"}", body);
        }

        [Fact]
        public async Task WriteAsJsonAsync_SerializesValueToJson_WithSerializerOptions()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();

            var response = new DefaultHttpResponse(httpContext);
            var value = new { Foo = "bar" };
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Act
            await response.WriteAsJsonAsync(value, options: options);

            // Assert
            var writer = new StringWriter();
            await response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("{\"Foo\":\"bar\"}"));
            response.BodyWriter.Complete();
            var body = await new StreamReader(response.Body).ReadToEndAsync();
            Assert.Equal("{\"Foo\":\"bar\"}", body);
        }

        [Fact]
        public async Task WriteAsJsonAsync_SerializesValueToJson_WithSerializerOptionsAndContentType()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();

            var response = new DefaultHttpResponse(httpContext);
            var value = new { Foo = "bar" };
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Act
            await response.WriteAsJsonAsync(value, options: options, contentType: "application/json");

            // Assert
            Assert.Equal("application/json", response.ContentType);
            var writer = new StringWriter();
            await response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("{\"Foo\":\"bar\"}"));
            response.BodyWriter.Complete();
            var body = await new StreamReader(response.Body).ReadToEndAsync();
            Assert.Equal("{\"Foo\":\"bar\"}", body);
        }
    }
}
