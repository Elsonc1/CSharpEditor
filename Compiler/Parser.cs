namespace CSharpEditor.Compiler;

public class ParserResult
{
    public ProgramNode? Program { get; set; }
    public List<string> Errors { get; } = new();
    public bool HasErrors => Errors.Count > 0;
}

public class Parser
{
    private readonly List<Token> _tokens;
    private int _pos;
    private readonly ParserResult _result = new();

    private static readonly HashSet<TokenType> TypeKeywords = new()
    {
        TokenType.KwInt, TokenType.KwDouble, TokenType.KwBoolean,
        TokenType.KwString, TokenType.KwVoid, TokenType.KwVar
    };

    public Parser(List<Token> tokens)
    {
        _tokens = tokens.Where(t =>
            t.Type != TokenType.LineComment && t.Type != TokenType.BlockComment).ToList();
        _pos = 0;
    }

    private Token Current => _pos < _tokens.Count ? _tokens[_pos] : _tokens[^1];
    private Token Previous => _pos > 0 ? _tokens[_pos - 1] : _tokens[0];
    private bool IsAtEnd => Current.Type == TokenType.EndOfFile;

    private Token Advance()
    {
        var token = Current;
        if (!IsAtEnd) _pos++;
        return token;
    }

    private bool Check(TokenType type) => Current.Type == type;

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private Token Expect(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        _result.Errors.Add($"Linha {Current.Line}, Coluna {Current.Column}: {message} (encontrado '{Current.Value}')");
        return Current;
    }

    private void Synchronize()
    {
        Advance();
        while (!IsAtEnd)
        {
            if (Previous.Type == TokenType.Semicolon) return;
            if (Previous.Type == TokenType.RightBrace) return;
            switch (Current.Type)
            {
                case TokenType.KwInt or TokenType.KwDouble or TokenType.KwBoolean
                    or TokenType.KwString or TokenType.KwVoid or TokenType.KwVar:
                case TokenType.KwIf:
                case TokenType.KwFor:
                case TokenType.KwWhile:
                case TokenType.KwSwitch:
                case TokenType.KwReturn:
                case TokenType.KwBreak:
                case TokenType.KwContinue:
                case TokenType.KwClass:
                case TokenType.KwPublic:
                case TokenType.KwPrivate:
                case TokenType.KwImport:
                    return;
            }
            Advance();
        }
    }

    public ParserResult Parse()
    {
        var program = new ProgramNode { Line = 1, Column = 1 };

        while (!IsAtEnd)
        {
            try
            {
                var stmt = ParseTopLevel();
                if (stmt != null)
                    program.Statements.Add(stmt);
            }
            catch (Exception)
            {
                Synchronize();
            }
        }

        _result.Program = program;
        return _result;
    }

    private AstNode? ParseTopLevel()
    {
        if (Check(TokenType.KwImport))
            return ParseImport();

        if (Check(TokenType.KwPublic) || Check(TokenType.KwPrivate))
        {
            int saved = _pos;
            Advance(); // access modifier
            if (Check(TokenType.KwClass))
            {
                _pos = saved;
                return ParseClass();
            }
            if (Check(TokenType.KwAgilizador))
            {
                _pos = saved;
                return ParseInterface();
            }
            _pos = saved;
        }

        if (Check(TokenType.KwClass))
            return ParseClass();

        if (Check(TokenType.KwAgilizador))
            return ParseInterface();

        return ParseStatement();
    }

    private ImportNode ParseImport()
    {
        var token = Advance(); // import
        var name = Expect(TokenType.Identifier, "Esperado nome do módulo após 'import'.");
        Expect(TokenType.Semicolon, "Esperado ';' após import.");
        return new ImportNode { ModuleName = name.Value, Line = token.Line, Column = token.Column };
    }

    private ClassNode ParseClass()
    {
        string access = "";
        int line = Current.Line, col = Current.Column;

        if (Match(TokenType.KwPublic)) access = "public";
        else if (Match(TokenType.KwPrivate)) access = "private";

        Expect(TokenType.KwClass, "Esperado 'class'.");
        var nameToken = Expect(TokenType.Identifier, "Esperado nome da classe.");

        string? baseClass = null;
        var interfaces = new List<string>();

        if (Match(TokenType.KwHerdar))
        {
            var baseToken = Expect(TokenType.Identifier, "Esperado nome da classe base.");
            baseClass = baseToken.Value;
        }

        if (Match(TokenType.KwAssinar))
        {
            var iface = Expect(TokenType.Identifier, "Esperado nome da interface.");
            interfaces.Add(iface.Value);
            while (Match(TokenType.Comma))
            {
                iface = Expect(TokenType.Identifier, "Esperado nome da interface.");
                interfaces.Add(iface.Value);
            }
        }

        var body = ParseBlock();
        var node = new ClassNode
        {
            AccessModifier = access,
            Name = nameToken.Value,
            BaseClass = baseClass,
            Body = body,
            Line = line,
            Column = col
        };
        foreach (var i in interfaces) node.Interfaces.Add(i);
        return node;
    }

