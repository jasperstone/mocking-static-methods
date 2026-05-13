using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpResponseJsonExtensionsTests
    {
        [Fact]
        public async Task WriteAsJsonAsync_UsesSerializerOptionsFromRequestServices()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            var jsonOptions = new JsonOptions();
            jsonOptions.SerializerOptions.PropertyNamingPolicy = null;

            var expected = JsonSerializer.Serialize(new SamplePayload { ValueName = "Test" }, jsonOptions.SerializerOptions);

            var provider = new TrackingServiceProvider(type =>
            {
                if (type == typeof(IOptions<JsonOptions>))
                {
                    return Options.Create(jsonOptions);
                }

                return null;
            });

            context.RequestServices = provider;

            var payload = new SamplePayload { ValueName = "Test" };

            // Act
            await context.Response.WriteAsJsonAsync(payload);
            await context.Response.BodyWriter.FlushAsync();

            // Assert
            var actual = Encoding.UTF8.GetString(responseStream.ToArray());
            Assert.Equal(expected, actual);
            Assert.Equal(1, provider.CallCount);
        }

        [Fact]
        public async Task WriteAsJsonAsync_FallsBackToDefaultSerializerOptionsWhenServiceNotResolved()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var responseStream = new MemoryStream();
            context.Response.Body = responseStream;

            var provider = new TrackingServiceProvider(_ => null);
            context.RequestServices = provider;

            var payload = new SamplePayload { ValueName = "Test" };
            var expected = JsonSerializer.Serialize(payload, JsonOptions.DefaultSerializerOptions);

            // Act
            await context.Response.WriteAsJsonAsync(payload);
            await context.Response.BodyWriter.FlushAsync();

            // Assert
            var actual = Encoding.UTF8.GetString(responseStream.ToArray());
            Assert.Equal(expected, actual);
            Assert.Equal(1, provider.CallCount);
        }

        private sealed class SamplePayload
        {
            public string ValueName { get; set; } = string.Empty;
        }

        private sealed class TrackingServiceProvider : IServiceProvider
        {
            private readonly Func<Type, object?> _resolver;

            public TrackingServiceProvider(Func<Type, object?> resolver)
            {
                _resolver = resolver;
            }

            public int CallCount { get; private set; }

            public object? GetService(Type serviceType)
            {
                CallCount++;
                return _resolver(serviceType);
            }
        }
    }
}
