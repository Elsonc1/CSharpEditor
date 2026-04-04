namespace CSharpEditor.Compiler;

public class SemanticResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public bool HasErrors => Errors.Count > 0;
}

public class Symbol
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public int DeclaredLine { get; set; }
}

public class SemanticAnalyzer
{
    private readonly SemanticResult _result = new();
    private readonly List<Dictionary<string, Symbol>> _scopes = new();
    private int _loopDepth;

    public SemanticResult Analyze(ProgramNode program)
    {
        PushScope();
        foreach (var stmt in program.Statements)
            AnalyzeNode(stmt);
        PopScope();
        return _result;
    }

    private void PushScope() => _scopes.Add(new Dictionary<string, Symbol>());
    private void PopScope() => _scopes.RemoveAt(_scopes.Count - 1);

    private Symbol? LookupSymbol(string name)
    {
        for (int i = _scopes.Count - 1; i >= 0; i--)
        {
            if (_scopes[i].TryGetValue(name, out var symbol))
                return symbol;
        }
        return null;
    }

    private bool DeclareSymbol(string name, string type, int line)
    {
        var currentScope = _scopes[^1];
        if (currentScope.ContainsKey(name))
            return false;
        currentScope[name] = new Symbol { Name = name, Type = type, DeclaredLine = line };
        return true;
    }

    private void AnalyzeNode(AstNode node)
    {
        switch (node)
        {
            case VarDeclarationNode decl: AnalyzeVarDeclaration(decl); break;
            case AssignmentNode assign: AnalyzeAssignment(assign); break;
            case IfNode ifNode: AnalyzeIf(ifNode); break;
            case WhileNode whileNode: AnalyzeWhile(whileNode); break;
            case ForNode forNode: AnalyzeFor(forNode); break;
            case SwitchNode switchNode: AnalyzeSwitch(switchNode); break;
            case ReturnNode ret: AnalyzeReturn(ret); break;
            case BreakNode brk: AnalyzeBreak(brk); break;
            case ContinueNode cont: AnalyzeContinue(cont); break;
            case PrintNode printNode: AnalyzeExpression(printNode.Expression); break;
            case BlockNode block: AnalyzeBlock(block); break;
            case ClassNode cls: AnalyzeClass(cls); break;
            case MethodNode method: AnalyzeMethod(method); break;
            case ImportNode: break;
            case InterfaceNode: break;
            default:
                if (node is AstNode expr)
                    AnalyzeExpression(expr);
                break;
        }
    }

    private void AnalyzeVarDeclaration(VarDeclarationNode decl)
    {
        if (!DeclareSymbol(decl.Name, decl.TypeName, decl.Line))
        {
            _result.Errors.Add(
                $"Linha {decl.Line}: Variável '{decl.Name}' já foi declarada neste escopo.");
            return;
        }

        if (decl.Initializer != null)
        {
            var initType = AnalyzeExpression(decl.Initializer);
            if (initType != null && decl.TypeName != "var" && !IsTypeCompatible(decl.TypeName, initType))
            {
                _result.Errors.Add(
                    $"Linha {decl.Line}: Tipo incompatível na declaração de '{decl.Name}'. " +
                    $"Esperado '{decl.TypeName}', encontrado '{initType}'.");
            }
        }
    }

    private void AnalyzeAssignment(AssignmentNode assign)
    {
        var symbol = LookupSymbol(assign.Name);
        if (symbol == null)
        {
            _result.Errors.Add(
                $"Linha {assign.Line}: Variável '{assign.Name}' não foi declarada.");
            return;
        }

        var valueType = AnalyzeExpression(assign.Value);

        if (assign.Operator is "+=" or "-=" or "*=" or "/=")
        {
            if (symbol.Type != "int" && symbol.Type != "double")
            {
                _result.Errors.Add(
                    $"Linha {assign.Line}: Operador '{assign.Operator}' requer tipo numérico para '{assign.Name}'.");
            }
            return;
        }

        if (valueType != null && !IsTypeCompatible(symbol.Type, valueType))
        {
            _result.Errors.Add(
                $"Linha {assign.Line}: Tipo incompatível na atribuição de '{assign.Name}'. " +
                $"Esperado '{symbol.Type}', encontrado '{valueType}'.");
        }
    }

