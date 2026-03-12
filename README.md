# Markdown2Html

Markdown2Html is a .NET 10 command-line app that converts a supported subset of Markdown into a full HTML document.

The project currently includes:

- file input and stdin support
- file output and stdout support
- a custom markdown parser and HTML renderer
- HTML document generation with basic built-in styling
- packaging as a .NET tool with the command name `markdown2html`
- automated tests for CLI behavior and rendering output

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
- `-h`, `--help`: show help text

## Install As A Tool

```powershell
dotnet pack
dotnet tool install --global --add-source .\nupkg Markdown2Html
markdown2html --help
```

For a local, repo-scoped install during development:

```powershell
dotnet pack
dotnet tool install --tool-path .\.tool-test --add-source .\nupkg Markdown2Html --version 1.0.0
.\.tool-test\markdown2html --help
```

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