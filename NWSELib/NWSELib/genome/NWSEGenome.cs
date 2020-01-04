using System;
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
        public JudgeGene judgeGene = new JudgeGene();


        /// <summary>
        /// 无效推理基因
        /// </summary>
        public List<NodeGene> invaildInferenceNodes = new List<NodeGene>();

        /// <summary>
        /// 有效推理基因
        /// </summary>
        public List<NodeGene> vaildInferenceNodes = new List<NodeGene>();

        #endregion

        #region 克隆、编码和等价性
        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public NWSEGenome clone()
        {
            NWSEGenome genome = new NWSEGenome();
            receptorGenes.ForEach(r => genome.receptorGenes.Add(r.clone<ReceptorGene>()));
            genome.handlerSelectionProb.AddRange(handlerSelectionProb);
            handlerGenes.ForEach(h => genome.handlerGenes.Add(h.clone<HandlerGene>()));
            infrernceGenes.ForEach(i => genome.infrernceGenes.Add(i.clone<InferenceGene>()));
            genome.connectionGene.AddRange(connectionGene);
            genome.judgeGene = this.judgeGene.clone<JudgeGene>();
            invaildInferenceNodes.ForEach(inf => genome.invaildInferenceNodes.Add(inf.clone<NodeGene>()));
            return genome;
        }

        /// <summary>
        /// 判断两个染色体是否等价：如果处理器基因和推理基因完全一样的话（不包括可变异参数在内）
        /// </summary>
        /// <param name="genome"></param>
        /// <returns></returns>
        public bool equiv(NWSEGenome genome)
        {
            for (int i = 0; i < handlerGenes.Count; i++)
            {
                for (int j = 0; j < genome.handlerGenes.Count; j++)
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
            return this.encodeNodeGene(g1) == this.encodeNodeGene(g2);
        }
        /// <summary>
        /// 对基因做编码，该编码可以作为等价类的编码
        /// </summary>
        /// <param name="gene"></param>
        /// <returns></returns>
        public String encodeNodeGene(NodeGene gene)
        {
            if (gene.GetType() == typeof(EffectorGene))
                return "Effector:" + gene.Name;
            else if (gene.GetType() == typeof(HandlerGene))
            {
                List<int> inputIds = this.getInputs(gene).ConvertAll(g => g.Id);
                inputIds.Sort();
                return "Handler:" + ((HandlerGene)gene).function + "(" +
                    inputIds.ConvertAll(x => x.ToString()).Aggregate((x, y) => x + "," + y) + ")";
            }
            else if (gene.GetType() == typeof(InferenceGene))
            {
                ((InferenceGene)gene).sort_dimension();
                return "Inference:" + ((InferenceGene)gene).dimensions.ConvertAll(d => d.Item1.ToString() + "-" + d.Item2.ToString())
                    .Aggregate((x, y) => x + "," + y);
            }
            else if (gene.GetType() == typeof(JudgeGene))
            {
                return "Judge:" ;
            }
            else if (gene.GetType() == typeof(ReceptorGene))
                return "Receptor:" + gene.Name;

            return "";
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

        

        

        public bool isVaildGene(NodeGene gene)
        {
            throw new NotImplementedException();
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
            //对感受器的分段数进行变异
            for (int i = 0; i < genome.receptorGenes.Count; i++)
            {
                Configuration.Sensor sensor = Session.GetConfiguration().agent.receptors.GetSensor(genome.receptorGenes[i].Name);
                int min1 = (int)sensor.Level.Min;
                int max1 = (int)sensor.Level.Max;

                genome.receptorGenes[i].SectionCount = new Random().Next(min1,max1+1);
            }

            //选择一个处理器对参数进行变异

            //对处理器的选择概率变异
            double[] handler_selection_prob = Session.GetConfiguration().evolution.mutate.Handlerprob.ToArray();
            int handler_index = new Random().Next(0,handler_selection_prob.Length);
            double min = Session.GetConfiguration().handlers[handler_index].Selection_prob_range.Min;
            double max = Session.GetConfiguration().handlers[handler_index].Selection_prob_range.Max;
            handler_selection_prob[handler_index] = new Random().NextDouble()*(max-min)+min;
            int handler_index2 = handler_index;
            while(handler_index2 == handler_index)
                handler_index2 = new Random().Next(0, handler_selection_prob.Length);
            handler_selection_prob[handler_index2] = 0.0;
            handler_selection_prob[handler_index2] = 1.0 - handler_selection_prob.ToList().Sum();

            //添加一个处理器
            Session.GetConfiguration().random_handler(handler_selection_prob);
            //删除无效处理器

            //对推理节点进行变异

            //对判定节点权重进行变异

            return genome;
        }

        #endregion


    }
}
