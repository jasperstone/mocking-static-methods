using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Http;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class NpmPackageInfoProviderTests
{
    [Fact]
    public async Task GetAsync_Should_Return_Package_When_Found()
    {
        // Arrange
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();

        var packageList = new List<NpmPackageInfo>
        {
            new NpmPackageInfo { Name = "test-package" }
        };

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("[{\"Name\":\"test-package\"}]")
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        
        // Use callback to capture call parameters instead of direct setup
        cliHttpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
            .Callback<bool, TimeSpan?, string>((needsAuth, timeout, clientName) => { })
            .Returns(httpClient);
            
        cancellationTokenProviderMock.Setup(x => x.Token).Returns(CancellationToken.None);

        jsonSerializerMock.Setup(x => x.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
            .Returns(packageList);

        remoteServiceExceptionHandlerMock.Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);

        var provider = new NpmPackageInfoProvider(
            jsonSerializerMock.Object,
            cancellationTokenProviderMock.Object,
            remoteServiceExceptionHandlerMock.Object,
            cliHttpClientFactoryMock.Object);

        // Act
        var result = await provider.GetAsync("test-package");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-package", result.Name);
    }

    [Fact]
    public async Task GetAsync_Should_Throw_When_Package_Not_Found()
    {
        // Arrange
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();

        var packageList = new List<NpmPackageInfo>();

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        
        cliHttpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
            .Callback<bool, TimeSpan?, string>((needsAuth, timeout, clientName) => { })
            .Returns(httpClient);
            
        cancellationTokenProviderMock.Setup(x => x.Token).Returns(CancellationToken.None);

        jsonSerializerMock.Setup(x => x.Deserialize<List<NpmPackageInfo>>(It.IsAny<string>()))
            .Returns(packageList);

        remoteServiceExceptionHandlerMock.Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);

        var provider = new NpmPackageInfoProvider(
            jsonSerializerMock.Object,
            cancellationTokenProviderMock.Object,
            remoteServiceExceptionHandlerMock.Object,
            cliHttpClientFactoryMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => provider.GetAsync("non-existent"));
        Assert.Equal("Package is not found or downloadable!", exception.Message);
    }

    [Fact]
    public async Task GetPackageListAsync_Should_Return_Deserialized_Packages()
    {
        // Arrange
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();

        var expectedPackages = new List<NpmPackageInfo> { new NpmPackageInfo { Name = "test" } };
        var jsonContent = "[{\"Name\":\"test\"}]";

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent)
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        
        cliHttpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
            .Callback<bool, TimeSpan?, string>((needsAuth, timeout, clientName) => { })
            .Returns(httpClient);
            
        cancellationTokenProviderMock.Setup(x => x.Token).Returns(CancellationToken.None);

        jsonSerializerMock.Setup(x => x.Deserialize<List<NpmPackageInfo>>(jsonContent)).Returns(expectedPackages);
        remoteServiceExceptionHandlerMock.Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);

        var provider = new NpmPackageInfoProvider(
            jsonSerializerMock.Object,
            cancellationTokenProviderMock.Object,
            remoteServiceExceptionHandlerMock.Object,
            cliHttpClientFactoryMock.Object);

        // Act
        var result = await provider.GetPackageListAsync();

        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal("test", result[0].Name);
    }
}

public class NpmPackageInfo
{
    public string Name { get; set; }
}
