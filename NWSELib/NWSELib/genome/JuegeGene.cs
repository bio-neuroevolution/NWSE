using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NWSELib.genome
{
    public class JudgeItem
    {
        public const String MAX = "max";
        public const String MIN = "min";
        public String expression;
        public List<String> conditions = new List<string>();
        public String variable;

        public override string ToString()
        {
            return expression + "(" + variable + " | " +
                conditions.Aggregate((x, y) => x + "," + y);
        }
        public static JudgeItem Parse(String s)
        {
            JudgeItem item = new JudgeItem();
            int b1 = s.IndexOf("(");
            int b2 = s.IndexOf("|");
            item.expression = s.Substring(0, b1).Trim();
            item.variable = s.Substring(b1 + 1, b2 - b1 - 1).Trim();
            s = s.Substring(b2 + 1, s.Length - b2 - 2).Trim();
            item.conditions.AddRange(s.Split(','));
            return item;
        }
    }
    public class JuegeGene
    {
        public List<JudgeItem> items = new List<JudgeItem>();
        public List<double> weights = new List<double>();

    }
}
