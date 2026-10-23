using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Http;
using Volo.Abp.Json;

namespace Volo.Abp.Cli.Core.Tests.ProjectBuilding;

public class NpmPackageInfoProviderTests
{
    private readonly Mock<IJsonSerializer> _jsonSerializerMock;
    private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
    private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
    private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;

    public NpmPackageInfoProviderTests()
    {
        _jsonSerializerMock = new Mock<IJsonSerializer>();
        _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
    }

    private HttpClient CreateHttpClient(HttpResponseMessage responseMessage)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage)
            .Verifiable();

        return new HttpClient(handlerMock.Object);
    }

    [Fact]
    public async Task GetPackageListAsync_ReturnsDeserializedList()
    {
        // Arrange
        var expectedPackages = new List<NpmPackageInfo>
        {
            new NpmPackageInfo { Name = "pkg1" },
            new NpmPackageInfo { Name = "pkg2" }
        };
        var json = "[{\"Name\":\"pkg1\"},{\"Name\":\"pkg2\"}]";

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        };

        var httpClient = CreateHttpClient(httpResponse);

        _cliHttpClientFactoryMock.Setup(f => f.CreateClient()).Returns(httpClient);
        _cancellationTokenProviderMock.SetupGet(c => c.Token).Returns(CancellationToken.None);
        _remoteServiceExceptionHandlerMock
            .Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);
        _jsonSerializerMock
            .Setup(s => s.Deserialize<List<NpmPackageInfo>>(json))
            .Returns(expectedPackages);

        var provider = new NpmPackageInfoProvider(
            _jsonSerializerMock.Object,
            _cancellationTokenProviderMock.Object,
            _remoteServiceExceptionHandlerMock.Object,
            _cliHttpClientFactoryMock.Object);

        // Act
        var result = await provider.GetPackageListAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("pkg1", result[0].Name);
        Assert.Equal("pkg2", result[1].Name);
    }

    [Fact]
    public async Task GetAsync_ReturnsPackage_WhenFound()
    {
        // Arrange
        var packageName = "pkg1";
        var packages = new List<NpmPackageInfo>
        {
            new NpmPackageInfo { Name = packageName },
            new NpmPackageInfo { Name = "pkg2" }
        };

        var providerMock = new Mock<NpmPackageInfoProvider>(
            _jsonSerializerMock.Object,
            _cancellationTokenProviderMock.Object,
            _remoteServiceExceptionHandlerMock.Object,
            _cliHttpClientFactoryMock.Object)
        { CallBase = true };

        providerMock
            .Setup(p => p.GetPackageListAsync())
            .ReturnsAsync(packages);

        // Act
        var result = await providerMock.Object.GetAsync(packageName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(packageName, result.Name);
    }

    [Fact]
    public async Task GetAsync_ThrowsException_WhenPackageNotFound()
    {
        // Arrange
        var packageName = "notfound";
        var packages = new List<NpmPackageInfo>
        {
            new NpmPackageInfo { Name = "pkg1" },
            new NpmPackageInfo { Name = "pkg2" }
        };

        var providerMock = new Mock<NpmPackageInfoProvider>(
            _jsonSerializerMock.Object,
            _cancellationTokenProviderMock.Object,
            _remoteServiceExceptionHandlerMock.Object,
            _cliHttpClientFactoryMock.Object)
        { CallBase = true };

        providerMock
            .Setup(p => p.GetPackageListAsync())
            .ReturnsAsync(packages);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => providerMock.Object.GetAsync(packageName));
        Assert.Equal("Package is not found or downloadable!", ex.Message);
    }
}

public class NpmPackageInfo
{
    public string Name { get; set; }
}

public interface ICancellationTokenProvider
{
    CancellationToken Token { get; }
}

public interface IRemoteServiceExceptionHandler
{
    Task EnsureSuccessfulHttpResponseAsync(HttpResponseMessage responseMessage);
}

public class CliHttpClientFactory
{
    public virtual HttpClient CreateClient() => new HttpClient();
}