    private InterfaceNode ParseInterface()
    {
        if (Match(TokenType.KwPublic) || Match(TokenType.KwPrivate)) { }
        var token = Expect(TokenType.KwAgilizador, "Esperado 'agilizador'.");
        var name = Expect(TokenType.Identifier, "Esperado nome da interface.");
        var body = ParseBlock();
        return new InterfaceNode { Name = name.Value, Body = body, Line = token.Line, Column = token.Column };
    }

    private AstNode? ParseStatement()
    {
        if (IsTypeKeyword())
            return ParseVarDeclarationOrMethod();

        if (Check(TokenType.KwIf)) return ParseIf();
        if (Check(TokenType.KwWhile)) return ParseWhile();
        if (Check(TokenType.KwFor)) return ParseFor();
        if (Check(TokenType.KwSwitch)) return ParseSwitch();
        if (Check(TokenType.KwReturn)) return ParseReturn();
        if (Check(TokenType.KwBreak)) return ParseBreak();
        if (Check(TokenType.KwContinue)) return ParseContinue();
        if (Check(TokenType.LeftBrace)) return ParseBlock();

        if (Check(TokenType.KwPublic) || Check(TokenType.KwPrivate))
        {
            int saved = _pos;
            Advance();
            if (IsTypeKeyword())
            {
                _pos = saved;
                return ParseVarDeclarationOrMethod();
            }
            _pos = saved;
        }

        return ParseExpressionStatement();
    }

    private bool IsTypeKeyword() => TypeKeywords.Contains(Current.Type);

    private AstNode ParseVarDeclarationOrMethod()
    {
        string access = "";
        if (Match(TokenType.KwPublic)) access = "public";
        else if (Match(TokenType.KwPrivate)) access = "private";

        var typeToken = Advance(); // type
        var nameToken = Expect(TokenType.Identifier, "Esperado nome.");

        // Method: type name ( ... ) { ... }
        if (Check(TokenType.LeftParen))
        {
            return ParseMethodBody(access, typeToken, nameToken);
        }

        // Variable declaration
        AstNode? initializer = null;
        if (Match(TokenType.Assign))
            initializer = ParseExpression();

        Expect(TokenType.Semicolon, "Esperado ';' após declaração de variável.");
        return new VarDeclarationNode
        {
            TypeName = typeToken.Value,
            Name = nameToken.Value,
            Initializer = initializer,
            Line = typeToken.Line,
            Column = typeToken.Column
        };
    }

    private MethodNode ParseMethodBody(string access, Token typeToken, Token nameToken)
    {
        Expect(TokenType.LeftParen, "Esperado '('.");
        var parameters = new List<ParameterNode>();

        if (!Check(TokenType.RightParen))
        {
            do
            {
                var pType = Advance();
                var pName = Expect(TokenType.Identifier, "Esperado nome do parâmetro.");
                parameters.Add(new ParameterNode
                {
                    TypeName = pType.Value, Name = pName.Value,
                    Line = pType.Line, Column = pType.Column
                });
            } while (Match(TokenType.Comma));
        }

        Expect(TokenType.RightParen, "Esperado ')'.");
        var body = ParseBlock();

        var method = new MethodNode
        {
            AccessModifier = access,
            ReturnType = typeToken.Value,
            Name = nameToken.Value,
            Body = body,
            Line = typeToken.Line,
            Column = typeToken.Column
        };
        foreach (var p in parameters) method.Parameters.Add(p);
        return method;
    }

    private AstNode ParseExpressionStatement()
    {
        var expr = ParseExpression();

        // identifier = expr ;
        if (expr is IdentifierNode id && Match(TokenType.Assign, TokenType.PlusAssign,
                TokenType.MinusAssign, TokenType.StarAssign, TokenType.SlashAssign))
        {
            var op = Previous;
            var value = ParseExpression();
            Expect(TokenType.Semicolon, "Esperado ';' após atribuição.");
            return new AssignmentNode
            {
                Name = id.Name, Operator = op.Value, Value = value,
                Line = id.Line, Column = id.Column
            };
        }

        // identifier++ ; or identifier-- ;
        if (expr is UnaryExprNode { IsPostfix: true })
        {
            Expect(TokenType.Semicolon, "Esperado ';'.");
            return expr;
        }

        // print(...) and other call expressions as statements
        if (expr is MethodCallNode or PrintNode)
        {
            Expect(TokenType.Semicolon, "Esperado ';'.");
            return expr;
        }

        Expect(TokenType.Semicolon, "Esperado ';'.");
        return expr;
    }

