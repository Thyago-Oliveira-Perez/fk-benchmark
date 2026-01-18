```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.3 LTS (Noble Numbat)
11th Gen Intel Core i5-1135G7 2.40GHz (Max: 2.36GHz), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.122
  [Host]     : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v4
  Job-CKVGHD : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v4

IterationCount=100  RunStrategy=Monitoring  

```
| Method                            | Mean      | Error     | StdDev    | Median     | Allocated |
|---------------------------------- |----------:|----------:|----------:|-----------:|----------:|
| &#39;Simple SELECT - WITH FK&#39;         |  3.925 ms |  9.975 ms | 29.411 ms |  0.7465 ms |   6.25 KB |
| &#39;Simple SELECT - WITHOUT FK&#39;      |  4.015 ms |  9.289 ms | 27.389 ms |  0.8778 ms |   6.55 KB |
| &#39;Multi-Table JOIN - WITH FK&#39;      |  4.756 ms |  9.550 ms | 28.157 ms |  1.6236 ms |   6.94 KB |
| &#39;Multi-Table JOIN - WITHOUT FK&#39;   |  4.513 ms |  8.762 ms | 25.836 ms |  1.6227 ms |   7.24 KB |
| &#39;Aggregation Query - WITH FK&#39;     |  3.683 ms |  8.553 ms | 25.217 ms |  0.8629 ms |   4.55 KB |
| &#39;Aggregation Query - WITHOUT FK&#39;  |  3.874 ms |  9.433 ms | 27.813 ms |  0.8235 ms |   4.24 KB |
| &#39;Complex Analytical - WITH FK&#39;    | 26.215 ms | 10.569 ms | 31.163 ms | 23.2584 ms |   4.49 KB |
| &#39;Complex Analytical - WITHOUT FK&#39; | 26.340 ms | 10.957 ms | 32.307 ms | 23.5062 ms |    4.8 KB |
