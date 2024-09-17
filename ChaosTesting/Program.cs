
using System.Fabric;
using System.Fabric.Chaos.DataStructures;
using System.Fabric.Health;
using System.Numerics;

static class ChaosTesting
{

    private class ChaosEventComparer : IEqualityComparer<ChaosEvent>
    {
        public bool Equals(ChaosEvent x, ChaosEvent y)
        {
            return x.TimeStampUtc.Equals(y.TimeStampUtc);
        }
        public int GetHashCode(ChaosEvent obj)
        {
            return obj.TimeStampUtc.GetHashCode();
        }
    }

    public static async Task TestChaos()
    {
        using (var client = new FabricClient("localhost:19000"))
        {
            var startTimeUtc = DateTime.UtcNow;
            var timeToRun = TimeSpan.FromMinutes(15.0);
            var stabilizationTimeout = TimeSpan.Zero;
            var maxConcurentFaults = 10;
            var startContext = new Dictionary<string, string> { { "ReasonForStart", "Testing" } };
            var waitTimeBetweenIterations = TimeSpan.Zero;
            var waitTimeBetweenFaults = TimeSpan.Zero;
            var clusterHealthPolicy = new ClusterHealthPolicy
            {
                ConsiderWarningAsError = true,
                MaxPercentUnhealthyApplications = 0,
                MaxPercentUnhealthyNodes = 0
            };

            var parameters = new ChaosParameters(
                maxClusterStabilizationTimeout: stabilizationTimeout,
                maxConcurrentFaults: maxConcurentFaults,
                enableMoveReplicaFaults: true, /* EnableMoveReplicaFault */
                timeToRun: timeToRun,
                context: startContext,
                waitTimeBetweenIterations: waitTimeBetweenIterations,
                waitTimeBetweenFaults: waitTimeBetweenFaults,
                clusterHealthPolicy: clusterHealthPolicy);
            { };


            try
            {
                await client.TestManager.StartChaosAsync(parameters);
            }
            catch (FabricChaosAlreadyRunningException)
            {
                Console.WriteLine("An instance of chaos is already running");
            }

            var filter = new ChaosReportFilter(startTimeUtc, DateTime.MaxValue);

            var eventSet = new HashSet<ChaosEvent>(new ChaosEventComparer());

            string continuationToken = null;

            while (true)
            {
                ChaosReport report;
                try
                {
                    report = string.IsNullOrEmpty(continuationToken)
                        ? await client.TestManager.GetChaosReportAsync(filter)
                        : await client.TestManager.GetChaosReportAsync(continuationToken);
                }
                catch (Exception e)
                {
                    if (e is FabricTransientException)
                    {
                        Console.WriteLine("A transient exception happened: '{0}'", e);
                    }
                    else if (e is TimeoutException)
                    {
                        Console.WriteLine("A timeout exception happened: '{0}'", e);
                    }
                    else
                    {
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1.0));
                    continue;
                }

                continuationToken = report.ContinuationToken;

                foreach (var chaosEvent in report.History)
                {
                    if (eventSet.Add(chaosEvent))
                    {
                        Console.WriteLine(chaosEvent);
                    }
                }

                // When Chaos stops, a StoppedEvent is created.
                // If a StoppedEvent is found, exit the loop.
                var lastEvent = report.History.LastOrDefault();

                if (lastEvent is StoppedEvent)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(1.0));
            }
        }
    }

    private class Restarter
    {
        public async Task RestartNodeAsync(string clusterUrl, BigInteger nodeId, string nodeName)
        {
            FabricClient fabricClient = new FabricClient(clusterUrl);
            await fabricClient.FaultManager.RestartNodeAsync(nodeName, nodeId, CompletionMode.Verify);
        }

        public async Task RestartReplicaAsync(string clusterUrl)
        {
            FabricClient fabricClient = new FabricClient(clusterUrl);

            PartitionSelector tripStoragePartition = PartitionSelector.PartitionIdOf(
                new Uri("fabric:/TicketFabric/TripStorage"), new Guid("47aead61-1eba-4b3b-b20f-3764d7eaf718"));

            ReplicaSelector replicaSelector = ReplicaSelector.PrimaryOf(tripStoragePartition);
            await fabricClient.FaultManager.RemoveReplicaAsync(replicaSelector, CompletionMode.Verify, true);
        }
    }

    public static async Task RunFaultAnalysis()
    {
        string clusterConnection = "localhost:19000";
        BigInteger nodeId = 133681137084377525;
        string nodeName = "_Node_3";

        try
        {
            Restarter restarter = new Restarter();
            // await restarter.RestartNodeAsync(clusterConnection, nodeId, nodeName);
            await restarter.RestartReplicaAsync(clusterConnection);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    static async Task Main(string[] args)
    {
        await TestChaos();
    }
}