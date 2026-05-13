using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Roslyn.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task LogInformation_Called_When_Relaunching_BuildHost()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerFactoryMock.Object);

            var buildHostProcessMock = new Mock<BuildHostProcess>();
            var remoteBuildHostMock = new Mock<RemoteBuildHost>();
            remoteBuildHostMock.Setup(rbh => rbh.FindBestMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MSBuildLocation { Path = "path/to/msbuild" });

            buildHostProcessMock.Setup(bhp => bhp.BuildHost).Returns(remoteBuildHostMock.Object);

            var processPath = "path/to/process";
            var dotnetPath = "path/to/dotnet";

            // Act
            await buildHostProcessManager.GetBuildHostAsync(BuildHostProcessKind.NetCore, "projectOrSolutionFilePath", dotnetPath, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
