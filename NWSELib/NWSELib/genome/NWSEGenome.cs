using System;
using System.Collections.Generic;
using System.Text;

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
        private List<ReceptorGene> receptorGenes = new List<ReceptorGene>();
        /// <summary>
        /// 不同处理器的选择概率
        /// </summary>
        private List<double> handlerSelectionProb = new List<double>();
        /// <summary>
        /// 处理器基因
        /// </summary>
        private List<HandlerGene> handlerGenes = new List<HandlerGene>();

        /// <summary>
        /// 连接基因，两个神经元的ID
        /// </summary>
        private List<ValueTuple<String, String>> connectionGene = new List<(string, string)>();

    }
}
