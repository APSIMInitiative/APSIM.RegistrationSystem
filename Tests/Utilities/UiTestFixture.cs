using System.Diagnostics;
using Microsoft.Playwright;

namespace Tests.Utilities;

public sealed class UiTestFixture : IAsyncLifetime
{
    private Process? webAppProcess;
    private IPlaywright? playwright;
    private readonly List<string> processOutput = new();

    public string BaseUrl { get; private set; } = string.Empty;

    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        int port = GetAvailablePort();
        BaseUrl = $"http://127.0.0.1:{port}";

        string repositoryRoot = ResolveRepositoryRoot();
        string webAppProjectPath = Path.Combine(repositoryRoot, "RegistrationWebApp", "RegistrationWebApp.csproj");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --no-build --project \"{webAppProjectPath}\" --urls {BaseUrl}",
            WorkingDirectory = repositoryRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        // These values are only needed so WebApiUtility can be created successfully at startup.
        startInfo.Environment["WEB_API_URL"] = "http://127.0.0.1:1";
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        webAppProcess = new Process { StartInfo = startInfo };
        webAppProcess.OutputDataReceived += (_, args) => CaptureProcessOutput(args.Data);
        webAppProcess.ErrorDataReceived += (_, args) => CaptureProcessOutput(args.Data);

        if (!webAppProcess.Start())
        {
            throw new InvalidOperationException("Failed to start RegistrationWebApp process for UI tests.");
        }

        webAppProcess.BeginOutputReadLine();
        webAppProcess.BeginErrorReadLine();

        await WaitForWebAppReadyAsync(BaseUrl);

        playwright = await Playwright.CreateAsync();
        Browser = await LaunchChromiumAsync(playwright);
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.CloseAsync();
        }

        playwright?.Dispose();

        if (webAppProcess is not null && !webAppProcess.HasExited)
        {
            webAppProcess.Kill(entireProcessTree: true);
            await webAppProcess.WaitForExitAsync();
        }

        webAppProcess?.Dispose();
    }

    private static async Task<IBrowser> LaunchChromiumAsync(IPlaywright playwright)
    {
        try
        {
            // Prefer local Edge on Windows to avoid requiring playwright browser install for CI smoke tests.
            return await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Channel = "msedge",
            });
        }
        catch
        {
            return await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
            });
        }
    }

    private async Task WaitForWebAppReadyAsync(string baseUrl)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var timeoutAt = DateTime.UtcNow.AddSeconds(45);

        while (DateTime.UtcNow < timeoutAt)
        {
            if (webAppProcess is { HasExited: true })
            {
                string output = string.Join(Environment.NewLine, processOutput);
                throw new InvalidOperationException(
                    $"RegistrationWebApp process exited before becoming ready.{Environment.NewLine}{output}");
            }

            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(baseUrl);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
                // Keep retrying until timeout.
            }

            await Task.Delay(300);
        }

        string timedOutOutput = string.Join(Environment.NewLine, processOutput);
        throw new TimeoutException(
            $"Timed out waiting for RegistrationWebApp to be reachable at {baseUrl}.{Environment.NewLine}{timedOutOutput}");
    }

    private void CaptureProcessOutput(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        lock (processOutput)
        {
            if (processOutput.Count > 300)
            {
                processOutput.RemoveAt(0);
            }

            processOutput.Add(line);
        }
    }

    private static string ResolveRepositoryRoot()
    {
        string? current = AppContext.BaseDirectory;

        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "RegistrationSystem.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing RegistrationSystem.sln.");
    }

    private static int GetAvailablePort()
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
