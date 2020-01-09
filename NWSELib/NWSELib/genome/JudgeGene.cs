using System;
using System.Collections.Generic;
using System.Linq;

using NWSELib.common;
namespace NWSELib.genome
{

    /// <summary>
    /// 评判基因
    /// </summary>
    public class JudgeGene : NodeGene
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
        /// <summary>
        /// 权重
        /// </summary>
        public double weight;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="genome"></param>
        public JudgeGene(NWSEGenome genome):base(genome)
        {

        }

        

        public override string Text
        {
            get
            {
                return expression + "(" +
                    owner[variable].Text +
                    "|env,action)";
            }
        }

        public JudgeGene clone()
        {
            JudgeGene item = new JudgeGene(this.owner)
            {
                expression = this.expression,
                variable = this.variable,
                weight = this.weight
                
            };
            item.conditions.AddRange(this.conditions);
            return item;
        }

        public override T clone<T>()
        {
            JudgeGene item = new JudgeGene(this.owner)
            {
                expression = this.expression,
                variable = this.variable,
                weight = this.weight
            };
            item.conditions.AddRange(this.conditions);
            return (T)(Object)item;
        }

        /// <summary>
        /// 转换字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "JudgeGene:" + Text + ";info:" + base.ToString() + ";param:weight=" + weight.ToString();    
                //conditions.ConvertAll(x => x.ToString()).Aggregate<String>((x, y) => x + "," + y);
        }
        /// <summary>
        /// 解析字符串
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static JudgeGene Parse(String s)
        {
            JudgeGene item = new JudgeGene(null);
            int t1 = s.IndexOf("JudgeGene")+10;
            int t2 = s.IndexOf("info");
            int t3 = s.IndexOf("param");
            String s1 = s.Substring(t1, t2 - t1 - 3);//Text部分
            String s2 = s.Substring(t2+5,t3-t2-7);//info部分
            String s3 = s.Substring(t3+6);//param部分

            //解析Text
            int t5 = s1.IndexOf("(");
            item.expression = s1.Substring(0, t5 - 1).Trim();
            int t6 = s1.IndexOf("|");
            item.variable = Session.idGenerator.getGeneId(s1.Substring(t5+1,t6-t5-2).Trim());
            

            //解析info
            item.parse(s2);
            //解析参数
            int t4 = s3.IndexOf("=");
            item.weight = double.Parse(s3.Substring(t4+1));

            
            return item;
        }

        
    }
}
