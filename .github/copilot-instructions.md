# Copilot Instructions for Markdown2Html

## Project Overview

**md2html** is a command-line Markdown-to-HTML converter built with .NET 10 and C#. It includes a custom recursive-descent Markdown parser, a visitor-pattern HTML renderer, and syntax highlighting — all with zero third-party runtime dependencies. The project is AOT-compatible and ships as both a .NET tool and native binaries.

## Build & Test

- **Build:** `dotnet build`
- **Run:** `dotnet run -- <args>` (e.g. `dotnet run -- README.md --open`)
- **Test:** `dotnet test .\Markdown2Html.Tests\Markdown2Html.Tests.csproj`
- **Pack (.NET tool):** `dotnet pack -c Release` → output in `nupkg/`
- **Publish native AOT (local):** `.\publish-aot.ps1` (builds for the current platform)
- **Publish native AOT (specific RID):** `.\publish-aot.ps1 -RuntimeIdentifiers win-x64`

## Architecture

All source code lives in `src/`. Key files:

| File | Responsibility |
|---|---|
| `Program.cs` | Entry point, wires up `App` |
| `App.cs` | Orchestrator: read input → parse → render → write output |
| `MarkdownParser.cs` | Recursive-descent parser, produces immutable `MarkdownDocument` AST |
| `MarkdownNodes.cs` | Immutable record types for the AST (blocks + inlines) |
| `InlineParser.cs` | Parses inline markdown (bold, italic, code, links, images, etc.) |
| `HtmlRenderer.cs` | Visitor-pattern AST → HTML fragment converter |
| `HtmlDocumentBuilder.cs` | Wraps HTML fragment in a full HTML5 document with embedded CSS |
| `SyntaxHighlighter.cs` | Token-based syntax highlighting for code blocks (8 languages) |
| `CommandLineParser.cs` | Stateless CLI argument parser |
| `CliOptions.cs` | CLI option data record |
| `TitleResolver.cs` | Resolves document title from AST or filename |
| `AnsiConsoleStyler.cs` | ANSI terminal styling for `--help` output |
| `AppInfo.cs` | Assembly version info via MinVer |

## Design Principles

- **No reflection.** The app is AOT-compiled; avoid patterns that rely on runtime reflection.
- **No third-party runtime packages.** All parsing, rendering, and highlighting is hand-written. MinVer is a build-only dependency.
- **Immutable AST.** All Markdown nodes are C# `record` types. Do not make them mutable.
- **Stateless parsers.** `MarkdownParser`, `InlineParser`, and `CommandLineParser` are static/stateless.
- **Visitor pattern for rendering.** New block/inline types must be handled in `HtmlRenderer.RenderBlock` / `RenderInline`.

## Versioning & Releases

- **MinVer 6.0.0** derives the version from git tags with prefix `v` (e.g. `v1.0.3` → version `1.0.3`).
- Pushing to `main` triggers the GitHub Actions workflow to build artifacts only (no release).
- **Pushing a `v*` tag** triggers the full release: builds native AOT binaries for 5 platforms (win-x64, win-arm64, linux-x64, osx-x64, osx-arm64) and publishes a GitHub Release with zip archives.
- To release: `git tag v<version>` then `git push origin v<version>`.

## CLI Options

```
md2html <input-file> [options]
cat file.md | md2html [options]

  -o, --output <path>   Write HTML to file instead of stdout.
  --open                Open the HTML file in the default browser.
  --github-style        Use GitHub-flavored light theme for HTML output.
  -h, --help            Show help.
  -v, --version         Show version.
```

## Testing

- Tests are in `Markdown2Html.Tests/AppTests.cs` using **xUnit**.
- Test project references the main project directly (no separate class library).
- Always run tests after making changes: `dotnet test .\Markdown2Html.Tests\Markdown2Html.Tests.csproj`
- When adding new markdown features, add corresponding rendering tests.

## Conventions

- Use file-scoped namespaces.
- Keep methods small and focused.
- Prefer `ReadOnlySpan<char>` / `StringComparison.Ordinal` for performance-critical parsing.
- CSS is embedded inline in `HtmlDocumentBuilder.cs` — there are two themes: default (serif) and GitHub-style (Primer light).
