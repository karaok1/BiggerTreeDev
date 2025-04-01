using System.Collections.Generic;
using Models;

public static class Rules
{
    public static List<Rule> X = new List<Rule>
    {
        new Rule { Value = "(F[+X][-X]FX)", Probability = 0.5f },
        new Rule { Value = "(F[-X]FX)", Probability = 0.05f },
        new Rule { Value = "(F[+X]FX)", Probability = 0.05f },
        new Rule { Value = "(F[++X][-X]FX)", Probability = 0.1f },
        new Rule { Value = "(F[+X][--X]FX)", Probability = 0.1f },
        new Rule { Value = "(F[+X][-X]FXA)", Probability = 0.1f },
        new Rule { Value = "(F[+X][-X]FXB)", Probability = 0.1f },
    };

    public static List<Rule> F = new List<Rule>
    {
        new Rule { Value = "F(F)", Probability = 0.85f },
        new Rule { Value = "F(FF)", Probability = 0.05f },
        new Rule { Value = "F", Probability = 0.1f },
    };
}