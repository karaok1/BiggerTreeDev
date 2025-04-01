using System;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace Utils
{
    public static class Helper
    {
        public static string ChooseOne(List<Rule> rules)
        {
            float total = rules.Sum(r => r.Probability);
            float randomPoint = UnityEngine.Random.Range(0f, total);
            float cumulative = 0f;

            foreach (var rule in rules)
            {
                cumulative += rule.Probability;
                if (randomPoint <= cumulative)
                {
                    return rule.Value;
                }
            }

            return rules.Last().Value;
        }
    }
}