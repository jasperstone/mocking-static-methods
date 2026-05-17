   using System.Threading.Tasks;
   using Microsoft.Extensions.Logging;
   using Moq;
   using Xunit;

   namespace Garnet.cluster.Tests
   {
       public class MigrationDriverTests
       {
           [Fact]
           public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTryPrepareLocalForMigrationFails()
           {
               // Arrange
               var loggerMock = new Mock<ILogger>();
               var clusterProviderMock = new Mock<ClusterProvider>();
               var migrationDriver = new MigrateSession
               {
                   logger = loggerMock.Object,
                   clusterProvider = clusterProviderMock.Object
               };

               // Mock the method to return false
               migrationDriver.TryPrepareLocalForMigration = () => false;

               // Act
               await migrationDriver.BeginAsyncMigrationTaskAsync();

               // Assert
               loggerMock.Verify(
                   x => x.LogError(
                       It.Is<string>(s => s.Contains("Failed to set local slots")),
                       It.IsAny<object[]>()),
                   Times.Once);
           }
       }
   }
   