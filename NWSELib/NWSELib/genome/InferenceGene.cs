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

        public int getVariableIndex(int varId)
        {
            (int t1, int t2) = this.getTimeDiff();
            for (int i = 0; i < dimensions.Count; i++)
            {
                if (dimensions[i].Item1 == varId && dimensions[i].Item2 == t2)
                    return i;
            }
            return -1;
        }
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

        public (int, int) getTimeDiff()
        {
            (int t1, int t2) = (
                dimensions.ConvertAll<int>(d => Math.Abs(d.Item2)).Max(),
                dimensions.ConvertAll<int>(d => Math.Abs(d.Item2)).Min()
                );
            return (t1, t2);
        }

        public List<(int, int)> getConditions()
        {
            (int t1, int t2) = this.getTimeDiff();
            return dimensions.FindAll(d => d.Item2 == t1);
        }

        public List<(int, int)> getVariables()
        {
            (int t1, int t2) = this.getTimeDiff();
            return dimensions.FindAll(d => d.Item2 == t2);
        }
        public (int, int) getVariable()
        {
            (int t1, int t2) = this.getTimeDiff();
            return dimensions.FindAll(d => d.Item2 == t2)[0];
        }

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

        public bool matchVariables(bool allmatch, params int[] variables)
        {
            (int t1, int t2) = this.getTimeDiff();
            List<int> vars = variables.ToList();
            for (int i = 0; i < dimensions.Count; i++)
            {
                if (!vars.Contains(dimensions[i].Item1)) continue;
                if (dimensions[i].Item2 != t2) return false;
                vars.Remove(dimensions[i].Item1);
            }
            return allmatch ? vars.Count <= 0 : vars.Count < variables.Length;
        }
        public bool match(int variable, params int[] conditions)
        {

            int variabletime = -1;
            for (int i = 0; i < dimensions.Count; i++)
            {
                if (dimensions[i].Item1 == variable)
                {
                    if (conditions == null || conditions.Length <= 0)
                    {
                        variabletime = dimensions[i].Item2; break;
                    }
                    else if (dimensions[i].Item2 == 0)
                    {
                        variabletime = dimensions[i].Item2; break;
                    }
                }
            }
            if (variabletime < 0) return false;

            List<int> conds = conditions.ToList();
            for (int i = 0; i < dimensions.Count; i++)
            {
                if (!conds.Contains(dimensions[i].Item1)) continue;
                if (dimensions[i].Item2 > variabletime) return false;
                conds.Remove(dimensions[i].Item1);
            }
            return variabletime >= 0 && conds.Count <= 0;
        }


    }
}
