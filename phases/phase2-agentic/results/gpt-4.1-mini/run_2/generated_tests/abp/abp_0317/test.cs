using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Http;
using Volo.Abp.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Cli.GitHub;
using Volo.Abp.Cli.ProjectBuilding.Templates.App;
using Volo.Abp.Cli.ProjectBuilding.Templates.Console;
using Volo.Abp.Cli.ProjectBuilding.Templates.Maui;
using Volo.Abp.Cli.ProjectBuilding.Templates.MvcModule;
using Volo.Abp.Cli.ProjectBuilding.Templates.Wpf;
using Volo.Abp.Http;
using Volo.Abp.IO;
using Volo.Abp.Threading;
using Microsoft.Extensions.Options;
using Xunit;

namespace Volo.Abp.Cli.ProjectBuilding.Tests
{
    public class AbpIoSourceCodeStoreTests
    {
        private class TestAbpIoSourceCodeStore : AbpIoSourceCodeStore
        {
            public TestAbpIoSourceCodeStore(
                IOptions<AbpCliOptions> options,
                IJsonSerializer jsonSerializer,
                IRemoteServiceExceptionHandler remoteServiceExceptionHandler,
                ICancellationTokenProvider cancellationTokenProvider,
                CliHttpClientFactory cliHttpClientFactory,
                CliVersionService cliVersionService)
                : base(options, jsonSerializer, remoteServiceExceptionHandler, cancellationTokenProvider, cliHttpClientFactory, cliVersionService)
            {
            }

            public Func<string, string, string, bool, Task<string>> GetLatestSourceCodeVersionAsyncFunc { get; set; }
            public Func<List<TemplateFile>> GetLocalTemplatesFunc { get; set; }
            public Func<string, string, Task<bool>> IsVersionExistsFunc { get; set; }
            public Func<string, string, string, Task<string>> GetTemplateNugetVersionAsyncFunc { get; set; }
            public Func<string, bool> IsNetworkSourceFunc { get; set; }

            protected override Task<string> GetLatestSourceCodeVersionAsync(string name, string type, string arg3, bool includePreReleases)
            {
                if (GetLatestSourceCodeVersionAsyncFunc != null)
                    return GetLatestSourceCodeVersionAsyncFunc(name, type, arg3, includePreReleases);
                return base.GetLatestSourceCodeVersionAsync(name, type, arg3, includePreReleases);
            }

            protected override List<TemplateFile> GetLocalTemplates()
            {
                if (GetLocalTemplatesFunc != null)
                    return GetLocalTemplatesFunc();
                return base.GetLocalTemplates();
            }

            protected override Task<bool> IsVersionExists(string name, string version)
            {
                if (IsVersionExistsFunc != null)
                    return IsVersionExistsFunc(name, version);
                return base.IsVersionExists(name, version);
            }

            protected override Task<string> GetTemplateNugetVersionAsync(string name, string type, string version)
            {
                if (GetTemplateNugetVersionAsyncFunc != null)
                    return GetTemplateNugetVersionAsyncFunc(name, type, version);
                return base.GetTemplateNugetVersionAsync(name, type, version);
            }

            protected override bool IsNetworkSource(string source)
            {
                if (IsNetworkSourceFunc != null)
                    return IsNetworkSourceFunc(source);
                return base.IsNetworkSource(source);
            }
        }

        [Fact]
        public async Task GetAsync_LogsWarning_WhenLatestVersionIsNullAndVersionNotSpecified()
        {
            // Arrange
            var optionsMock = new Mock<IOptions<AbpCliOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new AbpCliOptions());

            var jsonSerializerMock = new Mock<IJsonSerializer>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(null);
            var cliVersionServiceMock = new Mock<CliVersionService>(null, null);

            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();

            var store = new TestAbpIoSourceCodeStore(
                optionsMock.Object,
                jsonSerializerMock.Object,
                remoteServiceExceptionHandlerMock.Object,
                cancellationTokenProviderMock.Object,
                cliHttpClientFactoryMock.Object,
                cliVersionServiceMock.Object);

            store.Logger = loggerMock.Object;

            store.GetLatestSourceCodeVersionAsyncFunc = (name, type, arg3, includePreReleases) => Task.FromResult<string>(null);

            var localTemplates = new List<TemplateFile>
            {
                new TemplateFile(new byte[0], "Template1", "1.0.0", "1.0.0"),
                new TemplateFile(new byte[0], "Template2", "2.0.0", "2.0.0")
            };
            store.GetLocalTemplatesFunc = () => localTemplates;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<CliUsageException>(() => store.GetAsync("TestName", "TestType"));

            // Verify the warning logs were called with expected messages
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "The remote service is currently unavailable, please specify the version."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == string.Empty),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Exactly(2)); // two empty string logs

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Find the following template in your cache directory: "),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "\tTemplate Name\tVersion"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            foreach (var template in localTemplates)
            {
                var expectedLog = $"\t{template.TemplateName}\t\t{template.Version}";
                loggerMock.Verify(l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedLog),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            }

            Assert.Equal("Use command: abp new Acme.BookStore -v version", ex.Message);
        }
    }
}
