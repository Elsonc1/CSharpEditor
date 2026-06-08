using CSharpEditor.Compiler;
using FluentAssertions;
using Xunit;

namespace CSharpEditor.Tests;

/// <summary>
/// Cobre o requisito 2 do trabalho ("análise das estruturas").
/// Cada estrutura tem ≥ 1 caso feliz + ≥ 1 caso de erro.
/// </summary>
public class EstruturasTests
{
    private static (ParserResult parse, SemanticResult? sem) Run(string src)
    {
        var lex = new Lexer(src).Tokenize();
        var parse = new Parser(lex.Tokens).Parse();
        SemanticResult? sem = null;
        if (parse.Program != null)
            sem = new SemanticAnalyzer().Analyze(parse.Program);
        return (parse, sem);
    }

    // ── IF / ELSE / ELSE IF ──────────────────────────────────────────────────

    [Fact]
    public void If_Simples_Aceita()
    {
        var (p, s) = Run("""
            public class A { public void main() {
                if (true) { int x = 1; }
            } }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeFalse(string.Join("\n", s.Errors));
    }

    [Fact]
    public void If_ComElse_Aceita()
    {
        var (p, s) = Run("""
            public class A { public void main() {
                if (true) { int x = 1; } else { int y = 2; }
            } }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeFalse(string.Join("\n", s.Errors));
    }

    [Fact]
    public void If_ElseIf_EmCadeia_Aceita()
    {
        var (p, s) = Run("""
            public class A { public void main() {
                int x = 1;
                if (x == 1) { x = 10; }
                else if (x == 2) { x = 20; }
                else if (x == 3) { x = 30; }
                else { x = 0; }
            } }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeFalse(string.Join("\n", s.Errors));
    }

    [Fact]
    public void If_SemParenteses_ReportaErroSintatico()
    {
        var (p, _) = Run("""
            public class A { public void main() {
                if true { int x = 1; }
            } }
            """);
        p.HasErrors.Should().BeTrue();
        p.Errors.Should().Contain(e => e.Contains("'('"));
    }

    [Fact]
    public void If_CondicaoNaoBoolean_ReportaErroSemantico()
    {
        var (p, s) = Run("""
            public class A { public void main() {
                int x = 1;
                if (x) { int y = 1; }
            } }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeTrue();
        s.Errors.Should().Contain(e => e.Contains("boolean"));
    }

    [Fact]
    public void If_BlocoVazio_Aceita()
    {
        var (p, s) = Run("""
            public class A { public void main() {
                if (true) { }
            } }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeFalse(string.Join("\n", s.Errors));
    }

    // ── FOR ──────────────────────────────────────────────────────────────────

    [Fact]
    public void For_Padrao_Aceita()
    {
        var (p, s) = Run("""
            public class A { public void main() {
                for (int i = 0; i < 10; i++) { int x = i; }
            } }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeFalse(string.Join("\n", s.Errors));
    }

    [Fact]
    public void For_FaltandoSemicolon_ReportaErroSintatico()
    {
        var (p, _) = Run("""
            public class A { public void main() {
                for (int i = 0  i < 10; i++) { }
            } }
            """);
        p.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void For_CondicaoNaoBoolean_ReportaErroSemantico()
    {
        var (p, s) = Run("""
            public class A { public void main() {
                for (int i = 0; i; i++) { }
            } }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeTrue();
        s.Errors.Should().Contain(e => e.Contains("'for'") && e.Contains("boolean"));
    }

    // ── WHILE ────────────────────────────────────────────────────────────────

    [Fact]
    public void While_Padrao_Aceita()
    {
        var (p, s) = Run("""
            public class A { public void main() {
                int i = 0;
                while (i < 5) { i++; }
            } }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeFalse(string.Join("\n", s.Errors));
    }

    [Fact]
    public void While_CondicaoNaoBoolean_ReportaErroSemantico()
    {
        var (p, s) = Run("""
            public class A { public void main() {
                int x = 1;
                while (x) { x = x - 1; }
            } }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeTrue();
        s.Errors.Should().Contain(e => e.Contains("'while'") && e.Contains("boolean"));
    }

    // ── SWITCH / CASE ────────────────────────────────────────────────────────

    [Fact]
    public void Switch_ComMultiplosCases_Aceita()
    {
        var (p, s) = Run("""
            public class A { public void main() {
                int x = 1;
                switch (x) {
                    case 1: x = 10; break;
                    case 2: x = 20; break;
                }
            } }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeFalse(string.Join("\n", s.Errors));
    }

    // ── CLASSE / HERANÇA / INTERFACES ────────────────────────────────────────

    [Fact]
    public void Classe_ComHerdar_Aceita()
    {
        var (p, _) = Run("""
            public class B herdar A { }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
    }

    [Fact]
    public void Classe_ComAssinarMultiplasInterfaces_Aceita()
    {
        var (p, _) = Run("""
            public class C assinar I1, I2, I3 { }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
    }

    [Fact]
    public void Classe_SemNome_ReportaErroSintatico()
    {
        var (p, _) = Run("public class { }");
        p.HasErrors.Should().BeTrue();
        p.Errors.Should().Contain(e => e.Contains("nome da classe"));
    }

    // ── MÉTODOS ──────────────────────────────────────────────────────────────

    [Fact]
    public void Metodo_ComParametrosTipados_Aceita()
    {
        var (p, _) = Run("""
            public class A {
                public int soma(int a, int b) { return a; }
            }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
    }

    [Fact]
    public void Metodo_VoidComReturnComValor_ReportaErroSemantico()
    {
        var (p, s) = Run("""
            public class A {
                public void main() { return 1; }
            }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeTrue();
        s.Errors.Should().Contain(e => e.Contains("void") && e.Contains("não pode retornar"));
    }

    [Fact]
    public void Metodo_IntComReturnString_ReportaErroSemantico()
    {
        var (p, s) = Run("""
            public class A {
                public int x() { return "abc"; }
            }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeTrue();
        s.Errors.Should().Contain(e => e.Contains("Tipo de retorno incompatível"));
    }

    [Fact]
    public void Metodo_IntSemValorNoReturn_ReportaErroSemantico()
    {
        var (p, s) = Run("""
            public class A {
                public int x() { return; }
            }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeTrue();
        s.Errors.Should().Contain(e => e.Contains("Esperado valor de retorno"));
    }

    // ── BREAK / CONTINUE — contexto ─────────────────────────────────────────

    [Fact]
    public void Break_DentroDeWhile_Aceita()
    {
        var (p, s) = Run("""
            public class A { public void main() {
                while (true) { break; }
            } }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeFalse(string.Join("\n", s.Errors));
    }

    [Fact]
    public void Break_ForaDeContexto_ReportaErroSemantico()
    {
        var (p, s) = Run("""
            public class A { public void main() {
                break;
            } }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeTrue();
        s.Errors.Should().Contain(e => e.Contains("'break' fora"));
    }

    [Fact]
    public void Continue_DentroDeFor_Aceita()
    {
        var (p, s) = Run("""
            public class A { public void main() {
                for (int i = 0; i < 5; i++) { continue; }
            } }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeFalse(string.Join("\n", s.Errors));
    }

    [Fact]
    public void Continue_DentroDeSwitchSemLoop_ReportaErroSemantico()
    {
        // continue dentro de switch puro (sem loop envolvendo) é semanticamente inválido
        var (p, s) = Run("""
            public class A { public void main() {
                int x = 1;
                switch (x) {
                    case 1: continue;
                }
            } }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeTrue();
        s.Errors.Should().Contain(e => e.Contains("'continue' fora"));
    }

    [Fact]
    public void Break_DentroDeSwitch_Aceita()
    {
        var (p, s) = Run("""
            public class A { public void main() {
                int x = 1;
                switch (x) {
                    case 1: break;
                }
            } }
            """);
        p.HasErrors.Should().BeFalse(string.Join("\n", p.Errors));
        s!.HasErrors.Should().BeFalse(string.Join("\n", s.Errors));
    }

    // ── Mensagens de erro incluem linha E coluna ────────────────────────────

    [Fact]
    public void Erros_SemanticosIncluemColunaAlemDaLinha()
    {
        var (_, s) = Run("""
            public class A { public void main() {
                int y = z;
            } }
            """);
        s!.HasErrors.Should().BeTrue();
        s.Errors.Should().Contain(e => e.Contains("Linha") && e.Contains("Coluna"));
    }
}
