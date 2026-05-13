using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Localization;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;
using Volo.Abp.Http.Client;
using Volo.Abp.Json;
using Volo.Abp.Json.SystemTextJson;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Validation;
using Xunit;

namespace Abp.Tests.Cli.ProjectBuilding
{
    public class AbpIoSourceCodeStoreTests
    {
        private AbpIoSourceCodeStore CreateStore(
            ILogger<AbpIoSourceCodeStore> logger,
            Func<Task<string>> latestVersionResolver)
        {
            var options = Microsoft.Extensions.Options.Options.Create(
                new AbpIoSourceCodeStoreOptions
                {
                    // Set defaults if necessary
                });

            var jsonSerializer = new AbpCliDefaultJsonSerializer(new AbpJsonOptions());
            var remoteServiceExceptionHandler = Mock.Of<IRemoteServiceExceptionHandler>();

            var cancellationTokenProvider = new CancellationTokenProvider(new AmbientExecutionContextAccessor());

            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var cliVersionServiceMock = new Mock<CliVersionService>(
                Mock.Of<IHttpClientFactory>(),
                Mock.Of<IAbpLazyServiceProvider>(),
                NullLogger<CliVersionService>.Instance);

            var store = new AbpIoSourceCodeStore(
                options,
                jsonSerializer,
                remoteServiceExceptionHandler,
                cancellationTokenProvider,
                cliHttpClientFactoryMock.Object,
                cliVersionServiceMock.Object)
            {
                Logger = logger
            };

            // Inject behavior for GetLatestSourceCodeVersionAsync by overriding method via local function (not possible).
            // Instead, we rely on calling GetLatestSourceCodeVersionAsync through delegate (but method is not virtual).
            // So we assume version is provided via constructor.

            return store;
        }

        [Fact]
        public async Task Should_Log_Warnings_When_Remote_Service_Unavailable()
        {
            var loggerMock = new Mock<ILogger<AbpIoSourceCodeStore>>();

            // Arrange: Logger should have same pattern as calling sequence showing message.
            // Because GetAsync will yield multiple LogWarning calls - we want to ensure initial message recorded.
            loggerMock
                .Setup(logger => logger.IsEnabled(It.Is<LogLevel>(level => level == LogLevel.Warning)))
                .Returns(true);

            loggerMock.Setup(logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString() == "The remote service is currently unavailable, please specify the version."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            var store = CreateStore(loggerMock.Object, () => Task.FromResult<string>(null));

            await Assert.ThrowsAsync<CliUsageException>(() =>
                store.GetAsync("templateName", "templateType"));

            loggerMock.Verify();
        }
    }
}
