using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests
{
    private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
    private readonly Mock<IOptions<AbpCliOptions>> _optionsMock;
    private readonly Mock<IJsonSerializer> _jsonSerializerMock;
    private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
    private readonly AbpIoSourceCodeStore _sourceCodeStore;

    public AbpIoSourceCodeStoreTests()
    {
        _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        _optionsMock = new Mock<IOptions<AbpCliOptions>>();
        _jsonSerializerMock = new Mock<IJsonSerializer>();
        _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();

        _sourceCodeStore = new AbpIoSourceCodeStore(
            _optionsMock.Object,
            _jsonSerializerMock.Object,
            _remoteServiceExceptionHandlerMock.Object,
            NullCancellationTokenProvider.Instance,
            new Mock<CliHttpClientFactory>().Object,
            new Mock<CliVersionService>().Object);

        // Override the NullLogger with our mock using reflection
        typeof(AbpIoSourceCodeStore).GetProperty("Logger")!
            .SetValue(_sourceCodeStore, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_Should_LogWarning_When_LatestVersion_Is_Null()
    {
        // Arrange - Mock the internal GetLatestSourceCodeVersionAsync method via subclass
        var mockStore = new TestableAbpIoSourceCodeStore(
            _optionsMock.Object,
            _jsonSerializerMock.Object,
            _remoteServiceExceptionHandlerMock.Object,
            NullCancellationTokenProvider.Instance,
            new Mock<CliHttpClientFactory>().Object,
            new Mock<CliVersionService>().Object)
        {
            Logger = _loggerMock.Object,
            GetLatestVersionResult = null
        };

        // Create cache directory if needed
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "abp-cli-cache"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CliUsageException>(
            () => mockStore.GetAsync("BookStore", SourceCodeTypes.Template, version: null));

        Assert.Equal("Use command: abp new Acme.BookStore -v version", exception.Message);

        // Verify the specific LogWarning call on line 75
        _loggerMock.Verify(
            x => x.LogWarning("The remote service is currently unavailable, please specify the version."),
            Times.Once);
    }

    [Fact]
    public void LoggerProperty_Can_Be_Injected()
    {
        // Verify logger injection works
        Assert.NotNull(_sourceCodeStore.Logger);
        _loggerMock.VerifyNoOtherCalls();
    }

    // Testable subclass to control internal method behavior
    private class TestableAbpIoSourceCodeStore : AbpIoSourceCodeStore
    {
        public string GetLatestVersionResult { get; set; }

        public TestableAbpIoSourceCodeStore(
            IOptions<AbpCliOptions> options,
            IJsonSerializer jsonSerializer,
            IRemoteServiceExceptionHandler remoteServiceExceptionHandler,
            ICancellationTokenProvider cancellationTokenProvider,
            CliHttpClientFactory cliHttpClientFactory,
            CliVersionService cliVersionService)
            : base(options, jsonSerializer, remoteServiceExceptionHandler, cancellationTokenProvider, cliHttpClientFactory, cliVersionService)
        {
        }

        protected override Task<string> GetLatestSourceCodeVersionAsync(
            string name,
            string type,
            string templateSource,
            bool includePreReleases)
        {
            return Task.FromResult(GetLatestVersionResult);
        }

        protected override List<TemplateDefinition> GetLocalTemplates()
        {
            return new List<TemplateDefinition>();
        }
    }
}
