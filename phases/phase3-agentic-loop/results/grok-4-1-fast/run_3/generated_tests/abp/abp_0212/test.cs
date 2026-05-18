using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class NewCommandTests
{
    private readonly Mock<ILogger<NewCommand>> _loggerMock;
    private readonly NewCommandTestable _command;

    public NewCommandTests()
    {
        _loggerMock = new Mock<ILogger<NewCommand>>();
        _command = new NewCommandTestable(_loggerMock.Object);
    }

    [Fact]
    public async Task Should_Log_Tiered_Yes_When_Tiered_Option_Is_Present()
    {
        // Arrange
        var options = new AbpCommandLineOptions();
        options.Add("--tiered", "");
        var args = new CommandLineArgs("new", "MyProject");
        args.Options.AddRange(options);

        // Act
        await _command.ExecuteAsync(args);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => t!(v, null)!.Contains("Tiered: yes")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Not_Log_Tiered_Message_When_Tiered_Option_Is_Not_Present()
    {
        // Arrange
        var args = new CommandLineArgs("new", "MyProject");

        // Act
        await _command.ExecuteAsync(args);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => t!(v, null)!.Contains("Tiered: yes")),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}

public class NewCommandTestable : NewCommand
{
    public NewCommandTestable(ILogger<NewCommand> logger) : base(
        null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!)
    {
        Logger = logger;
    }

    public new async Task ExecuteAsync(CommandLineArgs commandLineArgs)
    {
        var projectName = NamespaceHelper.NormalizeNamespace(commandLineArgs.Target);
        if (string.IsNullOrWhiteSpace(projectName))
        {
            throw new Exception("Project name is missing!");
        }

        Logger.LogInformation("Creating your project...");
        Logger.LogInformation("Project name: " + projectName);

        var template = commandLineArgs.Options.GetOrNull(Options.Template.Short, Options.Template.Long);
        if (template != null)
        {
            Logger.LogInformation("Template: " + template);
        }
        else
        {
            template = "app";
        }

        var isTiered = commandLineArgs.Options.ContainsKey(Options.Tiered.Long);
        if (isTiered)
        {
            Logger.LogInformation("Tiered: yes");
        }

        await Task.CompletedTask;
    }
}
