using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System;
using System.Collections.Generic;

public class RespServerSessionTests
{
    [Fact]
    public void NetworkCONFIG_SET_LogsWarning_WhenClusterUsernameIsNotProvided()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var storeWrapper = new StoreWrapper
        {
            clusterProvider = new ClusterProvider(),
            serverOptions = new ServerOptions()
        };
        var session = new RespServerSession(storeWrapper, mockLogger.Object);

        // Act
        session.NetworkCONFIG_SET();

        // Assert
        mockLogger.Verify(
            logger => logger.LogWarning("Cluster username is not provided, will use new password with existing username"),
            Times.Once);
    }
}

public class StoreWrapper
{
    public ClusterProvider clusterProvider { get; set; }
    public ServerOptions serverOptions { get; set; }
}

public class ClusterProvider
{
    public void UpdateClusterAuth(string username, string password)
    {
        // Mock implementation
    }
}

public class ServerOptions
{
    public TlsOptions TlsOptions { get; set; }
}

public class TlsOptions
{
    public bool UpdateCertFile(string certFileName, string certPassword, out string errorMessage)
    {
        errorMessage = string.Empty;
        return true;
    }
}

public class RespServerSession : ServerSessionBase
{
    private readonly StoreWrapper storeWrapper;
    private readonly ILogger logger;

    public RespServerSession(StoreWrapper storeWrapper, ILogger logger)
    {
        this.storeWrapper = storeWrapper;
        this.logger = logger;
    }

    public bool NetworkCONFIG_SET()
    {
        string certFileName = null;
        string certPassword = null;
        string clusterUsername = null;
        string clusterPassword = null;
        string memorySize = null;
        string objLogMemory = null;
        string objHeapMemory = null;
        string index = null;
        string objIndex = null;

        var unknownOption = false;
        var unknownKey = "";

        for (var c = 0; c < parseState.Count; c += 2)
        {
            var key = parseState.GetArgSliceByRef(c).ReadOnlySpan;
            var value = parseState.GetArgSliceByRef(c + 1).ReadOnlySpan;

            if (key.EqualsLowerCaseSpanIgnoringCase(CmdStrings.Memory, allowNonAlphabeticChars: false))
                memorySize = Encoding.ASCII.GetString(value);
            else if (key.EqualsLowerCaseSpanIgnoringCase(CmdStrings.ObjLogMemory, allowNonAlphabeticChars: true))
                objLogMemory = Encoding.ASCII.GetString(value);
            else if (key.EqualsLowerCaseSpanIgnoringCase(CmdStrings.ObjHeapMemory, allowNonAlphabeticChars: true))
                objHeapMemory = Encoding.ASCII.GetString(value);
            else if (key.EqualsLowerCaseSpanIgnoringCase(CmdStrings.Index, allowNonAlphabeticChars: false))
                index = Encoding.ASCII.GetString(value);
            else if (key.EqualsLowerCaseSpanIgnoringCase(CmdStrings.ObjIndex, allowNonAlphabeticChars: true))
                objIndex = Encoding.ASCII.GetString(value);
            else if (key.EqualsLowerCaseSpanIgnoringCase(CmdStrings.CertFileName, allowNonAlphabeticChars: true))
                certFileName = Encoding.ASCII.GetString(value);
            else if (key.EqualsLowerCaseSpanIgnoringCase(CmdStrings.CertPassword, allowNonAlphabeticChars: true))
                certPassword = Encoding.ASCII.GetString(value);
            else if (key.EqualsLowerCaseSpanIgnoringCase(CmdStrings.ClusterUsername, allowNonAlphabeticChars: true))
                clusterUsername = Encoding.ASCII.GetString(value);
            else if (key.EqualsLowerCaseSpanIgnoringCase(CmdStrings.ClusterPassword, allowNonAlphabeticChars: true))
                clusterPassword = Encoding.ASCII.GetString(value);
            else
            {
                if (!unknownOption)
                {
                    unknownOption = true;
                    unknownKey = Encoding.ASCII.GetString(key);
                }
            }
        }

        var sbErrorMsg = new StringBuilder();

        if (unknownOption)
        {
            AppendError(sbErrorMsg, string.Format(CmdStrings.GenericErrUnknownOptionConfigSet, unknownKey));
        }
        else
        {
            if (clusterUsername != null || clusterPassword != null)
            {
                if (clusterUsername == null)
                    logger?.LogWarning("Cluster username is not provided, will use new password with existing username");
                if (storeWrapper.clusterProvider != null)
                    storeWrapper.clusterProvider?.UpdateClusterAuth(clusterUsername, clusterPassword);
                else
                {
                    AppendError(sbErrorMsg, "ERR Cluster is disabled.");
                }
            }

            if (certFileName != null || certPassword != null)
            {
                if (storeWrapper.serverOptions.TlsOptions != null)
                {
                    if (!storeWrapper.serverOptions.TlsOptions.UpdateCertFile(certFileName, certPassword, out var certErrorMessage))
                    {
                        AppendError(sbErrorMsg, certErrorMessage);
                    }
                }
                else
                {
                    sbErrorMsg.AppendLine("ERR TLS is disabled.");
                }
            }

            if (memorySize != null)
                HandleMemorySizeChange(memorySize, sbErrorMsg);

            if (objLogMemory != null)
                HandleMemorySizeChange(objLogMemory, sbErrorMsg, mainStore: false);

            if (index != null)
        }

        return true;
    }

    private void AppendError(StringBuilder sbErrorMsg, string errorMessage)
    {
        // Mock implementation
    }

    private void HandleMemorySizeChange(string memorySize, StringBuilder sbErrorMsg, bool mainStore = true)
    {
        // Mock implementation
    }

    private ParseState parseState = new ParseState();
}

public class ServerSessionBase
{
    // Mock implementation
}

public class ParseState
{
    public int Count { get; set; }
    public ReadOnlySpan<byte> GetArgSliceByRef(int index)
    {
        return new ReadOnlySpan<byte>();
    }
}

public static class CmdStrings
{
    public static string GenericErrUnknownOptionConfigSet = "Unknown option: {0}";
    public static string Memory = "memory";
    public static string ObjLogMemory = "objLogMemory";
    public static string ObjHeapMemory = "objHeapMemory";
    public static string Index = "index";
    public static string ObjIndex = "objIndex";
    public static string CertFileName = "certFileName";
    public static string CertPassword = "certPassword";
    public static string ClusterUsername = "clusterUsername";
    public static string ClusterPassword = "clusterPassword";
}

public static class StringExtensions
{
    public static bool EqualsLowerCaseSpanIgnoringCase(this ReadOnlySpan<byte> span, string value, bool allowNonAlphabeticChars)
    {
        return span.ToString().Equals(value, StringComparison.OrdinalIgnoreCase);
    }
}
