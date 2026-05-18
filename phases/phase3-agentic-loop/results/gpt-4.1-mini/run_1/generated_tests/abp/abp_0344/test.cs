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
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectBuilding;

public class NpmPackageInfoProviderTests
{
    [Fact]
    public async Task GetPackageListAsync_Should_Call_HttpClient_GetAsync_And_Deserialize_Result()
    {
        // Arrange
        var expectedUrl = "https://www.abp.io/api/download/npmPackages/";
        var cancellationToken = CancellationToken.None;

        var npmPackagesJson = "[{\"Name\":\"TestPackage\",\"Version\":\"1.0.0\"}]";

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Get &&
                  req.RequestUri == new Uri(expectedUrl)),
              ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(new HttpResponseMessage()
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(npmPackagesJson),
           })
           .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object);

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var cliHttpClientFactory = new CliHttpClientFactory(httpClientFactoryMock.Object, new CancellationTokenProviderStub(cancellationToken));

        var jsonSerializerMock = new Mock<IJsonSerializer>();
        jsonSerializerMock.Setup(s => s.Deserialize<List<NpmPackageInfo>>(npmPackagesJson))
            .Returns(new List<NpmPackageInfo> { new NpmPackageInfo { Name = "TestPackage", Version = "1.0.0" } });

        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        remoteServiceExceptionHandlerMock.Setup(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);

        var provider = new NpmPackageInfoProvider(
            jsonSerializerMock.Object,
            new CancellationTokenProviderStub(cancellationToken),
            remoteServiceExceptionHandlerMock.Object,
            cliHttpClientFactory);

        // Act
        var result = await provider.GetPackageListAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("TestPackage", result[0].Name);
        Assert.Equal("1.0.0", result[0].Version);

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri == new Uri(expectedUrl)),
            ItExpr.IsAny<CancellationToken>());

        remoteServiceExceptionHandlerMock.Verify(h => h.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()), Times.Once);
        jsonSerializerMock.Verify(s => s.Deserialize<List<NpmPackageInfo>>(npmPackagesJson), Times.Once);
    }

    [Fact]
    public async Task GetAsync_Should_Return_Package_When_Found()
    {
        // Arrange
        var packageName = "TestPackage";
        var packages = new List<NpmPackageInfo>
        {
            new NpmPackageInfo { Name = packageName, Version = "1.0.0" },
            new NpmPackageInfo { Name = "OtherPackage", Version = "2.0.0" }
        };

        var providerMock = new Mock<NpmPackageInfoProvider>(
            MockBehavior.Strict,
            Mock.Of<IJsonSerializer>(),
            new CancellationTokenProviderStub(CancellationToken.None),
            Mock.Of<IRemoteServiceExceptionHandler>(),
            Mock.Of<CliHttpClientFactory>())
        { CallBase = true };

        providerMock.Setup(p => p.GetPackageListAsync()).ReturnsAsync(packages);

        // Act
        var result = await providerMock.Object.GetAsync(packageName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(packageName, result.Name);
        Assert.Equal("1.0.0", result.Version);
    }

    [Fact]
    public async Task GetAsync_Should_Throw_When_Package_Not_Found()
    {
        // Arrange
        var packageName = "NonExistentPackage";

        var providerMock = new Mock<NpmPackageInfoProvider>(
            MockBehavior.Strict,
            Mock.Of<IJsonSerializer>(),
            new CancellationTokenProviderStub(CancellationToken.None),
            Mock.Of<IRemoteServiceExceptionHandler>(),
            Mock.Of<CliHttpClientFactory>())
        { CallBase = true };

        providerMock.Setup(p => p.GetPackageListAsync()).ReturnsAsync(new List<NpmPackageInfo>());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => providerMock.Object.GetAsync(packageName));
        Assert.Equal("Package is not found or downloadable!", ex.Message);
    }

    private class CancellationTokenProviderStub : ICancellationTokenProvider
    {
        public CancellationToken Token { get; }

        public CancellationTokenProviderStub(CancellationToken token)
        {
            Token = token;
        }
    }

    private class NpmPackageInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }
}
