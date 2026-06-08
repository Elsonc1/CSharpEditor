# 001 — Initial Agents Setup

Date: 2026-06-08
Author: Claude Code (acting on user request)

## Context

The CSharpEditor project already had a working lexer, parser, semantic
analyzer, and WPF UI in C#. The professor's Java reference lived in
`.claude/tmp/`. There were no agent definitions yet.

The user asked to create agents for the CSharpEditor project, modeled on the
`aiox-core/.claude/agents` structure but scoped down to this academic project.

## Goal

Create a focused set of specialized agent personas covering the project's
domains: lexer, parser, balancer, structure validation, semantic analysis, QA,
UI, professor-reference alignment, and an architect to coordinate them.

## Changes

Nine agent files were created under `.claude/agents/`:

- `cseditor-architect.md` — coordinator (model: opus)
- `cseditor-balanceador.md` — bracket balancer specialist
- `cseditor-syntax.md` — parser specialist
- `cseditor-estruturas.md` — structural-analysis specialist
- `cseditor-lexer.md` — tokenizer specialist
- `cseditor-semantica.md` — semantic-analyzer specialist
- `cseditor-qa.md` — test specialist
- `cseditor-ui.md` — WPF UI specialist
- `cseditor-prof-ref.md` — parity-with-Java specialist

A Portuguese README was also written; it was later replaced by an English
version in session 004.

## Decisions

- **Adapted from aiox-core, not copied.** The aiox structure includes hooks,
  AIOX-specific skills, and a `permissionMode: bypassPermissions` flag suited
  to large multi-month projects. None of that applies here. Each agent file
  was simplified to keep only `name`, `description`, `model`, and `tools`.
- **Portuguese persona files.** The coursework vocabulary is in Portuguese
  (`herdar`, `assinar`, `agilizador`). The personas use the same vocabulary
  so the agents reason in the same terms as the language they analyze.
- **One model split: `opus` for the architect, `sonnet` for everyone else.**
  The architect needs broader judgment; the specialists are mechanical enough
  for Sonnet.
- **No `permissionMode` override.** Default permission mode is fine for an
  academic project.

## Known gotcha

Custom subagent types in `.claude/agents/` are loaded by Claude Code at
session start. Agents created mid-session are NOT immediately available as
`subagent_type` values for the `Agent` tool. They become invokable only after
the next session restart. Until then, the main loop must follow the persona
specs directly.

This is why sessions 002 and 003 show the architect's persona being followed
in the main loop, not as a spawned subagent.

## Resulting state

- 9 agent files in `.claude/agents/`
- 1 README (later replaced in English in session 004)
- Production code untouched
