using CSharpEditor.Compiler;
using FluentAssertions;
using Xunit;

namespace CSharpEditor.Tests;

public class LexerTests
{
    [Fact]
    public void Tokenize_ReservedWords_ReturnsKeywordTokens()
    {
        var r = new Lexer("int void class if").Tokenize();
        r.HasErrors.Should().BeFalse();
        r.Tokens.Where(t => t.Type != TokenType.EndOfFile).Select(t => t.Type)
            .Should().Equal(TokenType.KwInt, TokenType.KwVoid, TokenType.KwClass, TokenType.KwIf);
    }

    [Fact]
    public void Tokenize_JavaSampleKeywords_MatchesUnifacvestList()
    {
        var src = "int double boolean string void if else for while switch case main public private var class herdar assinar agilizador break continue return import error igor new";
        var r = new Lexer(src).Tokenize();
        r.HasErrors.Should().BeFalse();
        var types = r.Tokens.Where(t => t.Type != TokenType.EndOfFile).Select(t => t.Type).ToList();
        types.Should().HaveCount(26, "a string de teste contém 26 lexemas reservadas (int … new)");
        foreach (var t in types)
            (t >= TokenType.KwInt && t <= TokenType.KwNew).Should().BeTrue("todas as palavras da lista Unifacvest devem ser reconhecidas como palavras reservadas");
    }

    [Fact]
    public void Tokenize_IntegerAndDouble_ClassifiesLiterals()
    {
        var r = new Lexer("42 3.14").Tokenize();
        r.HasErrors.Should().BeFalse();
        var toks = r.Tokens.Where(t => t.Type != TokenType.EndOfFile).ToList();
        toks[0].Type.Should().Be(TokenType.IntegerLiteral);
        toks[1].Type.Should().Be(TokenType.DoubleLiteral);
    }

    [Fact]
    public void Tokenize_OperatorsDoubleChar_RecognizesEqualsAndNotEqual()
    {
        var r = new Lexer("== !=").Tokenize();
        r.HasErrors.Should().BeFalse();
        var toks = r.Tokens.Where(t => t.Type != TokenType.EndOfFile).ToList();
        toks[0].Type.Should().Be(TokenType.Equal);
        toks[1].Type.Should().Be(TokenType.NotEqual);
    }

    [Fact]
    public void Tokenize_LineComment_ProducesLineCommentToken()
    {
        var r = new Lexer("// comentario\nx").Tokenize();
        r.HasErrors.Should().BeFalse();
        r.Tokens[0].Type.Should().Be(TokenType.LineComment);
        r.Tokens[1].Type.Should().Be(TokenType.Identifier);
    }

    [Fact]
    public void Tokenize_UnclosedString_AddsError()
    {
        var r = new Lexer("\"abc").Tokenize();
        r.HasErrors.Should().BeTrue();
        r.Errors.Should().Contain(e => e.Contains("String não fechada"));
    }
}
