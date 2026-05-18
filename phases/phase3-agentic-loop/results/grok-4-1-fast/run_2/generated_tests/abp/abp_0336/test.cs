using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Volo.Abp.Cli.GitHub;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly ServiceProvider _serviceProvider;
    private readonly AbpIoSourceCodeStore _sourceCodeStore;

    public AbpIoSourceCodeStoreTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ILogger<AbpIoSourceCodeStore>>(NullLogger<AbpIoSourceCodeStore>.Instance);
        services.AddSingleton<IJsonSerializer>(provider => new DefaultJsonSerializer());
        services.AddSingleton<IRemoteServiceExceptionHandler>(provider => new MockRemoteServiceExceptionHandler());
        services.AddSingleton<ICancellationTokenProvider>(provider => new MockCancellationTokenProvider());
        services.AddSingleton<CliVersionService>();
        services.AddSingleton<CliHttpClientFactory>();
        services.AddHttpClient(CliConsts.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => _httpMessageHandlerMock.Object);

        services.Configure<AbpCliOptions>(options => { });

        _serviceProvider = services.BuildServiceProvider();
        _sourceCodeStore = _serviceProvider.GetRequiredService<AbpIoSourceCodeStore>();
    }

    [Fact]
    public async Task IsVersionExists_ShouldCallGetAsync_OnSuccess()
    {
        // Arrange
        var url = "https://www.abp.io/api/download/all-versions?includePreReleases=true";
        var jsonResponse = JsonSerializer.Serialize(new GithubReleaseVersions
        {
            FrameworkAndCommercialVersions = new[] { new GithubReleaseVersion { Name = "1.0.0" } }
        });

        SetupHttpClientGetResponse(url, HttpStatusCode.OK, jsonResponse);

        // Act
        var result = await _sourceCodeStore.IsVersionExists("some-template", "1.0.0");

        // Assert
        Assert.True(result);
        _httpMessageHandlerMock.Protected()
            .Verify("SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == url && req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task IsVersionExists_ShouldReturnTrue_OnException()
    {
        // Arrange
        var url = "https://www.abp.io/api/download/all-versions?includePreReleases=true";
        SetupHttpClientGetResponse(url, HttpStatusCode.InternalServerError, string.Empty);

        // Act
        var result = await _sourceCodeStore.IsVersionExists("some-template", "1.0.0");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsVersionExists_ShouldReturnTrue_ForLeptonXTemplate_OnSuccess()
    {
        // Arrange
        var url = "https://www.abp.io/api/download/all-versions?includePreReleases=true";
        var jsonResponse = JsonSerializer.Serialize(new GithubReleaseVersions
        {
            LeptonXVersions = new[] { new GithubReleaseVersion { Name = "1.0.0" } }
        });

        SetupHttpClientGetResponse(url, HttpStatusCode.OK, jsonResponse);

        // Act
        var result = await _sourceCodeStore.IsVersionExists("LeptonX-template", "1.0.0");

        // Assert
        Assert.True(result);
    }

    private void SetupHttpClientGetResponse(string requestUri, HttpStatusCode statusCode, string content)
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == requestUri && req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}

public class MockRemoteServiceExceptionHandler : IRemoteServiceExceptionHandler, ITransientDependency
{
    public Task EnsureSuccessfulHttpResponseAsync(HttpResponseMessage response)
    {
        return Task.CompletedTask;
    }

    public Task<RemoteServiceErrorResponse> GetAbpRemoteServiceErrorAsync(HttpResponseMessage response)
    {
        throw new NotImplementedException();
    }
}

public class MockCancellationTokenProvider : ICancellationTokenProvider, ITransientDependency
{
    public CancellationToken Token => CancellationToken.None;

    public T Use<T>(Func<CancellationToken, T> func)
    {
        return func(Token);
    }

    public async Task<T> UseAsync<T>(Func<CancellationToken, Task<T>> func)
    {
        return await func(Token);
    }
}

public class GithubReleaseVersions
{
    public GithubReleaseVersion[] FrameworkAndCommercialVersions { get; set; } = Array.Empty<GithubReleaseVersion>();
    public GithubReleaseVersion[] LeptonXVersions { get; set; } = Array.Empty<GithubReleaseVersion>();
}

public class GithubReleaseVersion
{
    public string Name { get; set; } = string.Empty;
}

public class RemoteServiceErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
