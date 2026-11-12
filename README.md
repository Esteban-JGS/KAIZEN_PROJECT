# KaizenLang

<p align="center">
  <img src="KaizenLang/Resources/assets/LOGO_KAIZEN.png" alt="KaizenLang logo" width="170" />
</p>

<p align="center">
  <strong>Educational programming language with a full compiler pipeline, semantic analysis, interpreter, and custom Windows IDE.</strong>
</p>

<p align="center">
  <img alt=".NET 9" src="https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white" />
  <img alt="Platform Windows" src="https://img.shields.io/badge/Platform-Windows-0078D6?logo=windows&logoColor=white" />
  <img alt="Language C#" src="https://img.shields.io/badge/Language-C%23-239120?logo=c-sharp&logoColor=white" />
  <img alt="Status Functional" src="https://img.shields.io/badge/Status-Functional-2E7D32" />
</p>

<p align="center">
  <img src="KaizenLang/documentation/UNA%20Logo2.png" alt="Universidad Nacional de Costa Rica" width="360" />
</p>

---

## Table of Contents

- [Overview](#overview)
- [Project Status](#project-status)
- [Core Features](#core-features)
- [Language Snapshot](#language-snapshot)
- [Architecture](#architecture)
- [Repository Structure](#repository-structure)
- [Tech Stack and Dependencies](#tech-stack-and-dependencies)
- [Requirements](#requirements)
- [Quick Start](#quick-start)
- [How to Run](#how-to-run)
- [IDE Highlights](#ide-highlights)
- [Examples and Test Assets](#examples-and-test-assets)
- [Automated Tests](#automated-tests)
- [Useful Environment Variables](#useful-environment-variables)
- [Known Limitations](#known-limitations)
- [Troubleshooting](#troubleshooting)
- [Documentation and Assets](#documentation-and-assets)
- [Academic Context](#academic-context)

---

## Overview

KaizenLang is an academic language project designed to teach compiler and interpreter fundamentals through a custom syntax and a complete implementation stack.

The project includes:

1. Lexical analysis
2. Syntactic analysis
3. Semantic analysis
4. AST interpretation at runtime
5. A dedicated desktop IDE
6. A web playground that runs the same compiler in the browser (WebAssembly)
7. CLI tools for execution and testing

**Try it online:** [kaizen-project-xi.vercel.app](https://kaizen-project-xi.vercel.app), no installation required.

KaizenLang keeps familiar programming concepts (`if`, `for`, `while`, functions, arrays) while adding identity through custom block delimiters (`ying`, `yang`) and themed type names.

---

## Project Status

- Functional and usable across language core, runners, IDE, and web playground.
- CI builds the whole solution, runs the xUnit suite, and checks formatting on every push and pull request.
- Formal documentation is available in both Markdown and PDF.
- The Windows IDE supports compile and run workflows.
- Example suite is broad, including normal, edge, stress, and error cases.

---

## Core Features

### Compiler and Runtime

- Tokenization with line and column tracking.
- AST model with error collection helpers.
- Semantic checks for declarations, type compatibility, function arity, and collection rules.
- Runtime symbol scopes for blocks and functions.
- Built-in I/O support and collection length support.

### Language Design

- Strongly typed declarations.
- Custom primitive type names.
- Generic-style collection wrappers.
- Control flow and function support, including recursion.
- Clear diagnostics across lexical, syntax, and semantic stages.

### IDE Experience

- Code editor and output panel.
- Compile and execute buttons.
- Snippet insertion menus by category.
- Syntax highlighting with asynchronous debouncing.
- Line numbers and quick guide sidebar.
- File open/save workflow (`.kz`, `.txt`).
- Keyboard shortcuts and status indicators.

---

## Language Snapshot

### Primitive Types

| KaizenLang Type | Meaning |
| --- | --- |
| `gear` | Integer |
| `shikai` | Float-like decimal |
| `bankai` | Higher precision decimal |
| `shin` | Boolean |
| `grimoire` | String |

### Composite Types

| KaizenLang Type | Meaning |
| --- | --- |
| `chainsaw<T>` | 1D collection |
| `hogyoku<T>` | 2D collection |

### Built-ins

- `output(...)`
- `input(...)`
- `length(...)`

### Minimal Example

```kz
gear age = 20;
grimoire name = "Kaizen";

if (age >= 18) ying
    output("Hello " + name + ", access granted.");
yang else ying
    output("Access denied.");
yang
```

---

## Architecture

```mermaid
flowchart LR
    A[Source Code] --> B[Tokenizer / Lexer]
    B --> C[Parser]
    C --> D[AST Node Tree]
    D --> E[Semantic Analyzer]
    E --> F[Interpreter]
    F --> G[Program Output]

    E --> H[Diagnostics]
    F --> I[Symbol Tables and Scopes]
```

### Main Core Modules

- `src/KaizenLang.Core/Lexeme`:
  - `CharStream`
  - `Tokenizer`
  - `Lexer`
- `src/KaizenLang.Core/Syntax`:
  - parser split by expressions, statements, and control flow
- `src/KaizenLang.Core/Semantic`:
  - `SemanticAnalyzer`, `TypeResolver`, `DeclarationChecker`, `CollectionValidator`
- `src/KaizenLang.Core/Interpreter`:
  - runtime evaluation, functions, assignments, control flow, built-ins
- `src/KaizenLang.Core/Tokens`:
  - language words for types, reserved keywords, operators, and delimiters

---

## Repository Structure

```text
KaizenLang/
├─ src/
│  ├─ KaizenLang.Core/      # compiler, semantic analyzer, interpreter
│  ├─ KaizenLang.UI/        # WinForms UI, theming, compilation/execution services
│  └─ KaizenLang.App/       # desktop entry point
├─ tools/
│  ├─ IDERunner/            # CLI file runner
│  ├─ TestRunner/           # CLI runner for test files
│  ├─ QuickRunner/          # embedded snippet quick run
│  ├─ SnippetTester/        # target file snippet testing
│  └─ Tests/                # xUnit test project
├─ examples/                # functional, error, edge, and stress samples
├─ documentation/           # formal docs and editor setup notes
├─ Resources/               # app icon and visual assets
└─ KaizenLang.sln
```

---

## Tech Stack and Dependencies

### Target Frameworks

- Core: `net9.0`
- UI: `net9.0-windows`
- App: `net9.0-windows`
- Web playground: `net9.0` (Blazor WebAssembly)
- Tools and tests: `net9.0-windows`

### NuGet Packages (high level)

- `SkiaSharp`
- `SkiaSharp.Views.Desktop.Common`
- `Svg.Skia`
- `xunit`
- `Microsoft.NET.Test.Sdk`
- `coverlet.collector`

---

## Requirements

### Operating System

- Windows is required for the IDE and Windows-targeted projects.

### .NET SDK

- .NET SDK 9.0.x

If you do not have .NET installed:

```powershell
winget install Microsoft.DotNet.SDK.9
dotnet --info
```

Official download page:

- <https://dotnet.microsoft.com/download>

---

## Quick Start

The solution lives in the `KaizenLang/` folder; the commands below run from there:

```powershell
cd KaizenLang
dotnet restore KaizenLang.sln
dotnet build KaizenLang.sln -c Release
```

Run the desktop IDE:

```powershell
dotnet run --project src/KaizenLang.App/KaizenLang.App.csproj
```

Run one sample from CLI:

```powershell
dotnet run --project tools/IDERunner/IDERunner.csproj -- examples/test-01-variables-basicas.txt
```

---

## How to Run

### 1) Main IDE

```powershell
dotnet run --project src/KaizenLang.App/KaizenLang.App.csproj
```

### 2) IDERunner

```powershell
dotnet run --project tools/IDERunner/IDERunner.csproj -- examples/test-12-programa-completo.txt --verbose
```

### 3) TestRunner

```powershell
dotnet run --project tools/TestRunner/TestRunner.csproj -- examples/test-01-variables-basicas.txt
```

### 4) QuickRunner

```powershell
dotnet run --project tools/QuickRunner/QuickRunner.csproj
```

### 5) SnippetTester

```powershell
dotnet run --project tools/SnippetTester/SnippetTester.csproj -- examples/test-15-casos-limite.txt
```

### 6) Web playground

```powershell
dotnet run --project src/KaizenLang.Web/KaizenLang.Web.csproj
```

The playground is a Blazor WebAssembly app that references only `KaizenLang.Core`, so the browser runs the same lexer, parser, semantic analyzer and interpreter as the IDE. It includes the programs from `examples/` and a text box for `input()`.

It is deployed on Vercel from the repository root: `vercel.json` runs `src/KaizenLang.Web/vercel-build.sh`, which installs the .NET 9 SDK in the build image and publishes the static site to `dist/wwwroot`.

---

## IDE Highlights

### Recommended Workflow

1. Write or open KaizenLang code.
2. Compile to validate lexical, syntax, and semantic stages.
3. Execute to run the AST in the interpreter.
4. Review output and diagnostics in the output panel.

### Keyboard Shortcuts

- `F5`: Execute
- `F6`: Compile
- `Ctrl+N`: New file
- `Ctrl+O`: Open file
- `Ctrl+S`: Save file

### Editor and UX Features

- Syntax-aware coloring for keywords, types, functions, operators, strings, comments.
- Line and column tracking.
- Line number panel.
- Theme-aware UI.
- Sidebar quick guide.
- Prompt dialog integration for `input(...)`.

---

## Examples and Test Assets

Folder: `examples/`

### Functional Scenarios

- `test-01-variables-basicas.txt`
- `test-02-operaciones-aritmeticas.txt`
- `test-03-operaciones-logicas.txt`
- `test-04-estructuras-if.txt`
- `test-05-estructuras-while.txt`
- `test-06-estructuras-for.txt`
- `test-07-arrays-basicos.txt`
- `test-08-matrices.txt`
- `test-09-funciones-basicas.txt`
- `test-10-funciones-avanzadas.txt`
- `test-11-input-output.txt` (interactive)
- `test-12-programa-completo.txt`
- `test-15-casos-limite.txt`
- `test-16-stress-integral.txt`

### Error Validation Scenarios

- `test-13-errores-sintacticos.txt`
- `test-14-errores-semanticos.txt`
- `test-syntax-errors.txt`

### Debug and Diagnostics Scenarios

- `test-debug-index.txt`
- `test-debug-output.txt`
- `test-negative-fix.txt`
- `test-infinite-recursion.txt`

Testing guide:

- `examples/INSTRUCCIONES_TESTING.md`

---

## Automated Tests

```powershell
dotnet test KaizenLang.sln
```

Current covered domains include:

- function execution behavior
- semantic collection validation
- declaration checks
- null-handling behavior
- regression cases for the parser, semantic analyzer and interpreter (`RegressionTests.cs`)

---

## Useful Environment Variables

### `KAIZEN_LOG_LEVEL`

Controls logger verbosity in Core.

Values:

- `Debug`
- `Info`
- `Warning`
- `Error`
- `None`

Example:

```powershell
$env:KAIZEN_LOG_LEVEL="Debug"
```

### `KAIZEN_VERBOSE_OUTPUT`

Enables more detailed compilation output.

Example:

```powershell
$env:KAIZEN_VERBOSE_OUTPUT="1"
```

---

## Known Limitations

These are reflected in the current codebase behavior:

- The desktop IDE is Windows-only (`net9.0-windows`); the web playground runs anywhere.
- `while`, `do-while` and `for` loops stop after 10,000 iterations with a warning.
- Recursion depth is capped by default (100).
- The IDE stops a program after 5 seconds of execution (time spent answering `input()` does not count); the web playground stops it after 2,000,000 evaluation steps.
- `input()` tokenizes by whitespace, full free-form line input is limited.
- Functions must be declared before they are called.
- SVG menu icons are loaded from `Resources/icons/*.svg`, which is not included in the repository, so IDE menus show text only.

---

## Troubleshooting

### `dotnet` is not recognized

Install .NET SDK 9, then restart your terminal:

```powershell
winget install Microsoft.DotNet.SDK.9
dotnet --info
```

### Tests fail due to path assumptions

Run the commands from the `KaizenLang/` folder (where `KaizenLang.sln` lives). The test project is `tools/Tests/KaizenLang.Tests.csproj`.

### Program appears blocked waiting for input

- Start with non-interactive examples.
- Use `examples/test-11-input-output.txt` only when providing input.

### IDE does not run on non-Windows platforms

Expected behavior, the UI stack is WinForms-based.

---

## Documentation and Assets

- Formal document (Markdown): `documentation/KaizenDocumentation.md`
- Formal document (PDF): `documentation/KaizenDocumentation.pdf`
- VS Code setup notes: `documentation/vs-config.md`
- Testing instructions: `examples/INSTRUCCIONES_TESTING.md`
- Kaizen logo: `Resources/assets/LOGO_KAIZEN.png`
- Application icon: `Resources/icon.ico`

---

## Academic Context

- University: Universidad Nacional de Costa Rica (UNA)
- Course: Paradigmas de Programacion
- Professor: Josias Ariel Chaves Murillo
- Term: II 2025
- Students:
  - Esteban Javier Granados Sibaja
  - Juan Carlos Camacho Solano

---

## Final Note

KaizenLang demonstrates not only custom language syntax, but a complete educational toolchain: lexer, parser, semantic analysis, AST execution, IDE integration, CLI tooling, and test coverage in a single cohesive project.
