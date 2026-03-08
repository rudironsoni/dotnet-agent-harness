namespace DotnetAgentHarness.Cli.Services;

public interface IHookDownloader
{
    Task<HookDownloadResult> DownloadHooksAsync(string[] hookNames, string sourceRepo, string targetPath);
}

public record HookDownloadResult(bool Success, string[] DownloadedHooks, string? ErrorMessage);

public class HookDownloader : IHookDownloader
{
    private readonly HttpClient _httpClient;

    public HookDownloader(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HookDownloadResult> DownloadHooksAsync(string[] hookNames, string sourceRepo, string targetPath)
    {
        var hooksDir = Path.Combine(targetPath, ".rulesync", "hooks");
        Directory.CreateDirectory(hooksDir);

        var downloaded = new List<string>();
        
        foreach (var hookName in hookNames)
        {
            var url = $"https://raw.githubusercontent.com/{sourceRepo}/main/.rulesync/hooks/{hookName}";
            var outputPath = Path.Combine(hooksDir, hookName);

            try
            {
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    return new HookDownloadResult(
                        false,
                        downloaded.ToArray(),
                        $"Failed to download {hookName}: HTTP {(int)response.StatusCode}"
                    );
                }

                var content = await response.Content.ReadAsStringAsync();
                
                // Validate it's a shell script
                if (!content.TrimStart().StartsWith("#!/"))
                {
                    return new HookDownloadResult(
                        false,
                        downloaded.ToArray(),
                        $"Downloaded file doesn't look like a shell script: {hookName}"
                    );
                }

                await File.WriteAllTextAsync(outputPath, content);
                downloaded.Add(hookName);
            }
            catch (Exception ex)
            {
                return new HookDownloadResult(
                    false,
                    downloaded.ToArray(),
                    $"Error downloading {hookName}: {ex.Message}"
                );
            }
        }

        return new HookDownloadResult(true, downloaded.ToArray(), null);
    }
}
