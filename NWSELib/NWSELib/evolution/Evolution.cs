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
    public class Evolution
    {
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="inds"></param>
        public void execute(List<Network> inds,Session session)
        {
            //1.将无效推理（可靠度小于阈值）的推理节点和有效推理节点向周边传播 
            //  使得周围个体的下一代不会产生无效推理节点，并必然产生有效推理节点
            for (int i = 0; i < inds.Count; i++)
            {
                List<NodeGene> invaildInference = inds[i].getInvaildInferenceGene();
                List<NodeGene> vaildInference = inds[i].getVaildInferenceGene();

                List<EvolutionTreeNode> nearest = EvolutionTreeNode.search(session.getEvolutionRootNode(), inds[i]).getNearestNode();
                nearest.ForEach(node => node.network.Genome.gene_drift(invaildInference, vaildInference));
            }

            //2.计算每个个体所有节点平均可靠度的总和、均值和方差
            List<double> reability = inds.ConvertAll(ind => ind.GetNodeAverageReability());
            double sum_reability = reability.Sum();
            reability = reability.ConvertAll(r => r / sum_reability);
            (double avg_reability, double variance_reability) = new Vector(reability).avg_variance();
            
            //3.根据所有个体可靠度的均值和方差， 确定淘汰个体的可靠度下限：以可靠度均值和方差构成的高斯分布最大值的
            Gaussian gaussian = Gaussian.FromMeanAndVariance(avg_reability, variance_reability);
            double lowlimit_rebility = gaussian.GetQuantile(gaussian.GetMode() * Session.GetConfiguration().evolution.selection.reability_lowlimit);
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
                EvolutionTreeNode node = EvolutionTreeNode.search(session.getEvolutionRootNode(), inds[i]);
                int childcount = planPropagateCount[i];
                for(int j=0;j<childcount;j++)
                {
                    NWSEGenome mutateGenome = inds[i].Genome.mutate(session);
                    while (inds.Exists(ind => ind.Genome.equiv(mutateGenome)))
                        mutateGenome = inds[i].Genome.mutate(session);
                    Network mutateNet = new Network(mutateGenome);
                    newinds.Add(mutateNet);
                    EvolutionTreeNode cnode = new EvolutionTreeNode(mutateNet, node);
                    node.childs.Add(cnode);
                }
            }

        }
    }
}
