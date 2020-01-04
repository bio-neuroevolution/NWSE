using System;
using System.Collections.Generic;
using System.Linq;

using NWSELib.common;
namespace NWSELib.genome
{
    /// <summary>
    ///  判定项
    /// </summary>
    public class JudgeItem
    {
        public const String ARGMAX = "argmax";
        public const String ARGMIN = "argmin";
        /// <summary>
        /// 判定表达式，目前只能是argmin或者argmax两种
        /// </summary>
        public String expression;
        /// <summary>
        /// 判定条件
        /// </summary>
        public List<int> conditions = new List<int>();
        /// <summary>
        /// 判定后置变量
        /// </summary>
        public int variable;

        public JudgeItem clone()
        {
            JudgeItem item = new JudgeItem()
            {
                expression = this.expression,
                variable = this.variable
            };
            item.conditions.AddRange(this.conditions);
            return item;
        }

        /// <summary>
        /// 转换字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            conditions.Sort();
            return expression + "(" + variable.ToString() + " | " +
                conditions.ConvertAll(x => x.ToString()).Aggregate<String>((x, y) => x + "," + y);
        }
        /// <summary>
        /// 解析字符串
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static JudgeItem Parse(String s)
        {
            JudgeItem item = new JudgeItem();
            int b1 = s.IndexOf("(");
            int b2 = s.IndexOf("|");
            item.expression = s.Substring(0, b1).Trim();
            String variable = s.Substring(b1 + 1, b2 - b1 - 1).Trim();
            item.variable = int.Parse(variable);
            s = s.Substring(b2 + 1, s.Length - b2 - 2).Trim();
            item.conditions = s.Split(',').ToList().ConvertAll(x => int.Parse(x));
            return item;
        }
    }
    /// <summary>
    /// 评判基因
    /// </summary>
    public class JudgeGene : NodeGene
    {
        /// <summary>
        /// 评判项
        /// </summary>
        public List<JudgeItem> items = new List<JudgeItem>();
        /// <summary>
        /// 每项的权重
        /// </summary>
        public List<double> weights = new List<double>();


        public override T clone<T>()
        {
            JudgeGene gene = new JudgeGene().copy<JudgeGene>(this);
            gene.items = this.items.ConvertAll(x => x.clone()).ToList();
            gene.weights.AddRange(this.weights);
            return (T)(Object)gene;
        }

        public override string ToString()
        {
            return base.ToString() + ",weights="+
                weights.ConvertAll(w=>w.ToString()).Aggregate((x,y)=>x+","+y)+
                ",items="+items.ConvertAll(i=>i.ToString()).Aggregate((x,y)=>x+":"+y);
        }
        public static new JudgeGene parse(String str)
        {
            JudgeGene gene = new JudgeGene();
            ((NodeGene)gene).parse(str);

            int i1 = str.IndexOf("weights");
            int i2 = str.IndexOf("=", i1 + 1);
            int i3 = str.IndexOf(",",i2+1);

            gene.weights = Utility.parse(str.Substring(i2 + 1, i3 - i2 - 1));

            i1 = str.IndexOf("items");
            i2 = str.IndexOf("=", i1 + 1);
            str = str.Substring(i2+1);

            gene.items = str.Split(':').ToList().ConvertAll(s=>JudgeItem.Parse(s));
            return gene;

        }
    }
}
