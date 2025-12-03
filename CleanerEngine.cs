using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CodeTweakrsNetCleaner
{
    public class CleanerEngine
    {
        private Dictionary<string, List<string>> _paths = new();

        public async Task InitializeAsync()
        {
            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("CodeTweakrsNetCleaner.junkcfg.json")
                ?? throw new Exception("Embedded config not found.");

            using var reader = new StreamReader(stream);
            string json = await reader.ReadToEndAsync();
            _paths = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json)
                     ?? new Dictionary<string, List<string>>();
        }

        public async Task<(int count, double size)> AnalyzeAsync(IProgress<string>? progress = null, int maxThreads = 0)
        {
            return await Task.Run(() =>
            {
                int total = 0;
                double totalSize = 0;
                object sizeLock = new();

                var opts = new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxThreads > 0 ? maxThreads : Math.Max(1, Environment.ProcessorCount / 2)
                };

                Parallel.ForEach(_paths.Values, opts, pathList =>
                {
                    foreach (var p in pathList)
                    {
                        string expanded = Environment.ExpandEnvironmentVariables(p);
                        if (expanded.Contains("powershell:")) continue;

                        try
                        {
                            if (Directory.Exists(expanded))
                            {
                                var files = Directory.GetFiles(expanded, "*", SearchOption.AllDirectories);
                                Interlocked.Add(ref total, files.Length);

                                double localSize = 0;
                                foreach (var f in files)
                                {
                                    try { localSize += new FileInfo(f).Length / 1024d / 1024d; } catch { }
                                }

                                lock (sizeLock) totalSize += localSize;
                            }
                        }
                        catch { }
                    }
                });

                progress?.Report($"Found ~{total:N0} files totaling {totalSize:F2} MB.");
                return (total, totalSize);
            });
        }

        public async Task CleanAsync(IProgress<string>? progress = null, int maxThreads = 0)
        {
            await Task.Run(() =>
            {
                int cleanedCategories = 0;
                var opts = new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxThreads > 0 ? maxThreads : Math.Max(1, Environment.ProcessorCount / 2)
                };

                Parallel.ForEach(_paths, opts, kvp =>
                {
                    bool any = false;
                    foreach (var p in kvp.Value)
                    {
                        string expanded = Environment.ExpandEnvironmentVariables(p);

                        try
                        {
                            if (expanded.StartsWith("powershell:", StringComparison.OrdinalIgnoreCase))
                            {
                                string cmd = expanded.Replace("powershell:", "");
                                var psi = new System.Diagnostics.ProcessStartInfo("powershell", $"-NoLogo -NoProfile -Command \"{cmd}\"")
                                {
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                };
                                System.Diagnostics.Process.Start(psi)?.WaitForExit();
                                any = true;
                            }
                            else if (Directory.Exists(expanded))
                            {
                                Directory.Delete(expanded, true);
                                any = true;
                            }
                        }
                        catch { }
                    }

                    if (any) Interlocked.Increment(ref cleanedCategories);
                });

                progress?.Report($"Cleanup completed — {cleanedCategories} sections cleared.");
            });
        }
    }
}
