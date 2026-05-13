using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Http.Extensions.Tests
{
    public class HttpRequestJsonExtensionsTests
    {
        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("application/xml", false)]
        [InlineData("text/json", false)]
        [InlineData("text/json; charset=utf-8", false)]
        [InlineData("application/json", true)]
        [InlineData("application/json; charset=utf-8", true)]
        [InlineData("application/ld+json", true)]
        [InlineData("APPLICATION/JSON", true)]
        [InlineData("APPLICATION/JSON; CHARSET=UTF-8", true)]
        [InlineData("APPLICATION/LD+JSON", true)]
        public void HasJsonContentType(string? contentType, bool hasJsonContentType)
        {
            var request = new DefaultHttpContext().Request;
            request.ContentType = contentType;

            Assert.Equal(hasJsonContentType, request.HasJsonContentType());
        }

        [Fact]
        public async Task ReadFromJsonAsyncGeneric_NonJsonContentType_ThrowError()
        {
            var context = new DefaultHttpContext();
            context.Request.ContentType = "text/json";

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await context.Request.ReadFromJsonAsync<int>());

            var expectedMessage = $"Unable to read the request as JSON because the request content type 'text/json' is not a known JSON content type.";
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task ReadFromJsonAsyncGeneric_NoBodyContent_ThrowError()
        {
            var context = new DefaultHttpContext();
            context.Request.ContentType = "application/json";

            var ex = await Assert.ThrowsAsync<JsonException>(async () => await context.Request.ReadFromJsonAsync<int>());

            Assert.StartsWith("The input does not contain any JSON tokens", ex.Message);
        }

        [Fact]
        public async Task ReadFromJsonAsyncGeneric_ValidBodyContent_ReturnValue()
        {
            var context = new DefaultHttpContext();
            context.Request.ContentType = "application/json";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("1"));

            var result = await context.Request.ReadFromJsonAsync<int>();

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task ReadFromJsonAsyncGeneric_WithOptions_ReturnValue()
        {
            var context = new DefaultHttpContext();
            context.Request.ContentType = "application/json";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("[1,2,]"));

            var options = new JsonSerializerOptions { AllowTrailingCommas = true };

            var result = await context.Request.ReadFromJsonAsync<List<int>>(options);

            Assert.NotNull(result);
            Assert.Collection(result, i => Assert.Equal(1, i), i => Assert.Equal(2, i));
        }

        [Fact]
        public async Task ReadFromJsonAsyncGeneric_Utf8Encoding_ReturnValue()
        {
            var context = new DefaultHttpContext();
            context.Request.ContentType = "application/json; charset=utf-8";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("[1,2]"));

            var result = await context.Request.ReadFromJsonAsync<List<int>>();

            Assert.NotNull(result);
            Assert.Collection(result, i => Assert.Equal(1, i), i => Assert.Equal(2, i));
        }

        [Fact]
        public async Task ReadFromJsonAsyncGeneric_Utf16Encoding_ReturnValue()
        {
            var context = new DefaultHttpContext();
            context.Request.ContentType = "application/json; charset=utf-16";
            context.Request.Body = new MemoryStream(Encoding.Unicode.GetBytes(@"{""name"": ""激光這兩個字是甚麼意思""}"));

            var result = await context.Request.ReadFromJsonAsync<Dictionary<string, string>>();

            Assert.Equal("激光這兩個字是甚麼意思", result!["name"]);
        }

        [Fact]
        public async Task ReadFromJsonAsyncGeneric_WithCancellationToken_CancellationRaised()
        {
            var context = new DefaultHttpContext();
            context.Request.ContentType = "application/json";
            context.Request.Body = new TestStream();

            var cts = new CancellationTokenSource();

            var readTask = context.Request.ReadFromJsonAsync<List<int>>(cts.Token);
            Assert.False(readTask.IsCompleted);

            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await readTask);
        }

        [Fact]
        public async Task ReadFromJsonAsyncGeneric_InvalidEncoding_ThrowError()
        {
            var context = new DefaultHttpContext();
            context.Request.ContentType = "application/json; charset=invalid";

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await context.Request.ReadFromJsonAsync<object>());

            Assert.Equal("Unable to read the request as JSON because the request content type charset 'invalid' is not a known encoding.", ex.Message);
        }

        [Fact]
        public async Task ReadFromJsonAsync_ValidBodyContent_ReturnValue()
        {
            var context = new DefaultHttpContext();
            context.Request.ContentType = "application/json";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("1"));

            var result = (int?)await context.Request.ReadFromJsonAsync(typeof(int));

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task ReadFromJsonAsync_Utf16Encoding_ReturnValue()
        {
            var context = new DefaultHttpContext();
            context.Request.ContentType = "application/json; charset=utf-16";
            context.Request.Body = new MemoryStream(Encoding.Unicode.GetBytes(@"{""name"": ""激光這兩個字是甚麼意思""}"));

            var result = (Dictionary<string, string>?)await context.Request.ReadFromJsonAsync(typeof(Dictionary<string, string>));

            Assert.Equal("激光這兩個字是甚麼意思", result!["name"]);
        }

        [Fact]
        public async Task ReadFromJsonAsync_InvalidEncoding_ThrowError()
        {
            var context = new DefaultHttpContext();
            context.Request.ContentType = "application/json; charset=invalid";

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await context.Request.ReadFromJsonAsync(typeof(object)));

            Assert.Equal("Unable to read the request as JSON because the request content type charset 'invalid' is not a known encoding.", ex.Message);
        }

        [Fact]
        public async Task ReadFromJsonAsync_WithOptions_ReturnValue()
        {
            var context = new DefaultHttpContext();
            context.Request.ContentType = "application/json";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("[1,2,]"));

            var options = new JsonSerializerOptions { AllowTrailingCommas = true };

            var result = (List<int>?)await context.Request.ReadFromJsonAsync(typeof(List<int>), options);

            Assert.NotNull(result);
            Assert.Collection(result, i => Assert.Equal(1, i), i => Assert.Equal(2, i));
        }
    }

    // Helper class for testing cancellation
    public class TestStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => 0;
        public override long Position { get; set; }

        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] buffer, int offset, int count) => 0;
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
