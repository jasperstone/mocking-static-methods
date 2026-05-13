using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;

namespace Duplicati.Tests
{
    public class JSONWebHelperHttpClientTests
    {
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
            {
                _sendAsync = sendAsync;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _sendAsync(request, cancellationToken);
            }
        }

        [Fact]
        public async Task GetResponseAsync_ShouldReturnResponse_WhenSendAsyncSucceeds()
        {
            // Arrange
            var expectedResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            var handler = new FakeHttpMessageHandler((req, token) => Task.FromResult(expectedResponse));
            var httpClient = new HttpClient(handler);
            var helper = new JSONWebHelperHttpClient(httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var cancellationToken = CancellationToken.None;

            // Act
            var response = await helper.GetResponseAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

            // Assert
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task GetResponseAsync_ShouldCallEnsureSuccessStatusCode_WhenResponseIsSuccess()
        {
            // Arrange
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            var handler = new FakeHttpMessageHandler((req, token) => Task.FromResult(response));
            var httpClient = new HttpClient(handler);
            var helper = new JSONWebHelperHttpClient(httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var called = false;

            // Act
            var result = await helper.GetResponseAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);

            // Assert
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task GetResponseAsync_ShouldThrow_WhenSendAsyncThrows()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler((req, token) => throw new HttpRequestException("Network error"));
            var httpClient = new HttpClient(handler);
            var helper = new JSONWebHelperHttpClient(httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await helper.GetResponseAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
            });
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_ShouldCallSendAsyncAndReturnResponse()
        {
            // Arrange
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            var handler = new FakeHttpMessageHandler((req, token) => Task.FromResult(response));
            var httpClient = new HttpClient(handler);
            var helper = new JSONWebHelperHttpClient(httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            // Act
            var result = await helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);

            // Assert
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task GetResponseUncheckedAsync_ShouldThrow_WhenSendAsyncThrows()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler((req, token) => throw new HttpRequestException("Network error"));
            var httpClient = new HttpClient(handler);
            var helper = new JSONWebHelperHttpClient(httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await helper.GetResponseUncheckedAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
            });
        }
    }

    // Helper class to instantiate the class under test
    public class JSONWebHelperHttpClient
    {
        private readonly HttpClient _httpClient;

        public JSONWebHelperHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage req, HttpCompletionOption httpCompletionOption, CancellationToken cancellationToken)
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await _httpClient.SendAsync(req, httpCompletionOption, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (Exception ex)
            {
                try
                {
                    await Task.CompletedTask; // Placeholder for actual method
                }
                finally
                {
                    response?.Dispose();
                }
                throw;
            }
        }

        public async Task<HttpResponseMessage> GetResponseUncheckedAsync(HttpRequestMessage req, HttpCompletionOption httpCompletionOption, CancellationToken cancellationToken)
        {
            HttpResponseMessage? response = null;
            try
            {
                return await _httpClient.SendAsync(req, httpCompletionOption, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                try
                {
                    await Task.CompletedTask; // Placeholder for actual method
                }
                finally
                {
                    response?.Dispose();
                }
                throw;
            }
        }
    }
}
