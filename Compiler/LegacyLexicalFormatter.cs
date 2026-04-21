using System.IO;

namespace CSharpEditor.Compiler;

/// <summary>
/// Gera saída no estilo do exemplo Java (Unifacvest / AnaliseLexica):
/// "Linha N: (lexema, CATEGORIA)" por token, para comparação com o compilador de referência.
/// </summary>
public static class LegacyLexicalFormatter
{
    public static string Format(LexerResult result)
    {
        using var w = new StringWriter();
        foreach (var t in result.Tokens.Where(x => x.Type != TokenType.EndOfFile))
        {
            w.Write("Linha ");
            w.Write(t.Line);
            w.Write(": ");
            w.Write('(');
            w.Write(t.Value);
            w.Write(", ");
            w.Write(GetLegacyCategory(t));
            w.WriteLine(')');
        }
        return w.ToString();
    }

    /// <summary>Categorias alinhadas ao exemplo Java (INTEIRO, REAL, OPERADOR, DELIMITADOR, PALAVRA RESERVADA, IDENTIFICADOR) mais extensões óbvias.</summary>
    public static string GetLegacyCategory(Token t) => t.Type switch
    {
        TokenType.IntegerLiteral => "INTEIRO",
        TokenType.DoubleLiteral => "REAL",
        TokenType.BooleanLiteral => "LITERAL BOOLEANO",
        TokenType.StringLiteral => "STRING",
        TokenType.CharLiteral => "CARACTERE",

        >= TokenType.KwInt and <= TokenType.KwFalse => "PALAVRA RESERVADA",

        >= TokenType.Plus and <= TokenType.SlashAssign => "OPERADOR",

        >= TokenType.LeftParen and <= TokenType.SingleQuote => "DELIMITADOR",

        TokenType.LineComment or TokenType.BlockComment => "COMENTARIO",

        TokenType.Identifier => "IDENTIFICADOR",

        TokenType.Unknown => "DESCONHECIDO",

        TokenType.EndOfFile => "FIM",

        _ => "DESCONHECIDO"
    };
}
