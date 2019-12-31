using System;
using System.Collections.Generic;

namespace NWSELib.genome
{
    /// <summary>
    /// 推断基因
    /// </summary>
    public class InferenceGene : NodeGene
    {
        /// <summary>
        /// 推断各维的节点ID或者名称
        /// </summary>
        private List<String> dimensions = new List<String>();
    }
}
