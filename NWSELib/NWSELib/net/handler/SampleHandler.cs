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

        public override Object Activate(Network net, int time, Object value = null)
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
            base.Activate(net, time, result);
            return result;
        }

        public override void think(Network net, int time, Vector value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsThinkCompleted(time)))
                return;
            int t = time;

            //取得时间参数
            int valueCount = (int)TimeSpan;

            //从记忆库中获取各个节点对应配置的输入
            List<Vector> vs = new List<Vector>(valueCount);
            for(int i=0;i<valueCount;i++)
            {
                Vector v = inputs[0].getThinkValues(time - i);
                if (v == null) return;
                vs[i] = v.clone();
            }
            
            Vector result = vs.average();
            base.think(net, time, result);

        }
    }
}
