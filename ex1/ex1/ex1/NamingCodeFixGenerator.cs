using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace ex1;


[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NamingCodeFixGenerator)), Shared]
public class NamingCodeFixGenerator : CodeFixProvider
{
    private const string CommonName = "Common";

    // Specify the diagnostic IDs of analyzers that are expected to be linked.
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(NamingSyntacticAnalyzer.DiagnosticId);

    // If you don't need the 'fix all' behaviour, return null.
    public override FixAllProvider? GetFixAllProvider() => null;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // We link only one diagnostic and assume there is only one diagnostic in the context.
        var diagnostic = context.Diagnostics.Single();

        // 'SourceSpan' of 'Location' is the highlighted area. We're going to use this area to find the 'SyntaxNode' to rename.
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Get the root of Syntax Tree that contains the highlighted diagnostic.
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        // Find SyntaxNode corresponding to the diagnostic.
        var diagnosticNode = root?.FindNode(diagnosticSpan);


        string oldName = "";
        string newName = "";
        
        
        // To get the required metadata, we should match the Node to the specific type: 'ClassDeclarationSyntax'.
        if (diagnosticNode is ClassDeclarationSyntax classDeclaration)
        {
            oldName = classDeclaration.Identifier.Text;
            bool startWithUpperCase = true;
            newName = GetCamelCaseNaming(oldName, startWithUpperCase);    
        }
        else if (diagnosticNode is MethodDeclarationSyntax methodDeclaration)
        {
            oldName = methodDeclaration.Identifier.Text;
            bool startWithUpperCase = true;
            newName = GetCamelCaseNaming(oldName, startWithUpperCase);  
        }
        else if (diagnosticNode?.Parent?.Parent is FieldDeclarationSyntax)
        {
            var fieldDeclaration = (VariableDeclaratorSyntax)diagnosticNode;  
            oldName = fieldDeclaration.Identifier.Text;
            newName = GetSnakeCaseNaming(oldName);
        }
        else if (diagnosticNode?.Parent?.Parent is LocalDeclarationStatementSyntax)
        {
            var fieldDeclaration = (VariableDeclaratorSyntax)diagnosticNode;  
            oldName = fieldDeclaration.Identifier.Text;
            bool dontStartWithUpperCase = false;
            newName = GetCamelCaseNaming(oldName, dontStartWithUpperCase);
        }

        

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: string.Format(Resources.CodeFixActionDescription, oldName, newName),
                createChangedSolution: c => RenameNodeIdentifier(context.Document, diagnosticNode, c, newName),
                equivalenceKey: nameof(Resources.CodeFixActionDescription)),
            diagnostic);
    }

    string GetCamelCaseNaming(string identifier, bool startWithUpperCase)
    {
        StringBuilder newNameBuilder = new StringBuilder();
        bool isNextCapital = startWithUpperCase;

        for (int i = 0; i < identifier.Length; i++)
        {
            char nextChar = identifier[i];

            if (!Char.IsLetterOrDigit(nextChar))
            {
                isNextCapital = true;
                continue;
            }


            if (Char.IsLetter(nextChar) && isNextCapital)
            {
                nextChar = Char.ToUpper(nextChar);
                isNextCapital = false;
            }
            else if (Char.IsDigit(nextChar))
            {
                isNextCapital = true;
            }


            newNameBuilder.Append(nextChar);
        }

        return newNameBuilder.ToString();
    }
    
    string GetSnakeCaseNaming(string identifier)
    {
        StringBuilder newNameBuilder = new StringBuilder();
        bool isStart = true;
        bool isPrevUnderScore = false;
        for (int i = 0; i < identifier.Length; i++)
        {
            char nextChar = identifier[i];

            if (!Char.IsLetter(nextChar) && nextChar != '_')
            {
                continue;
            }
            if (nextChar == '_')
            {
                isPrevUnderScore = true;
                continue;
            }
            
            if (isPrevUnderScore)
            {
                newNameBuilder.Append('_');
            }
            else if(!isStart && Char.IsUpper(nextChar))
            {
                newNameBuilder.Append('_');
            }

            newNameBuilder.Append(Char.ToUpper(nextChar));
            
            isPrevUnderScore = false;
            isStart = false;
        }

        return newNameBuilder.ToString();
    }
    
    
    private async Task<Solution> RenameNodeIdentifier(Document document,
        SyntaxNode node, CancellationToken cancellationToken, string newName)
    {
        // To make a refactoring, we need to get compiled code metadata: the Semantic Model.
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        // Attempt to find the 'TypeSymbol' (compile time metadata of the class) based on highlighted Class Declaration Syntax.
        var typeSymbol = semanticModel?.GetDeclaredSymbol(node, cancellationToken);
        if (typeSymbol == null) return document.Project.Solution;

        // Produce a new solution that has all references to the class being renamed, including the declaration.
        var newSolution = await Renamer
            .RenameSymbolAsync(document.Project.Solution, typeSymbol, new SymbolRenameOptions(), newName,
                cancellationToken)
            .ConfigureAwait(false);

        // Return the new solution with the updated type name.
        return newSolution;
    }
}