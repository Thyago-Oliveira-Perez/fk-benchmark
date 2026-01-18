```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.3 LTS (Noble Numbat)
11th Gen Intel Core i5-1135G7 2.40GHz (Max: 2.36GHz), 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.122
  [Host]     : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v4
  Job-VNIRUV : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v4

IterationCount=20  

```
| Method                                  | Mean       | Error       | StdDev      | Median     | Allocated |
|---------------------------------------- |-----------:|------------:|------------:|-----------:|----------:|
| &#39;CASCADE Delete (100 txns) - WITH FK&#39;   |  44.693 ms |   0.5813 ms |   0.6694 ms |  44.746 ms |   8.38 KB |
| &#39;Manual Delete (100 txns) - WITHOUT FK&#39; |  47.655 ms |   0.7229 ms |   0.8035 ms |  47.464 ms |   9.72 KB |
| &#39;CASCADE Delete (1K txns) - WITH FK&#39;    |  61.905 ms |   3.1951 ms |   3.6795 ms |  59.888 ms |   8.35 KB |
| &#39;Manual Delete (1K txns) - WITHOUT FK&#39;  |  43.176 ms |   0.8002 ms |   0.9215 ms |  43.292 ms |   9.72 KB |
| &#39;CASCADE Delete (10K txns) - WITH FK&#39;   | 326.730 ms |   6.6990 ms |   7.1679 ms | 325.857 ms |   8.44 KB |
| &#39;Manual Delete (10K txns) - WITHOUT FK&#39; | 249.792 ms | 114.5645 ms | 122.5827 ms | 198.678 ms |   9.72 KB |
| &#39;Soft Delete (no CASCADE) - WITH FK&#39;    |   9.751 ms |   0.2150 ms |   0.2111 ms |   9.804 ms |   2.95 KB |
| &#39;Soft Delete (no CASCADE) - WITHOUT FK&#39; |  10.158 ms |   0.2548 ms |   0.2617 ms |  10.089 ms |    3.1 KB |
