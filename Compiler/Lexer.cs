namespace CSharpEditor.Compiler;

public class LexerResult
{
    public List<Token> Tokens { get; } = new();
    public List<string> Errors { get; } = new();
    public bool HasErrors => Errors.Count > 0;
}

public class Lexer
{
    private readonly string _source;
    private int _pos;
    private int _line;
    private int _column;

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        ["int"] = TokenType.KwInt,
        ["double"] = TokenType.KwDouble,
        ["boolean"] = TokenType.KwBoolean,
        ["string"] = TokenType.KwString,
        ["void"] = TokenType.KwVoid,
        ["if"] = TokenType.KwIf,
        ["else"] = TokenType.KwElse,
        ["for"] = TokenType.KwFor,
        ["while"] = TokenType.KwWhile,
        ["switch"] = TokenType.KwSwitch,
        ["case"] = TokenType.KwCase,
        ["public"] = TokenType.KwPublic,
        ["private"] = TokenType.KwPrivate,
        ["var"] = TokenType.KwVar,
        ["class"] = TokenType.KwClass,
        ["main"] = TokenType.KwMain,
        ["herdar"] = TokenType.KwHerdar,
        ["assinar"] = TokenType.KwAssinar,
        ["agilizador"] = TokenType.KwAgilizador,
        ["break"] = TokenType.KwBreak,
        ["continue"] = TokenType.KwContinue,
        ["return"] = TokenType.KwReturn,
        ["import"] = TokenType.KwImport,
        ["error"] = TokenType.KwError,
        ["igor"] = TokenType.KwIgor,
        ["new"] = TokenType.KwNew,
        ["true"] = TokenType.KwTrue,
        ["false"] = TokenType.KwFalse,
    };

    public Lexer(string source)
    {
        _source = source;
        _pos = 0;
        _line = 1;
        _column = 1;
    }

    private char Current => _pos < _source.Length ? _source[_pos] : '\0';
    private char Peek => _pos + 1 < _source.Length ? _source[_pos + 1] : '\0';
    private bool IsAtEnd => _pos >= _source.Length;

    private char Advance()
    {
        var ch = Current;
        _pos++;
        if (ch == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }
        return ch;
    }

    public LexerResult Tokenize()
    {
        var result = new LexerResult();

        while (!IsAtEnd)
        {
            SkipWhitespace();
            if (IsAtEnd) break;

            int startLine = _line;
            int startCol = _column;

            // Single-line comment: //
            if (Current == '/' && Peek == '/')
            {
                result.Tokens.Add(ReadLineComment(startLine, startCol));
                continue;
            }

            // Block comment: /* ... */
            if (Current == '/' && Peek == '*')
            {
                result.Tokens.Add(ReadBlockComment(startLine, startCol, result));
                continue;
            }

            var token = ReadToken(result);
            if (token != null)
                result.Tokens.Add(token);
        }

        result.Tokens.Add(new Token(TokenType.EndOfFile, "", _line, _column));
        return result;
    }

    private void SkipWhitespace()
    {
        while (!IsAtEnd && char.IsWhiteSpace(Current))
            Advance();
    }

    private Token ReadLineComment(int line, int col)
    {
        int start = _pos;
        while (!IsAtEnd && Current != '\n')
            Advance();
        string value = _source[start.._pos];
        return new Token(TokenType.LineComment, value, line, col);
    }

    private Token ReadBlockComment(int line, int col, LexerResult result)
    {
        int start = _pos;
        Advance(); // /
        Advance(); // *

        while (!IsAtEnd)
        {
            if (Current == '*' && Peek == '/')
            {
                Advance(); // *
                Advance(); // /
                string value = _source[start.._pos];
                return new Token(TokenType.BlockComment, value, line, col);
            }
            Advance();
        }

        result.Errors.Add($"Linha {line}, Coluna {col}: Comentário de bloco não fechado.");
        string partial = _source[start.._pos];
        return new Token(TokenType.BlockComment, partial, line, col);
    }

    private Token? ReadToken(LexerResult result)
    {
        int startLine = _line;
        int startCol = _column;

        if (char.IsLetter(Current) || Current == '_')
            return ReadIdentifierOrKeyword(startLine, startCol);

        if (char.IsDigit(Current))
            return ReadNumber(startLine, startCol, result);

        if (Current == '"')
            return ReadString(startLine, startCol, result);

        if (Current == '\'')
            return ReadChar(startLine, startCol, result);

        return ReadOperatorOrDelimiter(startLine, startCol, result);
    }

    private Token ReadIdentifierOrKeyword(int line, int col)
    {
        int start = _pos;
        while (!IsAtEnd && (char.IsLetterOrDigit(Current) || Current == '_'))
            Advance();

        string value = _source[start.._pos];

        if (Keywords.TryGetValue(value, out var kwType))
        {
            if (kwType == TokenType.KwTrue || kwType == TokenType.KwFalse)
                return new Token(TokenType.BooleanLiteral, value, line, col);
            return new Token(kwType, value, line, col);
        }

        return new Token(TokenType.Identifier, value, line, col);
    }

    private Token ReadNumber(int line, int col, LexerResult result)
    {
        int start = _pos;
        bool hasDecimal = false;

        while (!IsAtEnd && (char.IsDigit(Current) || Current == '.'))
        {
            if (Current == '.')
            {
                if (hasDecimal)
                {
                    result.Errors.Add($"Linha {line}, Coluna {col}: Número com múltiplos pontos decimais.");
                    break;
                }
                hasDecimal = true;
            }
            Advance();
        }

        string value = _source[start.._pos];
        var type = hasDecimal ? TokenType.DoubleLiteral : TokenType.IntegerLiteral;
        return new Token(type, value, line, col);
    }

    private Token ReadString(int line, int col, LexerResult result)
    {
        Advance(); // opening "
        int start = _pos;

        while (!IsAtEnd && Current != '"' && Current != '\n')
        {
            if (Current == '\\') Advance(); // skip escaped char
            Advance();
        }

        if (IsAtEnd || Current == '\n')
        {
            result.Errors.Add($"Linha {line}, Coluna {col}: String não fechada.");
            string partial = _source[start.._pos];
            return new Token(TokenType.StringLiteral, $"\"{partial}", line, col);
        }

        string value = _source[start.._pos];
        Advance(); // closing "
        return new Token(TokenType.StringLiteral, $"\"{value}\"", line, col);
    }

    private Token ReadChar(int line, int col, LexerResult result)
    {
        Advance(); // opening '
        int start = _pos;

        while (!IsAtEnd && Current != '\'' && Current != '\n')
        {
            if (Current == '\\') Advance();
            Advance();
        }

        if (IsAtEnd || Current == '\n')
        {
            result.Errors.Add($"Linha {line}, Coluna {col}: Caractere não fechado.");
            string partial = _source[start.._pos];
            return new Token(TokenType.CharLiteral, $"'{partial}", line, col);
        }

        string value = _source[start.._pos];
        Advance(); // closing '
        return new Token(TokenType.CharLiteral, $"'{value}'", line, col);
    }

    private Token? ReadOperatorOrDelimiter(int line, int col, LexerResult result)
    {
        char ch = Advance();

        switch (ch)
        {
            // ── Operators with multi-char variants ──
            case '+':
                if (Current == '+') { Advance(); return new Token(TokenType.PlusPlus, "++", line, col); }
                if (Current == '=') { Advance(); return new Token(TokenType.PlusAssign, "+=", line, col); }
                return new Token(TokenType.Plus, "+", line, col);

            case '-':
                if (Current == '-') { Advance(); return new Token(TokenType.MinusMinus, "--", line, col); }
                if (Current == '=') { Advance(); return new Token(TokenType.MinusAssign, "-=", line, col); }
                return new Token(TokenType.Minus, "-", line, col);

            case '*':
                if (Current == '=') { Advance(); return new Token(TokenType.StarAssign, "*=", line, col); }
                return new Token(TokenType.Star, "*", line, col);

            case '/':
                if (Current == '=') { Advance(); return new Token(TokenType.SlashAssign, "/=", line, col); }
                return new Token(TokenType.Slash, "/", line, col);

            case '%':
                return new Token(TokenType.Percent, "%", line, col);

            case '=':
                if (Current == '=') { Advance(); return new Token(TokenType.Equal, "==", line, col); }
                return new Token(TokenType.Assign, "=", line, col);

            case '!':
                if (Current == '=') { Advance(); return new Token(TokenType.NotEqual, "!=", line, col); }
                return new Token(TokenType.Not, "!", line, col);

            case '<':
                if (Current == '=') { Advance(); return new Token(TokenType.LessEqual, "<=", line, col); }
                return new Token(TokenType.Less, "<", line, col);

            case '>':
                if (Current == '=') { Advance(); return new Token(TokenType.GreaterEqual, ">=", line, col); }
                return new Token(TokenType.Greater, ">", line, col);

            case '&':
                if (Current == '&') { Advance(); return new Token(TokenType.And, "&&", line, col); }
                result.Errors.Add($"Linha {line}, Coluna {col}: Caractere inesperado '&'. Você quis dizer '&&'?");
                return new Token(TokenType.Unknown, "&", line, col);

            case '|':
                if (Current == '|') { Advance(); return new Token(TokenType.Or, "||", line, col); }
                result.Errors.Add($"Linha {line}, Coluna {col}: Caractere inesperado '|'. Você quis dizer '||'?");
                return new Token(TokenType.Unknown, "|", line, col);

            // ── Delimiters ──
            case '(': return new Token(TokenType.LeftParen, "(", line, col);
            case ')': return new Token(TokenType.RightParen, ")", line, col);
            case '[': return new Token(TokenType.LeftBracket, "[", line, col);
            case ']': return new Token(TokenType.RightBracket, "]", line, col);
            case '{': return new Token(TokenType.LeftBrace, "{", line, col);
            case '}': return new Token(TokenType.RightBrace, "}", line, col);
            case ',': return new Token(TokenType.Comma, ",", line, col);
            case ';': return new Token(TokenType.Semicolon, ";", line, col);
            case '.': return new Token(TokenType.Dot, ".", line, col);
            case ':': return new Token(TokenType.Colon, ":", line, col);

            default:
                result.Errors.Add($"Linha {line}, Coluna {col}: Caractere inesperado '{ch}'.");
                return new Token(TokenType.Unknown, ch.ToString(), line, col);
        }
    }
}
