using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
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
                .AddSingleton<IBackgroundJobExecuter, MockBackgroundJobExecuter>()
                .AddSingleton<IBackgroundJobSerializer, MockBackgroundJobSerializer>()
                .BuildServiceProvider();

            var workerContext = new PeriodicBackgroundWorkerContext(serviceProvider, CancellationToken.None);

            var backgroundJobWorker = new BackgroundJobWorker(
                new AbpAsyncTimer(),
                new Options<AbpBackgroundJobOptions>(new AbpBackgroundJobOptions()),
                new Options<AbpBackgroundJobWorkerOptions>(new AbpBackgroundJobWorkerOptions()),
                new ServiceScopeFactory(new ServiceCollection().BuildServiceProvider()),
                new Mock<IAbpDistributedLock>().Object);

            // Act
            await backgroundJobWorker.DoWorkAsync(workerContext);

            // Assert
            var store = serviceProvider.GetRequiredService<IBackgroundJobStore>();
            Assert.NotNull(store);
        }
    }

    public class MockBackgroundJobStore : IBackgroundJobStore
    {
        public Task<BackgroundJobInfo> FindAsync(Guid jobId)
        {
            return Task.FromResult(new BackgroundJobInfo());
        }

        public Task InsertAsync(BackgroundJobInfo jobInfo)
        {
            return Task.CompletedTask;
        }

        public Task<List<BackgroundJobInfo>> GetWaitingJobsAsync(string? applicationName, int maxResultCount)
        {
            return Task.FromResult(new List<BackgroundJobInfo>());
        }

        public Task DeleteAsync(Guid jobId)
        {
            return Task.CompletedTask;
        }

        public Task UpdateAsync(BackgroundJobInfo jobInfo)
        {
            return Task.CompletedTask;
        }
    }

    public class MockBackgroundJobExecuter : IBackgroundJobExecuter
    {
        public Task ExecuteAsync(JobExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }

    public class MockBackgroundJobSerializer : IBackgroundJobSerializer
    {
        public object Deserialize(string serializedObject, Type objectType)
        {
            return null;
        }

        public string Serialize(object obj)
        {
            return string.Empty;
        }
    }
}
