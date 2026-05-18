using Xunit;
using Moq;
using Moq.Language.Flow;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests.Commands;

public class ProjectCreationCommandBaseTests
{
    [Fact]
    public async void GetProjectBuildArgsAsync_ShouldLogConnectionString_WhenConnectionStringIsProvided()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NewCommand>>();
        loggerMock.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Connection string:")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        )).Verifiable();

        var connectionStringProviderMock = new Mock<object>();
        var solutionPackageVersionFinderMock = new Mock<object>();
        var cmdHelperMock = new Mock<object>();
        var installLibsServiceMock = new Mock<object>();
        var cliServiceMock = new Mock<object>();
        var angularPwaSupportAdderMock = new Mock<object>();
        var initialMigrationCreatorMock = new Mock<object>();
        var themePackageAdderMock = new Mock<object>();
        var eventBusMock = new Mock<object>();
        var bundlingServiceMock = new Mock<object>();
        var angularThemeConfigurerMock = new Mock<object>();
        var cliVersionServiceMock = new Mock<object>();

        var commandBase = new Mock<ProjectCreationCommandBase>(
            connectionStringProviderMock.Object,
            solutionPackageVersionFinderMock.Object,
            cmdHelperMock.Object,
            installLibsServiceMock.Object,
            cliServiceMock.Object,
            angularPwaSupportAdderMock.Object,
            initialMigrationCreatorMock.Object,
            themePackageAdderMock.Object,
            eventBusMock.Object,
            bundlingServiceMock.Object,
            angularThemeConfigurerMock.Object,
            cliVersionServiceMock.Object
        ) { CallBase = true };

        commandBase.Setup(x => x.Logger).Returns(loggerMock.Object);

        var commandLineArgs = new CommandLineArgs(ArgsSource.ParsedCommandLine, new Dictionary<string, string>
        {
            { "--connection-string", "Server=localhost;Database=MyDb;" }
        });

        // Act
        await commandBase.Object.GetProjectBuildArgsAsync(commandLineArgs, "app", "MyProject");

        // Assert
        loggerMock.Verify();
    }
}
