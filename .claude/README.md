# .claude/ — Project Memory and Configuration

This directory keeps everything an AI agent (or a new contributor) needs to pick
up the CSharpEditor project without reading prior chat sessions.

## Project at a glance

CSharpEditor is a coursework project for the Compilers course at UNIFACVEST
(semester 2026/1). The deliverable (due 2026-06-09) requires:

1. A bracket balancer for `(`, `[`, `{`, `}`, `]`, `)`.
2. Structural analysis of the language (`if`, `for`, `while`, `switch`,
   `class`, methods).

The implementation is in C# / .NET 10 / WPF, with an AvalonEdit-based editor.
The professor delivered a Java reference (kept in `tmp/src/br/com/unifacvest/`),
and the C# version preserves output compatibility with it via
`Compiler/LegacyLexicalFormatter.cs`.

## Folder layout

```
.claude/
├── README.md                  ← you are here
├── agents/                    ← specialized agent personas
│   ├── README.md              ← catalog and recommended workflow
│   └── cseditor-*.md          ← nine personas (see catalog)
├── history/                   ← chronological log of past sessions
│   ├── README.md              ← log conventions and index
│   └── NNN-*.md               ← one file per session
├── tmp/                       ← professor's Java reference (do not modify)
│   └── src/br/com/unifacvest/
└── settings.local.json        ← per-machine Claude Code settings
```

## How to start a new session

1. Read this file (you already are).
2. Read `history/README.md` and at least the latest entry to see the current
   state.
3. Skim `agents/README.md` to know which persona fits the task.
4. Run `git log --oneline -10` for recent commits.
5. Run `dotnet build` and `dotnet test` to confirm a clean baseline.

## Current state (as of 2026-06-08, after session 005)

| Item                                          | Status                                |
|-----------------------------------------------|---------------------------------------|
| Requirement 1 — bracket balancer              | done                                  |
| Requirement 2 — structural analysis           | done                                  |
| Bonus — semantic analyzer                     | done                                  |
| Test suite                                    | 50/50 passing                         |
| Delivery doc for the professor (`ENTREGA.md`) | done at the repo root, in English     |
| GitHub-facing `README.md`                     | done at the repo root                 |
| Internal documentation (`.claude/`)           | this folder, fully in American English |

## Conventions

- All Markdown files in this directory are written in American English.
- A few identifiers in agent filenames (`cseditor-balanceador`,
  `cseditor-estruturas`, `cseditor-semantica`) and inside the codebase
  (`Balanceador`, `Erros`, `Balanceado`) intentionally keep their Portuguese
  spelling — they match the vocabulary of the coursework language and the
  professor's reference, and are kept verbatim to preserve traceability.
- History entries are numbered `NNN-slug.md` in chronological order. Add new
  entries; do not edit old ones except to fix factual mistakes (note the fix in
  the file itself).
- The `tmp/` folder is the professor's reference. Never modify it.
- The user (Elson) is the only human committer. Claude Code is the agent.
