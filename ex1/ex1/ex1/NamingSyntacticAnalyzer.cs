using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ex1;


[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NamingSyntacticAnalyzer : DiagnosticAnalyzer
{
    enum Status
    {
        Valid, 
        InvalidSyntax,
        InvalidGrammar
    }
    
    public const string CompanyName = "MyCompany";
    public const string DiagnosticId = "CS236651";

    private static class ConventionErrorRule
    {
        // Feel free to use raw strings if you don't need localization.
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ConventionErrorTitle),
            Resources.ResourceManager, typeof(Resources));

        // The message that will be displayed to the user.
        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(nameof(Resources.ConventionErrorMessageFormat), Resources.ResourceManager,
                typeof(Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(Resources.ConventionErrorDescription), Resources.ResourceManager,
                typeof(Resources));
        
        private const string Category = "Naming";
        
        public static DiagnosticDescriptor Rule { get; } = new(DiagnosticId,Title, MessageFormat, Category,
            DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
    }
    

    
    
    private static readonly DiagnosticDescriptor DebugRule = new("DiagnosticId", "Debug", "Debug", "Debug",
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "Debug.");

    // Keep in mind: you have to list your rules here.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DebugRule, ConventionErrorRule.Rule);
    
    
    public override void Initialize(AnalysisContext context)
    {
        
        // You must call this method to avoid analyzing generated code.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // You must call this method to enable the Concurrent Execution.
        context.EnableConcurrentExecution();

        // Subscribe to the Syntax Node with the appropriate 'SyntaxKind' (ClassDeclaration) action.
        // To figure out which Syntax Nodes you should choose, consider installing the Roslyn syntax tree viewer plugin Rossynt: https://plugins.jetbrains.com/plugin/16902-rossynt/
        context.RegisterSyntaxNodeAction(ValidateClassDeclaration, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(ValidateMethodDeclaration, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(ValidateLocalVariable, SyntaxKind.LocalDeclarationStatement);
        context.RegisterSyntaxNodeAction(ValidateGlobalVariable, SyntaxKind.GlobalStatement);

        // Check other 'context.Register...' methods that might be helpful for your purposes.
    }

    private void ValidateMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclarationNode)
            return;
        
        var methodIdentifier = methodDeclarationNode.Identifier;

        // Find class symbols whose name contains the company name.
        if (CheckUpperCamelCaseNaming(methodIdentifier.Text) == Status.InvalidSyntax)
        {
            var diagnostic = Diagnostic.Create(ConventionErrorRule.Rule,
                methodIdentifier.GetLocation(),
                methodIdentifier.Text);
            
            context.ReportDiagnostic(diagnostic);
        }
    }

    private void ValidateLocalVariable(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LocalDeclarationStatementSyntax localDeclarationNode)
            return;
        

        foreach (var variableDeclaration in localDeclarationNode.Declaration.Variables)
        {
            var variableIdentifier = variableDeclaration.Identifier; 
            if (CheckLowerCamelCaseNaming(variableIdentifier.Text) == Status.InvalidSyntax)
            {
                var diagnostic = Diagnostic.Create(ConventionErrorRule.Rule,
                    variableIdentifier.GetLocation(),
                    variableIdentifier.Text);

                context.ReportDiagnostic(diagnostic);    
            }
            
        }
    }
    
    
    
    private void ValidateGlobalVariable(SyntaxNodeAnalysisContext context)
    {
        
    }
    
   
    private void ValidateClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclarationNode)
            return;
        
        var classDeclarationIdentifier = classDeclarationNode.Identifier;

        // Find class symbols whose name contains the company name.
        if (CheckUpperCamelCaseNaming(classDeclarationIdentifier.Text) == Status.InvalidSyntax)
        {
            var diagnostic = Diagnostic.Create(ConventionErrorRule.Rule,
                classDeclarationIdentifier.GetLocation(),
                classDeclarationIdentifier.Text);
            
            context.ReportDiagnostic(diagnostic);
        }
    }


    Status CheckUpperCamelCaseNaming(string identifier)
    {
        var pattern = @"^([A-Z][a-z]*[0-9]*)+$";
        var matches = Regex.Matches(identifier, pattern);
        if (matches.Count != 1)
            return Status.InvalidSyntax;

        /*
        foreach (var word in matches[0].Groups[2].Captures)
        {
            string text = word.ToString();
            int a = 7;
        }
        */

        return Status.Valid;
    }
    
    
    Status CheckLowerCamelCaseNaming(string identifier)
    {
        var pattern = @"^([a-z]+[0-9]*)([A-Z][a-z]*[0-9]*)*$";
        var matches = Regex.Matches(identifier, pattern);
        if (matches.Count != 1)
            return Status.InvalidSyntax;

        /*
        matches[0].Groups[1]
        foreach (var word in matches[0].Groups[2].Captures)
        {
            string text = word.ToString();
            int a = 7;
        }  
        */

        return Status.Valid;
    }
}