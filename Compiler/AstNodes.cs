namespace CSharpEditor.Compiler;

public abstract class AstNode
{
    public int Line { get; set; }
    public int Column { get; set; }
}

// ── Program ──

public class ProgramNode : AstNode
{
    public List<AstNode> Statements { get; } = new();
}

// ── Top-level ──

public class ImportNode : AstNode
{
    public string ModuleName { get; set; } = "";
}

public class ClassNode : AstNode
{
    public string AccessModifier { get; set; } = "";
    public string Name { get; set; } = "";
    public string? BaseClass { get; set; }
    public List<string> Interfaces { get; } = new();
    public BlockNode Body { get; set; } = null!;
}

public class MethodNode : AstNode
{
    public string AccessModifier { get; set; } = "";
    public string ReturnType { get; set; } = "";
    public string Name { get; set; } = "";
    public List<ParameterNode> Parameters { get; } = new();
    public BlockNode Body { get; set; } = null!;
}

public class ParameterNode : AstNode
{
    public string TypeName { get; set; } = "";
    public string Name { get; set; } = "";
}

public class InterfaceNode : AstNode
{
    public string Name { get; set; } = "";
    public BlockNode Body { get; set; } = null!;
}

// ── Statements ──

public class VarDeclarationNode : AstNode
{
    public string TypeName { get; set; } = "";
    public string Name { get; set; } = "";
    public AstNode? Initializer { get; set; }
}

public class AssignmentNode : AstNode
{
    public string Name { get; set; } = "";
    public string Operator { get; set; } = "=";
    public AstNode Value { get; set; } = null!;
}

public class IfNode : AstNode
{
    public AstNode Condition { get; set; } = null!;
    public BlockNode ThenBranch { get; set; } = null!;
    public BlockNode? ElseBranch { get; set; }
}

public class WhileNode : AstNode
{
    public AstNode Condition { get; set; } = null!;
    public BlockNode Body { get; set; } = null!;
}

public class ForNode : AstNode
{
    public AstNode? Init { get; set; }
    public AstNode? Condition { get; set; }
    public AstNode? Increment { get; set; }
    public BlockNode Body { get; set; } = null!;
}

public class SwitchNode : AstNode
{
    public AstNode Expression { get; set; } = null!;
    public List<CaseNode> Cases { get; } = new();
}

public class CaseNode : AstNode
{
    public AstNode? Value { get; set; } // null = default
    public List<AstNode> Statements { get; } = new();
}

public class PrintNode : AstNode
{
    public AstNode Expression { get; set; } = null!;
}

public class ReturnNode : AstNode
{
    public AstNode? Expression { get; set; }
}

public class BreakNode : AstNode { }

public class ContinueNode : AstNode { }

public class BlockNode : AstNode
{
    public List<AstNode> Statements { get; } = new();
}

// ── Expressions ──

public class BinaryExprNode : AstNode
{
    public AstNode Left { get; set; } = null!;
    public string Operator { get; set; } = "";
    public AstNode Right { get; set; } = null!;
}

public class UnaryExprNode : AstNode
{
    public string Operator { get; set; } = "";
    public AstNode Operand { get; set; } = null!;
    public bool IsPostfix { get; set; }
}

public class IdentifierNode : AstNode
{
    public string Name { get; set; } = "";
}

public class MemberAccessNode : AstNode
{
    public AstNode Object { get; set; } = null!;
    public string Member { get; set; } = "";
}

public class MethodCallNode : AstNode
{
    public AstNode Callee { get; set; } = null!;
    public List<AstNode> Arguments { get; } = new();
}

public class NewObjectNode : AstNode
{
    public string ClassName { get; set; } = "";
    public List<AstNode> Arguments { get; } = new();
}

public class ArrayAccessNode : AstNode
{
    public AstNode Array { get; set; } = null!;
    public AstNode Index { get; set; } = null!;
}

public class IntLiteralNode : AstNode
{
    public int Value { get; set; }
}

public class DoubleLiteralNode : AstNode
{
    public double Value { get; set; }
}

public class StringLiteralNode : AstNode
{
    public string Value { get; set; } = "";
}

public class BoolLiteralNode : AstNode
{
    public bool Value { get; set; }
}

public class CharLiteralNode : AstNode
{
    public char Value { get; set; }
}
