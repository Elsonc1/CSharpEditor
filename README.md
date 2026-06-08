# CSharpEditor

An academic compiler front-end for a small Java/C#-inspired language, written
in C# / .NET 10 / WPF, with a built-in code editor based on AvalonEdit.

This is the deliverable for the Compilers course at UNIFACVEST (semester
2026/1). The professor provided a Java reference implementation; this C#
version preserves output compatibility with it while extending the front-end
with a recursive-descent parser, an AST, a semantic analyzer, and a WPF
editor.

## Coursework requirements

The assignment (due 2026-06-09) asks for:

1. A **bracket balancer** for `(`, `[`, `{`, `}`, `]`, `)`.
2. **Structural analysis** of the language — `if`/`else`, `for`, `while`,
   `switch`, classes, and methods.

Both are implemented and covered by 50 automated tests. See
[ENTREGA.md](ENTREGA.md) for the full delivery document (mapping to the
professor's Java reference, conscious extensions, documented limitations, and
a demo script for the defense).

## Features

- **Lexical analysis** — tokenizer with literals, operators, delimiters,
  keywords, and comments (line and block). The legacy output format is
  compatible with the professor's Java reference via
  `Compiler/LegacyLexicalFormatter.cs`.
- **Bracket balancer** — stack-based check over tokens, so symbols inside
  string literals or comments do not confuse the analysis. Reports every
  unbalanced opening with line and column.
- **Syntax analysis** — recursive-descent parser with error recovery via
  `Synchronize()`. Supports `if`/`else`/`else if`, `for`, `while`,
  `switch`/`case`, classes with `herdar` (extends) and `assinar`
  (implements), and methods with typed parameters.
- **Semantic analysis** (bonus) — scope-based symbol table, type checking
  for assignments and expressions, `break`/`continue` context validation,
  and return-type checking against method signatures.
- **WPF editor** — dark theme, syntax highlighting via AvalonEdit, keyboard
  shortcuts: **F4** for the balancer, **F5** for lexical analysis, **F6**
  for semantic analysis.

## The language

The target language is inspired by Java/C# but uses Portuguese keywords
matching the professor's reference:

| Keyword         | Meaning                  |
|-----------------|--------------------------|
| `herdar`        | extends (class)          |
| `assinar`       | implements (interface)   |
| `agilizador`    | interface                |
| `igor`          | thread                   |
| `int`, `double`, `boolean`, `string`, `void`, `var` | primitive types |
| `if`, `else`, `for`, `while`, `switch`, `case` | control flow |
| `class`, `public`, `private`, `main`, `new`, `return` | OO and flow |
| `break`, `continue`, `import`, `error`         | misc.                 |

A sample program (also used as the editor's default content):

```
import utils;

public class Animal {
    private string nome;
    private int idade;

    public void main() {
        string saudacao = "Olá, mundo!";
        int x = 10;

        if (x > 5) {
            x = x * 2;
        } else {
            x -= 1;
        }

        for (int i = 0; i < 10; i++) {
            x = x + i;
        }
    }
}
```

## Quick start

Requirements: Windows, .NET 10 SDK.

```powershell
git clone <repo-url>
cd CSharpEditor
dotnet build CSharpEditor.sln
dotnet test CSharpEditor.Tests
dotnet run --project CSharpEditor.csproj
```

The WPF window opens with a sample program loaded. Press **F4**, **F5**, or
**F6** to run the analyses; the output appears in the bottom panel with a
Tokens table and a Messages tab.

## Project structure

```
CSharpEditor/
├── Compiler/
│   ├── Token.cs                    — token types and helpers
│   ├── Lexer.cs                    — tokenizer
│   ├── LegacyLexicalFormatter.cs   — output compatible with the Java reference
│   ├── Balanceador.cs              — bracket balancer (requirement 1)
│   ├── AstNodes.cs                 — AST node types
│   ├── Parser.cs                   — recursive-descent parser (requirement 2)
│   └── SemanticAnalyzer.cs         — semantic analyzer (bonus)
├── CSharpEditor.Tests/             — xUnit + FluentAssertions (50 tests)
├── MainWindow.xaml / .xaml.cs      — WPF UI
├── CSharpSyntax.xshd               — AvalonEdit syntax highlighting
├── ENTREGA.md                      — full delivery document
└── .claude/                        — agent personas, session history, Java reference
```

## Tech stack

- **Language**: C# 12
- **Runtime**: .NET 10
- **UI**: WPF with AvalonEdit
- **Tests**: xUnit and FluentAssertions
- **Reference**: Java (professor's implementation, kept under `.claude/tmp/`)

## Further documentation

- **[ENTREGA.md](ENTREGA.md)** — full academic delivery document, including
  the mapping to the professor's Java reference, conscious extensions, and
  documented limitations.
- **[.claude/README.md](.claude/README.md)** — internal project memory,
  intended for AI agent sessions and new contributors.
- **[.claude/agents/README.md](.claude/agents/README.md)** — catalog of
  specialized agent personas for working on this project.
- **[.claude/history/](./.claude/history/)** — chronological log of every
  work session.

## Notes on naming

A few identifiers in the codebase and in the agent catalog use Portuguese
names (`Balanceador`, `cseditor-balanceador`, `cseditor-estruturas`,
`cseditor-semantica`). They match the vocabulary of the coursework language
and the professor's reference, and are kept verbatim to preserve traceability.
All prose documentation is in American English.

## License

Academic coursework, UNIFACVEST 2026/1. Not licensed for commercial use.
