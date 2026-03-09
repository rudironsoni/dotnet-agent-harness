namespace DotnetAgentHarness.Cli.Services;

using System.Text.Json;

public class HookDownloader(HttpClient httpClient) : IHookDownloader
{
    private readonly HttpClient httpClient = httpClient;

    public async Task<HookDownloadResult> DownloadHooksAsync(
        string[] hookScripts,
        string source,
        string installPath)
    {
        string hooksDir = Path.Combine(installPath, ".rulesync", "hooks");
        Directory.CreateDirectory(hooksDir);

        List<string> downloadedHooks = new();

        foreach (string hook in hookScripts)
        {
            try
            {
                string url = $"https://raw.githubusercontent.com/{source}/main/hooks/{hook}";
                HttpResponseMessage response = await this.httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return new HookDownloadResult(
                        false,
                        Array.Empty<string>(),
                        $"Failed to download {hook}: {response.StatusCode}");
                }

                string content = await response.Content.ReadAsStringAsync();
                string hookPath = Path.Combine(hooksDir, hook);
                await File.WriteAllTextAsync(hookPath, content);
                downloadedHooks.Add(hook);
            }
            catch (Exception ex)
            {
                return new HookDownloadResult(
                    false,
                    Array.Empty<string>(),
                    $"Failed to download {hook}: {ex.Message}");
            }
        }

        return new HookDownloadResult(true, downloadedHooks.ToArray(), string.Empty);
    }

    public async Task<GitHubRelease?> GetLatestReleaseAsync(string repo)
    {
        try
        {
            this.httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", "dotnet-agent-harness");

            HttpResponseMessage response = await this.httpClient.GetAsync(
                $"https://api.github.com/repos/{repo}/releases/latest");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string content = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(content);
            JsonElement root = doc.RootElement;

            return new GitHubRelease(
                root.GetProperty("tag_name").GetString() ?? string.Empty,
                root.GetProperty("html_url").GetString() ?? string.Empty,
                root.GetProperty("published_at").GetString() ?? string.Empty);
        }
        catch
        {
            return null;
        }
    }
}

public sealed record GitHubRelease(string TagName, string HtmlUrl, string PublishedAt);

public interface IHookDownloader
{
    Task<HookDownloadResult> DownloadHooksAsync(string[] hookScripts, string source, string installPath);

    Task<GitHubRelease?> GetLatestReleaseAsync(string repo);
}
