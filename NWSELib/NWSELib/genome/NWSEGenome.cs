using System.Collections.Generic;

namespace NWSELib.genome
{
    /// <summary>
    /// 染色体
    /// </summary>
    public class NWSEGenome
    {
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

        /// <summary>
        /// 基因漂移处理
        /// </summary>
        /// <param name="invaildInferenceNodes"></param>
        /// <param name="vaildInferenceNodes"></param>
        public void gene_drift(List<NodeGene> invaildInferenceNodes, List<NodeGene> vaildInferenceNodes)
        {

        }





    }
}
