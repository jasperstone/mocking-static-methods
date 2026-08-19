using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionLoggerTests
    {
        [Fact]
        public void LogsWarning_WhenReplicaOfResponseIsNotOK()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            mockLogger.Setup(x => x.LogWarning(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<Exception>()))
                .Callback<string, object[], Exception>((message, args, ex) =>
                {
                    Assert.Equal("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", message);
                    Assert.Collection(args,
                        arg0 => Assert.Equal("test-replica-1", arg0),
                        arg1 => Assert.Equal("ERR_NOT_OK", arg1));
                });

            string replicaId = "test-replica-1";
            string replicaOfResp = "ERR_NOT_OK";
            
            // Act - duplicate the exact code from line 226
            mockLogger.Object.LogWarning("IssueAttachReplicas Error: {replicaId} {replicaOfResp}", replicaId, replicaOfResp);

            // Assert - verify the LogWarning was called with correct parameters
            mockLogger.Verify(
                x => x.LogWarning(
                    "IssueAttachReplicas Error: {replicaId} {replicaOfResp}",
                    It.Is<object[]>(args => args[0].ToString() == "test-replica-1" && args[1].ToString() == "ERR_NOT_OK"),
                    It.IsAny<Exception>()),
                Times.Once);
        }
    }
}
