KaizenLang - Simple IDE Runner

Usage:

- Build and run from the solution root or from this folder:

  dotnet run --project tools/IDERunner/IDERunner.csproj -- <path-to-source-file>

- Example:

  dotnet run --project tools/IDERunner/IDERunner.csproj -- examples/test-01-variables-basicas.txt

- To include debug lines (logger [DBG] output) in the program output, pass --verbose:

  dotnet run --project tools/IDERunner/IDERunner.csproj -- tools/IDERunner/sample.kaizen --verbose

What it does:

- Uses `CompilationService` to compile the provided KaizenLang source file.
- Prints compilation diagnostics and the compilation output summary.
- If compilation succeeds, runs the resulting AST with the `Interpreter` and prints program outputs.

Notes:

- The runner targets `net9.0-windows` to match UI project TFM.
- You can wire this up to a VS Code task to run the active file quickly.
