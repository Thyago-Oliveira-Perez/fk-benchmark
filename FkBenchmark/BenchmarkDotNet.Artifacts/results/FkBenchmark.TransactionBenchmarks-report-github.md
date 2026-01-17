```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.3 LTS (Noble Numbat)
11th Gen Intel Core i5-1135G7 2.40GHz (Max: 1.46GHz), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.417
  [Host]     : .NET 8.0.23 (8.0.23, 8.0.2325.60607), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 8.0.23 (8.0.23, 8.0.2325.60607), X64 RyuJIT x86-64-v4


```
| Method                         | Mean       | Error     | StdDev    | Rank | Gen0      | Allocated |
|------------------------------- |-----------:|----------:|----------:|-----:|----------:|----------:|
| &#39;Batch Insert WITHOUT FK&#39;      |   114.2 ms |   2.24 ms |   3.56 ms |    1 | 1000.0000 |    4.2 MB |
| &#39;Batch Insert WITH FK&#39;         |   151.6 ms |   2.79 ms |   2.61 ms |    2 | 1000.0000 |    4.2 MB |
| &#39;Concurrent Insert WITHOUT FK&#39; |   817.2 ms |  24.57 ms |  67.67 ms |    3 | 1000.0000 |   4.19 MB |
| &#39;Concurrent Insert WITH FK&#39;    |   818.5 ms |  24.74 ms |  71.38 ms |    3 | 1000.0000 |   4.19 MB |
| &#39;Mixed Operations WITHOUT FK&#39;  | 2,697.4 ms |  63.19 ms | 180.27 ms |    4 |         - |   2.94 MB |
| &#39;Mixed Operations WITH FK&#39;     | 2,870.9 ms |  74.87 ms | 214.82 ms |    4 |         - |   2.94 MB |
| &#39;Sequential Insert WITHOUT FK&#39; | 5,012.4 ms | 119.40 ms | 340.66 ms |    5 | 1000.0000 |    4.2 MB |
| &#39;Sequential Insert WITH FK&#39;    | 5,286.2 ms | 122.30 ms | 344.96 ms |    5 | 1000.0000 |    4.2 MB |
