using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Infrastrucre.Services
{
    public class UpstashRestCache : IDistributedCache
    {
        private readonly HttpClient _httpClient;

        public UpstashRestCache(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            var baseUrl = configuration["UpstashRedis:RestUrl"];
            var token = configuration["UpstashRedis:RestToken"];

            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Upstash REST URL or Token is missing in configuration.");
            }

            _httpClient.BaseAddress = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public byte[]? Get(string key) => GetAsync(key).GetAwaiter().GetResult();

        public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        {
            var response = await _httpClient.GetAsync($"get/{key}", token);
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync(token);
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.String)
            {
                var val = result.GetString();
                return val != null ? Convert.FromBase64String(val) : null;
            }

            return null;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
            SetAsync(key, value, options).GetAwaiter().GetResult();

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            var base64Value = Convert.ToBase64String(value);
            long ttlSeconds = 86400; // الافتراضي يوم واحد

            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                ttlSeconds = (long)options.AbsoluteExpirationRelativeToNow.Value.TotalSeconds;
            }

            // إرسال الأمر عبر Upstash REST API مع التحديد الزمي (EX)
            await _httpClient.PostAsync($"set/{key}/{base64Value}/EX/{ttlSeconds}", null, token);
        }

        public void Refresh(string key) { }
        public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

        public void Remove(string key) => RemoveAsync(key).GetAwaiter().GetResult();

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            await _httpClient.PostAsync($"del/{key}", null, token);
        }
    }
}