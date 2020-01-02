using System;
using System.Collections.Generic;
using System.Linq;

namespace NWSELib.genome
{
    public class JudgeItem
    {
        public const String MAX = "max";
        public const String MIN = "min";
        public String expression;
        public List<int> conditions = new List<int>();
        public List<int> variables = new List<int>();

        public override string ToString()
        {
            return expression + "(" + variables.ConvertAll(x => x.ToString()).Aggregate<String>((x, y) => x + "," + y) + " | " +
                conditions.ConvertAll(x => x.ToString()).Aggregate<String>((x, y) => x + "," + y);
        }
        public static JudgeItem Parse(String s)
        {
            JudgeItem item = new JudgeItem();
            int b1 = s.IndexOf("(");
            int b2 = s.IndexOf("|");
            item.expression = s.Substring(0, b1).Trim();
            String variables = s.Substring(b1 + 1, b2 - b1 - 1).Trim();
            item.variables = variables.Split(',').ToList().ConvertAll(x => int.Parse(x));
            s = s.Substring(b2 + 1, s.Length - b2 - 2).Trim();
            item.conditions = s.Split(',').ToList().ConvertAll(x => int.Parse(x));
            return item;
        }
    }
    public class JuegeGene
    {
        public List<JudgeItem> items = new List<JudgeItem>();
        public List<double> weights = new List<double>();

    }
}
