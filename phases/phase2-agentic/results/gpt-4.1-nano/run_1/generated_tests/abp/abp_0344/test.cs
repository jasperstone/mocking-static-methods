using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Volo.Abp.Json;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;

namespace Volo.Abp.Cli.Tests
{
    public class NpmPackageInfoProviderTests
    {
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
        private readonly Mock<HttpClient> _httpClientMock;

        public NpmPackageInfoProviderTests()
        {
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            _httpClientMock = new Mock<HttpClient>();
        }

        [Fact]
        public async Task GetPackageListAsync_Should_Call_GetAsync_And_Return_List()
        {
            // Arrange
            var expectedList = new List<NpmPackageInfo>
            {
                new NpmPackageInfo { Name = "TestPackage" }
            };
            var responseMessageMock = new Mock<HttpResponseMessage>();
            var contentMock = new Mock<HttpContent>();
            var responseContent = "[{\"Name\":\"TestPackage\"}]";

            responseMessageMock.Setup(r => r.Content).Returns(contentMock.Object);
            contentMock.Setup(c => c.ReadAsStringAsync()).ReturnsAsync(responseContent);
            responseMessageMock.Setup(r => r.Dispose());

            var clientMock = new Mock<HttpClient>();
            clientMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseMessageMock.Object);

            _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(clientMock.Object);

            var provider = new NpmPackageInfoProvider(
                _jsonSerializerMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object,
                _cliHttpClientFactoryMock.Object
            );

            // Act
            var result = await provider.GetPackageListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("TestPackage", result[0].Name);
        }
    }
}
