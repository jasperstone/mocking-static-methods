using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Volo.Abp.Cli.ProjectBuilding;

namespace Volo.Abp.Cli.Tests
{
    // Minimal interface definitions to satisfy the compiler
    public interface IJsonSerializer
    {
        T Deserialize<T>(string json);
    }

    public interface ICancellationTokenProvider
    {
        CancellationToken Token { get; }
    }

    public class CliHttpClientFactory
    {
        public virtual HttpClient CreateClient() => throw new NotImplementedException();
    }

    public class NpmPackageInfoProviderTests
    {
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<HttpClient> _httpClientMock;
        private readonly NpmPackageInfoProvider _provider;

        public NpmPackageInfoProviderTests()
        {
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _httpClientMock = new Mock<HttpClient>();

            _cliHttpClientFactoryMock
                .Setup(f => f.CreateClient())
                .Returns(_httpClientMock.Object);

            _provider = new NpmPackageInfoProvider(
                _jsonSerializerMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cliHttpClientFactoryMock.Object);
        }

        [Fact]
        public async Task GetPackageListAsync_Should_Call_GetAsync_And_Return_List()
        {
            // Arrange
            var responseMessageMock = new Mock<HttpResponseMessage>();
            var contentMock = new Mock<HttpContent>();
            var dummyJson = "[{\"Name\":\"TestPackage\"}]";

            responseMessageMock.Setup(r => r.Content).Returns(contentMock.Object);
            responseMessageMock.Setup(r => r.Dispose()).Verifiable();

            contentMock.Setup(c => c.ReadAsStringAsync()).ReturnsAsync(dummyJson);

            _httpClientMock
                .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessageMock.Object);

            _remoteServiceExceptionHandlerMock
                .Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
                .Returns(Task.CompletedTask);

            _jsonSerializerMock
                .Setup(s => s.Deserialize<List<NpmPackageInfo>>(dummyJson))
                .Returns(new List<NpmPackageInfo> { new NpmPackageInfo { Name = "TestPackage" } });

            // Act
            var result = await _provider.GetPackageListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("TestPackage", result[0].Name);
            _httpClientMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
