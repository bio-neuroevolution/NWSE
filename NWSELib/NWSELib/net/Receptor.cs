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
        
        /// <summary>分段值</summary>
        protected int sectionIndex;

        /// <summary>
        /// 设置当前值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public override Object SetCurrentValue(Object value, int time)
        {
            Object prevValue = base.SetCurrentValue(value, time);

            double range = Session.GetConfiguration().agent.receptors.GetSensor(this.gene.Name).Range.Distance;
            double unit = range / ((ReceptorGene)this.gene).SectionCount;
            sectionIndex = (int)((double)value / unit);
            return prevValue;
        }
    }
}
