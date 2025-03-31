using System.Collections.Generic;

public static class Rules
{
    public static Dictionary<string, float> X = new()
    {
        { "F[+X][-X]FX", 0.5f },
        { "F[-X]FX", 0.15f },
        { "F[+X]FX", 0.15f },
        { "F[++X][-X]FX", 0.1f },
        { "F[+X][--X]FX", 0.1f },
    };

    public static Dictionary<string, float> F = new()
    {
        { "FF", 0.85f },
        { "FFF", 0.05f },
        { "F", 0.1f },
    };
}