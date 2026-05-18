using Xunit;
using Moq;
using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Garnet.Tests
{
    public class GarnetServerNodeTests
    {
        [Fact]
        public async Task TestGossipTaskFaults_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var garnetServerNodeType = typeof(GarnetServerNode);
            var constructorInfo = garnetServerNodeType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];
            var garnetServerNode = (GarnetServerNode)constructorInfo.Invoke(new object[] { clusterProviderMock.Object, null, null, null, loggerMock.Object });

            // Act
            var task = Task.FromException(new Exception("Test exception"));
            var fieldInfo = garnetServerNodeType.GetField("gossipTask", BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo.SetValue(garnetServerNode, task);
            await (Task)garnetServerNodeType.GetMethod("GossipAsync", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(garnetServerNode, new object[] { new byte[0] });

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
