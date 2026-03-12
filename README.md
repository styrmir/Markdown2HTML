# Markdown2Html

Markdown2Html is a .NET 10 command-line app that converts a supported subset of Markdown into a full HTML document.

The project currently includes:

- file input and stdin support
- file output and stdout support
- a custom markdown parser and HTML renderer
- HTML document generation with basic built-in styling
- company branding in help output and generated HTML documents
- packaging as a .NET tool with the command name `markdown2html`
- automated tests for CLI behavior and rendering output

## Company Information

- Company: T1/Abra ehf
- Website: https://abra.is

This information is shown in two places:

- in the command help output shown by `markdown2html --help`
- at the top of generated HTML documents

## Usage

```powershell
dotnet run -- --input README.md --output README.html
dotnet run -- README.md > README.html
Get-Content README.md | dotnet run --
```

You can also use the packaged tool command after installation:

```powershell
markdown2html --input README.md --output README.html
markdown2html README.md > README.html
Get-Content README.md | markdown2html
```

## Command Line Options

- `-i`, `--input <file>`: read markdown from a file
- `-o`, `--output <file>`: write HTML to a file instead of stdout
- `--open`: open the generated HTML file after conversion completes
- `-h`, `--help`: show help text

`--open` is intended for file output and should be used together with `--output`.

Example:

```powershell
markdown2html --input README.md --output README.html --open
```

If the app is run without parameters and without piped stdin, it shows:

- the app name
- the company information
- the message `No input was provided. Use --input <file> or pipe markdown through stdin.`
- the normal help text

## Install As A Tool

```powershell
dotnet pack
dotnet tool install --global --add-source .\nupkg Markdown2Html
markdown2html --help
```

For a local, repo-scoped install during development:

```powershell
dotnet pack
dotnet tool install --tool-path .\.tool-test --add-source .\nupkg Markdown2Html --version 1.0.1
.\.tool-test\markdown2html --help
```

## Releases

Prebuilt standalone binaries are available from the GitHub Releases page:

- https://github.com/styrmir/Markdown2HTML/releases

Each release can include platform-specific zip files such as:

- Windows x64
- Windows ARM64
- Linux x64
- macOS x64
- macOS ARM64

## Standalone AOT Publish

The project is configured so it can be published as a standalone, single-file Native AOT executable.

One-off example for Windows x64:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishAot=true -p:PublishSingleFile=true -o .\publish\win-x64
```

One-off example for Linux x64:

```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishAot=true -p:PublishSingleFile=true -o ./publish/linux-x64
```

One-off example for macOS Intel:

```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishAot=true -p:PublishSingleFile=true -o ./publish/osx-x64
```

One-off example for macOS Apple Silicon:

```bash
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishAot=true -p:PublishSingleFile=true -o ./publish/osx-arm64
```

The default publish output should go into the `publish` directory.

### PowerShell Publish Script

The repository includes [publish-aot.ps1](publish-aot.ps1) to publish the requested targets into `publish/<rid>`.

Default behavior:

- Windows host: publishes `win-x64` and `win-arm64`
- Linux host: publishes `linux-x64`
- macOS host: publishes `osx-x64` and `osx-arm64`

Examples:

```powershell
./publish-aot.ps1
./publish-aot.ps1 -Clean
./publish-aot.ps1 -RuntimeIdentifiers win-x64,win-arm64
./publish-aot.ps1 -RuntimeIdentifiers linux-x64
```

Notes:

- Native AOT cross-OS publishing is host-dependent. The script skips targets that are not expected to build on the current OS.
- Output is written to `publish/<rid>`.
- The publish script removes `.pdb` files so the release output contains the deployable binaries only.

### Automated GitHub Releases

The repository includes a GitHub Actions workflow that builds release binaries and attaches them to GitHub Releases when a version tag such as `v1.0.1` is pushed.

Workflow behavior:

- Windows runner builds `win-x64` and `win-arm64`
- Linux runner builds `linux-x64`
- macOS Intel runner builds `osx-x64`
- macOS Apple Silicon runner builds `osx-arm64`

This makes the binaries directly downloadable from the Releases page without building locally.

## Supported Markdown

- ATX headings using `#` through `######`
- Paragraphs
- Fenced code blocks using triple backticks
- Unordered lists using `-`, `*`, or `+`
- Ordered lists using `1.` style markers
- Nested lists using indentation
- Task lists using `- [ ]` and `- [x]`
- Pipe tables with an alignment separator row
- Blockquotes using `>`
- Inline emphasis using `*text*`
- Inline strong emphasis using `**text**`
- Inline code using backticks
- Inline links using `[label](https://example.com)`

## Example Markdown

```md
# Release Checklist

- [x] Build parser
- [x] Add renderer
- [ ] Publish package
	- verify local tool install
	- tag release

| Step | Status |
| :--- | ---: |
| Build | 10 |
| Tests | 10 |
```

## Example Output

The app generates a complete HTML document, not only a fragment. The markdown above will render into a document containing elements such as:

```html
<h1>Release Checklist</h1>
<ul>
	<li><input type="checkbox" disabled checked> Build parser</li>
	<li><input type="checkbox" disabled checked> Add renderer</li>
	<li><input type="checkbox" disabled> Publish package<ul><li>verify local tool install</li><li>tag release</li></ul></li>
</ul>
<table>
	<thead>
		<tr><th style="text-align:left">Step</th><th style="text-align:right">Status</th></tr>
	</thead>
</table>
```

## Output Behavior

- Output is wrapped in a full HTML document with `<!doctype html>`, `<head>`, and `<body>`
- The document title is derived from the input file name when a file path is provided
- When reading from stdin, the default title is `Document`
- The generated HTML includes a header at the top with the company name and website
- Plain text and code content are HTML-escaped
- Unsafe `javascript:` links are rewritten to `#`

## Current Limits

- This is not a full CommonMark or GitHub Flavored Markdown implementation.
- Raw HTML passthrough is not supported.
- Reference links, footnotes, and multi-paragraph list items are not implemented.
- Deeply complex nested markdown edge cases are intentionally out of scope for the current parser.

## Development

Build the project:

```powershell
dotnet build
```

Run the test suite:

```powershell
dotnet test .\Markdown2Html.Tests\Markdown2Html.Tests.csproj
```

Pack the .NET tool:

```powershell
dotnet pack
```

## Project Status

Completed so far:

- implemented a practical CLI for file and stdin workflows
- built a custom parser and renderer instead of using an external markdown library
- added support for headings, paragraphs, code blocks, blockquotes, inline formatting, links, tables, task lists, and nested lists
- added automated tests covering CLI and rendering behavior
- packaged the app as an installable .NET tool

## Notes

- The parser is intentionally small and explicit, which makes it easier to extend but means it does not aim for full markdown spec parity.
- The current design keeps parsing, rendering, and document wrapping separate so additional syntax can be added without pushing logic into the CLI entry point.