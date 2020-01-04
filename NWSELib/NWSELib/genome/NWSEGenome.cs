using System.Collections.Generic;
using System.Linq;

using NWSELib.common;

namespace NWSELib.genome
{
    /// <summary>
    /// 染色体
    /// </summary>
    public class NWSEGenome
    {
        #region 染色体组成
        /// <summary>
        /// 染色体Id
        /// </summary>
        public int id;
        /// <summary>
        /// 感受器基因
        /// </summary>
        public readonly List<ReceptorGene> receptorGenes = new List<ReceptorGene>();

        
        /// <summary>
        /// 不同处理器的选择概率
        /// </summary>
        public readonly List<double> handlerSelectionProb = new List<double>();
        /// <summary>
        /// 处理器基因
        /// </summary>
        public readonly List<HandlerGene> handlerGenes = new List<HandlerGene>();

        
        /// <summary>
        /// 推理器基因
        /// </summary>
        public readonly List<InferenceGene> infrernceGenes = new List<InferenceGene>();

        /// <summary>
        /// 连接基因，两个神经元的ID
        /// </summary>
        public readonly List<(int, int)> connectionGene = new List<(int, int)>();

        
        /// <summary>
        /// 评判基因
        /// </summary>
        public JuegeGene judgeGene = new JuegeGene();


        /// <summary>
        /// 无效推理基因
        /// </summary>
        public List<NodeGene> invaildInferenceNodes = new List<NodeGene>();

        public NWSEGenome clone()
        {
            NWSEGenome genome = new NWSEGenome();
            receptorGenes.ForEach(r => genome.receptorGenes.Add(r.clone()));
            genome.handlerSelectionProb.AddRange(handlerSelectionProb);
            handlerGenes.ForEach(h => genome.handlerGenes.Add(h.clone()));
            infrernceGenes.ForEach(i => genome.infrernceGenes.Add(i.clone()));
            genome.connectionGene.AddRange(connectionGene);
            genome.judgeGene = this.judgeGene.clone();
            invaildInferenceNodes.ForEach(inf => genome.invaildInferenceNodes.Add(inf.clone()));
            return genome;
        }
        

        #endregion

        #region 查询
        /// <summary>
        /// 根据Id查找
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public NodeGene this[int id]
        {
            get
            {
                NodeGene gene = receptorGenes.FirstOrDefault(g => g.Id == id);
                if (gene != null) return gene;
                gene = handlerGenes.FirstOrDefault(g => g.Id == id);
                if (gene != null) return gene;
                gene = infrernceGenes.FirstOrDefault(g => g.Id == id);
                return gene;
            }
        }
        /// <summary>
        /// 查找某节点的传入节点
        /// </summary>
        /// <param name="gene"></param>
        /// <returns></returns>
        public List<NodeGene> getInputs(NodeGene gene)
        {
            List<int> ids = this.connectionGene.FindAll(c => c.Item2 == gene.Id).ConvertAll(c => c.Item1);
            return ids.ConvertAll(id => this[id]);
        }
        #endregion

        

        /// <summary>
        /// 判断两个染色体是否等价：如果处理器基因和推理基因完全一样的话
        /// </summary>
        /// <param name="genome"></param>
        /// <returns></returns>
        public bool equiv(NWSEGenome genome)
        {
            for(int i = 0; i < handlerGenes.Count; i++)
            {
                for(int j=0;j<genome.handlerGenes.Count;j++)
                {
                    if (!equiv(handlerGenes[i], genome.handlerGenes[j])) return false;
                }
            }
            for (int i = 0; i < genome.handlerGenes.Count; i++)
            {
                if (handlerGenes.Exists(g => g.Id == genome.handlerGenes[i].Id))
                    continue;
                for (int j = 0; j < handlerGenes.Count; j++)
                {
                    if (!equiv(genome.handlerGenes[i], handlerGenes[j])) return false;
                }
            }

            for (int i = 0; i < infrernceGenes.Count; i++)
            {
                for (int j = 0; j < genome.infrernceGenes.Count; j++)
                {
                    if (!equiv(infrernceGenes[i], genome.infrernceGenes[j])) return false;
                }
            }
            for (int i = 0; i < genome.infrernceGenes.Count; i++)
            {
                if (infrernceGenes.Exists(g => g.Id == genome.infrernceGenes[i].Id))
                    continue;
                for (int j = 0; j < infrernceGenes.Count; j++)
                {
                    if (!equiv(genome.infrernceGenes[i], infrernceGenes[j])) return false;
                }
            }
            return true;
        }

        
        /// <summary>
        /// 是否等价
        /// </summary>
        /// <param name="gene"></param>
        /// <returns></returns>
        public bool equiv(NodeGene g1, NodeGene g2)
        {
            if (g1 == null || g2 == null) return false;
            if (g1.GetType() != g2.GetType()) return false;

            if(g1.GetType() == typeof(EffectorGene))
            {
                return g1.Name == g2.Name;
            }else if(g1.GetType() == typeof(HandlerGene))
            {
                HandlerGene h1 = (HandlerGene)g1;
                HandlerGene h2 = (HandlerGene)g2;
                if (h1.function != h2.function) return false;
                List<int> h1_inputs = this.getInputs(h1).ConvertAll(n=>n.Id);
                List<int> h2_inputs = this.getInputs(h2).ConvertAll(n => n.Id);
                return Utility.equals(h1_inputs, h2_inputs);

            }else if(g1.GetType() == typeof(InferenceGene))
            {
                InferenceGene i1 = (InferenceGene)g1;
                InferenceGene i2 = (InferenceGene)g2;
                int r = i1.relation(i2);
                return r == 0;
            }

            return false;
        }

        #region 漂移和变异
        /// <summary>
        /// 基因漂移处理
        /// </summary>
        /// <param name="invaildInferenceNodes"></param>
        /// <param name="vaildInferenceNodes"></param>
        public void gene_drift(List<NodeGene> invaildInferenceNodes, List<NodeGene> vaildInferenceNodes)
        {
            for (int i = 0; i < invaildInferenceNodes.Count; i++)
            {
                NodeGene g1 = invaildInferenceNodes[i];
                bool exist = false;
                for (int j = 0; j < this.invaildInferenceNodes.Count; j++)
                {
                    NodeGene g2 = this.invaildInferenceNodes[j];
                    if (this.equiv(g1, g2)) { exist = true; break; }
                }
                if (!exist) this.invaildInferenceNodes.Add(g1);
            }
        }

        public NWSEGenome mutate()
        {
            NWSEGenome genome = this.clone();

        }

        #endregion


    }
}
