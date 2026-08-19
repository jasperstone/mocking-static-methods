using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests
{
    private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;
    private readonly List<string> _loggedWarnings;
    private readonly AbpIoSourceCodeStore _sourceCodeStore;

    public AbpIoSourceCodeStoreTests()
    {
        _loggedWarnings = new List<string>();
        
        _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        _loggerMock.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Warning),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((level, id, state, ex, formatter) =>
            {
                var message = formatter(state, ex);
                if (!string.IsNullOrEmpty(message))
                {
                    _loggedWarnings.Add(message);
                }
            });

        // Create with NullLogger dependencies
        var options = Options.Create(new AbpCliOptions());
        _sourceCodeStore = new AbpIoSourceCodeStore(
            options,
            new DefaultJsonSerializer(),
            Mock.Of<IRemoteServiceExceptionHandler>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<CliVersionService>());

        // Override Logger with our mock using reflection
        typeof(AbpIoSourceCodeStore).GetProperty("Logger")!
            .SetValue(_sourceCodeStore, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_Should_LogWarningOnLine75_When_LatestVersionIsNull()
    {
        // Arrange - Create cache directory and fake template to trigger GetLocalTemplates
        if (!Directory.Exists(CliPaths.TemplateCache))
        {
            Directory.CreateDirectory(CliPaths.TemplateCache);
        }
        
        // Create a fake cache file that GetLocalTemplates can parse
        var fakeCachePath = Path.Combine(CliPaths.TemplateCache, "bookstore-1.0.0.json");
        File.WriteAllText(fakeCachePath, "{\"TemplateName\":\"bookstore\",\"Version\":\"1.0.0\"}");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CliUsageException>(
            () => _sourceCodeStore.GetAsync("bookstore", SourceCodeTypes.Template, version: null));

        Assert.Equal("Use command: abp new Acme.BookStore -v version", exception.Message);

        // Assert the specific LogWarning call on line 75 was executed
        Assert.Contains("The remote service is currently unavailable, please specify the version.", _loggedWarnings);
        Assert.True(_loggedWarnings.Count >= 4, $"Expected at least 4 warnings logged, got {_loggedWarnings.Count}");
    }
}
