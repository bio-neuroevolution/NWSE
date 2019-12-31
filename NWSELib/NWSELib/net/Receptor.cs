using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Text;

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

        
        public Receptor(NodeGene gene):base(gene){
            
        }

        /// <summary>
        /// 设置当前值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public override Object activate(Network net,int time,Object value=null)
        {
            Object prevValue = base.SetCurrentValue(value, time);

            double range = Session.GetConfiguration().agent.receptors.GetSensor(this.gene.Name).Range.Distance;
            double unit = range / ((ReceptorGene)this.gene).SectionCount;
            sectionIndex = (int)((double)value / unit);
            sectionValue = (sectionIndex*unit + (sectionIndex+1)*unit)/2.0;
            return prevValue;
        }
    }
}
