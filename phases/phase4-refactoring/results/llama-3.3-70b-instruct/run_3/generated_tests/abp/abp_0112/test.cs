using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.BackgroundJobs.Tests
{
    public class BackgroundJobWorkerTests
    {
        [Fact]
        public async Task DoWorkAsync_WaitingJobsExist_JobsAreExecuted()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IBackgroundJobStore>(Mock.Of<IBackgroundJobStore>())
                .AddSingleton<IBackgroundJobExecuter>(Mock.Of<IBackgroundJobExecuter>())
                .AddSingleton<Volo.Abp.Timing.IClock>(Mock.Of<Volo.Abp.Timing.IClock>())
                .AddSingleton<IBackgroundJobSerializer>(Mock.Of<IBackgroundJobSerializer>())
                .BuildServiceProvider();

            var workerContext = new PeriodicBackgroundWorkerContext(serviceProvider, CancellationToken.None);

            var jobInfo = new BackgroundJobInfo
            {
                Id = Guid.NewGuid(),
                JobName = "TestJob",
                JobArgs = "TestArgs",
                CreationTime = DateTime.Now,
                LastTryTime = DateTime.Now,
                NextTryTime = DateTime.Now,
                TryCount = 0,
                IsAbandoned = false,
                ApplicationName = "TestApp"
            };

            var jobStoreMock = Mock.Get<IBackgroundJobStore>(serviceProvider.GetService<IBackgroundJobStore>());
            jobStoreMock.Setup(s => s.GetWaitingJobsAsync("TestApp", 10)).ReturnsAsync(new List<BackgroundJobInfo> { jobInfo });

            var jobExecuterMock = Mock.Get<IBackgroundJobExecuter>(serviceProvider.GetService<IBackgroundJobExecuter>());
            jobExecuterMock.Setup(e => e.ExecuteAsync(It.IsAny<JobExecutionContext>())).Returns(Task.CompletedTask);

            var backgroundJobWorker = new BackgroundJobWorker(
                new AbpAsyncTimer(),
                Options.Create(new AbpBackgroundJobOptions()),
                Options.Create(new AbpBackgroundJobWorkerOptions()),
                serviceProvider.GetService<IServiceScopeFactory>(),
                Mock.Of<IAbpDistributedLock>());

            // Act
            await backgroundJobWorker.DoWorkAsync(workerContext);

            // Assert
            jobExecuterMock.Verify(e => e.ExecuteAsync(It.IsAny<JobExecutionContext>()), Times.Once);
        }
    }
}
