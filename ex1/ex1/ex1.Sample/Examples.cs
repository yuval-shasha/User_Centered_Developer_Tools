// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace ex1.Sample;

// If you don't see warnings, build the Analyzers Project.

public class Examples
{
    private int _ggg;
    
    public class My5CompanyClass // Try to apply quick fix using the IDE.
    {
        
    }

    public void Foo()
    {
        _ggg = 7;
        int a = _ggg;
    }

    public void ToStars()
    {
        int a, b, c = 8;
        var spaceship = new Spaceship();
        spaceship.SetSpeed(300000000); // Invalid value, it should be highlighted.
        spaceship.SetSpeed(42);
    }
}