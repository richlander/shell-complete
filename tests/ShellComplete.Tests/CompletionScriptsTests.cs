using ShellComplete;
using Xunit;

namespace ShellComplete.Tests;

public class CompletionScriptsTests
{
    [Theory]
    [InlineData("bash")]
    [InlineData("zsh")]
    [InlineData("fish")]
    [InlineData("powershell")]
    public void Generate_ReturnsScript_ForSupportedShells(string shell)
    {
        string? script = CompletionScripts.Generate("my-tool", shell);

        Assert.NotNull(script);
        Assert.Contains("my-tool", script);
    }

    [Fact]
    public void Generate_ReturnsNull_ForUnsupportedShell()
    {
        string? script = CompletionScripts.Generate("my-tool", "cmd");

        Assert.Null(script);
    }

    [Fact]
    public void Generate_Bash_ContainsCompleteFunctionAndRegistration()
    {
        string? script = CompletionScripts.Generate("dotnet-install", "bash");

        Assert.NotNull(script);
        Assert.Contains("_dotnet_install_completions()", script);
        Assert.Contains("complete -F _dotnet_install_completions dotnet-install", script);
        Assert.Contains("[suggest:", script);
    }

    [Fact]
    public void Generate_Zsh_ContainsCompdefRegistration()
    {
        string? script = CompletionScripts.Generate("dotnet-install", "zsh");

        Assert.NotNull(script);
        Assert.Contains("compdef _dotnet_install dotnet-install", script);
    }

    [Fact]
    public void Generate_Fish_ContainsCompleteCommand()
    {
        string? script = CompletionScripts.Generate("dotnet-inspect", "fish");

        Assert.NotNull(script);
        Assert.Contains("complete -c dotnet-inspect", script);
    }

    [Fact]
    public void Generate_PowerShell_ContainsArgumentCompleter()
    {
        string? script = CompletionScripts.Generate("dotnet-install", "powershell");

        Assert.NotNull(script);
        Assert.Contains("Register-ArgumentCompleter -Native -CommandName dotnet-install", script);
    }

    [Fact]
    public void Generate_Pwsh_IsSameAsPowerShell()
    {
        string? ps = CompletionScripts.Generate("my-tool", "powershell");
        string? pwsh = CompletionScripts.Generate("my-tool", "pwsh");

        Assert.Equal(ps, pwsh);
    }

    [Fact]
    public void Generate_IsCaseInsensitive()
    {
        string? lower = CompletionScripts.Generate("my-tool", "bash");
        string? upper = CompletionScripts.Generate("my-tool", "BASH");
        string? mixed = CompletionScripts.Generate("my-tool", "Bash");

        Assert.Equal(lower, upper);
        Assert.Equal(lower, mixed);
    }

    [Theory]
    [InlineData("bash")]
    [InlineData("zsh")]
    [InlineData("fish")]
    [InlineData("powershell")]
    public void Generate_SanitizesHyphensInFunctionNames(string shell)
    {
        string? script = CompletionScripts.Generate("my-cool-tool", shell);

        Assert.NotNull(script);
        // Function names should use underscores, not hyphens
        Assert.DoesNotContain("my-cool-tool()", script);
        Assert.DoesNotContain("my-cool-tool {", script);
    }

    [Fact]
    public void SupportedShells_ContainsExpectedShells()
    {
        Assert.Contains("bash", CompletionScripts.SupportedShells);
        Assert.Contains("zsh", CompletionScripts.SupportedShells);
        Assert.Contains("fish", CompletionScripts.SupportedShells);
        Assert.Contains("powershell", CompletionScripts.SupportedShells);
    }
}