    private void AnalyzeIf(IfNode ifNode)
    {
        var condType = AnalyzeExpression(ifNode.Condition);
        if (condType != null && condType != "boolean")
        {
            _result.Errors.Add(
                $"Linha {ifNode.Line}: Condição do 'if' deve ser do tipo 'boolean', encontrado '{condType}'.");
        }
        AnalyzeBlock(ifNode.ThenBranch);
        if (ifNode.ElseBranch != null)
            AnalyzeBlock(ifNode.ElseBranch);
    }

    private void AnalyzeWhile(WhileNode whileNode)
    {
        var condType = AnalyzeExpression(whileNode.Condition);
        if (condType != null && condType != "boolean")
        {
            _result.Errors.Add(
                $"Linha {whileNode.Line}: Condição do 'while' deve ser do tipo 'boolean', encontrado '{condType}'.");
        }
        _loopDepth++;
        AnalyzeBlock(whileNode.Body);
        _loopDepth--;
    }

    private void AnalyzeFor(ForNode forNode)
    {
        PushScope();
        if (forNode.Init != null) AnalyzeNode(forNode.Init);
        if (forNode.Condition != null)
        {
            var condType = AnalyzeExpression(forNode.Condition);
            if (condType != null && condType != "boolean")
            {
                _result.Errors.Add(
                    $"Linha {forNode.Line}: Condição do 'for' deve ser do tipo 'boolean', encontrado '{condType}'.");
            }
        }
        if (forNode.Increment != null) AnalyzeNode(forNode.Increment);
        _loopDepth++;
        AnalyzeBlock(forNode.Body);
        _loopDepth--;
        PopScope();
    }

    private void AnalyzeSwitch(SwitchNode switchNode)
    {
        AnalyzeExpression(switchNode.Expression);
        _loopDepth++;
        foreach (var c in switchNode.Cases)
        {
            if (c.Value != null) AnalyzeExpression(c.Value);
            foreach (var s in c.Statements) AnalyzeNode(s);
        }
        _loopDepth--;
    }

    private void AnalyzeReturn(ReturnNode ret)
    {
        if (ret.Expression != null)
            AnalyzeExpression(ret.Expression);
    }

    private void AnalyzeBreak(BreakNode brk)
    {
        if (_loopDepth == 0)
            _result.Errors.Add($"Linha {brk.Line}: 'break' fora de um loop ou switch.");
    }

    private void AnalyzeContinue(ContinueNode cont)
    {
        if (_loopDepth == 0)
            _result.Errors.Add($"Linha {cont.Line}: 'continue' fora de um loop.");
    }

    private void AnalyzeClass(ClassNode cls)
    {
        DeclareSymbol(cls.Name, "class", cls.Line);
        PushScope();
        foreach (var stmt in cls.Body.Statements)
            AnalyzeNode(stmt);
        PopScope();
    }

    private void AnalyzeMethod(MethodNode method)
    {
        DeclareSymbol(method.Name, method.ReturnType, method.Line);
        PushScope();
        foreach (var p in method.Parameters)
            DeclareSymbol(p.Name, p.TypeName, p.Line);
        foreach (var stmt in method.Body.Statements)
            AnalyzeNode(stmt);
        PopScope();
    }

    private void AnalyzeBlock(BlockNode block)
    {
        PushScope();
        foreach (var stmt in block.Statements)
            AnalyzeNode(stmt);
        PopScope();
    }

