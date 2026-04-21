using CSharpEditor.Compiler;
using FluentAssertions;
using Xunit;

namespace CSharpEditor.Tests;

public class LegacyLexicalFormatterTests
{
    [Fact]
    public void Format_MatchesJavaStyleLines()
    {
        var r = new Lexer("10 2.5 + if nome").Tokenize();
        r.HasErrors.Should().BeFalse();
        var log = LegacyLexicalFormatter.Format(r);
        log.Should().Contain("Linha 1: (10, INTEIRO)");
        log.Should().Contain("Linha 1: (2.5, REAL)");
        log.Should().Contain("Linha 1: (+, OPERADOR)");
        log.Should().Contain("Linha 1: (if, PALAVRA RESERVADA)");
        log.Should().Contain("Linha 1: (nome, IDENTIFICADOR)");
    }

    [Theory]
    [InlineData("int", "PALAVRA RESERVADA")]
    [InlineData("x", "IDENTIFICADOR")]
    [InlineData("(", "DELIMITADOR")]
    [InlineData("==", "OPERADOR")]
    public void GetLegacyCategory_SingleToken(string src, string expectedCategory)
    {
        var t = new Lexer(src).Tokenize().Tokens.First(x => x.Type != TokenType.EndOfFile);
        LegacyLexicalFormatter.GetLegacyCategory(t).Should().Be(expectedCategory);
    }
}
