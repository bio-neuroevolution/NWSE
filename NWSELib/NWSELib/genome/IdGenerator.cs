using System;
using System.Collections.Generic;
using System.Text;

namespace NWSELib.genome
{
    public class IdGenerator
    {
        protected int currentGeneId = 0;
        protected int currentGenmoeId = 0;
        protected Dictionary<String, int> ids = new Dictionary<string, int>();
        public int getGeneId(NWSEGenome genome,NodeGene gene)
        {
            String code = genome.encodeNodeGene(gene);
            if (ids.ContainsKey(code)) return ids[code];
            ids.Add(code, ++currentGeneId);
            return currentGeneId;
        }
        public int getGenomeId()
        {
            return ++currentGenmoeId;
        }
    }
}
