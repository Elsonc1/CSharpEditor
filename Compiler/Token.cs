namespace CSharpEditor.Compiler;

public enum TokenType
{
    // ── Literais ──
    IntegerLiteral,
    DoubleLiteral,
    StringLiteral,
    CharLiteral,
    BooleanLiteral,

    // ── Identificador ──
    Identifier,

    // ── Palavras Reservadas ──
    KwInt,
    KwDouble,
    KwBoolean,
    KwString,
    KwVoid,
    KwIf,
    KwElse,
    KwFor,
    KwWhile,
    KwSwitch,
    KwCase,
    KwPublic,
    KwPrivate,
    KwVar,
    KwClass,
    KwMain,
    KwHerdar,
    KwAssinar,       // implements
    KwAgilizador,    // interface
    KwBreak,
    KwContinue,
    KwReturn,
    KwImport,
    KwError,
    KwIgor,          // thread
    KwNew,
    KwTrue,
    KwFalse,

    // ── Operadores ──
    Plus,            // +
    Minus,           // -
    Star,            // *
    Slash,           // /
    Percent,         // %
    Assign,          // =
    Equal,           // ==
    NotEqual,        // !=
    Less,            // <
    Greater,         // >
    LessEqual,       // <=
    GreaterEqual,    // >=
    And,             // &&
    Or,              // ||
    Not,             // !
    PlusPlus,        // ++
    MinusMinus,      // --
    PlusAssign,      // +=
    MinusAssign,     // -=
    StarAssign,      // *=
    SlashAssign,     // /=

    // ── Delimitadores ──
    LeftParen,       // (
    RightParen,      // )
    LeftBracket,     // [
    RightBracket,    // ]
    LeftBrace,       // {
    RightBrace,      // }
    Comma,           // ,
    Semicolon,       // ;
    Dot,             // .
    Colon,           // :
    Quote,           // "
    SingleQuote,     // '

    // ── Comentários ──
    LineComment,     // // ...
    BlockComment,    // /* ... */

    // ── Especial ──
    EndOfFile,
    Unknown
}

public class Token
{
    public string Value { get; }
    public TokenType Type { get; }
    public int Line { get; }
    public int Column { get; }

    public Token(TokenType type, string value, int line, int column)
    {
        Type = type;
        Value = value;
        Line = line;
        Column = column;
    }

    public string TypeName => Type switch
    {
        TokenType.IntegerLiteral => "Número Inteiro",
        TokenType.DoubleLiteral => "Número Decimal",
        TokenType.StringLiteral => "String",
        TokenType.CharLiteral => "Caractere",
        TokenType.BooleanLiteral => "Literal Booleano",

        TokenType.Identifier => "Identificador",

        >= TokenType.KwInt and <= TokenType.KwFalse => "Palavra Reservada",

        >= TokenType.Plus and <= TokenType.StarAssign => "Operador",

        >= TokenType.LeftParen and <= TokenType.SingleQuote => "Delimitador",

        TokenType.LineComment or TokenType.BlockComment => "Comentário",

        TokenType.EndOfFile => "Fim de Arquivo",
        _ => "Desconhecido"
    };

    public override string ToString() => $"[{TypeName}] '{Value}' (Ln {Line}, Col {Column})";
}
