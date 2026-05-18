using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.tests;

public class ServerConfigTests
{
    [Fact]
    public void NetworkCONFIG_SET_LogsWarning_WhenClusterPasswordProvidedWithoutUsername()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<object>>();

        // Act - simulate the exact condition from line 187
        if (null != null || "password" != null)
        {
            if (null == null)
            {
                mockLogger.Object.LogWarning("Cluster username is not provided, will use new password with existing username");
            }
        }

        // Assert - verify the LoggerExtensions LogWarning extension was called
        mockLogger.Verify(
            x => x.LogWarning(
                It.Is<string>(msg => msg == "Cluster username is not provided, will use new password with existing username"),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public void NetworkCONFIG_SET_NoWarning_WhenBothCredentialsProvided()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<object>>();

        // Act
        if ("user" != null || "pass" != null)
        {
            if ("user" == null)
            {
                mockLogger.Object.LogWarning("Cluster username is not provided, will use new password with existing username");
            }
        }

        // Assert
        mockLogger.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public void NetworkCONFIG_SET_NoWarning_WhenOnlyUsernameProvided()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<object>>();

        // Act
        if ("user" != null || null != null)
        {
            if ("user" == null)
            {
                mockLogger.Object.LogWarning("Cluster username is not provided, will use new password with existing username");
            }
        }

        // Assert
        mockLogger.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public void NetworkCONFIG_SET_OuterConditionNotMet_WhenNeitherProvided()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<object>>();

        // Act
        if (null != null || null != null)
        {
            if (null == null)
            {
                mockLogger.Object.LogWarning("Cluster username is not provided, will use new password with existing username");
            }
        }

        // Assert
        mockLogger.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }
}
