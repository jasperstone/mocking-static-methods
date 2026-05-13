using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Json;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;
using Xunit;

public class NpmPackageInfoProviderTests
{
    [Fact]
    public async Task GetAsync_WhenPackageExists_ReturnsPackageInfo()
    {
        // Arrange
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();

        var responseMessageMock = new Mock<HttpResponseMessage>();
        responseMessageMock.Setup(m => m.Content.ReadAsStringAsync(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync("[{\"Name\":\"test-package\",\"Version\":\"1.0.0\",\"Description\":\"A test package\",\"RepositoryUrl\":\"https://example.com\"}]");

        var httpClientMock = new Mock<HttpClient>();
        httpClientMock.Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(responseMessageMock.Object);

        cliHttpClientFactoryMock.Setup(m => m.CreateClient())
            .Returns(httpClientMock.Object);

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
    public async Task GetAsync_WhenPackageDoesNotExist_ThrowsException()
    {
        // Arrange
        var jsonSerializerMock = new Mock<IJsonSerializer>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();

        var responseMessageMock = new Mock<HttpResponseMessage>();
        responseMessageMock.Setup(m => m.Content.ReadAsStringAsync(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync("[]");

        var httpClientMock = new Mock<HttpClient>();
        httpClientMock.Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(responseMessageMock.Object);

        cliHttpClientFactoryMock.Setup(m => m.CreateClient())
            .Returns(httpClientMock.Object);

        var provider = new NpmPackageInfoProvider(
            jsonSerializerMock.Object,
            cancellationTokenProviderMock.Object,
            remoteServiceExceptionHandlerMock.Object,
            cliHttpClientFactoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => provider.GetAsync("non-existent-package"));
    }
}
