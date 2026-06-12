namespace TCZPOS.Components.Services.Hardware
{
    public class DiscoveryService
    {
        private static readonly int[] CommonPorts = [7000, 7001, 7002, 5000, 5001];
        public static string GetApiUrl()
        {
#if DEBUG
            // DEBUG MODE: Look for your local Aspire / API Service
            // You can use the "Temp File" trick we discussed or a fixed local port
            string tempPath = Path.Combine(Path.GetTempPath(), "TCZPOS_api.txt");

            if (File.Exists(tempPath))
            {
                return File.ReadAllText(tempPath);
            }

            return "https://localhost:7001"; // Default local fallback
#else
        // PRODUCTION MODE: Your actual live server URL
        return "https://api.TCZPOS-solutions.com"; 
#endif
        }

        public static async Task<string> AutoDetectApiAsync()
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(500) };

            foreach (var port in CommonPorts)
            {
                try
                {
                    // We check your /health endpoint we defined in the ApiService
                    var response = await client.GetAsync($"https://localhost:{port}/health");
                    if (response.IsSuccessStatusCode)
                    {
                        return $"https://localhost:{port}";
                    }
                }
                catch { /* Keep looking... */ }
            }

            // Fallback to a default if nothing is found
            return "https://localhost:7001";
        }
    }
}
