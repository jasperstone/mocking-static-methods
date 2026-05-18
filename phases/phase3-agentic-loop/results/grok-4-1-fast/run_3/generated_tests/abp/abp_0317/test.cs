using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests;

public class AbpIoSourceCodeStoreTests
{
    private readonly Mock<ILogger<AbpIoSourceCodeStore>> _loggerMock;

    public AbpIoSourceCodeStoreTests()
    {
        _loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();
        _loggerMock.Setup(x => x.LogWarning(It.IsAny<string>()));
    }

    [Fact]
    public async Task GetAsync_Should_LogWarning_When_LatestVersion_Is_Null()
    {
        // Arrange
        var store = CreateStoreWithMockedLogger();

        // Mock the protected GetLatestSourceCodeVersionAsync method to return null
        MockProtectedMethod(store, "GetLatestSourceCodeVersionAsync", (string name, string type, string version, bool includePreReleases) => Task.FromResult<string>(null));
        
        // Mock GetLocalTemplates to return empty list
        MockProtectedMethod(store, "GetLocalTemplates", () => new List<TemplateFile>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CliUsageException>(
            () => store.GetAsync("test", SourceCodeTypes.Template, version: null)
        );

        Assert.Equal("Use command: abp new Acme.BookStore -v version", exception.Message);

        // Verify the specific LogWarning call on line 75
        _loggerMock.Verify(x => x.LogWarning("The remote service is currently unavailable, please specify the version."), Times.Once);
        
        // Verify subsequent LogWarning calls
        _loggerMock.Verify(x => x.LogWarning(It.Is<string>(s => string.IsNullOrEmpty(s))), Times.Exactly(2));
        _loggerMock.Verify(x => x.LogWarning("Find the following template in your cache directory: "), Times.Once);
        _loggerMock.Verify(x => x.LogWarning("\tTemplate Name\tVersion"), Times.Once);
    }

    [Fact]
    public void Constructor_Should_Set_NullLogger_By_Default()
    {
        // Arrange
        var options = Mock.Of<IOptions<AbpCliOptions>>();
        var jsonSerializer = Mock.Of<object>();
        var remoteServiceExceptionHandler = Mock.Of<object>();
        var cancellationTokenProvider = Mock.Of<object>();
        var cliHttpClientFactory = Mock.Of<object>();
        var cliVersionService = Mock.Of<object>();

        // Act
        var store = new AbpIoSourceCodeStore(
            options,
            jsonSerializer,
            remoteServiceExceptionHandler,
            cancellationTokenProvider,
            cliHttpClientFactory,
            cliVersionService
        );

        // Assert
        Assert.IsType<NullLogger<AbpIoSourceCodeStore>>(store.Logger);
    }

    private AbpIoSourceCodeStore CreateStoreWithMockedLogger()
    {
        var options = Mock.Of<IOptions<AbpCliOptions>>();
        var jsonSerializer = Mock.Of<object>();
        var remoteServiceExceptionHandler = Mock.Of<object>();
        var cancellationTokenProvider = Mock.Of<object>();
        var cliHttpClientFactory = Mock.Of<object>();
        var cliVersionService = Mock.Of<object>();

        var store = new AbpIoSourceCodeStore(
            options,
            jsonSerializer,
            remoteServiceExceptionHandler,
            cancellationTokenProvider,
            cliHttpClientFactory,
            cliVersionService
        )
        {
            Logger = _loggerMock.Object
        };

        return store;
    }

    private void MockProtectedMethod(AbpIoSourceCodeStore store, string methodName, Delegate mockImplementation)
    {
        var method = typeof(AbpIoSourceCodeStore).GetMethod(methodName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (method != null)
        {
            var field = new System.Reflection.FieldInfo[]
            {
                method.DeclaringType.GetField(
                    "_"+methodName.Substring(0,1).ToLower()+methodName.Substring(1),
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
                ) ?? method
            }.FirstOrDefault(f => f != null);

            if (field != null)
            {
                field.SetValue(store, mockImplementation);
            }
        }
    }
}
