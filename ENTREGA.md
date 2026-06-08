# Entrega — Análise Sintática (Compiladores 2026/1)

**Aluno:** Elson Vinicius de Souza Lopes
**Disciplina:** Compiladores — UNIFACVEST
**Prazo:** 09/06
**Implementação:** C# / .NET 10 / WPF (referência Java do professor em `.claude/tmp/src/br/com/unifacvest/`)

---

## Requisitos atendidos

### 1. Balanceador de `( [ { } ] )`

- **Arquivo:** `Compiler/Balanceador.cs`
- **Algoritmo:** pilha (idêntico em ideia ao `Balanceador.java` do professor)
- **Diferencial:** opera sobre tokens — símbolos dentro de strings literais e
  comentários não confundem a análise.
- **UI:** botão "Balanceador" (atalho **F4**) em `MainWindow.xaml`.
- **Testes:** `CSharpEditor.Tests/BalanceadorTests.cs` (11 casos).

### 2. Análise das estruturas

Implementada via parser descendente recursivo (`Compiler/Parser.cs`) + analisador
semântico (`Compiler/SemanticAnalyzer.cs`).

| Estrutura | Sintaxe                                          | Validações |
|-----------|--------------------------------------------------|------------|
| `if`      | `if (cond) {} [else {}] / [else if (...) {}]`    | parens, blocos, cond `boolean`, suporte a cadeia `else if` |
| `for`     | `for (init; cond; incr) {}`                      | 2× `;`, cond `boolean`, escopo da init |
| `while`   | `while (cond) {}`                                | parens, bloco, cond `boolean` |
| `switch`  | `switch (e) { case v: ...; break; }`             | bloco, cases, `break` opcional |
| `class`   | `[public/private] class N [herdar B] [assinar I,...] {}` | nome, ordem, corpo |
| método    | `[acesso] tipo nome(params) {}`                  | parens, params, return type vs corpo |
| `return`  | `return [expr];`                                 | tipo compatível com método, void não retorna valor |
| `break`   | `break;`                                         | só em loop ou switch |
| `continue`| `continue;`                                      | só em loop (switch puro rejeita) |

- **Testes:** `CSharpEditor.Tests/EstruturasTests.cs` (≥ 24 casos).

---

## Mapeamento C# ↔ Java do professor

| Componente Java                                              | C# correspondente                                |
|--------------------------------------------------------------|--------------------------------------------------|
| `controller/AnaliseLexica.java`                              | `Compiler/Lexer.cs` + `LegacyLexicalFormatter.cs`|
| `controller/Balanceador.java`                                | `Compiler/Balanceador.cs`                        |
| `controller/AnaliseSintatica.java` (skeleton com `ifEstrutura`) | `Compiler/Parser.cs` (recursive descent completo) |
| `model/PalavrasReservadas.java`                              | `Lexer.Keywords` (dicionário)                    |
| `model/Operadores.java`                                      | `Lexer.ReadOperatorOrDelimiter`                  |
| `model/Delimitadores.java`                                   | tokens em `Token.cs`                             |
| `view/JFrameCompilador.java` (Swing)                         | `MainWindow.xaml` (WPF + AvalonEdit)             |
| `Principal.java`                                             | `App.xaml.cs` + `MainWindow`                     |

---

## Extensões conscientes (além da referência Java)

- **AST formal** (`Compiler/AstNodes.cs`) — facilita análise semântica e futura geração de código.
- **Tokens com linha + coluna** — todos os erros indicam onde.
- **Error recovery** no parser (método `Synchronize`) — não para no primeiro erro.
- **Analisador semântico** — bônus além do trabalho:
  - tabela de símbolos com escopos aninhados
  - checagem de tipos (atribuição, expressões binárias/unárias, retornos)
  - `break`/`continue` validados em contexto
  - redeclaração detectada
  - tipo de retorno validado contra assinatura do método
- **Cadeia `else if`** suportada no parser.
- **`LegacyLexicalFormatter`** — preserva compatibilidade com o formato `(lexema, CATEGORIA)` do Java.
- **Suite de testes automatizados** — xUnit + FluentAssertions, > 45 casos.

---

## Limitações documentadas

| Limitação                                       | Motivo |
|-------------------------------------------------|--------|
| `default` em `switch` não é palavra reservada   | Não consta em `PalavrasReservadas.java` do prof — paridade preservada |
| Sem checagem de visibilidade `private`/`public` | Fora do escopo do trabalho |
| Sem code generation / interpretação             | Trabalho exige só análise |
| Char literal aceita multi-char (`'ab'`)          | Mantido como warning futuro |
| Sem checagem de método não declarado em chamada | Apenas variáveis são validadas |

---

## Como rodar e demonstrar

### Pré-requisitos
- Windows
- .NET 10 SDK
- Visual Studio 2022+ (opcional, dá pra rodar via CLI)

### Build e testes
```powershell
dotnet build CSharpEditor.sln
dotnet test CSharpEditor.Tests
```

### Rodar a aplicação WPF
```powershell
dotnet run --project CSharpEditor.csproj
```

### Demonstração (roteiro sugerido)
1. Abrir o editor — código de exemplo já vem carregado.
2. **F4** — Balanceador. Mostra "Balanceado".
3. Alterar `if (x > 5) {` para `if (x > 5 {` (remover `)`) e **F4** novamente — mostra `Linha N, Coluna M: '(' aberto e não fechado`.
4. **F5** — Análise Léxica. Tabela de tokens + formato legado (compatível com o Java).
5. **F6** — Análise Semântica. Pipeline lex → parse → semantic com erros estruturais.
6. Testar `else if` em cadeia, `for (int i = 0; i < 10; i++)`, `switch case break`.

---

## Estrutura do projeto

```
CSharpEditor/
├── Compiler/
│   ├── Token.cs                    — enum TokenType + classe Token
│   ├── Lexer.cs                    — tokenizador
│   ├── LegacyLexicalFormatter.cs   — saída no formato Java do prof
│   ├── Balanceador.cs              — REQUISITO 1
│   ├── AstNodes.cs                 — nós da AST
│   ├── Parser.cs                   — REQUISITO 2 (análise sintática)
│   └── SemanticAnalyzer.cs         — bônus (análise semântica)
├── CSharpEditor.Tests/
│   ├── LexerTests.cs               — 6 testes
│   ├── LegacyLexicalFormatterTests.cs — 5 testes
│   ├── BalanceadorTests.cs         — 11 testes (REQUISITO 1)
│   ├── EstruturasTests.cs          — 24+ testes (REQUISITO 2)
│   └── ParserSemanticSmokeTests.cs — 3 testes
├── MainWindow.xaml / .xaml.cs      — UI WPF + AvalonEdit
├── CSharpSyntax.xshd               — syntax highlight
└── .claude/
    ├── agents/                     — 9 agents especializados + README
    └── tmp/src/br/com/unifacvest/  — referência Java do professor
```

---

## Estatísticas

- **Linhas de código:** ~1500 (excluindo testes e UI gerada)
- **Testes automatizados:** > 45 (todos verdes)
- **Cobertura:** lexer, balanceador, parser (todas estruturas), semantic
- **Tecnologias:** C# 12, .NET 10, WPF, AvalonEdit, xUnit, FluentAssertions
