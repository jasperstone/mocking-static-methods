using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Http;

namespace Volo.Abp.Cli.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private readonly Mock<CliHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _exceptionHandlerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<CliVersionService> _cliVersionServiceMock;
        private readonly AbpIoSourceCodeStore _store;

        public AbpIoSourceCodeStoreTests()
        {
            _httpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _exceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _cliVersionServiceMock = new Mock<CliVersionService>();

            var options = Options.Create(new AbpCliOptions());
            _store = new AbpIoSourceCodeStore(
                options,
                _jsonSerializerMock.Object,
                _exceptionHandlerMock.Object,
                _cancellationTokenProviderMock.Object,
                _httpClientFactoryMock.Object,
                _cliVersionServiceMock.Object);
        }

        [Fact]
        public async Task IsVersionExists_Should_Call_GetAsync_And_Return_Result()
        {
            // Arrange
            var url = "https://example.com/api/download/all-versions?includePreReleases=true";
            var mockResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"FrameworkAndCommercialVersions\": [{\"Name\": \"1.0.0\"}], \"LeptonXVersions\": [{\"Name\": \"2.0.0\"}]}")
            };

            var mockHttpClient = new Mock<HttpClient>();
            var responseTask = Task.FromResult(mockResponse);

            var clientMock = new Mock<IHttpClientFactory>();
            clientMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(new FakeHttpMessageHandler(responseTask)));

            _httpClientFactoryMock.Setup(f => f.CreateClient()).Returns(new HttpClient(new FakeHttpMessageHandler(responseTask)));

            // Act
            var result = await _store.GetType().GetMethod("IsVersionExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .InvokeAsync<bool>(_store, new object[] { "TestTemplate", "1.0.0" });

            // Assert
            Assert.True(result);
        }

        // Helper class to mock HttpMessageHandler
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Task<HttpResponseMessage> _responseTask;

            public FakeHttpMessageHandler(Task<HttpResponseMessage> responseTask)
            {
                _responseTask = responseTask;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _responseTask;
            }
        }
    }
}
