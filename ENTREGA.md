# Delivery — Syntax Analysis (Compilers 2026/1)

**Student:** Elson Vinicius de Souza Lopes
**Course:** Compilers — UNIFACVEST
**Due date:** 2026-06-09
**Implementation:** C# / .NET 10 / WPF (professor's Java reference kept under
`.claude/tmp/src/br/com/unifacvest/`)

---

## Requirements met

### 1. Bracket balancer for `( [ { } ] )`

- **File:** `Compiler/Balanceador.cs`
- **Algorithm:** stack (same idea as the professor's `Balanceador.java`)
- **Conscious extension:** operates on tokens rather than raw text, so
  symbols inside string literals and comments do not confuse the analysis.
- **UI:** "Balanceador" button (shortcut **F4**) in `MainWindow.xaml`.
- **Tests:** `CSharpEditor.Tests/BalanceadorTests.cs` (11 cases).

### 2. Structural analysis

Implemented by the recursive-descent parser (`Compiler/Parser.cs`) and the
semantic analyzer (`Compiler/SemanticAnalyzer.cs`).

| Structure | Syntax                                                | Validations                                                            |
|-----------|-------------------------------------------------------|------------------------------------------------------------------------|
| `if`      | `if (cond) {} [else {}] / [else if (...) {}]`         | parentheses, blocks, boolean condition, `else if` chain supported      |
| `for`     | `for (init; cond; incr) {}`                           | two `;`, boolean condition, scoped initializer                         |
| `while`   | `while (cond) {}`                                     | parentheses, block, boolean condition                                  |
| `switch`  | `switch (e) { case v: ...; break; }`                  | block, cases, optional `break`                                         |
| `class`   | `[public/private] class N [herdar B] [assinar I,...] {}` | name, modifier order, body                                           |
| method    | `[access] type name(params) {}`                       | parentheses, parameters, return type matched against body              |
| `return`  | `return [expr];`                                      | type compatible with the method; `void` cannot return a value          |
| `break`   | `break;`                                              | only inside a loop or a `switch`                                       |
| `continue`| `continue;`                                           | only inside a loop (a bare `switch` is rejected)                       |

- **Tests:** `CSharpEditor.Tests/EstruturasTests.cs` (25 cases).

---

## C# ↔ Java mapping

| Java component                                                 | C# counterpart                                          |
|----------------------------------------------------------------|---------------------------------------------------------|
| `controller/AnaliseLexica.java`                                | `Compiler/Lexer.cs` + `Compiler/LegacyLexicalFormatter.cs` |
| `controller/Balanceador.java`                                  | `Compiler/Balanceador.cs`                               |
| `controller/AnaliseSintatica.java` (skeleton with `ifEstrutura`) | `Compiler/Parser.cs` (full recursive descent)         |
| `model/PalavrasReservadas.java`                                | `Lexer.Keywords` dictionary                             |
| `model/Operadores.java`                                        | `Lexer.ReadOperatorOrDelimiter`                         |
| `model/Delimitadores.java`                                     | tokens in `Compiler/Token.cs`                           |
| `view/JFrameCompilador.java` (Swing)                           | `MainWindow.xaml` (WPF + AvalonEdit)                    |
| `Principal.java`                                               | `App.xaml.cs` + `MainWindow`                            |

---

## Conscious extensions over the reference

- **Formal AST** (`Compiler/AstNodes.cs`) — enables semantic analysis and any
  future code generation.
- **Line and column on every token and every error** — the Java reference
  reports line only.
- **Parser error recovery** via `Synchronize()` — the parser does not stop on
  the first error.
- **Semantic analyzer** — bonus over the assignment:
  - symbol table with nested scopes
  - type checking (assignments, binary and unary expressions, returns)
  - `break`/`continue` validated against their enclosing context
  - redeclaration detection
  - return type validated against the method signature
- **`else if` chain** supported by the parser.
- **`LegacyLexicalFormatter`** — preserves compatibility with the
  `(lexeme, CATEGORY)` format produced by the Java reference.
- **Automated test suite** — xUnit and FluentAssertions, 50 cases.

---

## Documented limitations

| Limitation                                              | Reason                                                                       |
|---------------------------------------------------------|------------------------------------------------------------------------------|
| `default` is not a reserved word in `switch`            | Not present in the professor's `PalavrasReservadas.java`; parity preserved   |
| No visibility check (`private`/`public`)                | Out of scope                                                                 |
| No code generation or interpretation                    | The assignment requires only analysis                                        |
| Char literal accepts multiple characters (`'ab'`)       | Kept as a future warning                                                     |
| No check that a called method exists                    | Only variables are validated against the symbol table                        |

---

## How to build, run, and demo

### Prerequisites
- Windows
- .NET 10 SDK
- Visual Studio 2022+ is optional; everything also runs from the CLI.

### Build and tests
```powershell
dotnet build CSharpEditor.sln
dotnet test CSharpEditor.Tests
```

### Run the WPF application
```powershell
dotnet run --project CSharpEditor.csproj
```

### Demo script (suggested)
1. Open the editor — a sample program is preloaded.
2. Press **F4** — Balancer. The output shows "Balanceado" (balanced).
3. Change `if (x > 5) {` to `if (x > 5 {` (remove the `)`) and press **F4**
   again — the output reports `Linha N, Coluna M: '(' aberto e não fechado`
   (`'(' opened and not closed`).
4. Press **F5** — Lexical analysis. The Tokens table is populated, and the
   legacy format (compatible with the Java reference) is shown in the
   Messages tab.
5. Press **F6** — Semantic analysis. The full pipeline runs:
   lexer → parser → semantic analyzer, with any structural errors listed.
6. Try `else if` chains, `for (int i = 0; i < 10; i++)`, and
   `switch`/`case`/`break` blocks.

---

## Project layout

```
CSharpEditor/
├── Compiler/
│   ├── Token.cs                    — TokenType enum and Token class
│   ├── Lexer.cs                    — tokenizer
│   ├── LegacyLexicalFormatter.cs   — output in the Java reference format
│   ├── Balanceador.cs              — REQUIREMENT 1
│   ├── AstNodes.cs                 — AST node types
│   ├── Parser.cs                   — REQUIREMENT 2 (syntax analysis)
│   └── SemanticAnalyzer.cs         — bonus (semantic analysis)
├── CSharpEditor.Tests/
│   ├── LexerTests.cs                       — 6 tests
│   ├── LegacyLexicalFormatterTests.cs      — 5 tests
│   ├── BalanceadorTests.cs                 — 11 tests (REQUIREMENT 1)
│   ├── EstruturasTests.cs                  — 25 tests (REQUIREMENT 2)
│   └── ParserSemanticSmokeTests.cs         — 3 tests
├── MainWindow.xaml / .xaml.cs       — WPF UI and AvalonEdit hosting
├── CSharpSyntax.xshd                — syntax highlighting definition
└── .claude/
    ├── agents/                      — 9 specialized agent personas and a README
    ├── history/                     — chronological log of every session
    └── tmp/src/br/com/unifacvest/   — professor's Java reference
```

---

## Numbers

- **Production code:** ~1500 lines (excluding tests and generated UI code)
- **Automated tests:** 50, all passing
- **Coverage:** lexer, balancer, parser (all structures), semantic analyzer
- **Tech stack:** C# 12, .NET 10, WPF, AvalonEdit, xUnit, FluentAssertions
