using Microsoft.Extensions.Logging;

namespace Garnet.server
{
    internal sealed partial class RespServerSession : ServerSessionBase
    {
        private readonly ILogger<RespServerSession> _logger;

        public RespServerSession(ILogger<RespServerSession> logger)
        {
            _logger = logger;
        }

        private unsafe bool NetworkCONFIG_SET()
        {
            if (parseState.Count == 0 || parseState.Count % 2 != 0)
            {
                return AbortWithWrongNumberOfArguments($"{nameof(RespCommand.CONFIG)}|{nameof(CmdStrings.SET)}");
            }

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
                        _logger?.LogWarning("Cluster username is not provided, will use new password with existing username");
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
                    // Handle index change
                if (objIndex != null)
                    // Handle objIndex change
            }

            return true;
        }
    }
}
