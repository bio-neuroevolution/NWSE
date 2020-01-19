using System;
using System.Collections.Generic;
using System.Linq;

using NWSELib.common;

namespace NWSELib.genome
{
    /// <summary>
    /// 处理节点基因
    /// </summary>
    public class HandlerGene : NodeGene
    {
        #region 基本信息
        /// <summary>
        /// 处理功能
        /// </summary>
        public readonly String function;
        /// <summary>
        /// 输入节点基因id，其中可能会有重复
        /// </summary>
        public List<int> inputs = new List<int>();
        /// <summary>
        /// 处理参数，依据function不同而不同
        /// </summary>
        public List<double> param = new List<double>();

        public override List<int> Dimensions
        {
            get
            {
                List<NodeGene> inputs = this.inputs.ConvertAll(i => owner[i]);
                return inputs.ConvertAll(e => e.Dimension);
            }
        }

        /// <summary>
        /// 取得输入基因
        /// </summary>
        /// <returns></returns>
        public override List<NodeGene> getInputGenes()
        {
            return this.inputs.ConvertAll(i => owner[i]);
        }

        #endregion

        #region 显示和初始化
        public HandlerGene(NWSEGenome genome, String function, List<int> inputs, params double[] ps) : base(genome)
        {

            this.function = function;
            this.inputs.AddRange(inputs);
            if (ps != null || ps.Length <= 0) param.AddRange(ps);
        }

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
                return function + "(" + inputs.ConvertAll(x => x.Text).Aggregate((m, n) => m + "," + n) + ")";
            }
        }


        public override T clone<T>()
        {
            return new HandlerGene(this.owner, this.function, this.inputs, this.param.ToArray()).copy<T>(this);
        }



        public void sortInput()
        {
            this.inputs.Sort();
        }

        #endregion

        #region 读写

        /// <summary>
        /// 转换字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "HandlerGene:" + Text + ";info:" + base.ToString() + ";param:ps=" +
                (param.Count <= 0 ? "" : param.ConvertAll(p => p.ToString()).Aggregate<String>((x, y) => x.ToString() + "," + y.ToString()));
        }


        public static HandlerGene Parse(String s, ref List<int> inputids)
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
            while (t6 >= 0)
            {
                int t7 = s1.IndexOf("}", t6 + 1);
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
        public virtual void mutate() { }
        #endregion
    }

    /// <summary>
    /// 均值基因
    /// </summary>
    public class AvgHandlerGene : HandlerGene
    {

        public AvgHandlerGene(NWSEGenome genome, String function, List<int> inputs, params double[] ps) : base(genome, function, inputs, ps)
        {
        }
        
        public override void mutate()
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

        }


    }
    /// <summary>
    /// 最大值索引基因
    /// </summary>
    public class ArgmaxHandlerGene : HandlerGene
    {
        public ArgmaxHandlerGene(NWSEGenome genome, String function, List<int> inputs, params double[] ps) : base(genome, function, inputs, ps)
        {
        }
    }

    
    /// <summary>
    /// 差值基因
    /// </summary>
    public class DiffHandlerGene : HandlerGene
    {
        public DiffHandlerGene(NWSEGenome genome, String function, List<int> inputs, params double[] ps) : base(genome, function, inputs, ps)
        {
        }

        public override void mutate()
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
        }
    }
    /// <summary>
    /// 方向基因
    /// </summary>
    public class DirectionHandlerGene : HandlerGene
    {
        public DirectionHandlerGene(NWSEGenome genome, String function, List<int> inputs, params double[] ps) : base(genome, function, inputs, ps)
        {
        }
    }
}
