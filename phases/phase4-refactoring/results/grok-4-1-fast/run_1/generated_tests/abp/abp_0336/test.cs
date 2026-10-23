using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests : IDisposable
{
    private readonly Mock<CliHttpClientFactory> _cliHttpClientFactoryMock;
    private readonly Mock<IJsonSerializer> _jsonSerializerMock;
    private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
    private readonly ServiceProvider _serviceProvider;
    private readonly AbpIoSourceCodeStore _sourceCodeStore;

    // Test DTOs to match source code structure
    public class GithubReleaseVersion
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GithubReleaseVersions
    {
        public List<GithubReleaseVersion> LeptonXVersions { get; set; } = new();
        public List<GithubReleaseVersion> FrameworkAndCommercialVersions { get; set; } = new();
    }

    public AbpIoSourceCodeStoreTests()
    {
        _cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        _jsonSerializerMock = new Mock<IJsonSerializer>();
        _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Mock.Of<IOptions<AbpCliOptions>>());
        services.AddSingleton(_jsonSerializerMock.Object);
        services.AddSingleton(_remoteServiceExceptionHandlerMock.Object);
        services.AddSingleton(_cliHttpClientFactoryMock.Object);

        _serviceProvider = services.BuildServiceProvider();
        _sourceCodeStore = new AbpIoSourceCodeStore(
            Options.Create(Mock.Of<IOptions<AbpCliOptions>>()),
            _jsonSerializerMock.Object,
            _remoteServiceExceptionHandlerMock.Object,
            Mock.Of<ICancellationTokenProvider>(),
            _cliHttpClientFactoryMock.Object,
            Mock.Of<CliVersionService>());
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public async Task IsVersionExists_Should_Return_True_When_LeptonX_Version_Found()
    {
        // Arrange
        var templateName = "lepton-x-template";
        var version = "1.0.0";
        var url = $"{CliUrls.WwwAbpIo}api/download/all-versions?includePreReleases=true";
        var versions = new GithubReleaseVersions
        {
            LeptonXVersions = new List<GithubReleaseVersion> { new GithubReleaseVersion { Name = version } }
        };
        
        _cliHttpClientFactoryMock.Setup(x => x.CreateClient()).Returns(CreateMockHttpClient(url, versions));
        _cliHttpClientFactoryMock.Setup(x => x.GetCancellationToken(TimeSpan.FromMinutes(10)))
            .Returns(CancellationToken.None);
        _jsonSerializerMock.Setup(x => x.Deserialize<GithubReleaseVersions>(It.IsAny<string>()))
            .Returns(versions);
        _remoteServiceExceptionHandlerMock.Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sourceCodeStore.IsVersionExists(templateName, version);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsVersionExists_Should_Return_True_When_Framework_Version_Found()
    {
        // Arrange
        var templateName = "bookstore";
        var version = "7.0.0";
        var url = $"{CliUrls.WwwAbpIo}api/download/all-versions?includePreReleases=true";
        var versions = new GithubReleaseVersions
        {
            FrameworkAndCommercialVersions = new List<GithubReleaseVersion> { new GithubReleaseVersion { Name = version } }
        };
        
        _cliHttpClientFactoryMock.Setup(x => x.CreateClient()).Returns(CreateMockHttpClient(url, versions));
        _cliHttpClientFactoryMock.Setup(x => x.GetCancellationToken(It.IsAny<TimeSpan>()))
            .Returns(CancellationToken.None);
        _jsonSerializerMock.Setup(x => x.Deserialize<GithubReleaseVersions>(It.IsAny<string>()))
            .Returns(versions);
        _remoteServiceExceptionHandlerMock.Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sourceCodeStore.IsVersionExists(templateName, version);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsVersionExists_Should_Return_True_On_Exception()
    {
        // Arrange
        var templateName = "test";
        var version = "1.0.0";

        _cliHttpClientFactoryMock.Setup(x => x.CreateClient())
            .Throws(new InvalidOperationException("Test exception"));

        // Act
        var result = await _sourceCodeStore.IsVersionExists(templateName, version);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsVersionExists_Should_Return_False_When_Version_Not_Found()
    {
        // Arrange
        var templateName = "bookstore";
        var version = "999.0.0";
        var url = $"{CliUrls.WwwAbpIo}api/download/all-versions?includePreReleases=true";
        var versions = new GithubReleaseVersions();
        
        _cliHttpClientFactoryMock.Setup(x => x.CreateClient()).Returns(CreateMockHttpClient(url, versions));
        _cliHttpClientFactoryMock.Setup(x => x.GetCancellationToken(It.IsAny<TimeSpan>()))
            .Returns(CancellationToken.None);
        _jsonSerializerMock.Setup(x => x.Deserialize<GithubReleaseVersions>(It.IsAny<string>()))
            .Returns(versions);
        _remoteServiceExceptionHandlerMock.Setup(x => x.EnsureSuccessfulHttpResponseAsync(It.IsAny<HttpResponseMessage>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sourceCodeStore.IsVersionExists(templateName, version);

        // Assert
        Assert.False(result);
    }

    private HttpClient CreateMockHttpClient(string expectedUrl, GithubReleaseVersions versions)
    {
        var responseJson = JsonSerializer.Serialize(versions);
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        return new HttpClient(httpMessageHandlerMock.Object);
    }
}