    private IfNode ParseIf()
    {
        var ifToken = Advance();
        Expect(TokenType.LeftParen, "Esperado '(' após 'if'.");
        var condition = ParseExpression();
        Expect(TokenType.RightParen, "Esperado ')' após condição do 'if'.");
        var thenBranch = ParseBlock();

        BlockNode? elseBranch = null;
        if (Match(TokenType.KwElse))
            elseBranch = ParseBlock();

        return new IfNode
        {
            Condition = condition, ThenBranch = thenBranch, ElseBranch = elseBranch,
            Line = ifToken.Line, Column = ifToken.Column
        };
    }

    private WhileNode ParseWhile()
    {
        var token = Advance();
        Expect(TokenType.LeftParen, "Esperado '(' após 'while'.");
        var condition = ParseExpression();
        Expect(TokenType.RightParen, "Esperado ')' após condição do 'while'.");
        var body = ParseBlock();

        return new WhileNode
        {
            Condition = condition, Body = body,
            Line = token.Line, Column = token.Column
        };
    }

    private ForNode ParseFor()
    {
        var token = Advance();
        Expect(TokenType.LeftParen, "Esperado '(' após 'for'.");

        AstNode? init = null;
        if (!Check(TokenType.Semicolon))
        {
            if (IsTypeKeyword())
            {
                var typeToken = Advance();
                var nameToken = Expect(TokenType.Identifier, "Esperado nome de variável.");
                AstNode? initializer = null;
                if (Match(TokenType.Assign))
                    initializer = ParseExpression();
                init = new VarDeclarationNode
                {
                    TypeName = typeToken.Value, Name = nameToken.Value, Initializer = initializer,
                    Line = typeToken.Line, Column = typeToken.Column
                };
            }
            else
            {
                init = ParseExpression();
            }
        }
        Expect(TokenType.Semicolon, "Esperado ';' no 'for'.");

        AstNode? condition = null;
        if (!Check(TokenType.Semicolon))
            condition = ParseExpression();
        Expect(TokenType.Semicolon, "Esperado ';' no 'for'.");

        AstNode? increment = null;
        if (!Check(TokenType.RightParen))
        {
            increment = ParseExpression();
            // Handle postfix ++ / -- or assignment in increment
            if (increment is IdentifierNode incId)
            {
                if (Match(TokenType.PlusPlus))
                    increment = new UnaryExprNode
                    {
                        Operator = "++", Operand = incId, IsPostfix = true,
                        Line = incId.Line, Column = incId.Column
                    };
                else if (Match(TokenType.MinusMinus))
                    increment = new UnaryExprNode
                    {
                        Operator = "--", Operand = incId, IsPostfix = true,
                        Line = incId.Line, Column = incId.Column
                    };
                else if (Match(TokenType.Assign, TokenType.PlusAssign,
                             TokenType.MinusAssign, TokenType.StarAssign, TokenType.SlashAssign))
                {
                    var op = Previous;
                    var val = ParseExpression();
                    increment = new AssignmentNode
                    {
                        Name = incId.Name, Operator = op.Value, Value = val,
                        Line = incId.Line, Column = incId.Column
                    };
                }
            }
        }
        Expect(TokenType.RightParen, "Esperado ')' no 'for'.");

        var body = ParseBlock();
        return new ForNode
        {
            Init = init, Condition = condition, Increment = increment, Body = body,
            Line = token.Line, Column = token.Column
        };
    }

    private SwitchNode ParseSwitch()
    {
        var token = Advance();
        Expect(TokenType.LeftParen, "Esperado '(' após 'switch'.");
        var expr = ParseExpression();
        Expect(TokenType.RightParen, "Esperado ')' após expressão do 'switch'.");
        Expect(TokenType.LeftBrace, "Esperado '{'.");

        var node = new SwitchNode { Expression = expr, Line = token.Line, Column = token.Column };

        while (!Check(TokenType.RightBrace) && !IsAtEnd)
        {
            if (Match(TokenType.KwCase))
            {
                var caseVal = ParseExpression();
                Expect(TokenType.Colon, "Esperado ':' após valor do 'case'.");
                var caseNode = new CaseNode { Value = caseVal, Line = Previous.Line, Column = Previous.Column };
                while (!Check(TokenType.KwCase) && !Check(TokenType.KwBreak)
                       && !Check(TokenType.RightBrace) && !IsAtEnd)
                {
                    var stmt = ParseStatement();
                    if (stmt != null) caseNode.Statements.Add(stmt);
                }
                if (Match(TokenType.KwBreak))
                    Expect(TokenType.Semicolon, "Esperado ';' após 'break'.");
                node.Cases.Add(caseNode);
            }
            else
            {
                Advance();
            }
        }

        Expect(TokenType.RightBrace, "Esperado '}'.");
        return node;
    }

