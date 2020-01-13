using NWSELib.common;
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

        public override T clone<T>()
        {
            InferenceGene gene = new InferenceGene(this.owner).copy<InferenceGene>(this);
            gene.dimensions.AddRange(this.dimensions);
            return (T)(Object)gene;
        }
        public InferenceGene(NWSEGenome genome):base(genome)
        { 
        }
        /// <summary>
        /// 显示文本
        /// </summary>
        public override String Text
        {
            get
            {
                (int t1, int t2) = this.getTimeDiff();
                return this.getConditions()
                    .ConvertAll(c => c.Item1)
                    .ConvertAll(id => owner[id])
                    .ConvertAll(g => g.Text)
                    .Aggregate((x, y) => x + "," + y) +
                    (t1 == t2 ? "<=>" : "=>") +
                owner[this.getVariable().Item1].Text;
                //.Aggregate((x, y) => "["+x+"],[" + y+"]") +
            }
        }

        public override string ToString()
        {
            this.sort_dimension();
            return "InferenceGene:" + Text + ";info:" + base.ToString() + ";param:";
        }
        public static new InferenceGene Parse(String s)
        {
            int t1 = s.IndexOf("InferenceGene") + "InferenceGene".Length;
            int t2 = s.IndexOf("info");
            int t3 = s.IndexOf("param");
            String s1 = s.Substring(t1, t2 - t1 - 3);//Text部分
            String s2 = s.Substring(t2 + 5, t3 - t2 - 7);//info部分
            String s3 = s.Substring(t3 + 6);//param部分

            //解析text
            List<(int, int)> conditions = new List<(int, int)>();
            int t4 = s1.IndexOf("<=>");
            bool causeeffect_or_assocation = t4 < 0;
            int t5 = s1.IndexOf("[");
            while(t5>=0)
            {
                int t6 = s1.IndexOf("]",t5+1);
                int geneid = Session.idGenerator.getGeneId(s1.Substring(t5 + 1, t6 - t5 - 1));
                conditions.Add((geneid, (t6 < t4 ? 1 : 0)));
            }
            //解析info
            InferenceGene gene = new InferenceGene(null);
            gene.parse(s2);


            
            return gene;
        }

        private int comp_dimension((int,int) t1,(int,int) t2)
        {
            if (t1.Item2 > t2.Item2) return -1;
            else if (t1.Item2 < t2.Item2) return 1;
            else
            {
                if (t1.Item1 > t2.Item1) return 1;
                else if (t1.Item1 < t2.Item1) return -1;
                return 0;
            }
        }
        public void sort_dimension()
        {
            this.dimensions.Sort(comp_dimension);
        }

        /// <summary>
        /// 两个推理基因的关系
        /// </summary>
        /// <param name="gene">基因</param>
        /// <returns>0表示一致；1表示this包含另外一个；-1表示this被包含；2表示交叉；-2表示没有交叉</returns>
        public int relation(InferenceGene gene)
        {
            int[] rs = { 1, 1, 1, 1, 1 };
            int[] r = { 0, 1, -1, 2, -2 };
            for(int i=0;i<dimensions.Count;i++)
            {
                if (!gene.dimensions.Exists(d => d.Item1 == this.dimensions[i].Item1))
                {
                    rs[0] = rs[2] = 0;
                    continue;
                }
                if (!gene.dimensions.Exists(d => d.Item1 == this.dimensions[i].Item1 && d.Item2 == this.dimensions[i].Item2))
                {
                    rs[0] = 0;
                }
                else rs[4] = 0;

            }

            for (int i = 0; i < gene.dimensions.Count; i++)
            {
                if (!dimensions.Exists(d => d.Item1 == gene.dimensions[i].Item1))
                {
                    rs[0] = rs[1] = 0;
                    continue;
                }
                if (!dimensions.Exists(d => d.Item1 == gene.dimensions[i].Item1 && d.Item2 == gene.dimensions[i].Item2))
                {
                    rs[0] = 0;
                }
                else rs[4] = 0;

            }
            for (int i = 0; i < rs.Length; i++)
                if (rs[i] != 0) return r[i];
            throw new ExecutionEngineException();

        }
        
        /// <summary>
        /// 后置变量数
        /// </summary>
        public int VariableCount
        {
            get
            {
                return this.dimensions.FindAll(d => d.Item2 == 0).Count;
            }
        }
        /// <summary>
        /// 前置条件数
        /// </summary>
        public int ConditionCount
        {
            get
            {
                return this.dimensions.FindAll(d => d.Item2 == 1).Count;
            }
        }

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

        public int[] getVariableIndexs()
        {
            (int t1, int t2) = this.getTimeDiff();
            
            List<int> r = new List<int>();
            
            for (int i = 0; i < dimensions.Count; i++)
            {
                if (dimensions[i].Item2 == t2)
                    r.Add(i);
            }
            return r.ToArray();
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
        public bool matchVariable(params int[] conditions)
        {
            (int t1, int t2) = this.getTimeDiff();
            if(t1 == t2) //该节点各项无时间差异，属于关联记忆节点
            {
                List<int> conds = conditions.ToList();
                return Utility.intersection<int>(conditions.ToList(), conds);
            }
            else
            {
                int varid = this.getVariable().Item1;
                return conditions.Contains(varid);
            }
            
        }

        public List<(int,int)> getVariables()
        {
            (int t1, int t2) = this.getTimeDiff();
            return this.dimensions.FindAll(d => d.Item2 == t2);
        }

        public List<int> getVariableIds()
        {
            (int t1, int t2) = this.getTimeDiff();
            return this.dimensions.FindAll(d => d.Item2 == t2).ConvertAll(d => d.Item1);
        }

        public List<int> getConditionsExcludeActionSensor()
        {
            List<int> r = new List<int>();
            (int t1, int t2) = this.getTimeDiff();
            for (int i=0;i<dimensions.Count;i++)
            {
                if (dimensions[i].Item2 != t1) continue;
                if (this.owner[dimensions[i].Item1].Group.Contains("action"))
                    continue;
                r.Add(dimensions[i].Item1);

            }
            return r;
        }

        public List<int> getActionSensorsConditions()
        {
            List<int> r = new List<int>();
            (int t1, int t2) = this.getTimeDiff();
            for (int i = 0; i < dimensions.Count; i++)
            {
                if (dimensions[i].Item2 != t1) continue;
                if (!this.owner[dimensions[i].Item1].Group.Contains("action"))
                    continue;
                r.Add(dimensions[i].Item1);

            }
            return r;
        }
    }
}
