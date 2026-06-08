namespace CSharpEditor.Compiler;

public class BalanceadorResult
{
    public bool Balanceado { get; set; }
    public List<string> Erros { get; } = new();
}

/// <summary>
/// Verifica o balanceamento dos delimitadores ( ) [ ] { } exigido no trabalho
/// acadêmico (UNIFACVEST 2026/1). Espelha o algoritmo de pilha de
/// <c>Balanceador.java</c> do professor, mas opera sobre <see cref="Token"/>s
/// — assim símbolos dentro de strings/comentários não confundem a análise.
/// </summary>
public static class Balanceador
{
    public static BalanceadorResult Verificar(string codigo)
    {
        var lex = new Lexer(codigo).Tokenize();
        return Verificar(lex.Tokens);
    }

    public static BalanceadorResult Verificar(IEnumerable<Token> tokens)
    {
        var result = new BalanceadorResult();
        var pilha = new Stack<Token>();

        foreach (var token in tokens)
        {
            if (IsAbertura(token.Type))
            {
                pilha.Push(token);
            }
            else if (IsFechamento(token.Type))
            {
                if (pilha.Count == 0)
                {
                    result.Erros.Add(
                        $"Linha {token.Line}, Coluna {token.Column}: " +
                        $"'{token.Value}' fechado sem abertura correspondente.");
                    continue;
                }

                var topo = pilha.Pop();
                if (!Combina(topo.Type, token.Type))
                {
                    result.Erros.Add(
                        $"Linha {token.Line}, Coluna {token.Column}: " +
                        $"Esperado '{Par(topo.Type)}' para fechar '{topo.Value}' " +
                        $"da linha {topo.Line}, coluna {topo.Column}, " +
                        $"encontrado '{token.Value}'.");
                }
            }
        }

        while (pilha.Count > 0)
        {
            var aberto = pilha.Pop();
            result.Erros.Add(
                $"Linha {aberto.Line}, Coluna {aberto.Column}: " +
                $"'{aberto.Value}' aberto e não fechado " +
                $"(esperado '{Par(aberto.Type)}').");
        }

        result.Balanceado = result.Erros.Count == 0;
        return result;
    }

    private static bool IsAbertura(TokenType t) =>
        t is TokenType.LeftParen or TokenType.LeftBracket or TokenType.LeftBrace;

    private static bool IsFechamento(TokenType t) =>
        t is TokenType.RightParen or TokenType.RightBracket or TokenType.RightBrace;

    private static bool Combina(TokenType abertura, TokenType fechamento) =>
        (abertura, fechamento) switch
        {
            (TokenType.LeftParen, TokenType.RightParen) => true,
            (TokenType.LeftBracket, TokenType.RightBracket) => true,
            (TokenType.LeftBrace, TokenType.RightBrace) => true,
            _ => false
        };

    private static string Par(TokenType t) => t switch
    {
        TokenType.LeftParen => ")",
        TokenType.LeftBracket => "]",
        TokenType.LeftBrace => "}",
        TokenType.RightParen => "(",
        TokenType.RightBracket => "[",
        TokenType.RightBrace => "{",
        _ => "?"
    };
}