    private string? AnalyzeExpression(AstNode node)
    {
        switch (node)
        {
            case IntLiteralNode: return "int";
            case DoubleLiteralNode: return "double";
            case StringLiteralNode: return "string";
            case CharLiteralNode: return "string";
            case BoolLiteralNode: return "boolean";

            case IdentifierNode id:
            {
                var symbol = LookupSymbol(id.Name);
                if (symbol == null)
                {
                    _result.Errors.Add($"Linha {id.Line}: Variável '{id.Name}' não foi declarada.");
                    return null;
                }
                return symbol.Type;
            }

            case UnaryExprNode unary: return AnalyzeUnary(unary);
            case BinaryExprNode binary: return AnalyzeBinary(binary);
            case NewObjectNode: return "object";
            case MethodCallNode call:
            {
                if (call.Callee is IdentifierNode callId)
                {
                    var sym = LookupSymbol(callId.Name);
                    foreach (var arg in call.Arguments) AnalyzeExpression(arg);
                    return sym?.Type;
                }
                foreach (var arg in call.Arguments) AnalyzeExpression(arg);
                return null;
            }
            case MemberAccessNode mem:
            {
                AnalyzeExpression(mem.Object);
                return null;
            }
            case ArrayAccessNode arr:
            {
                AnalyzeExpression(arr.Array);
                var idxType = AnalyzeExpression(arr.Index);
                if (idxType != null && idxType != "int")
                    _result.Errors.Add($"Linha {arr.Line}: Índice do array deve ser 'int', encontrado '{idxType}'.");
                return null;
            }
            default: return null;
        }
    }

    private string? AnalyzeUnary(UnaryExprNode unary)
    {
        var operandType = AnalyzeExpression(unary.Operand);
        if (operandType == null) return null;

        switch (unary.Operator)
        {
            case "!":
                if (operandType != "boolean")
                    _result.Errors.Add(
                        $"Linha {unary.Line}: Operador '!' requer tipo 'boolean', encontrado '{operandType}'.");
                return "boolean";
            case "-":
                if (!IsNumericType(operandType))
                    _result.Errors.Add(
                        $"Linha {unary.Line}: Operador '-' requer tipo numérico, encontrado '{operandType}'.");
                return operandType;
            case "++" or "--":
                if (!IsNumericType(operandType))
                    _result.Errors.Add(
                        $"Linha {unary.Line}: Operador '{unary.Operator}' requer tipo numérico, encontrado '{operandType}'.");
                return operandType;
            default:
                return null;
        }
    }

    private string? AnalyzeBinary(BinaryExprNode binary)
    {
        var leftType = AnalyzeExpression(binary.Left);
        var rightType = AnalyzeExpression(binary.Right);
        if (leftType == null || rightType == null) return null;

        switch (binary.Operator)
        {
            case "+" when leftType == "string" || rightType == "string":
                return "string";

            case "+" or "-" or "*" or "/" or "%":
            {
                if (!IsNumericType(leftType) || !IsNumericType(rightType))
                {
                    _result.Errors.Add(
                        $"Linha {binary.Line}: Operador '{binary.Operator}' requer tipos numéricos, " +
                        $"encontrado '{leftType}' e '{rightType}'.");
                    return null;
                }
                return (leftType == "double" || rightType == "double") ? "double" : "int";
            }

            case "==" or "!=":
            {
                if (!IsTypeCompatible(leftType, rightType) && !IsTypeCompatible(rightType, leftType))
                {
                    _result.Errors.Add(
                        $"Linha {binary.Line}: Não é possível comparar '{leftType}' com '{rightType}'.");
                }
                return "boolean";
            }

            case "<" or ">" or "<=" or ">=":
            {
                if (!IsNumericType(leftType) || !IsNumericType(rightType))
                {
                    _result.Errors.Add(
                        $"Linha {binary.Line}: Operador '{binary.Operator}' requer tipos numéricos, " +
                        $"encontrado '{leftType}' e '{rightType}'.");
                }
                return "boolean";
            }

            case "&&" or "||":
            {
                if (leftType != "boolean" || rightType != "boolean")
                {
                    _result.Errors.Add(
                        $"Linha {binary.Line}: Operador '{binary.Operator}' requer tipos 'boolean', " +
                        $"encontrado '{leftType}' e '{rightType}'.");
                }
                return "boolean";
            }

            default: return null;
        }
    }

    private static bool IsNumericType(string type) => type is "int" or "double";

    private static bool IsTypeCompatible(string expected, string actual)
    {
        if (expected == actual) return true;
        if (expected == "double" && actual == "int") return true;
        if (expected == "var") return true;
        return false;
    }
}
