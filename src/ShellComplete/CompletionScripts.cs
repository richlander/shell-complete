namespace ShellComplete;

/// <summary>
/// Generates shell completion scripts that use System.CommandLine's [suggest] directive.
/// Scripts are parameterized by command name so any S.CL app can use them.
/// </summary>
public static class CompletionScripts
{
    /// <summary>
    /// Generate a shell completion script for the given command name.
    /// </summary>
    /// <param name="commandName">The CLI command name (e.g. "dotnet-install")</param>
    /// <param name="shell">Shell type: bash, zsh, fish, powershell, pwsh</param>
    /// <returns>The completion script, or null if the shell is not supported.</returns>
    public static string? Generate(string commandName, string shell)
    {
        return shell.ToLowerInvariant() switch
        {
            "bash" => Bash(commandName),
            "zsh" => Zsh(commandName),
            "fish" => Fish(commandName),
            "powershell" or "pwsh" => PowerShell(commandName),
            _ => null
        };
    }

    /// <summary>
    /// Write a completion script to stdout, returning an exit code.
    /// Writes an error to stderr if the shell is unsupported.
    /// </summary>
    public static int WriteToConsole(string commandName, string shell)
    {
        string? script = Generate(commandName, shell);

        if (script is null)
        {
            Console.Error.WriteLine($"error: unsupported shell '{shell}'");
            Console.Error.WriteLine("Supported: bash, zsh, fish, powershell");
            return 1;
        }

        Console.Write(script);
        return 0;
    }

    /// <summary>Supported shell names.</summary>
    public static IReadOnlyList<string> SupportedShells { get; } =
        ["bash", "zsh", "fish", "powershell"];

    static string Bash(string cmd) => $$"""
        # bash completion for {{cmd}}
        # Add to ~/.bashrc: eval "$({{cmd}} completion bash)"
        _{{Sanitize(cmd)}}_completions() {
            local cur="${COMP_WORDS[COMP_CWORD]}"
            local words="${COMP_WORDS[*]}"
            local position=$COMP_POINT

            COMPREPLY=()
            local suggestions
            suggestions=$({{cmd}} "[suggest:$position]" $words 2>/dev/null)
            if [ $? -eq 0 ]; then
                COMPREPLY=( $(compgen -W "$suggestions" -- "$cur") )
            fi
        }
        complete -F _{{Sanitize(cmd)}}_completions {{cmd}}
        """;

    static string Zsh(string cmd) => $$"""
        # zsh completion for {{cmd}}
        # Add to ~/.zshrc: eval "$({{cmd}} completion zsh)"
        _{{Sanitize(cmd)}}() {
            local -a completions
            local words="${words[*]}"
            local position=$CURSOR

            completions=(${(f)"$({{cmd}} "[suggest:$position]" $words 2>/dev/null)"})
            compadd -a completions
        }
        compdef _{{Sanitize(cmd)}} {{cmd}}
        """;

    static string Fish(string cmd) => $$"""
        # fish completion for {{cmd}}
        # Add to fish config: {{cmd}} completion fish | source
        complete -c {{cmd}} -f -a '(
            set -l words (commandline -cop)
            set -l position (commandline -C)
            {{cmd}} "[suggest:$position]" $words 2>/dev/null
        )'
        """;

    static string PowerShell(string cmd) => $$"""
        # PowerShell completion for {{cmd}}
        # Add to $PROFILE: {{cmd}} completion powershell | Invoke-Expression
        Register-ArgumentCompleter -Native -CommandName {{cmd}} -ScriptBlock {
            param($wordToComplete, $commandAst, $cursorPosition)
            $args = $commandAst.ToString().Split()
            $suggestions = & {{cmd}} "[suggest:$cursorPosition]" @args 2>$null
            $suggestions | ForEach-Object {
                [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
            }
        }
        """;

    static string Sanitize(string name) => name.Replace('-', '_');
}
