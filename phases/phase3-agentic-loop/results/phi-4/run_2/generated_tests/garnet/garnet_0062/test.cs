using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Garnet.cluster
{
    internal sealed partial class FailoverSession : IDisposable
    {
        private readonly ILogger logger;
        private readonly Func<string, byte[], Task> broadcastConfigAndRequestAttachAsync;
        private bool disposed = false;

        public FailoverSession(ILogger logger, Func<string, byte[], Task> broadcastConfigAndRequestAttachAsync)
        {
            this.logger = logger;
            this.broadcastConfigAndRequestAttachAsync = broadcastConfigAndRequestAttachAsync;
        }

        private async Task BroadcastConfigAndRequestAttachAsync(string replicaId, byte[] configByteArray)
        {
            await Task.Yield();
            var oldPrimaryId = oldConfig.LocalNodePrimaryId;
            var client = oldPrimaryId.Equals(replicaId) ? primaryClient : await GetConnectionAsync(replicaId);

            try
            {
                if (client == null)
                {
                    logger?.LogError("Failed to initialize connection to replica {replicaId}", replicaId);
                    return;
                }

                var resp = await client.GossipAsync(configByteArray).WaitAsync(failoverTimeout, cts.Token).ConfigureAwait(false);
                // Additional logic...
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "BroadcastConfigAndRequestAttachAsync Error");
                throw;
            }
        }

        public async Task PerformFailoverAsync()
        {
            var attachReplicaTasks = new List<Task>();

            foreach (var replicaId in replicaIds)
            {
                try
                {
                    attachReplicaTasks.Add(broadcastConfigAndRequestAttachAsync(replicaId, configByteArray));
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "IssueAttachReplicas Error");
                }
            }

            if (attachReplicaTasks.Count > 0)
            {
                try
                {
                    await Task.WhenAll(attachReplicaTasks).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "WaitingForAttachToComplete Error");
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                }
                disposed = true;
            }
        }
    }
}
