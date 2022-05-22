// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using Collections.Pooled;
using Dispose.Scope;

var runner = BenchmarkRunner.Run<BenchmarkPooledScope>();

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class BenchmarkPooledScope
{
    public static readonly SomeClassEntity[] Database = Enumerable.Range(0, 10000).Select(i => new SomeClassEntity
    {
        F1 = i,
        F2 = i + 1,
        F3 = i.ToString()
    }).ToArray();

    [Benchmark(Baseline = true)]
    public int GetSomeClass()
    {
        var bag = new ConcurrentBag<long>();
        Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 4},
            i => { bag.Add(GetSomeClassApi().Count); });
        return bag.Count;
    }

    private List<SomeClassVm> GetSomeClassApi()
    {
        var entities = GetSomeClassFromDatabase();
        return entities.Select(s => new SomeClassVm
        {
            F1 = s.F1, F2 = s.F2, F3 = s.F3
        }).ToList();
    }

    private List<SomeClassEntity> GetSomeClassFromDatabase()
    {
        return Database.Where(c => true).ToList();
    }


    [Benchmark]
    public int GetSomeClassUsePooled()
    {
        var bag = new ConcurrentBag<long>();
        Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 4},
            i => { bag.Add(GetSomeClassApiUsePooled().Count); });
        return bag.Count;
    }

    private PooledList<SomeClassVm> GetSomeClassApiUsePooled()
    {
        var entities = GetSomeClassFromDatabaseUsePooled();
        return entities.Select(s => new SomeClassVm
        {
            F1 = s.F1, F2 = s.F2, F3 = s.F3
        }).ToPooledList();
    }

    private PooledList<SomeClassEntity> GetSomeClassFromDatabaseUsePooled()
    {
        return Database.Where(c => true).ToPooledList();
    }

    [Benchmark]
    public int GetSomeClassUsePooledUsing()
    {
        var bag = new ConcurrentBag<long>();
        Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 4}, i =>
        {
            using var vms = GetSomeClassApiUsePooledUsing();
            bag.Add(vms.Count);
        });
        return bag.Count;
    }

    private PooledList<SomeClassVm> GetSomeClassApiUsePooledUsing()
    {
        using var entities = GetSomeClassFromDatabaseUsePooledUsing();
        return entities.Select(s => new SomeClassVm
        {
            F1 = s.F1, F2 = s.F2, F3 = s.F3
        }).ToPooledList();
    }

    private PooledList<SomeClassEntity> GetSomeClassFromDatabaseUsePooledUsing()
    {
        return Database.Where(c => true).ToPooledList();
    }
    
    [Benchmark]
    public int GetSomeClassUsePooledScope()
    {
        var bag = new ConcurrentBag<long>();
        Parallel.For(0, 1000, new ParallelOptions {MaxDegreeOfParallelism = 4}, i =>
        {
            using (_ = DisposeScope.BeginScope())
            {
                var result = GetSomeClassApiUsePooledScope();
                bag.Add(result.Count);
            }
        });
        return bag.Count;
    }

    private PooledList<SomeClassVm> GetSomeClassApiUsePooledScope()
    {
        var entities = GetSomeClassFromDatabaseUsePooledScope();
        return entities.Select(s => new SomeClassVm
        {
            F1 = s.F1, F2 = s.F2, F3 = s.F3
        }).ToPooledListScope();
    }

    private PooledList<SomeClassEntity> GetSomeClassFromDatabaseUsePooledScope()
    {
        return Database.Where(c => true).ToPooledListScope();
    }
}


public class SomeClassEntity
{
    public int F1 { get; set; }

    public int F2 { get; set; }

    public string? F3 { get; set; }
}

public class SomeClassVm
{
    public int F1 { get; set; }

    public int F2 { get; set; }

    public string? F3 { get; set; }
}