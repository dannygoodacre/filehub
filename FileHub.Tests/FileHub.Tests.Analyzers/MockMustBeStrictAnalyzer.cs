using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FileHub.Tests.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MockMustBeStrictAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _rule = new(
        id: "FHT002",
        title: "Mocks must be created with MockBehavior.Strict",
        messageFormat: "Mock for type '{0}' must be created with MockBehavior.Strict",
        category: "Mocking",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "To enforce strict mocking, always pass MockBehavior.Strict to the Mock<T> constructor.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNewMock, SyntaxKind.ObjectCreationExpression);
    }

    private static void AnalyzeNewMock(SyntaxNodeAnalysisContext context)
    {
        var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

        var semanticModel = context.SemanticModel;

        if (semanticModel.GetSymbolInfo(objectCreation.Type).Symbol is not INamedTypeSymbol typeSymbol
            || !typeSymbol.ConstructedFrom.ToDisplayString().StartsWith("Moq.Mock"))
        {
            return;
        }

        var arguments = objectCreation.ArgumentList?.Arguments.ToList();

        if (arguments is null || arguments.Count == 0)
        {
            // Defaults to MockBehavior.Loose.
            Report(context, objectCreation, typeSymbol.TypeArguments[0]);
            return;
        }

        var mockBehaviorType = context.Compilation.GetTypeByMetadataName("Moq.MockBehavior");

        foreach (var argument in arguments)
        {
            var argumentType = semanticModel.GetTypeInfo(argument.Expression).Type;

            if (argumentType is null || !argumentType.Equals(mockBehaviorType, SymbolEqualityComparer.Default))
            {
                continue;
            }

            var constantValue = semanticModel.GetConstantValue(argument.Expression);

            if (!constantValue.HasValue || constantValue.Value is not 0)
            {
                continue;
            }

            // 0 == MockBehavior.Strict.
            return;
        }

        Report(context, objectCreation, typeSymbol.TypeArguments[0]);
    }

    private static void Report(SyntaxNodeAnalysisContext context, ObjectCreationExpressionSyntax objectCreation, ITypeSymbol mockedType)
    {
        var diagnostic = Diagnostic.Create(_rule, objectCreation.GetLocation(), mockedType.ToDisplayString());

        context.ReportDiagnostic(diagnostic);
    }
}
