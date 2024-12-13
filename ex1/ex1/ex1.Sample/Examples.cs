// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace ex1.Sample;

// If you don't see warnings, build the Analyzers Project.


public class Examples
{
    public const int ame = 5, BB_C_FF = 7;

    public static readonly int THIS_IS_FALSE = 5;
    public static readonly int THIS_IS_BAD = 5, bb1 = 7;
    public static readonly int I_AM_UA;
    public static readonly int ASDHKJASD;
    private int _ggg;
    
    public class MyCslass // Try to apply quick fix using the IDE.
    {
        void Sdvsdvsd()
        {
            //MyClass m = new MyClass();
            int lodddd = 6;
        }
    }
    
    

    public void Foo()
    {
        _ggg = 7;
        int aaaT = _ggg;

        aaaT = 5;
    }

    public void ToStars()
    {
        int a, b, c = 8;
        var spaceship = new Spaceship();
        spaceship.SetSpeed(300000000); // Invalid value, it should be highlighted.
        spaceship.SetSpeed(42);
    }
}