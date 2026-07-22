using Xunit;
using OpenRA.Mods.Common;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using System.Threading;

namespace OpenRA.Mods.Common.Tests
{
    public interface IHttpClientWrapper
    {
        Task<HttpResponseMessage> GetAsync(string requestUri);
    }

    public class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly HttpClient _httpClient;

        public HttpClientWrapper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return _httpClient.GetAsync(requestUri);
        }
    }

    public class WebServicesTests
    {
        [Fact]
        public async Task CheckModVersion_ShouldSetModVersionStatus()
        {
            // Arrange
            var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();
            var response = new HttpResponseMessage
            {
                Content = new StringContent("latest")
            };
            mockHttpClientWrapper
                .Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(response);

            var webServices = new WebServices(mockHttpClientWrapper.Object);

            // Act
            webServices.CheckModVersion();
            await Task.Delay(1000); // Wait for the task to complete

            // Assert
            Assert.Equal(ModVersionStatus.Latest, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_ShouldSetModVersionStatusToOutdated()
        {
            // Arrange
            var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();
            var response = new HttpResponseMessage
            {
                Content = new StringContent("outdated")
            };
            mockHttpClientWrapper
                .Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(response);

            var webServices = new WebServices(mockHttpClientWrapper.Object);

            // Act
            webServices.CheckModVersion();
            await Task.Delay(1000); // Wait for the task to complete

            // Assert
            Assert.Equal(ModVersionStatus.Outdated, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_ShouldSetModVersionStatusToUnknown()
        {
            // Arrange
            var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();
            var response = new HttpResponseMessage
            {
                Content = new StringContent("unknown")
            };
            mockHttpClientWrapper
                .Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(response);

            var webServices = new WebServices(mockHttpClientWrapper.Object);

            // Act
            webServices.CheckModVersion();
            await Task.Delay(1000); // Wait for the task to complete

            // Assert
            Assert.Equal(ModVersionStatus.Unknown, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_ShouldSetModVersionStatusToPlaytestAvailable()
        {
            // Arrange
            var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();
            var response = new HttpResponseMessage
            {
                Content = new StringContent("playtest")
            };
            mockHttpClientWrapper
                .Setup(_ => _.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(response);

            var webServices = new WebServices(mockHttpClientWrapper.Object);

            // Act
            webServices.CheckModVersion();
            await Task.Delay(1000); // Wait for the task to complete

            // Assert
            Assert.Equal(ModVersionStatus.PlaytestAvailable, webServices.ModVersionStatus);
        }
    }
}
