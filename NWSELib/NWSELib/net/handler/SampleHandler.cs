using NWSELib.common;
using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWSELib.net.handler
{
    public class SampleHandler : Handler
    {
        /// <summary>
        /// 时间跨度
        /// </summary>
        public override double TimeSpan
        {
            get
            {
                int valueTime = (int)((HandlerGene)this.gene).param[0];
                if (valueTime < 0 || valueTime > Session.GetConfiguration().agent.shorttermcapacity)
                    valueTime = 2;
                return valueTime;
            }
        }

        public SampleHandler(NodeGene gene, Network net) : base(gene, net)
        {
            
        }

        public override Object activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            int t = time;

            //取得时间参数
            int valueCount = (int)TimeSpan;
            
            //从记忆库中获取各个节点对应配置的输入
            List<Vector> vs = inputs[0].GetValues(time, valueCount);
            if (vs == null || vs.Count != valueCount) return null;
            Vector result = vs.average();
            base.activate(net, time, result);
            return result;
        }
    }
}