    private ReturnNode ParseReturn()
    {
        var token = Advance();
        AstNode? expr = null;
        if (!Check(TokenType.Semicolon))
            expr = ParseExpression();
        Expect(TokenType.Semicolon, "Esperado ';' após 'return'.");
        return new ReturnNode { Expression = expr, Line = token.Line, Column = token.Column };
    }

    private BreakNode ParseBreak()
    {
        var token = Advance();
        Expect(TokenType.Semicolon, "Esperado ';' após 'break'.");
        return new BreakNode { Line = token.Line, Column = token.Column };
    }

    private ContinueNode ParseContinue()
    {
        var token = Advance();
        Expect(TokenType.Semicolon, "Esperado ';' após 'continue'.");
        return new ContinueNode { Line = token.Line, Column = token.Column };
    }

    private BlockNode ParseBlock()
    {
        var braceToken = Current;
        Expect(TokenType.LeftBrace, "Esperado '{'.");

        var block = new BlockNode { Line = braceToken.Line, Column = braceToken.Column };

        while (!Check(TokenType.RightBrace) && !IsAtEnd)
        {
            try
            {
                var stmt = ParseStatement();
                if (stmt != null)
                    block.Statements.Add(stmt);
            }
            catch (Exception)
            {
                Synchronize();
            }
        }

        Expect(TokenType.RightBrace, "Esperado '}'.");
        return block;
    }

    // ── Expression parsing (precedence climbing) ──

    private AstNode ParseExpression() => ParseOr();

    private AstNode ParseOr()
    {
        var left = ParseAnd();
        while (Match(TokenType.Or))
        {
            var op = Previous;
            var right = ParseAnd();
            left = new BinaryExprNode
            {
                Left = left, Operator = op.Value, Right = right,
                Line = op.Line, Column = op.Column
            };
        }
        return left;
    }

    private AstNode ParseAnd()
    {
        var left = ParseEquality();
        while (Match(TokenType.And))
        {
            var op = Previous;
            var right = ParseEquality();
            left = new BinaryExprNode
            {
                Left = left, Operator = op.Value, Right = right,
                Line = op.Line, Column = op.Column
            };
        }
        return left;
    }

    private AstNode ParseEquality()
    {
        var left = ParseComparison();
        while (Match(TokenType.Equal, TokenType.NotEqual))
        {
            var op = Previous;
            var right = ParseComparison();
            left = new BinaryExprNode
            {
                Left = left, Operator = op.Value, Right = right,
                Line = op.Line, Column = op.Column
            };
        }
        return left;
    }

    private AstNode ParseComparison()
    {
        var left = ParseAddition();
        while (Match(TokenType.Less, TokenType.Greater, TokenType.LessEqual, TokenType.GreaterEqual))
        {
            var op = Previous;
            var right = ParseAddition();
            left = new BinaryExprNode
            {
                Left = left, Operator = op.Value, Right = right,
                Line = op.Line, Column = op.Column
            };
        }
        return left;
    }

    private AstNode ParseAddition()
    {
        var left = ParseMultiplication();
        while (Match(TokenType.Plus, TokenType.Minus))
        {
            var op = Previous;
            var right = ParseMultiplication();
            left = new BinaryExprNode
            {
                Left = left, Operator = op.Value, Right = right,
                Line = op.Line, Column = op.Column
            };
        }
        return left;
    }

    private AstNode ParseMultiplication()
    {
        var left = ParseUnary();
        while (Match(TokenType.Star, TokenType.Slash, TokenType.Percent))
        {
            var op = Previous;
            var right = ParseUnary();
            left = new BinaryExprNode
            {
                Left = left, Operator = op.Value, Right = right,
                Line = op.Line, Column = op.Column
            };
        }
        return left;
    }

