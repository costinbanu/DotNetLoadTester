using Microsoft.Extensions.Configuration;
using RandomTestValues;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace DotNetLoadTester
{
    public class Tester
    {
        private readonly Regex _regex;
        private readonly string _baseUrl;
        private readonly int _maxParallelThreads;
        private readonly int _maxSessionDurationMinutes;
        private readonly Regex _includePattern;
        private readonly Regex _excludePattern;
        private int _counter;

        public Tester(IConfiguration config)
        {
            _baseUrl = config.GetValue<string>("BaseUrl") ?? throw new Exception("BaseUrl is invalid");
            _maxParallelThreads = config.GetValue<int?>("MaxParallelThreads") ?? throw new Exception("MaxParallelThreads is invalid");
            _maxSessionDurationMinutes = config.GetValue<int?>("MaxDurationMinutes") ?? throw new Exception("MaxDurationMinutes is invalid");

            var include = config.GetValue<string>("IncludePattern");
            if (!string.IsNullOrWhiteSpace(include))
            {
                _includePattern = new Regex(include, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }

            var exclude = config.GetValue<string>("ExcludePattern");
            if (!string.IsNullOrWhiteSpace(exclude))
            {
                _excludePattern = new Regex(exclude, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }

            ServicePointManager.DefaultConnectionLimit = _maxParallelThreads;
            _regex = new Regex($"href=\"(\\/|\\.\\/|\\.\\.\\/|{Regex.Escape(_baseUrl)})(.*?)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _counter = 0;
        }

        public async Task Test()
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };

            await Task.WhenAll(RandomValue.Array<object>(_maxParallelThreads).Select(_ => Crawl(client)));
        }

        private async Task Crawl(HttpClient client)
        {
            var start = DateTime.UtcNow;
            var uri = "/";
            var cur = Interlocked.Increment(ref _counter);
            Console.WriteLine($"Starting thread #{cur}.");
            do
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, uri);
                    var result = await client.SendAsync(request);
                    result.EnsureSuccessStatusCode();
                    var html = HttpUtility.HtmlDecode(await result.Content.ReadAsStringAsync());
                    var urls = _regex.Matches(html).Select(x => x.Groups[1].Value + x.Groups[2].Value).Distinct();
                    if (_includePattern != null)
                    {
                        urls = urls.Where(x => _includePattern.IsMatch(x));
                    }
                    if (_excludePattern != null)
                    {
                        urls = urls.Where(x => !_excludePattern.IsMatch(x));
                    }
                    var urlList = urls.ToList();
                    uri = urlList[RandomValue.Int(urlList.Count - 1, 0)];
                    Thread.Sleep((1 + cur % 2) * 1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while running thread #{cur} on uri '{uri}': {ex.Message}.");
                    break;
                }
            }
            while (DateTime.UtcNow.Subtract(start).TotalMinutes < _maxSessionDurationMinutes);
            Console.WriteLine($"Thread #{cur} has exited.");
        }
    }
}