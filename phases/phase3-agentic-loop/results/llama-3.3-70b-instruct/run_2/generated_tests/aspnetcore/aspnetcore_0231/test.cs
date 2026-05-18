using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
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
                    options.SerializerOptions = new JsonSerializerOptions { WriteIndented = true };
                })
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var response = new HttpResponse(new StreamWriter(Console.OpenStandardOutput()));
            response.HttpContext = httpContext;

            var value = new { Foo = "bar" };

            // Act
            await HttpResponseJsonExtensions.WriteAsJsonAsync(response, value);

            // Assert
            var serializerOptions = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);
            Assert.NotNull(serializerOptions);
            Assert.True(serializerOptions.WriteIndented);
        }

        [Fact]
        public async Task WriteAsJsonAsync_UsesDefaultSerializerOptions_WhenNoOptionsInDI()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var response = new HttpResponse(new StreamWriter(Console.OpenStandardOutput()));
            response.HttpContext = httpContext;

            var value = new { Foo = "bar" };

            // Act
            await HttpResponseJsonExtensions.WriteAsJsonAsync(response, value);

            // Assert
            var serializerOptions = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);
            Assert.NotNull(serializerOptions);
            Assert.False(serializerOptions.WriteIndented);
        }

        [Fact]
        public async Task WriteAsJsonAsync_SerializesValueToJson()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions<JsonOptions>()
                .Configure<JsonOptions>(options =>
                {
                    options.SerializerOptions = new JsonSerializerOptions { WriteIndented = true };
                })
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var response = new HttpResponse(new StreamWriter(Console.OpenStandardOutput()));
            response.HttpContext = httpContext;

            var value = new { Foo = "bar" };

            // Act
            await HttpResponseJsonExtensions.WriteAsJsonAsync(response, value);

            // Assert
            var writer = (StreamWriter)response.Body;
            writer.Flush();
            var result = writer.BaseStream.Position;
            writer.BaseStream.Position = 0;
            var reader = new StreamReader(writer.BaseStream);
            var json = await reader.ReadToEndAsync();
            Assert.Contains("{\"Foo\":\"bar\"}", json);
        }
    }
}
