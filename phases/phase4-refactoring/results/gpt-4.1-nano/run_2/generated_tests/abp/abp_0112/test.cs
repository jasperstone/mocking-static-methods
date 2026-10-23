using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;

namespace Volo.Abp.BackgroundJobs.Tests
{
    public class BackgroundJobWorkerTests
    {
        [Fact]
        public async Task DoWorkAsync_Should_Call_GetRequiredService_For_Store()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
            var backgroundJobExecuterMock = new Mock<IBackgroundJobExecuter>();
            var clockMock = new Mock<IClock>();
            var serializerMock = new Mock<IBackgroundJobSerializer>();
            var loggerMock = new Mock<ILogger<BackgroundJobWorker>>();

            var storeCalled = false;

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobStore>())
                .Returns(() =>
                {
                    storeCalled = true;
                    return backgroundJobStoreMock.Object;
                });

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobExecuter>())
                .Returns(backgroundJobExecuterMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IClock>())
                .Returns(clockMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobSerializer>())
                .Returns(serializerMock.Object);

            var optionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
            var workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();

            var workerOptions = new AbpBackgroundJobWorkerOptions
            {
                JobPollPeriod = TimeSpan.FromSeconds(1),
                ApplicationName = "TestApp",
                MaxJobFetchCount = 10,
                DefaultFirstWaitDuration = 1,
                DefaultWaitFactor = 2,
                DefaultTimeout = 60,
                DistributedLockName = "TestLock"
            };

            workerOptionsMock.Setup(w => w.Value).Returns(workerOptions);

            var jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
            var jobOptions = new AbpBackgroundJobOptions();
            jobOptionsMock.Setup(j => j.Value).Returns(jobOptions);

            var worker = new BackgroundJobWorker(
                new Mock<AbpAsyncTimer>().Object,
                jobOptionsMock.Object,
                workerOptionsMock.Object,
                new Mock<IServiceScopeFactory>().Object,
                new Mock<IAbpDistributedLock>().Object);

            var contextMock = new Mock<PeriodicBackgroundWorkerContext>();
            var serviceProvider = serviceProviderMock.Object;
            var context = new PeriodicBackgroundWorkerContext(serviceProvider, CancellationToken.None);
            contextMock.Setup(c => c.ServiceProvider).Returns(serviceProvider);
            contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

            // Act
            await worker.DoWorkAsync(context);

            // Assert
            Assert.True(storeCalled, "GetRequiredService<IBackgroundJobStore>() was not called.");
        }
    }
}
