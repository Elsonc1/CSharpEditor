# CSharpEditor Agents

Nine specialized agent personas for the Compilers coursework project at
UNIFACVEST (semester 2026/1). Inspired by the structure of
`aiox-core/.claude/agents`, adapted to a smaller academic scope.

## How to invoke

In chat:

```
Use the cseditor-balanceador agent to add a new test.
```

Or via the `Agent` tool with `subagent_type: cseditor-<name>`.

**Gotcha:** custom subagent types defined here are loaded by Claude Code at
session start. Agents created mid-session are **not** immediately available as
`subagent_type` values for the `Agent` tool — they become invokable only after
the next session restart. In the session where the agents were just created,
the main loop must follow each persona's spec directly. See
`../history/001-initial-agents-setup.md` for context.

## Catalog

| Agent                  | Use when                                                  | Model  |
|------------------------|-----------------------------------------------------------|--------|
| `cseditor-architect`   | Planning, impact analysis, coordinating the other agents  | opus   |
| `cseditor-balanceador` | Working on the bracket balancer (requirement 1)           | sonnet |
| `cseditor-syntax`      | Editing `Parser.cs`, improving syntax error messages      | sonnet |
| `cseditor-estruturas`  | Validating `if`/`for`/`while`/`switch`/`class` (req. 2)   | sonnet |
| `cseditor-lexer`       | Editing the tokenizer or the legacy lexical format        | sonnet |
| `cseditor-semantica`   | Editing `SemanticAnalyzer.cs` (bonus feature)             | sonnet |
| `cseditor-qa`          | Writing or running tests (`dotnet test`)                  | sonnet |
| `cseditor-ui`          | Buttons, shortcuts, output panel in the WPF UI            | sonnet |
| `cseditor-prof-ref`    | Keeping parity with the professor's Java reference        | sonnet |

All persona files (`cseditor-*.md`) are written in American English. A few
filenames keep their Portuguese form (`balanceador`, `estruturas`,
`semantica`) to match the coursework vocabulary and the corresponding C#
class names (`Balanceador`); the prose inside each file is English.

## Recommended workflow for delivery

```
cseditor-architect    — plan
   ↓
cseditor-balanceador  — implements Balanceador.cs and tests           [req. 1]
   ↓
cseditor-estruturas   — reviews parser/semantic for all structures    [req. 2]
   ↓
cseditor-ui           — wires UI buttons and shortcuts
   ↓
cseditor-qa           — runs tests, writes report
   ↓
cseditor-prof-ref     — updates ENTREGA.md for the professor
```

## Reference Java

`.claude/tmp/src/br/com/unifacvest/` contains the professor's implementation:

- `controller/Balanceador.java` — stack-based bracket checker
- `controller/AnaliseLexica.java` — tokenizer
- `controller/AnaliseSintatica.java` — syntax skeleton with `ifEstrutura()`
- `model/PalavrasReservadas.java`, `Operadores.java`, `Delimitadores.java`
- `Principal.java`, `view/JFrameCompilador.java`

Every agent should compare against these files when finalizing work.
