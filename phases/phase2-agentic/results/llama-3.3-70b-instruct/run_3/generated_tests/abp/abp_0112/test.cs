using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.BackgroundJobs.Tests
{
    public class BackgroundJobWorkerTests
    {
        [Fact]
        public async Task DoWorkAsync_GetRequiredService_Called()
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
    }

    public class MockBackgroundJobStore : IBackgroundJobStore
    {
        public Task<JobInfo> GetAsync(string jobId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string jobId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<JobInfo> InsertAsync(JobInfo jobInfo, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(JobInfo jobInfo, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<JobInfo[]> GetWaitingJobsAsync(string applicationName, int maxResultCount, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new JobInfo[0]);
        }
    }
}
