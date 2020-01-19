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
        #region 基本信息
        /// <summary>
        /// 推断各维的前提条件节点ID或者名称,以及时间项
        /// </summary>
        public List<(int, int)> conditions = new List<(int, int)>();

        /// <summary>
        /// 推断各维的后置变量节点ID或者名称,以及时间项
        /// </summary>
        public List<(int, int)> variables = new List<(int, int)>();
        /// <summary>
        /// 合并前提和后置
        /// </summary>
        /// <returns></returns>
        public List<(int,int)> getDimensions()
        {
            List<(int, int)> r = new List<(int, int)>(conditions);
            r.AddRange(variables);
            return r;

        }

        public override List<int> Dimensions
        {
            get
            {
                return getDimensions().ConvertAll(d => this.owner[d.Item1].Dimension);
            }
        }

        private int comp_dimension((int, int) t1, (int, int) t2)
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
            this.conditions.Sort(comp_dimension);
            this.variables.Sort(comp_dimension);
        }


        /// <summary>
        /// 后置变量数
        /// </summary>
        public int VariableCount { get => this.variables.Count; }

        /// <summary>
        /// 前置条件数
        /// </summary>
        public int ConditionCount { get => this.conditions.Count; }


        /// <summary>
        /// 得到所有的条件，包括Id和相对时间
        /// </summary>
        /// <returns></returns>
        public List<(int, int)> getConditions()
        {
            return this.conditions;
        }

        /// <summary>
        /// 得到所有的条件Id
        /// </summary>
        /// <returns></returns>
        public List<int> getConditionIds()
        {
            return this.conditions.ConvertAll(c => c.Item1);
        }
        /// <summary>
        /// 得到后置变量Id和相对时间
        /// </summary>
        /// <returns></returns>
        public (int, int) getVariable()
        {
            return this.variables.Count > 0 ? this.variables[0] : (0, 0);
        }

        public List<(int, int)> getVariables()
        {
            return this.variables;
        }

        public List<int> getVariableIds()
        {
            return this.variables.ConvertAll(c => c.Item1);
        }

        public List<int> getConditionsExcludeActionSensor()
        {
            return this.conditions.FindAll(c => !owner[c.Item1].IsActionSensor())
                .ConvertAll(c => c.Item1);
        }

        public List<int> getActionSensorsConditions()
        {
            return this.conditions.FindAll(c => owner[c.Item1].IsActionSensor())
                .ConvertAll(c => c.Item1);
        }

        /// <summary>
        /// 取得输入基因
        /// </summary>
        /// <returns></returns>
        public override List<NodeGene> getInputGenes()
        {
            List<NodeGene> r = new List<NodeGene>();
            if(this.conditions.Count>0)
                r.AddRange(this.conditions.ConvertAll(c=>c.Item1).ConvertAll(c=>owner[c]));
            if(this.variables.Count>0)
                r.AddRange(this.variables.ConvertAll(c => c.Item1).ConvertAll(c => owner[c]));
            return r;
        }

        /// <summary>
        /// 该基因的条件部分全部包含了gene的条件部分
        /// </summary>
        /// <param name="gene"></param>
        /// <returns></returns>
        public bool contains(InferenceGene gene)
        {
            if (gene == null) return false;
            foreach ((int, int) small in gene.conditions)
            {
                int index = this.conditions.ConvertAll(d => d.Item1).IndexOf(small.Item1);
                if (index < 0) return false;
                if (this.conditions[index].Item2 != small.Item2) continue;
            }
            return true;
        }

        #endregion

        #region 初始化和显示
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="genome"></param>
        public InferenceGene(NWSEGenome genome) : base(genome)
        {
        }

        public override T clone<T>()
        {
            InferenceGene gene = new InferenceGene(this.owner).copy<InferenceGene>(this);
            gene.conditions.AddRange(this.conditions);
            gene.variables.AddRange(this.variables);
            return (T)(Object)gene;
        }
        
        /// <summary>
        /// 显示文本
        /// </summary>
        public override String Text
        {
            get
            {
                String s1 = this.getConditions()
                    .ConvertAll(c => c.Item1)
                    .ConvertAll(id => owner[id])
                    .ConvertAll(g => g.Text)
                    .Aggregate((x, y) => x + "," + y);
                String s2 = this.getVariables().ConvertAll(c => c.Item1)
                    .ConvertAll(id => owner[id])
                    .ConvertAll(g => g.Text)
                    .Aggregate((x, y) => x + "," + y);
                return s1 + (s2 == null || s2.Trim() == "" ? "" : "=>" + s2);
            }
        }
        #endregion

        #region 读写
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

        #endregion

    }
}
