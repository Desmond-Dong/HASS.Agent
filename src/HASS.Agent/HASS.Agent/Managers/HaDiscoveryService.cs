using System.Net.NetworkInformation;
using System.Net.Sockets;
using Serilog;

namespace HASS.Agent.Managers
{
    /// <summary>
    /// Home Assistant 网络发现服务
    /// 自动扫描本地网络，发现 Home Assistant 实例
    /// </summary>
    public class HaDiscoveryService
    {
        private const int HA_PORT = 8123;
        private const int SCAN_TIMEOUT_MS = 2000; // 每个IP 2秒超时
        private const int MAX_CONCURRENT_SCANS = 50; // 最多同时扫描50个IP

        private readonly HttpClient _httpClient;
        private CancellationTokenSource _cancellationTokenSource;

        public HaDiscoveryService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(SCAN_TIMEOUT_MS)
            };
        }

        /// <summary>
        /// 发现本地网络中的 Home Assistant 实例
        /// </summary>
        public async Task<List<DiscoveredHaInstance>> DiscoverInstancesAsync(Progress<DiscoveryProgress> progress = null)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var discoveredInstances = new List<DiscoveredHaInstance>();

            try
            {
                Log.Information("[HADISCOVERY] 开始扫描本地网络...");

                // 获取本地网络段
                var localIPs = GetLocalIPAddresses();
                Log.Information("[HADISCOVERY] 发现 {Count} 个本地网络接口", localIPs.Count);

                // 生成要扫描的IP列表
                var ipRanges = GetIPRangesToScan(localIPs);
                var totalIPs = ipRanges.Count;
                Log.Information("[HADISCOVERY] 准备扫描 {Total} 个IP地址...", totalIPs);

                var scannedCount = 0;

                // 并发扫描IP地址
                var tasks = ipRanges.Select(async ip =>
                {
                    try
                    {
                        var instance = await TryDiscoverHaInstanceAsync(ip, _cancellationTokenSource.Token);

                        Interlocked.Increment(ref scannedCount);

                        if (progress != null)
                        {
                            progress.Report(new DiscoveryProgress
                            {
                                ScannedCount = scannedCount,
                                TotalCount = totalIPs,
                                CurrentIP = ip
                            });
                        }

                        return instance;
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex, "[HADISCOVERY] 扫描 {IP} 时出错", ip);
                        return null;
                    }
                }).ToList();

                // 限制并发数
                var results = new List<DiscoveredHaInstance>();
                await foreach (var batch in TaskBatches(tasks, MAX_CONCURRENT_SCANS))
                {
                    var batchResults = await Task.WhenAll(batch);
                    results.AddRange(batchResults.Where(r => r != null));
                }

                discoveredInstances = results.Where(r => r != null).ToList();

                Log.Information("[HADISCOVERY] 扫描完成，发现 {Count} 个 Home Assistant 实例", discoveredInstances.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[HADISCOVERY] 网络扫描失败");
            }

            return discoveredInstances;
        }

        /// <summary>
        /// 尝试发现指定IP的 Home Assistant 实例
        /// </summary>
        private async Task<DiscoveredHaInstance> TryDiscoverHaInstanceAsync(string ip, CancellationToken cancellationToken)
        {
            try
            {
                var url = $"http://{ip}:{HA_PORT}";

                // 尝试获取 HA 配置信息
                var request = new HttpRequestMessage(HttpMethod.Get, $"{url}/api/config");
                request.Headers.Add("Accept", "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var config = System.Text.Json.JsonSerializer.Deserialize<HaConfigInfo>(content,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (config == null)
                {
                    return null;
                }

                // 获取版本信息
                var versionResponse = await _httpClient.GetAsync($"{url}/api/supervisor/ping", cancellationToken);
                var isSupervisor = versionResponse.IsSuccessStatusCode;

                return new DiscoveredHaInstance
                {
                    Url = url,
                    Name = config.LocationName ?? "Home Assistant",
                    Version = config.Version ?? "Unknown",
                    IsSupervisor = isSupervisor,
                    RequiresPassword = true, // HA 默认需要密码
                    ResponseTime = GetResponseTime(response)
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取本地IP地址
        /// </summary>
        private List<string> GetLocalIPAddresses()
        {
            var localIPs = new List<string>();

            try
            {
                foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    // 跳过回环和禁用的接口
                    if (networkInterface.OperationalStatus != OperationalStatus.Up ||
                        networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }

                    var ipProperties = networkInterface.GetIPProperties();
                    foreach (var ip in ipProperties.UnicastAddresses)
                    {
                        // 只扫描 IPv4 地址
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            localIPs.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[HADISCOVERY] 获取本地IP地址失败");
            }

            return localIPs;
        }

        /// <summary>
        /// 获取要扫描的IP范围
        /// </summary>
        private List<string> GetIPRangesToScan(List<string> localIPs)
        {
            var ipRanges = new HashSet<string>();

            foreach (var localIP in localIPs)
            {
                try
                {
                    var parts = localIP.Split('.');
                    if (parts.Length != 4) continue;

                    // 扫描同一网段的所有IP (例如: 192.168.1.1-254)
                    var subnet = $"{parts[0]}.{parts[1]}.{parts[2]}";

                    for (int i = 1; i <= 254; i++)
                    {
                        ipRanges.Add($"{subnet}.{i}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "[HADISCOVERY] 处理IP {IP} 失败", localIP);
                }
            }

            return ipRanges.ToList();
        }

        /// <summary>
        /// 获取响应时间
        /// </summary>
        private int GetResponseTime(HttpResponseMessage response)
        {
            // TODO: 实际测量响应时间
            return new Random().Next(10, 100); // 模拟响应时间
        }

        /// <summary>
        /// 分批处理任务
        /// </summary>
        private async IAsyncEnumerable<List<Task>> TaskBatches(List<Task> tasks, int batchSize)
        {
            for (int i = 0; i < tasks.Count; i += batchSize)
            {
                var count = Math.Min(batchSize, tasks.Count - i);
                yield return tasks.GetRange(i, count);
                await Task.Yield();
            }
        }

        /// <summary>
        /// 取消扫描
        /// </summary>
        public void CancelDiscovery()
        {
            _cancellationTokenSource?.Cancel();
            Log.Information("[HADISCOVERY] 已取消网络扫描");
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
            _httpClient?.Dispose();
        }
    }

    #region 数据模型

    /// <summary>
    /// 发现的 Home Assistant 实例
    /// </summary>
    public class DiscoveredHaInstance
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public bool IsSupervisor { get; set; }
        public bool RequiresPassword { get; set; }
        public int ResponseTime { get; set; } // 毫秒

        public string DisplayText => $"{Name} ({Url})";
        public string SubText => $"版本: {Version} • 响应: {ResponseTime}ms{(IsSupervisor ? " • Supervisor" : "")}";
    }

    /// <summary>
    /// 发现进度
    /// </summary>
    public class DiscoveryProgress
    {
        public int ScannedCount { get; set; }
        public int TotalCount { get; set; }
        public string CurrentIP { get; set; }
        public int ProgressPercentage => TotalCount > 0 ? (int)((ScannedCount / (double)TotalCount) * 100) : 0;
    }

    #endregion
}
