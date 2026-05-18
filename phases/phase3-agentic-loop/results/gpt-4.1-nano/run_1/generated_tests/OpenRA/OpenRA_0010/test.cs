using Xunit;
using Moq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Widgets;
using System;

namespace OpenRA.Tests
{
    public class DownloadPackageLogicTests
    {
        [Fact]
        public async Task DownloadUrl_CallsHttpClientGetAsync()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Setup the mock handler to return a successful response
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("response content")
            };
            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) => await Task.FromResult(responseMessage));

            // Mock the static HttpClientFactory.Create() method
            // Assuming we can do this via a test hook or dependency injection
            // For illustration, suppose we have a way to override it:
            HttpClientFactory.SetCreateMethod(() => mockHttpClient);

            // Create dummy widget and dependencies
            var widget = new Mock<Widget>();
            var modData = new Mock<ModData>();
            var download = new ModContent.ModDownload
            {
                URL = "http://example.com/file.zip",
                SHA1 = null,
                Type = "TestType",
                Extract = new Dictionary<string, string> { { "path/to/file", "packageEntry" } }
            };
            var onSuccess = new Action(() => { });

            var logic = new DownloadPackageLogic(widget.Object, modData.Object, download, onSuccess);

            // Act
            // Call the method that triggers the download
            // Since the constructor calls ShowDownloadDialog, which calls DownloadUrl
            // We need to invoke the DownloadUrl method directly or simulate the flow
            // For simplicity, assume we can call DownloadUrl directly
            // But DownloadUrl is private, so we might need to invoke via reflection or test setup
            // Alternatively, we can simulate the flow by calling the private method via reflection
            var method = typeof(DownloadPackageLogic).GetMethod("DownloadUrl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method.Invoke(logic, new object[] { "http://example.com/file.zip" });

            // Assert
            mockHttpMessageHandler.Verify(m => m.Send(It.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == "http://example.com/file.zip")), Times.Once);
        }
    }
}
