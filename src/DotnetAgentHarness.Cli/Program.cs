using System.CommandLine;
using DotnetAgentHarness.Cli.Commands;
using DotnetAgentHarness.Cli.Services;
using DotnetAgentHarness.Cli.Utils;

namespace DotnetAgentHarness.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Setup dependency injection (manual for simplicity)
        var httpClient = new HttpClient();
        var processRunner = new ProcessRunner();
        
        var prerequisiteChecker = new PrerequisiteChecker(processRunner);
        var rulesyncRunner = new RulesyncRunner(processRunner);
        var configDetector = new ConfigDetector();
        var transactionManager = new TransactionManager();
        var hookDownloader = new HookDownloader(httpClient);

        var rootCommand = new RootCommand("Cross-platform installer for dotnet-agent-harness toolkit");
        
        rootCommand.AddCommand(new InstallCommand(
            prerequisiteChecker,
            rulesyncRunner,
            configDetector,
            transactionManager,
            hookDownloader));
        
        rootCommand.AddCommand(new UninstallCommand());
        rootCommand.AddCommand(new UpdateCommand(rulesyncRunner, hookDownloader));
        rootCommand.AddCommand(new SelfUpdateCommand());
        
        var versionCommand = new Command("version", "Show version information");
        versionCommand.SetHandler(() =>
        {
            Console.WriteLine("dotnet-agent-harness version 1.0.0");
            return Task.FromResult(0);
        });
        rootCommand.AddCommand(versionCommand);

        return await rootCommand.InvokeAsync(args);
    }
}
