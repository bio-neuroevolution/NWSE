using System;
using System.Collections.Generic;
using System.Text;

namespace NWSELib.genome
{
    public class IdGenerator
    {
        #region 状态信息
        /// <summary>
        /// 当前基因Id
        /// </summary>
        protected int currentGeneId = 0;
        /// <summary>
        /// 当前染色体Id
        /// </summary>
        protected int currentGenmoeId = 0;
        /// <summary>
        /// 已经使用的基因Id
        /// </summary>
        protected Dictionary<String, int> ids = new Dictionary<string, int>();
        /// <summary>
        /// 每个基因的可靠度(这里只放可靠度过大或者过小的基因)
        /// </summary>
        protected Dictionary<int, double> reability = new Dictionary<int, double>();

        #endregion

        #region 取得最新Id
        /// <summary>
        /// 取得基因Id
        /// </summary>
        /// <param name="genome"></param>
        /// <param name="gene"></param>
        /// <returns></returns>
        public int getGeneId(NWSEGenome genome,NodeGene gene)
        {
            String code = genome.encodeNodeGene(gene);
            if (ids.ContainsKey(code)) return ids[code];
            ids.Add(code, ++currentGeneId);
            return currentGeneId;
        }
        /// <summary>
        /// 取得染色体Id
        /// </summary>
        /// <returns></returns>
        public int getGenomeId()
        {
            return ++currentGenmoeId;
        }
        #endregion

        #region 基因可靠度
        public void setReability(params (int,double)[] reability)
        {
            for(int i=0;i<reability.Length;i++)
            {
                if (!this.reability.ContainsKey(reability[i].Item1))
                    this.reability.Add(reability[i].Item1, reability[i].Item2);
                else
                    this.reability[reability[i].Item1] = reability[i].Item2;
            }
        }
        public double getReability(int geneid)
        {
            return this.reability.ContainsKey(geneid) ? this.reability[geneid] : 0;
        }
        public bool isVaildGene(int geneid)
        {
            double r = this.getReability(geneid);
            return r > Session.GetConfiguration().evaluation.gene_reability_range.Max;
        }
        public bool isInvaildGene(int geneid)
        {
            double r = this.getReability(geneid);
            return r < Session.GetConfiguration().evaluation.gene_reability_range.Min;
        }
        #endregion
    }
}
