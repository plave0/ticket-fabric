
using System.Diagnostics;
using System.Xml.Linq;

public static class PerformanceTracking
{
    public static void Main()
    {

        PerformanceCounter pcSf = new(
            categoryName: "Service Fabric Actor",
            counterName: "Average milliseconds per request",
            instanceName: "d178dd81-a18e-41a2-b33e-e2f8f2eba1f6_638598482467641237");

        PerformanceCounter pcCpu = new(
            categoryName: "Processor",
            counterName: "% Processor Time",
            instanceName: "_Total");

        PrintCounter(pcSf);

        var startTime = pcSf.NextSample().TimeStamp;
        Console.WriteLine($"Starting timestapm: {startTime}");
        Console.WriteLine($"{pcSf.NextSample().SystemFrequency}");
        Console.WriteLine($"{pcSf.NextSample().CounterFrequency}");

        while (true)
        {
            Console.WriteLine(pcSf.NextValue());
        }
    }

    public static void PrintCounter(PerformanceCounter pc)
    {
        Console.WriteLine(pc.CategoryName);
        Console.WriteLine(pc.CounterHelp);
        Console.WriteLine(pc.CounterName);
        Console.WriteLine(pc.CounterType);
        Console.WriteLine(pc.InstanceLifetime);
        Console.WriteLine(pc.InstanceName);
        Console.WriteLine(pc.MachineName);
    }


}