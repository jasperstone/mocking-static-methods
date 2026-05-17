using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library.Utility;
using Xunit;

namespace Duplicati.Library.Utility.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task UploadStream_SuccessfulRequest_ReturnsResponse()
        {
            // Arrange
            var mockHandler = new HttpMessageHandlerMock();
            mockHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.OK));
            using var httpClient = new HttpClient(mockHandler);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com");

            // Act
            var result = await httpClient.UploadStream(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            mockHandler.VerifySendAsyncCalledOnce(request, HttpCompletionOption.ResponseContentRead);
        }

        [Fact]
        public async Task UploadStream_WithCancellationToken_CancelsRequest()
        {
            // Arrange
            var mockHandler = new HttpMessageHandlerMock();
            var cts = new CancellationTokenSource();
            mockHandler.DelayResponse(TimeSpan.FromMilliseconds(100), cts.Token);
            using var httpClient = new HttpClient(mockHandler);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com");

            // Act & Assert
            var task = httpClient.UploadStream(request, cts.Token);
            cts.Cancel();
            await Assert.ThrowsAnyAsync<TaskCanceledException>(() => task);
            mockHandler.VerifySendAsyncCalledOnce(request, HttpCompletionOption.ResponseContentRead);
        }

        [Fact]
        public async Task DownloadFile_Stream_NoProgress_SuccessfullyDownloads()
        {
            // Arrange
            var expectedContent = "Hello, World!";
            var mockHandler = new HttpMessageHandlerMock();
            mockHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(expectedContent)
            });
            using var httpClient = new HttpClient(mockHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var memoryStream = new MemoryStream();

            // Act
            await httpClient.DownloadFile(request, memoryStream);

            // Assert
            memoryStream.Position = 0;
            var result = new StreamReader(memoryStream).ReadToEnd();
            Assert.Equal(expectedContent, result);
            mockHandler.VerifySendAsyncCalledOnce(request, HttpCompletionOption.ResponseHeadersRead);
        }

        [Fact]
        public async Task DownloadFile_Stream_WithProgress_ReportsProgress()
        {
            // Arrange
            var progressCalled = false;
            void ProgressCallback(long bytes) => progressCalled = true;

            var mockHandler = new HttpMessageHandlerMock();
            mockHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Test content")
            });
            using var httpClient = new HttpClient(mockHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var memoryStream = new MemoryStream();

            // Act
            await httpClient.DownloadFile(request, memoryStream, ProgressCallback);

            // Assert
            Assert.True(progressCalled);
            mockHandler.VerifySendAsyncCalledOnce(request, HttpCompletionOption.ResponseHeadersRead);
        }

        [Fact]
        public async Task DownloadFile_Filename_SuccessfullyDownloads()
        {
            // Arrange
            var expectedContent = "Hello, World!";
            var mockHandler = new HttpMessageHandlerMock();
            mockHandler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(expectedContent)
            });
            using var httpClient = new HttpClient(mockHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                await httpClient.DownloadFile(request, tempFile);

                // Assert
                var result = await File.ReadAllTextAsync(tempFile);
                Assert.Equal(expectedContent, result);
                mockHandler.VerifySendAsyncCalledOnce(request, HttpCompletionOption.ResponseHeadersRead);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        private class HttpMessageHandlerMock : DelegatingHandler
        {
            private readonly Queue<HttpResponseMessage> _responses = new();
            private TaskCompletionSource<HttpResponseMessage>? _delayedTcs;
            private readonly List<(HttpRequestMessage request, HttpCompletionOption option, CancellationToken token)> _sendAsyncCalls = new();

            public HttpMessageHandlerMock()
            {
                InnerHandler = new TestHttpMessageHandler();
            }

            public void EnqueueResponse(HttpResponseMessage response) => _responses.Enqueue(response);

            public void DelayResponse(TimeSpan delay, CancellationToken ct)
            {
                _delayedTcs = new TaskCompletionSource<HttpResponseMessage>();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(delay, ct);
                        _delayedTcs!.SetResult(new HttpResponseMessage(HttpStatusCode.OK));
                    }
                    catch (OperationCanceledException)
                    {
                        _delayedTcs!.SetCanceled();
                    }
                });
            }

            public void VerifySendAsyncCalledOnce(HttpRequestMessage expectedRequest, HttpCompletionOption expectedOption)
            {
                Assert.Single(_sendAsyncCalls);
                var call = _sendAsyncCalls[0];
                Assert.Equal(expectedRequest.Method, call.request.Method);
                Assert.Equal(expectedRequest.RequestUri, call.request.RequestUri);
                Assert.Equal(expectedOption, call.option);
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _sendAsyncCalls.Add((request, HttpCompletionOption.ResponseHeadersRead, cancellationToken));

                if (_delayedTcs != null)
                {
                    var response = await _delayedTcs.Task;
                    _delayedTcs = null;
                    return response;
                }

                if (_responses.Count == 0)
                    throw new InvalidOperationException("No response enqueued");

                var queuedResponse = _responses.Dequeue();
                return queuedResponse;
            }
        }

        private class TestHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => 
                throw new NotImplementedException();
        }
    }
}
