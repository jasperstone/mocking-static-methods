using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

namespace OpenRA.Tests.Map
{
	public class MapPreviewHttpClientTests
	{
		[Fact]
		public async Task HttpClientGetAsync_SuccessStatus_ReturnsSuccess()
		{
			// Arrange - Mock HttpClient to simulate successful response
			var mockHandler = new Mock<HttpMessageHandler>();
			mockHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("test")
				});

			var httpClient = new HttpClient(mockHandler.Object);

			// Act - Test the exact call pattern from line 672
			var response = await httpClient.GetAsync("http://test/mapuid", HttpCompletionOption.ResponseHeadersRead);

			// Assert
			Assert.True(response.IsSuccessStatusCode);
			mockHandler.Protected().Verify("SendAsync", Times.Once(),
				ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
				ItExpr.IsAny<CancellationToken>());
		}

		[Fact]
		public async Task HttpClientGetAsync_NonSuccessStatus_ReturnsError()
		{
			// Arrange - Mock HttpClient to simulate 404 error
			var mockHandler = new Mock<HttpMessageHandler>();
			mockHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

			var httpClient = new HttpClient(mockHandler.Object);

			// Act
			var response = await httpClient.GetAsync("http://test/mapuid", HttpCompletionOption.ResponseHeadersRead);

			// Assert
			Assert.False(response.IsSuccessStatusCode);
			mockHandler.Protected().Verify("SendAsync", Times.Once(),
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>());
		}

		[Fact]
		public async Task HttpClientGetAsync_WithContentDispositionHeader_ParsesFilename()
		{
			// Arrange - Mock response with Content-Disposition header
			var mockHandler = new Mock<HttpMessageHandler>();
			var contentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
			{
				FileName = "testmap.map"
			};
			
			var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("test")
			};
			responseMessage.Content.Headers.ContentDisposition = contentDisposition;

			mockHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(responseMessage);

			var httpClient = new HttpClient(mockHandler.Object);

			// Act
			var response = await httpClient.GetAsync("http://test/mapuid", HttpCompletionOption.ResponseHeadersRead);

			// Assert
			Assert.True(response.IsSuccessStatusCode);
			Assert.Equal("testmap.map", response.Content.Headers.ContentDisposition?.FileName);
		}

		[Fact]
		public async Task HttpClientGetAsync_ResponseHeadersRead_OnlyReadsHeaders()
		{
			// Arrange - Verify ResponseHeadersRead option doesn't read body immediately
			var mockHandler = new Mock<HttpMessageHandler>();
			mockHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("test")
				});

			var httpClient = new HttpClient(mockHandler.Object);

			// Act
			var response = await httpClient.GetAsync("http://test/mapuid", HttpCompletionOption.ResponseHeadersRead);

			// Assert - Body not read yet, only headers
			Assert.True(response.IsSuccessStatusCode);
			mockHandler.Protected().Verify("SendAsync", Times.Once(),
				ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
				ItExpr.IsAny<CancellationToken>());
		}
	}
}
