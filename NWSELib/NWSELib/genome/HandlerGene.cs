using System;
using System.Collections.Generic;
using System.Linq;

using NWSELib.common;

namespace NWSELib.genome
{
    public class HandlerGene : NodeGene
    {
        public readonly String function;
        public List<int> inputs = new List<int>();
        public List<double> param = new List<double>();
        Random rng = new Random();

        /// <summary>
        /// 显示文本
        /// </summary>
        public override String Text 
        { 
            get
            {
                List<NodeGene> inputs = this.inputs.ConvertAll(i => owner[i]);
                inputs.Sort();
                //return function + "(" + inputs.ConvertAll(x => x.Text).Aggregate((m, n) =>"{" + m + "}"+ ",{" + n + "}") + ")";
                return function + "(" + inputs.ConvertAll(x => x.Text).Aggregate((m, n) => m + "," + n ) + ")";
            }
        }

        public override List<int> Dimensions
        {
            get
            {
                List<NodeGene> inputs = this.inputs.ConvertAll(i => owner[i]);
                return inputs.ConvertAll(e => e.Dimension);
            }
        }
        public override T clone<T>()
        {
            return new HandlerGene(this.owner,this.function,this.inputs,this.param.ToArray()).copy<T>(this);
        }

        public HandlerGene(NWSEGenome genome,String function,List<int> inputs,params double[] ps):base(genome)
        {
            
            this.function = function;
            this.inputs.AddRange(inputs);
            if (ps != null || ps.Length <= 0) param.AddRange(ps);
        }

        public void sortInput()
        {
            this.inputs.Sort();
        }

        /// <summary>
        /// 转换字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "HandlerGene:" + Text + ";info:" + base.ToString() + ";param:ps=" +
                (param.Count <= 0 ? "" : param.ConvertAll(p => p.ToString()).Aggregate<String>((x, y) => x.ToString() + "," + y.ToString()));
        }
        

        public static HandlerGene Parse(String s,ref List<int> inputids)
        {
            int t1 = s.IndexOf("HandlerGene") + "HandlerGene".Length;
            int t2 = s.IndexOf("info");
            int t3 = s.IndexOf("param");
            String s1 = s.Substring(t1, t2 - t1 - 3);//Text部分
            String s2 = s.Substring(t2 + 5, t3 - t2 - 7);//info部分
            String s3 = s.Substring(t3 + 6);//param部分

            //解析text
            int t4 = s1.IndexOf("(");
            String function = s1.Substring(0, t4);
            if (inputids == null) inputids = new List<int>();
            int t6 = s1.IndexOf("{");
            while(t6>=0)
            {
                int t7 = s1.IndexOf("}",t6+1);
                String s4 = s1.Substring(t6 + 1, t7 - t6 - 1);
                inputids.Add(Session.idGenerator.getGeneId(s4.Trim()));
                t6 = s1.IndexOf("{");
            }
             //解析info
             HandlerGene gene = new HandlerGene(null, function, inputids, null);
            gene.parse(s2);
            //解析参数
            int t5 = s3.IndexOf("=");
            gene.param = Utility.parse(s3.Substring(t5 + 1).Trim());

            return gene;
        }

        public void mutate()
        {

            if(function == "avg")
            {
                int valueTime = (int)param[0];
                if (valueTime == 0)
                {
                    if (rng.NextDouble() <= 0.5) valueTime = 1;
                }
                else
                {
                    if (rng.NextDouble() <= 0.8) valueTime -= 1;
                }
                param[0] = valueTime;
            }else if(function == "diff")
            {
                int valueTime = (int)param[0];
                if (valueTime == 0)
                {
                    if (rng.NextDouble() <= 0.5) valueTime = 1;
                }
                else
                {
                    if (rng.NextDouble() <= 0.8) valueTime -= 1;
                }
                param[0] = valueTime;
            }else if(function == "direction")
            {

            }else if(function == "variance")
            {
                List<double> ts = param;
                int index = rng.Next(0, ts.Count);

                int valueTime = (int)param[index];
                if (valueTime == 0)
                {
                    if (rng.NextDouble() <= 0.5) valueTime = 1;
                }
                else
                {
                    if (rng.NextDouble() <= 0.8) valueTime -= 1;
                }
                param[index] = valueTime;
            }
            

        }


    }
}
