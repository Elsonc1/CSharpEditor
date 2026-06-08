# 002 — Balanceador Implementation (Requirement 1)

Date: 2026-06-08
Personas followed: `cseditor-architect` (plan), `cseditor-balanceador`
(implement), `cseditor-ui` (UI wiring), `cseditor-qa` (tests).

## Context

The coursework deliverable due 2026-06-09 has two explicit items:

1. Bracket balancer for `( [ { } ] )`.
2. Structural analysis.

Item (2) was already covered by the existing `Parser.cs` and
`SemanticAnalyzer.cs`. Item (1) was missing entirely from the C# code — there
was no `Compiler/Balanceador.cs`. The professor's
`tmp/src/br/com/unifacvest/controller/Balanceador.java` uses a simple stack
over raw text.

## Goal

Add the balancer, integrate it into the WPF UI with a keyboard shortcut, and
provide automated test coverage.

## Changes

Created:

- `Compiler/Balanceador.cs` — `Balanceador` static class exposing both
  `Verificar(string)` and `Verificar(IEnumerable<Token>)`. Stack of `Token`.
  Reports unclosed openings, unexpected closings, and type mismatches with
  line and column.
- `CSharpEditor.Tests/BalanceadorTests.cs` — 11 xUnit + FluentAssertions
  tests: simple pair, nested types, unclosed openings, closing without
  opening, type mismatch, real editor code, brackets inside string literals,
  brackets inside comments, empty input, line/column reporting, and the
  `IEnumerable<Token>` overload.

Modified:

- `MainWindow.xaml` — new orange button "Balanceador" placed before the
  lexical analysis button.
- `MainWindow.xaml.cs` — new `BtnBalance_Click` handler and an F4 keyboard
  binding registered in `RegisterKeyBindings()`.

## Decisions

- **Operate on tokens, not raw text.** The professor's Java works on raw
  text; the C# version delegates tokenization to the existing `Lexer` and
  then walks the token stream. This means brackets inside strings or
  comments are never seen by the balancer, which the test suite verifies.
  Documented as a conscious extension over the reference.
- **Report every opening that stays open, not just true/false.** The Java
  version only returns a boolean. The C# version produces a list with line
  and column for each unbalanced symbol. This is a UX improvement worth
  highlighting in the defense.
- **Error messages in Portuguese, format `Linha N, Coluna M: <msg>`.**
  Matches the existing lexer and parser conventions.

## Verification

```
dotnet build : 0 errors
dotnet test  : 25/25 passing
```

Test count went from 14 to 25 (+11).

## Resulting state

Requirement 1 complete. UI shortcut F4 active. The balancer handles every
case from the coursework rubric plus the bonus cases (strings, comments,
line/column reporting).
