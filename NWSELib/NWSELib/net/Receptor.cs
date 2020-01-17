using NWSELib.common;
using NWSELib.genome;
using System;

namespace NWSELib.net
{
    /// <summary>
    /// 感受器
    /// </summary>
    public class Receptor : Node
    {

        /// <summary>分段索引</summary>
        protected int sectionIndex;
        /// <summary>分段值</summary>
        protected double sectionValue;


        public Receptor(NodeGene gene) : base(gene)
        {

        }

        /// <summary>
        /// 设置当前值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public override Object activate(Network net, int time, Object value = null)
        {

            Object prevValue = this.Value;
            //prevValue = base.activate(net, time, new Vector(sectionIndex));
            prevValue = base.activate(net, time, value);
            return prevValue;
        }
    }
}
