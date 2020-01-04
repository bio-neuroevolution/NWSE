using NWSELib.net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NWSELib.genome;
using NWSELib.common;
using Microsoft.ML.Probabilistic.Distributions;

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
            //1.将无效推理（可靠度小于阈值）的推理节点和有效推理节点向周边传播 
            for (int i = 0; i < inds.Count; i++)
            {
                List<NodeGene> invaildInference = inds[i].getInvaildInferenceGene();
                List<NodeGene> vaildInference = inds[i].getVaildInferenceGene();

                List<EvolutionTreeNode> nearest = EvolutionTreeNode.search(Session.getEvolutionRootNode(), inds[i]).getNearestNode();
                nearest.ForEach(node => node.network.Genome.gene_drift(invaildInference, vaildInference));
            }

            //1.计算每个个体所有节点平均可靠度的总和、均值和方差
            List<double> reability = inds.ConvertAll(ind => ind.GetNodeAverageReability());
            double sum_reability = reability.Sum();
            (double avg_reability,double variance_reability) = new Vector(reability).avg_variance();
            reability = reability.ConvertAll(r => r / sum_reability);
            
            //3.根据所有个体可靠度的均值和方差， 确定淘汰个体的可靠度下限：以可靠度均值和方差构成的高斯分布最大值的
            Gaussian gaussian = Gaussian.FromMeanAndPrecision(avg_reability, variance_reability);
            double lowlimit_rebility = gaussian.GetQuantile(gaussian.GetMode() * 1 / 2);
            for(int i=0;i<inds.Count;i++)
            {
                if (inds[i].Reability <= lowlimit_rebility)
                {
                    inds.RemoveAt(i);i--;
                }
            }
            //4.计算每个个体所有节点可靠度总和所占全部的比例，该比例乘以基数为下一代数量
            int propagate_base_count = inds.Count * Session.GetConfiguration().evolution.propagate_base_count;
            List<int> planPropagateCount = new List<int>();
            for (int i = 0; i < reability.Count; i++)
            {
                inds[i].Reability = reability[i];
                planPropagateCount.Add((int)(reability[i] * propagate_base_count));
            }

            //5.通过变异生成下一代
            List<Network> newinds = new List<Network>();
            for (int i = 0; i < inds.Count; i++)
            {
                EvolutionTreeNode node = EvolutionTreeNode.search(Session.getEvolutionRootNode(), inds[i]);
                int childcount = planPropagateCount[i];
                for(int j=0;j<childcount;j++)
                {
                    NWSEGenome mutateGenome = inds[i].Genome.mutate();
                    while (inds.Exists(ind => ind.Genome.equiv(mutateGenome)))
                        mutateGenome = inds[i].Genome.mutate();
                    Network mutateNet = new Network(mutateGenome);
                    newinds.Add(mutateNet);
                    EvolutionTreeNode cnode = new EvolutionTreeNode(mutateNet, node);
                    node.childs.Add(cnode);
                }
            }

        }
    }
}
