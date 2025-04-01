using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    public class Branch
    {
        public int Depth { get; set; }
        public List<Vector3> Points { get; set; }
        public List<Fruit> Fruits { get; set; }
    }
}