using NWSELib.common;
using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWSELib.net.handler
{
    public class AverageHandler : Handler
    {
        public AverageHandler(NodeGene gene) : base(gene)
        {
        }
        public override Object activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            int t = time;

            //取得时间参数
            int valueTime = (int)((HandlerGene)this.gene).param[0];


            //从记忆库中获取各个节点对应配置的输入
            List<Vector> vs = new List<Vector>();
            if (inputs.Count == 1)
            {
                int valueCount = valueTime;
                if (valueCount < 0 || valueCount > Session.GetConfiguration().agent.shorttermcapacity)
                    valueCount = 2;
                vs = inputs[0].GetValues(time, valueCount);

            }
            else
            {
                if (valueTime < 0 || valueTime > Session.GetConfiguration().agent.shorttermcapacity)
                    valueTime = 0;
                for (int i = 0; i < inputs.Count; i++)
                {
                    vs.Add(inputs[i].GetValue(time, valueTime));
                }
            }
            Vector result = vs.average();
            base.activate(net, time, result);
            return result;
        }
        
    }
}

