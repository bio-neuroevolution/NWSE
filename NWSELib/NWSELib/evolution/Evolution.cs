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
            session.triggerEvent(Session.EVT_LOG, "population evoluting...");
            //1.将无效推理（可靠度小于阈值）的推理节点和有效推理节点向周边传播 
            //  使得周围个体的下一代不会产生无效推理节点，并必然产生有效推理节点
            int invaildCount = 0, vaildCount = 0;
            for (int i = 0; i < inds.Count; i++)
            {
                List<NodeGene> invaildInference = inds[i].findNewInvaildInference();
                if(invaildInference != null && invaildInference.Count>0)
                {
                    invaildInference.ForEach(iinf =>
                        session.triggerEvent(Session.EVT_INVAILD_GENE, inds[i],iinf));
                    invaildCount += invaildInference.Count;
                }
                
                List<NodeGene> vaildInference = inds[i].findNewVaildInferences();
                vaildCount += vaildInference.Count;
                vaildInference.ForEach(vinf => session.triggerEvent(Session.EVT_VAILD_GENE, inds[i],vinf));
                List<EvolutionTreeNode> nearest = EvolutionTreeNode.search(session.getEvolutionRootNode(), inds[i]).getNearestNode();
                nearest.ForEach(node => node.network.Genome.gene_drift(invaildInference, vaildInference));


            }
            session.triggerEvent(Session.EVT_LOG, "count of invaild inf=" + invaildCount.ToString()+",count of vaild inf="+ vaildCount.ToString());


            //2.计算每个个体所有节点平均可靠度的总和、均值和方差
            List<double> reability = inds.ConvertAll(ind => ind.AverageReability);
            session.triggerEvent(Session.EVT_LOG, "reability=" + Utility.toString(reability));

            double sum_reability = reability.FindAll(r=>!double.IsNaN(r)).Sum();
            if(sum_reability != 0)
                reability = reability.ConvertAll(r => r / sum_reability);
            (double avg_reability, double variance_reability) = new Vector(reability).avg_variance();
            session.triggerEvent(Session.EVT_REABILITY, avg_reability, variance_reability);
            session.triggerEvent(Session.EVT_LOG, "Average of Reability=" + avg_reability + ",Variance of reability=" + variance_reability.ToString());

            //3.根据所有个体可靠度的均值和方差， 确定淘汰个体的可靠度下限：以可靠度均值和方差构成的高斯分布最大值的
            if (inds.Count >= Session.GetConfiguration().evolution.selection.count)
            {
                int prevcount = inds.Count;
                Gaussian gaussian = Gaussian.FromMeanAndVariance(avg_reability, variance_reability);
                //double lowlimit_rebility = gaussian.GetQuantile(gaussian.GetMode() * Session.GetConfiguration().evolution.selection.reability_lowlimit);
                double lowlimit_rebility = Utility.getGaussianByProb(avg_reability, variance_reability, gaussian.GetMode() * Session.GetConfiguration().evolution.selection.reability_lowlimit)[0];
                for (int i = 0; i < inds.Count; i++)
                {
                    if (inds[i].AverageReability <= lowlimit_rebility)
                    {
                        inds.RemoveAt(i); i--;
                    }
                }
                session.triggerEvent(Session.EVT_LOG, "die out: prev="+prevcount.ToString()+",now="+inds.Count.ToString()+ ",lowlimit of rebility="+lowlimit_rebility.ToString());
            }
            
            //4.计算每个个体所有节点可靠度总和所占全部的比例，该比例乘以基数为下一代数量
            int propagate_base_count = inds.Count * Session.GetConfiguration().evolution.propagate_base_count;
            List<int> planPropagateCount = new List<int>();
            for (int i = 0; i < reability.Count; i++)
            {
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

                    session.judgePaused();
                }
            }
            inds.AddRange(newinds);

            session.triggerEvent(Session.EVT_LOG, "population evoluting end");
            session.triggerEvent(Session.EVT_GENERATION_END);

        }
    }
}
