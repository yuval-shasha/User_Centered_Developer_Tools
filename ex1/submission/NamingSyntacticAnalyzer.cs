using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
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
    
    public const string DiagnosticId = "CS236651";
    public const string GrammarDiagnosticId = "CS236651_Grammar";
    private readonly HashSet<string> words;

    private static class ConventionErrorRule
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.ConventionErrorTitle),
            Resources.ResourceManager, typeof(Resources));
        
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

    private static class GrammarErrorRule
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(Resources.GrammarErrorTitle),
            Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(nameof(Resources.GrammarErrorMessageFormat), Resources.ResourceManager,
                typeof(Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(Resources.GrammarErrorDescription), Resources.ResourceManager,
                typeof(Resources));

        private const string Category = "Naming";

        public static DiagnosticDescriptor Rule { get; } = new(GrammarDiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
    }


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(ConventionErrorRule.Rule, GrammarErrorRule.Rule);

    public NamingSyntacticAnalyzer()
    {
        words = new HashSet<string>();
        LoadWords();
    }


    void LoadWords()
    {
        Stream? resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ex1.words.txt");
        if (resourceStream == null)
        {
            throw new InvalidOperationException($"Can't find words file");
        }
        
        StreamReader reader = new StreamReader(resourceStream);
        
        while (!reader.EndOfStream)
        {
            var word = reader.ReadLine()?.Trim().ToLower();
            if (!string.IsNullOrEmpty(word))
            {
                words.Add(word);
            }
        }
        
        reader.Close();
        resourceStream.Close();
    }
    
    public override void Initialize(AnalysisContext context)
    {
        // You must call this method to avoid analyzing generated code.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // You must call this method to enable the Concurrent Execution.
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(ValidateClassDeclaration, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(ValidateMethodDeclaration, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(ValidateLocalVariable, SyntaxKind.LocalDeclarationStatement);
        context.RegisterSyntaxNodeAction(ValidatePublicConstant, SyntaxKind.FieldDeclaration);
    }
    
    private void ValidateMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclarationNode)
            return;
        
        var methodIdentifier = methodDeclarationNode.Identifier;
        Status status = CheckUpperCamelCaseNaming(methodIdentifier.Text);
        handleStatus(status, context, methodIdentifier);

    }

    private void ValidateLocalVariable(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LocalDeclarationStatementSyntax localDeclarationNode)
            return;
        

        foreach (var variableDeclaration in localDeclarationNode.Declaration.Variables)
        {
            var variableIdentifier = variableDeclaration.Identifier;
            Status status = CheckLowerCamelCaseNaming(variableIdentifier.Text);
            handleStatus(status, context, variableIdentifier);
        }
    }
    
    private void ValidatePublicConstant(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not FieldDeclarationSyntax fieldDeclarationNode)
            return;

        var fieldModifiers = fieldDeclarationNode.Modifiers; 
        bool isConst = fieldModifiers.IndexOf(SyntaxKind.ConstKeyword) != -1;
        bool isPublic = fieldModifiers.IndexOf(SyntaxKind.PublicKeyword) != -1;
        bool isReadonly = fieldModifiers.IndexOf(SyntaxKind.ReadOnlyKeyword) != -1;
        bool isStatic = fieldModifiers.IndexOf(SyntaxKind.StaticKeyword) != -1;

        if (!isPublic || !(isConst || (isStatic && isReadonly)))
            return;
        
        foreach (var variableDeclaration in fieldDeclarationNode.Declaration.Variables)
        {
            var variableIdentifier = variableDeclaration.Identifier;
            Status status = CheckSnakeCaseNaming(variableIdentifier.Text);
            handleStatus(status, context, variableIdentifier);
        }
    }
    
    private void ValidateClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclarationNode)
            return;
        
        var classDeclarationIdentifier = classDeclarationNode.Identifier;

        // Find class symbols whose name contains the company name.
        Status status = CheckUpperCamelCaseNaming(classDeclarationIdentifier.Text);
        handleStatus(status, context, classDeclarationIdentifier);
    }

    private void handleStatus(Status status, SyntaxNodeAnalysisContext context, SyntaxToken methodIdentifier)
    {
        switch (status)
        {
            case Status.InvalidSyntax:
                ReportConventionError(context, methodIdentifier);
                break;
            case Status.InvalidGrammar:
                ReportGrammarError(context, methodIdentifier);
                break;
        }
    }
    
    private void ReportConventionError(SyntaxNodeAnalysisContext context, SyntaxToken methodIdentifier)
    {
        var diagnostic = Diagnostic.Create(ConventionErrorRule.Rule,
            methodIdentifier.GetLocation(),
            methodIdentifier.Text);

        context.ReportDiagnostic(diagnostic);
    }
    
    private void ReportGrammarError(SyntaxNodeAnalysisContext context, SyntaxToken methodIdentifier)
    {
        var diagnostic = Diagnostic.Create(GrammarErrorRule.Rule,
            methodIdentifier.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }


    Status CheckUpperCamelCaseNaming(string identifier)
    {
        var pattern = @"^(([A-Z][a-z]*)[0-9]*)+$";
        var matches = Regex.Matches(identifier, pattern);
        if (matches.Count != 1)
            return Status.InvalidSyntax;

        
        foreach (var word in matches[0].Groups[2].Captures)
        {
            string text = word.ToString().ToLower();
            if (!words.Contains(text))
            {
                return Status.InvalidGrammar;
            }
        }

        return Status.Valid;
    }
    
    
    Status CheckLowerCamelCaseNaming(string identifier)
    {
        var pattern = @"^(([a-z]+)[0-9]*)(([A-Z][a-z]*)[0-9]*)*$";
        var matches = Regex.Matches(identifier, pattern);
        if (matches.Count != 1)
            return Status.InvalidSyntax;

        
        foreach (var groupIndex in (new[]{2,4}))
        {
            foreach (var word in matches[0].Groups[groupIndex].Captures)
            {
                string text = word.ToString().ToLower();
                if (!words.Contains(text))
                {
                    return Status.InvalidGrammar;
                }
            }
        }
        return Status.Valid;
    }

    Status CheckSnakeCaseNaming(string identifier)
    {
        var pattern = @"^([A-Z]+)(_([A-Z]+))*$";
        var matches = Regex.Matches(identifier, pattern);
        if (matches.Count != 1)
            return Status.InvalidSyntax;
        
        foreach (var groupIndex in (new[]{1,3}))
        {
            foreach (var word in matches[0].Groups[groupIndex].Captures)
            {
                string text = word.ToString().ToLower();
                if (!words.Contains(text))
                {
                    return Status.InvalidGrammar;
                }
            }
        }
        return Status.Valid;
    }
}