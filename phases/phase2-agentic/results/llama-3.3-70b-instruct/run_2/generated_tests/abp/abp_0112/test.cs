using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundJobs.Abstractions;
using Xunit;

namespace Volo.Abp.BackgroundJobs.Tests
{
    public class BackgroundJobWorkerTests
    {
        [Fact]
        public async Task DoWorkAsync_GetWaitingJobsAsync_CallsGetRequiredService()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IBackgroundJobStore, MockBackgroundJobStore>()
                .BuildServiceProvider();

            var workerContext = new PeriodicBackgroundWorkerContext(serviceProvider, CancellationToken.None);

            var backgroundJobWorker = new BackgroundJobWorker(
                new AbpAsyncTimer(),
                new OptionsWrapper<AbpBackgroundJobOptions>(new AbpBackgroundJobOptions()),
                new OptionsWrapper<AbpBackgroundJobWorkerOptions>(new AbpBackgroundJobWorkerOptions()),
                new ServiceScopeFactory(serviceProvider),
                new Mock<IAbpDistributedLock>().Object);

            // Act
            await backgroundJobWorker.DoWorkAsync(workerContext);

            // Assert
            var store = serviceProvider.GetRequiredService<IBackgroundJobStore>();
            Assert.IsType<MockBackgroundJobStore>(store);
        }

        private class MockBackgroundJobStore : IBackgroundJobStore
        {
            public Task<List<JobInfo>> GetWaitingJobsAsync(string applicationName, int maxJobFetchCount)
            {
                return Task.FromResult(new List<JobInfo>());
            }
        }
    }
}
