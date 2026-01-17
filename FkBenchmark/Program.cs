using BenchmarkDotNet.Running;

namespace FkBenchmark;

class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<TransactionBenchmarks>();
    }
}
