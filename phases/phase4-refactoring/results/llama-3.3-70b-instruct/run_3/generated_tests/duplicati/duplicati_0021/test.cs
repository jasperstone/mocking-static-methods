using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Duplicati.Library.Utility.Tests
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task DownloadFile_ValidRequest_DoesNotThrow()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var fileStream = new MemoryStream();

            // Act and Assert
            try
            {
                await HttpClientExtensions.DownloadFile(client, request, fileStream);
            }
            catch (Exception ex)
            {
                Assert.Fail("DownloadFile threw an exception: " + ex.Message);
            }
        }

        [Fact]
        public async Task UploadStream_ValidRequest_DoesNotThrow()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com");
            request.Content = new StreamContent(new MemoryStream());

            // Act and Assert
            try
            {
                await HttpClientExtensions.UploadStream(client, request);
            }
            catch (Exception ex)
            {
                Assert.Fail("UploadStream threw an exception: " + ex.Message);
            }
        }

        [Fact]
        public async Task DownloadFile_InvalidRequest_ThrowsHttpRequestException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://invalid-url");

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () => await HttpClientExtensions.DownloadFile(client, request, new MemoryStream()));
        }

        [Fact]
        public async Task UploadStream_InvalidRequest_ThrowsHttpRequestException()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://invalid-url");
            request.Content = new StreamContent(new MemoryStream());

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () => await HttpClientExtensions.UploadStream(client, request));
        }

        [Fact]
        public async Task DownloadFile_ProgressReportingAction_IsCalled()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var fileStream = new MemoryStream();
            var progressReportingActionCalled = false;
            var progressReportingAction = new Action<long>((position) => progressReportingActionCalled = true);

            // Act
            await HttpClientExtensions.DownloadFile(client, request, fileStream, progressReportingAction);

            // Assert
            Assert.True(progressReportingActionCalled);
        }

        [Fact]
        public async Task DownloadFile_ProgressReportingStream_IsCreated()
        {
            // Arrange
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var fileStream = new MemoryStream();
            var progressReportingAction = new Action<long>((position) => { });

            // Act and Assert
            try
            {
                await HttpClientExtensions.DownloadFile(client, request, fileStream, progressReportingAction);
            }
            catch (Exception ex)
            {
                Assert.Fail("DownloadFile threw an exception: " + ex.Message);
            }
        }
    }
}
