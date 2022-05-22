``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.203
  [Host]     : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
  DefaultJob : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT


```
|                     Method |     Mean |   Error |  StdDev | Ratio | RatioSD |       Gen 0 |      Gen 1 |      Gen 2 | Allocated |
|--------------------------- |---------:|--------:|--------:|------:|--------:|------------:|-----------:|-----------:|----------:|
| GetSomeClassUsePooledUsing | 169.4 ms | 1.60 ms | 1.50 ms |  0.70 |    0.01 |  53333.3333 | 24333.3333 |          - |    305 MB |
| GetSomeClassUsePooledScope | 169.6 ms | 1.47 ms | 1.30 ms |  0.70 |    0.01 |  53000.0000 | 24333.3333 |          - |    306 MB |
|               GetSomeClass | 240.9 ms | 1.92 ms | 1.60 ms |  1.00 |    0.00 | 112333.3333 | 58000.0000 | 41333.3333 |    632 MB |
|      GetSomeClassUsePooled | 402.2 ms | 7.78 ms | 8.96 ms |  1.68 |    0.03 |  83000.0000 | 83000.0000 | 83000.0000 |    556 MB |
