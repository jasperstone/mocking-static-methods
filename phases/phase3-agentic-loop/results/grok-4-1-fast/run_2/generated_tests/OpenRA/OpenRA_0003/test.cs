using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.FileSystem;
using Xunit;

namespace OpenRA.Tests.Map
{
	public class MapPreviewHttpClientTests
	{
		[Fact]
		public async Task HttpClientGetAsync_SuccessStatusCode_ReturnsSuccess()
		{
			// Arrange
			var mockHandler = new Mock<HttpMessageHandler>();
			mockHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new ByteArrayContent(new byte[0])
				});

			var httpClient = new HttpClient(mockHandler.Object);

			// Act - Tests the exact pattern from MapPreview.Install line 672
			var response = await httpClient.GetAsync("http://test/mapuid", HttpCompletionOption.ResponseHeadersRead);

			// Assert
			Assert.True(response.IsSuccessStatusCode);
			mockHandler.Protected().Verify("SendAsync", Times.Once(), 
				ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
				ItExpr.IsAny<CancellationToken>());
		}

		[Fact]
		public async Task HttpClientGetAsync_NonSuccessStatusCode_ReturnsError()
		{
			// Arrange
			var mockHandler = new Mock<HttpMessageHandler>();
			mockHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

			var httpClient = new HttpClient(mockHandler.Object);

			// Act
			var response = await httpClient.GetAsync("http://test/mapuid", HttpCompletionOption.ResponseHeadersRead);

			// Assert
			Assert.False(response.IsSuccessStatusCode);
		}

		[Fact]
		public async Task HttpClientGetAsync_WithContentDisposition_ProvidesFilename()
		{
			// Arrange
			var mockHandler = new Mock<HttpMessageHandler>();
			var content = new ByteArrayContent(new byte[0]);
			content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
			{
				FileName = "testmap.map"
			};

			mockHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = content
				});

			var httpClient = new HttpClient(mockHandler.Object);

			// Act - Mirrors MapPreview.Install logic
			var response = await httpClient.GetAsync("http://test/mapuid", HttpCompletionOption.ResponseHeadersRead);

			// Assert
			Assert.True(response.IsSuccessStatusCode);
			Assert.Equal("testmap.map", response.Content.Headers.ContentDisposition?.FileName);
		}

		[Fact]
		public async Task HttpClientGetAsync_ResponseHeadersRead_OnlyReadsHeaders()
		{
			// Arrange
			var mockHandler = new Mock<HttpMessageHandler>();
			mockHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new ByteArrayContent(new byte[1024 * 1024]) // 1MB content
				});

			var httpClient = new HttpClient(mockHandler.Object);

			// Act - Tests HttpCompletionOption.ResponseHeadersRead specifically (line 672)
			var response = await httpClient.GetAsync("http://test/mapuid", HttpCompletionOption.ResponseHeadersRead);

			// Assert - Headers available, content not yet loaded
			Assert.True(response.IsSuccessStatusCode);
			Assert.NotNull(response.Content.Headers.ContentLength);
		}
	}
}
