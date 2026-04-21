using CSharpEditor.Compiler;
using FluentAssertions;
using Xunit;

namespace CSharpEditor.Tests;

public class ParserSemanticSmokeTests
{
    [Fact]
    public void Parse_MinimalClassWithMain_NoSyntaxErrors()
    {
        const string src = """
            public class A {
                public void main() {
                }
            }
            """;
        var lex = new Lexer(src).Tokenize();
        lex.HasErrors.Should().BeFalse();
        var parse = new Parser(lex.Tokens).Parse();
        parse.HasErrors.Should().BeFalse(because: string.Join("; ", parse.Errors));
        parse.Program.Should().NotBeNull();
    }

    [Fact]
    public void Semantic_DuplicateVariableInSameBlock_ReportsError()
    {
        const string src = """
            public class A {
                public void main() {
                    int x = 1;
                    int x = 2;
                }
            }
            """;
        var lex = new Lexer(src).Tokenize();
        var parse = new Parser(lex.Tokens).Parse();
        parse.Program.Should().NotBeNull();
        var sem = new SemanticAnalyzer().Analyze(parse.Program!);
        sem.HasErrors.Should().BeTrue();
        sem.Errors.Should().Contain(e => e.Contains("já foi declarada", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Semantic_ValidExampleFromDefaultEditor_Passes()
    {
        const string src = """
            import utils;

            public class Animal {
                private string nome;
                private int idade;

                public void main() {
                    int x = 10;
                }
            }
            """;
        var lex = new Lexer(src).Tokenize();
        var parse = new Parser(lex.Tokens).Parse();
        parse.HasErrors.Should().BeFalse(string.Join("\n", parse.Errors));
        parse.Program.Should().NotBeNull();
        var sem = new SemanticAnalyzer().Analyze(parse.Program!);
        sem.HasErrors.Should().BeFalse(string.Join("\n", sem.Errors));
    }
}
