using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        ex1.NamingSyntacticAnalyzer>;

namespace ex1.Tests;

public class RegTest
{
    [Fact]
    public void METHOD()
    {
        string identifier = "geg34RegExp43TestName";
        identifier = "GEG_REG_EXP_NAME";
        
        var pattern = @"^(([A-Z][a-z]*)[0-9]*)+$";
        pattern = @"^(([a-z]+)[0-9]*)(([A-Z][a-z]*)[0-9]*)*$";
        pattern = @"^([A-Z]+)(_([A-Z]+))*$";
        var matches = Regex.Matches(identifier, pattern);
        if (matches.Count != 1)
            Assert.Fail("wrong");
            

        foreach (var word in matches[0].Groups[1].Captures)
        {
            string text = word.ToString();
            int a = 7;
        }    
    }
}


public class NamingSyntacticAnalyzerTests
{
    [Fact]
    public async Task ClassWithMyCompanyTitle_AlertDiagnostic()
    {
        const string text = @"
public class MyCompanyClass
{
}
";

        var expected = Verifier.Diagnostic()
            .WithLocation(2, 14)
            .WithArguments("MyCompanyClass");
        await Verifier.VerifyAnalyzerAsync(text, expected).ConfigureAwait(false);
    }
}