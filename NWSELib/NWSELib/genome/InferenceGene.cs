using System;
using System.Collections.Generic;
using System.Linq;

namespace NWSELib.genome
{

    /// <summary>
    /// 推断基因
    /// </summary>
    public class InferenceGene : NodeGene
    {
        /// <summary>
        /// 推断各维的节点ID或者名称,以及时间项
        /// </summary>
        public List<(int, int)> dimensions = new List<(int, int)>();

        /// <summary>
        /// 得到推理变量Id对应的索引
        /// </summary>
        /// <param name="varId"></param>
        /// <returns></returns>
        public int getVariableIndex(int varId=-1)
        {
            (int t1, int t2) = this.getTimeDiff();
            for (int i = 0; i < dimensions.Count; i++)
            {
                if ((varId == -1 || dimensions[i].Item1 == varId) && dimensions[i].Item2 == t2)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// 得到条件Id对应的索引
        /// </summary>
        /// <param name="condId"></param>
        /// <returns></returns>
        public int getConditionIndex(int condId)
        {
            (int t1, int t2) = this.getTimeDiff();
            for (int i = 0; i < dimensions.Count; i++)
            {
                if (dimensions[i].Item1 == condId && dimensions[i].Item2 == t1)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// 取得条件和后置变量的时间差
        /// </summary>
        /// <returns></returns>
        public (int, int) getTimeDiff()
        {
            (int t1, int t2) = (
                dimensions.ConvertAll<int>(d => Math.Abs(d.Item2)).Max(),
                dimensions.ConvertAll<int>(d => Math.Abs(d.Item2)).Min()
                );
            return (t1, t2);
        }
        /// <summary>
        /// 得到所有的条件，包括Id和相对时间
        /// </summary>
        /// <returns></returns>
        public List<(int, int)> getConditions()
        {
            (int t1, int t2) = this.getTimeDiff();
            return dimensions.FindAll(d => d.Item2 == t1);
        }
        /// <summary>
        /// 得到后置变量Id和相对时间
        /// </summary>
        /// <returns></returns>
        public (int, int) getVariable()
        {
            (int t1, int t2) = this.getTimeDiff();
            return dimensions.FirstOrDefault<(int,int)>(d => d.Item2 == t2);
        }
        
        /// <summary>
        /// 条件是否匹配
        /// </summary>
        /// <param name="allmatched">要求全部匹配</param>
        /// <param name="conditions">条件Id</param>
        /// <returns></returns>
        public bool matchCondition(bool allmatched, params int[] conditions)
        {
            (int t1, int t2) = this.getTimeDiff();
            List<int> conds = conditions.ToList();
            for (int i = 0; i < dimensions.Count; i++)
            {
                if (!conds.Contains(dimensions[i].Item1)) continue;
                if (dimensions[i].Item2 < t1) return false;
                conds.Remove(dimensions[i].Item1);
            }
            return allmatched ? conds.Count <= 0 : conds.Count < conditions.Length;
        }
        /// <summary>
        /// 变量是否匹配
        /// </summary>
        /// <param name="allmatch">全部匹配</param>
        /// <param name="variables"></param>
        /// <returns></returns>
        public bool matchVariable(int variableId)
        {
            (int t1, int t2) = this.getTimeDiff();
         
            for (int i = 0; i < dimensions.Count; i++)
            {
                if (variableId != dimensions[i].Item1) continue;
                if (dimensions[i].Item2 != t2) continue;
                return true;
            }
            return false;
        }
        


    }
}
