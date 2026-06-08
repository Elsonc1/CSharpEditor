using CSharpEditor.Compiler;
using FluentAssertions;
using Xunit;

namespace CSharpEditor.Tests;

public class BalanceadorTests
{
    [Fact]
    public void Verificar_ParesSimples_RetornaBalanceado()
    {
        var r = Balanceador.Verificar("()");
        r.Balanceado.Should().BeTrue();
        r.Erros.Should().BeEmpty();
    }

    [Fact]
    public void Verificar_AninhadosTodosOsTipos_RetornaBalanceado()
    {
        var r = Balanceador.Verificar("({[]})");
        r.Balanceado.Should().BeTrue();
    }

    [Fact]
    public void Verificar_AbertosSobrando_ReportaCadaUm()
    {
        var r = Balanceador.Verificar("((");
        r.Balanceado.Should().BeFalse();
        r.Erros.Should().HaveCount(2);
        r.Erros.Should().AllSatisfy(e => e.Should().Contain("aberto e não fechado"));
    }

    [Fact]
    public void Verificar_FechamentoSemAbertura_ReportaErro()
    {
        var r = Balanceador.Verificar("})");
        r.Balanceado.Should().BeFalse();
        r.Erros.Should().Contain(e => e.Contains("fechado sem abertura"));
    }

    [Fact]
    public void Verificar_MismatchParenColchete_ReportaTipoErrado()
    {
        var r = Balanceador.Verificar("(]");
        r.Balanceado.Should().BeFalse();
        r.Erros.Should().Contain(e => e.Contains("Esperado ')'") && e.Contains("encontrado ']'"));
    }

    [Fact]
    public void Verificar_CodigoRealDoEditor_RetornaBalanceado()
    {
        const string src = """
            public class Animal {
                private string nome;
                public void main() {
                    if (x > 5) {
                        x = x * 2;
                    } else {
                        x -= 1;
                    }
                }
            }
            """;
        var r = Balanceador.Verificar(src);
        r.Balanceado.Should().BeTrue(because: string.Join("\n", r.Erros));
    }

    [Fact]
    public void Verificar_ChavesDentroDeStringLiteral_NaoConfunde()
    {
        // string literal contém ( ) { } — não deve afetar o balanceamento
        const string src = """
            string s = "abc ( { not real }";
            """;
        var r = Balanceador.Verificar(src);
        r.Balanceado.Should().BeTrue(because: string.Join("\n", r.Erros));
    }

    [Fact]
    public void Verificar_ChavesDentroDeComentario_NaoConfunde()
    {
        const string src = "int x; /* { ( [ */ int y;";
        var r = Balanceador.Verificar(src);
        r.Balanceado.Should().BeTrue(because: string.Join("\n", r.Erros));
    }

    [Fact]
    public void Verificar_Vazio_RetornaBalanceado()
    {
        var r = Balanceador.Verificar("");
        r.Balanceado.Should().BeTrue();
    }

    [Fact]
    public void Verificar_RelataLinhaEColunaDoAbertoNaoFechado()
    {
        const string src = """
            int main() {
                if (x > 0
            """;
        var r = Balanceador.Verificar(src);
        r.Balanceado.Should().BeFalse();
        // o `(` do `if (` está na linha 2 — deve aparecer no relato
        r.Erros.Should().Contain(e => e.Contains("Linha 2") && e.Contains("'('"));
    }

    [Fact]
    public void Verificar_AceitaIEnumerableDeTokensDiretamente()
    {
        var lex = new Lexer("{[()]}").Tokenize();
        var r = Balanceador.Verificar(lex.Tokens);
        r.Balanceado.Should().BeTrue();
    }
}
