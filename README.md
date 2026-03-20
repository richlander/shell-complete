# ShellComplete

Generate shell completion scripts for [System.CommandLine](https://learn.microsoft.com/dotnet/standard/commandline/) apps. NAOT-friendly — no external tools required.

## Why not `dotnet-suggest`?

The official `dotnet-suggest` approach requires installing a separate .NET global tool (CoreCLR) just for tab completion. That doesn't work well for Native AOT apps that intentionally have zero .NET runtime dependencies.

ShellComplete follows the industry standard pattern used by Cargo, Cobra (Go), kubectl, docker, and gh — the app generates its own completion scripts directly.

## Usage

```csharp
using ShellComplete;

// Generate a script for a specific shell
string? script = CompletionScripts.Generate("my-tool", "bash");

// Or write directly to console with error handling
int exitCode = CompletionScripts.WriteToConsole("my-tool", args[0]);
```

### Typical CLI integration

```csharp
var completionCommand = new Command("completion", "Generate shell completion script");
completionCommand.Arguments.Add(new Argument<string>("shell"));
completionCommand.SetAction((parseResult, ct) =>
{
    string shell = parseResult.GetValue<string>("shell")!;
    return Task.FromResult(CompletionScripts.WriteToConsole("my-tool", shell));
});
```

### User setup

```bash
# bash — add to ~/.bashrc
eval "$(my-tool completion bash)"

# zsh — add to ~/.zshrc
eval "$(my-tool completion zsh)"

# fish
my-tool completion fish | source

# PowerShell — add to $PROFILE
my-tool completion powershell | Invoke-Expression
```

## Supported shells

- bash
- zsh
- fish
- PowerShell

## How it works

Scripts use System.CommandLine's `[suggest]` directive — the same mechanism `dotnet-suggest` uses, but invoked directly by the shell without a proxy tool.

## License

MIT
