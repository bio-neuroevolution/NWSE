using NWSELib.net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NWSELib.evolution
{
    public class Selection
    {
        /// <summary>
        /// 执行选择操作:确定每个个体的后继数量
        /// 1.计算每个个体所有节点可靠度的总和
        /// 2.计算每个个体所有节点可靠度总和所占全部的比例，该比例乘以基数为下一代数量
        /// 
        /// 3.将无效推理（可靠度小于阈值）的推理节点基因置入抑制基因库，将有效推理（可靠度大于阈值）的推理节点基因置于有效基因库
        /// 4.对抑制基因和优势基因进行随机基因漂移
        /// 5.通过变异生成下一代
        /// 6.若总个体数量超过阈值，则淘汰祖先中可靠度总数较低的个体
        /// </summary>
        public void execute(List<Network> inds)
        {
            /// 1.计算每个个体所有节点可靠度的总和
            /// 2.计算每个个体所有节点可靠度总和所占全部的比例，该比例乘以基数为下一代数量
            List<double> reability = inds.ConvertAll(ind => ind.getNodeReability());
            double sum_reability = reability.Sum();
            reability = reability.ConvertAll(r => r / sum_reability);
            int propagate_base_count = 0;
            for (int i = 0; i < reability.Count; i++)
            {
                inds[i].setReabiliy(reability[i]);
                inds[i].setPlanPropagateCount(reability[i]* propagate_base_count);
            }

        }
    }
}