    private AstNode ParseUnary()
    {
        if (Match(TokenType.Not, TokenType.Minus))
        {
            var op = Previous;
            var operand = ParseUnary();
            return new UnaryExprNode
            {
                Operator = op.Value, Operand = operand, IsPostfix = false,
                Line = op.Line, Column = op.Column
            };
        }
        if (Match(TokenType.PlusPlus, TokenType.MinusMinus))
        {
            var op = Previous;
            var operand = ParsePostfix();
            return new UnaryExprNode
            {
                Operator = op.Value, Operand = operand, IsPostfix = false,
                Line = op.Line, Column = op.Column
            };
        }
        return ParsePostfix();
    }

    private AstNode ParsePostfix()
    {
        var expr = ParsePrimary();

        while (true)
        {
            if (Match(TokenType.PlusPlus))
            {
                expr = new UnaryExprNode
                {
                    Operator = "++", Operand = expr, IsPostfix = true,
                    Line = Previous.Line, Column = Previous.Column
                };
            }
            else if (Match(TokenType.MinusMinus))
            {
                expr = new UnaryExprNode
                {
                    Operator = "--", Operand = expr, IsPostfix = true,
                    Line = Previous.Line, Column = Previous.Column
                };
            }
            else if (Match(TokenType.Dot))
            {
                var member = Expect(TokenType.Identifier, "Esperado nome do membro após '.'.");
                expr = new MemberAccessNode
                {
                    Object = expr, Member = member.Value,
                    Line = member.Line, Column = member.Column
                };
            }
            else if (Match(TokenType.LeftParen))
            {
                var args = new List<AstNode>();
                if (!Check(TokenType.RightParen))
                {
                    do { args.Add(ParseExpression()); } while (Match(TokenType.Comma));
                }
                Expect(TokenType.RightParen, "Esperado ')'.");
                var call = new MethodCallNode
                {
                    Callee = expr, Line = expr.Line, Column = expr.Column
                };
                foreach (var a in args) call.Arguments.Add(a);
                expr = call;
            }
            else if (Match(TokenType.LeftBracket))
            {
                var index = ParseExpression();
                Expect(TokenType.RightBracket, "Esperado ']'.");
                expr = new ArrayAccessNode
                {
                    Array = expr, Index = index,
                    Line = expr.Line, Column = expr.Column
                };
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private AstNode ParsePrimary()
    {
        if (Match(TokenType.IntegerLiteral))
        {
            int.TryParse(Previous.Value, out var val);
            return new IntLiteralNode { Value = val, Line = Previous.Line, Column = Previous.Column };
        }

        if (Match(TokenType.DoubleLiteral))
        {
            double.TryParse(Previous.Value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var val);
            return new DoubleLiteralNode { Value = val, Line = Previous.Line, Column = Previous.Column };
        }

        if (Match(TokenType.StringLiteral))
            return new StringLiteralNode { Value = Previous.Value, Line = Previous.Line, Column = Previous.Column };

        if (Match(TokenType.CharLiteral))
        {
            var charVal = Previous.Value.Length >= 3 ? Previous.Value[1] : '\0';
            return new CharLiteralNode { Value = charVal, Line = Previous.Line, Column = Previous.Column };
        }

        if (Match(TokenType.BooleanLiteral))
            return new BoolLiteralNode { Value = Previous.Value == "true", Line = Previous.Line, Column = Previous.Column };

        if (Match(TokenType.KwNew))
        {
            var className = Expect(TokenType.Identifier, "Esperado nome da classe após 'new'.");
            Expect(TokenType.LeftParen, "Esperado '(' após nome da classe.");
            var args = new List<AstNode>();
            if (!Check(TokenType.RightParen))
            {
                do { args.Add(ParseExpression()); } while (Match(TokenType.Comma));
            }
            Expect(TokenType.RightParen, "Esperado ')'.");
            var newNode = new NewObjectNode
            {
                ClassName = className.Value,
                Line = className.Line, Column = className.Column
            };
            foreach (var a in args) newNode.Arguments.Add(a);
            return newNode;
        }

        if (Check(TokenType.KwMain))
        {
            var mainToken = Advance();
            return new IdentifierNode { Name = mainToken.Value, Line = mainToken.Line, Column = mainToken.Column };
        }

        if (Match(TokenType.Identifier))
            return new IdentifierNode { Name = Previous.Value, Line = Previous.Line, Column = Previous.Column };

        if (Match(TokenType.LeftParen))
        {
            var expr = ParseExpression();
            Expect(TokenType.RightParen, "Esperado ')' após expressão.");
            return expr;
        }

        _result.Errors.Add($"Linha {Current.Line}, Coluna {Current.Column}: Expressão esperada, encontrado '{Current.Value}'.");
        throw new Exception("Parse error");
    }
}
