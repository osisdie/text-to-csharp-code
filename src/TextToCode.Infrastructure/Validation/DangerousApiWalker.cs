using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TextToCode.Core.Enums;
using TextToCode.Core.ValueObjects;

namespace TextToCode.Infrastructure.Validation;

internal sealed class DangerousApiWalker : CSharpSyntaxWalker
{
    private readonly List<ValidationViolation> _violations = [];

    public IReadOnlyList<ValidationViolation> Violations => _violations;

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        var namespaceName = node.Name?.ToString() ?? string.Empty;
        if (ValidationRuleSet.DangerousNamespaces.Contains(namespaceName))
        {
            AddViolation(node, "DangerousNamespace", $"Using dangerous namespace: {namespaceName}");
        }

        base.VisitUsingDirective(node);
    }

    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        var typeName = GetSimpleTypeName(node.Type);
        if (typeName is not null && ValidationRuleSet.DangerousTypes.Contains(typeName))
        {
            AddViolation(node, "DangerousType", $"Instantiation of dangerous type: {typeName}");
        }

        base.VisitObjectCreationExpression(node);
    }

    public override void VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
    {
        base.VisitImplicitObjectCreationExpression(node);
    }

    public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        var fullAccess = $"{GetSimpleTypeName(node.Expression)}.{node.Name.Identifier.Text}";
        if (ValidationRuleSet.DangerousMemberAccesses.Contains(fullAccess))
        {
            AddViolation(node, "DangerousApi", $"Call to dangerous API: {fullAccess}");
        }

        base.VisitMemberAccessExpression(node);
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (node.Expression is IdentifierNameSyntax { Identifier.Text: "DllImport" or "LibraryImport" })
        {
            AddViolation(node, "NativeInterop", "P/Invoke or native interop is not allowed");
        }

        base.VisitInvocationExpression(node);
    }

    public override void VisitAttributeList(AttributeListSyntax node)
    {
        foreach (var attr in node.Attributes)
        {
            var name = attr.Name.ToString();
            if (name is "DllImport" or "LibraryImport" or "UnmanagedCallersOnly")
            {
                AddViolation(attr, "NativeInterop", $"Native interop attribute [{name}] is not allowed");
            }
        }

        base.VisitAttributeList(node);
    }

    private static string? GetSimpleTypeName(SyntaxNode? node) =>
        node switch
        {
            IdentifierNameSyntax id => id.Identifier.Text,
            QualifiedNameSyntax qn => qn.Right.Identifier.Text,
            MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
            _ => node?.ToString()
        };

    private static string? GetSimpleTypeName(ExpressionSyntax? expression) =>
        GetSimpleTypeName(expression as SyntaxNode);

    private void AddViolation(SyntaxNode node, string rule, string description)
    {
        var lineSpan = node.GetLocation().GetLineSpan();
        _violations.Add(new ValidationViolation(
            rule,
            description,
            ValidationSeverity.Error,
            lineSpan.StartLinePosition.Line + 1,
            lineSpan.StartLinePosition.Character + 1));
    }
}
