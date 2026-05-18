using Microsoft.Extensions.DependencyInjection;
using Moq;
using Volo.Abp.BackgroundJobs;
using Xunit;

namespace Volo.Abp.BackgroundJobs.Tests
{
    public class BackgroundJobWorkerTests
    {
        [Fact]
        public async Task DoWorkAsync_GetRequiredService_Called()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IBackgroundJobStore>(Mock.Of<IBackgroundJobStore>())
                .AddSingleton<IBackgroundJobExecuter>(Mock.Of<IBackgroundJobExecuter>())
                .AddSingleton<IClock>(Mock.Of<IClock>())
                .AddSingleton<IBackgroundJobSerializer>(Mock.Of<IBackgroundJobSerializer>())
                .BuildServiceProvider();

            var workerContext = new PeriodicBackgroundWorkerContext(serviceProvider, default);

            var backgroundJobWorker = new BackgroundJobWorker(
                Mock.Of<AbpAsyncTimer>(),
                Mock.Of<IOptions<AbpBackgroundJobOptions>>(),
                Mock.Of<IOptions<AbpBackgroundJobWorkerOptions>>(),
                Mock.Of<IServiceScopeFactory>(),
                Mock.Of<IAbpDistributedLock>());

            // Act
            await backgroundJobWorker.DoWorkAsync(workerContext);

            // Assert
            var store = serviceProvider.GetRequiredService<IBackgroundJobStore>();
            Assert.NotNull(store);
        }
    }
}
