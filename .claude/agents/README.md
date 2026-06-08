# Agents do CSharpEditor

Agents especializados para o trabalho acadêmico de **Compiladores** (UNIFACVEST 2026/1).

Inspirados na estrutura do `aiox-core/.claude/agents`, mas adaptados ao escopo: editor C# que faz análise léxica, sintática (entrega 09/06) e semântica (bônus) de uma linguagem inspirada na referência Java do professor.

## Como invocar

No chat:
```
Use o agent cseditor-balanceador para criar o balanceador de chaves.
```

Ou pelo `Agent` tool com `subagent_type: cseditor-<nome>`.

## Catálogo

| Agent                  | Quando usar                                                       | Model  |
|------------------------|-------------------------------------------------------------------|--------|
| **cseditor-architect** | Planejar entregas, análise de impacto, coordenar os demais        | opus   |
| **cseditor-balanceador** | Criar/manter o balanceador `( [ { } ] )` — requisito do trabalho | sonnet |
| **cseditor-syntax**    | Mexer no `Parser.cs`, melhorar mensagens sintáticas               | sonnet |
| **cseditor-estruturas** | Validar if/for/while/switch/class — "análise das estruturas"     | sonnet |
| **cseditor-lexer**     | Mexer no tokenizador e formato legacy                              | sonnet |
| **cseditor-semantica** | Mexer no `SemanticAnalyzer.cs` (bônus)                            | sonnet |
| **cseditor-qa**        | Criar/rodar testes (`dotnet test`)                                 | sonnet |
| **cseditor-ui**        | Botões, atalhos, painel de saída no WPF                            | sonnet |
| **cseditor-prof-ref**  | Manter paridade com o Java do professor                            | sonnet |

## Fluxo recomendado para a entrega 09/06

```
cseditor-architect  → planeja
   ↓
cseditor-balanceador → cria Balanceador.cs + testes        ← REQUISITO 1
   ↓
cseditor-estruturas  → revisa parser/semantic              ← REQUISITO 2
   ↓
cseditor-ui          → wire botão Balanceador na UI
   ↓
cseditor-qa          → roda dotnet test, gera relatório
   ↓
cseditor-prof-ref    → atualiza ENTREGA.md para o professor
```

## Referência Java (gold standard)

`.claude/tmp/src/br/com/unifacvest/` contém:
- `controller/Balanceador.java` — pilha de delimitadores
- `controller/AnaliseLexica.java` — tokenizador
- `controller/AnaliseSintatica.java` — esqueleto com `ifEstrutura()`
- `model/PalavrasReservadas.java`, `Operadores.java`, `Delimitadores.java`
- `Principal.java`, `view/JFrameCompilador.java`

Todos os agents devem comparar contra esses arquivos antes de finalizar.
