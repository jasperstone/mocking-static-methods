using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;
using Microsoft.Extensions.Options;

namespace Volo.Abp.BackgroundJobs.Tests
{
    public class BackgroundJobWorkerTests
    {
        [Fact]
        public async Task DoWorkAsync_GetWaitingJobsAsync_Called()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IBackgroundJobStore>(Mock.Of<IBackgroundJobStore>())
                .AddSingleton<IBackgroundJobExecuter>(Mock.Of<IBackgroundJobExecuter>())
                .AddSingleton<IClock>(Mock.Of<IClock>())
                .AddSingleton<IBackgroundJobSerializer>(Mock.Of<IBackgroundJobSerializer>())
                .AddOptions<AbpBackgroundJobOptions>()
                .AddOptions<AbpBackgroundJobWorkerOptions>()
                .AddSingleton<IServiceScopeFactory>(new ServiceScopeFactory(Mock.Of<IServiceProvider>()))
                .BuildServiceProvider();

            var workerContext = new PeriodicBackgroundWorkerContext(serviceProvider, CancellationToken.None);
            var backgroundJobWorker = new BackgroundJobWorker(
                Mock.Of<AbpAsyncTimer>(),
                serviceProvider.GetService<IOptions<AbpBackgroundJobOptions>>(),
                serviceProvider.GetService<IOptions<AbpBackgroundJobWorkerOptions>>(),
                serviceProvider.GetService<IServiceScopeFactory>(),
                Mock.Of<IAbpDistributedLock>());

            // Act
            await backgroundJobWorker.DoWorkAsync(workerContext);

            // Assert
            var backgroundJobStore = serviceProvider.GetRequiredService<IBackgroundJobStore>();
            Mock.Get(backgroundJobStore).Verify(store => store.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }
    }
}
