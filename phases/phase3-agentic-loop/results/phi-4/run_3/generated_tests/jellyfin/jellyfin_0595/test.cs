using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

// Mock interface to simulate IJellyfinDatabaseProvider
public interface IJellyfinDatabaseProvider
{
    Task RunShutdownTask(CancellationToken cancellationToken);
}

public class ProgramTests
{
    [Fact]
    public async Task GetRequiredService_CallsRunShutdownTask()
    {
        // Arrange
        var mockDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
        var mockShutdownTask = new Mock<Func<CancellationToken, Task>>();

        mockDatabaseProvider
            .Setup(dp => dp.RunShutdownTask(It.IsAny<CancellationToken>()))
            .Returns(mockShutdownTask.Object);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IJellyfinDatabaseProvider)))
            .Returns(mockDatabaseProvider.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(60));

        // Act
        var databaseProvider = mockServiceProvider.Object.GetService(typeof(IJellyfinDatabaseProvider)) as IJellyfinDatabaseProvider;
        await databaseProvider.RunShutdownTask(cancellationTokenSource.Token);

        // Assert
        mockDatabaseProvider.Verify(dp => dp.RunShutdownTask(It.IsAny<CancellationToken>()), Times.Once);
    }
}
