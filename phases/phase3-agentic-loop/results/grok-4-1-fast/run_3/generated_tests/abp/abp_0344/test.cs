using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding;

public class NpmPackageInfoProviderTests
{
    private readonly Mock<IJsonSerializer> _mockJsonSerializer;
    private readonly Mock<ICancellationTokenProvider> _mockCancellationTokenProvider;
    private readonly Mock<IRemoteServiceExceptionHandler> _mockRemoteServiceExceptionHandler;
    private readonly CliHttpClientFactory _cliHttpClientFactory;
    private readonly NpmPackageInfoProvider _provider;

    public NpmPackageInfoProviderTests()
    {
        _mockJsonSerializer = new Mock<IJsonSerializer>();
        _mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
        _mockRemoteServiceExceptionHandler = new Mock<IRemoteServiceExceptionHandler>();
        _cliHttpClientFactory = new CliHttpClientFactory();

        _mockCancellationTokenProvider
            .Setup(x => x.Token)
            .Returns(CancellationToken.None);

        _mockRemoteServiceExceptionHandler
            .Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);

        _provider = new NpmPackageInfoProvider(
            _mockJsonSerializer.Object,
            _mockCancellationTokenProvider.Object,
            _mockRemoteServiceExceptionHandler.Object,
            _cliHttpClientFactory);
    }

    [Fact]
    public async Task GetPackageListAsync_Should_Return_Deserialized_Package_List()
    {
        // Arrange
        var expectedPackages = new List<NpmPackageInfo>
        {
            new NpmPackageInfo { Name = "test-package", Version = "1.0.0" }
        };
        var jsonContent = "[{\"name\":\"test-package\",\"version\":\"1.0.0\"}]";

        SetupHttpClientResponse(jsonContent);
        _mockJsonSerializer
            .Setup(x => x.Deserialize<List<NpmPackageInfo>>(jsonContent))
            .Returns(expectedPackages);

        // Act
        var result = await _provider.GetPackageListAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("test-package", result[0].Name);
        _mockJsonSerializer.Verify(x => x.Deserialize<List<NpmPackageInfo>>(jsonContent), Times.Once);
    }

    [Fact]
    public async Task GetPackageListAsync_Should_Call_RemoteServiceExceptionHandler_On_Success()
    {
        // Arrange
        var jsonContent = "[]";
        SetupHttpClientResponse(jsonContent);
        _mockJsonSerializer.Setup(x => x.Deserialize<List<NpmPackageInfo>>(jsonContent)).Returns(new List<NpmPackageInfo>());

        // Act
        await _provider.GetPackageListAsync();

        // Assert
        _mockRemoteServiceExceptionHandler.Verify(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_Should_Return_Package_When_Found()
    {
        // Arrange
        var packageList = new List<NpmPackageInfo>
        {
            new NpmPackageInfo { Name = "found-package", Version = "1.0.0" }
        };
        var jsonContent = "[{\"name\":\"found-package\",\"version\":\"1.0.0\"}]";

        SetupHttpClientResponse(jsonContent);
        _mockJsonSerializer.Setup(x => x.Deserialize<List<NpmPackageInfo>>(jsonContent)).Returns(packageList);

        // Act
        var result = await _provider.GetAsync("found-package");

        // Assert
        Assert.Equal("found-package", result.Name);
    }

    [Fact]
    public async Task GetAsync_Should_Throw_When_Package_Not_Found()
    {
        // Arrange
        var jsonContent = "[]";
        SetupHttpClientResponse(jsonContent);
        _mockJsonSerializer.Setup(x => x.Deserialize<List<NpmPackageInfo>>(jsonContent)).Returns(new List<NpmPackageInfo>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _provider.GetAsync("non-existent"));
        Assert.Equal("Package is not found or downloadable!", exception.Message);
    }

    private void SetupHttpClientResponse(string jsonContent)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(handlerMock.Object);

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("npmPackages/")),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent)
            });

        // Replace the real factory's client with our mocked one for this test
        var originalCreateClient = _cliHttpClientFactory.CreateClient;
        _cliHttpClientFactory.CreateClient = () => httpClient;
    }
}

public class NpmPackageInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}
