using System;
using System.Collections.Generic;
using System.Linq;

using NWSELib.common;

namespace NWSELib.genome
{
    public class HandlerGene : NodeGene
    {
        public readonly String function;
        public readonly List<double> param = new List<double>();

        public override T clone<T>()
        {
            return new HandlerGene(this.function,this.param.ToArray()).copy<T>(this);
        }

        public HandlerGene(String function,params double[] ps)
        {
            this.function = function;
            if (ps != null || ps.Length <= 0) param.AddRange(ps);
        }
        public override string ToString()
        {
            return base.ToString()+",function="+function+",param="+
                (param.Count<=0?"":param.ConvertAll(p=>p.ToString()).Aggregate<String>((x,y)=>x.ToString()+","+y.ToString()));
        }
        

        public static HandlerGene parse(String str)
        {
            int i1 = str.IndexOf("function");
            int i2 = str.IndexOf("=", i1 + 1);
            int i3 = str.IndexOf(",", i2 + 1);
            String s = str.Substring(i2 + 1, i3 - i2 - 1);
            
            i1 = str.IndexOf("param");
            i2 = str.IndexOf("=", i1 + 1);
            s = str.Substring(i2 + 1, str.Length - i2 - 1);
            List<double> ds = Utility.parse(s);

            HandlerGene gene = new HandlerGene(s, ds.ToArray());
            ((NodeGene)gene).parse(str);
            return gene;
        }

        public void mutate()
        {
            if(function == "avg")
            {
                int valueTime = (int)param[0];
                if (valueTime == 0)
                {
                    if (new Random().NextDouble() <= 0.5) valueTime = 1;
                }
                else
                {
                    if (new Random().NextDouble() <= 0.8) valueTime -= 1;
                }
                param[0] = valueTime;
            }else if(function == "diff")
            {
                int valueTime = (int)param[0];
                if (valueTime == 0)
                {
                    if (new Random().NextDouble() <= 0.5) valueTime = 1;
                }
                else
                {
                    if (new Random().NextDouble() <= 0.8) valueTime -= 1;
                }
                param[0] = valueTime;
            }else if(function == "direction")
            {

            }else if(function == "variance")
            {
                List<double> ts = param;
                int index = new Random().Next(0, ts.Count);

                int valueTime = (int)param[index];
                if (valueTime == 0)
                {
                    if (new Random().NextDouble() <= 0.5) valueTime = 1;
                }
                else
                {
                    if (new Random().NextDouble() <= 0.8) valueTime -= 1;
                }
                param[index] = valueTime;
            }
            

        }


    }
}
