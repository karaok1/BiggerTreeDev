using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public static class Helper
    {
        public static string ChooseOne(Dictionary<string, float> dictionary)
        {
            var r = UnityEngine.Random.Range(0f, 1f);
            var total = 0f;

            for (var i = 0; i < dictionary.Count; i++)
            {
                total += dictionary.ElementAt(i).Value;

                if (total > r)
                {
                    return dictionary.ElementAt(i).Key;
                }
            }

            throw new Exception("No valid choice found");
        }
    }
}