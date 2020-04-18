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
        /// 推断的前后时间差
        /// </summary>
        public readonly int timediff;
        /// <summary>
        /// 推断各维的前提条件节点ID或者名称,以及时间项
        /// </summary>
        public readonly List<int> conditions = new List<int>();

        /// <summary>
        /// 推断各维的后置变量节点ID或者名称,以及时间项
        /// </summary>
        public readonly List<int> variables = new List<int>();
        

        
        
        /// <summary>
        /// 后置变量数
        /// </summary>
        public int VariableCount { get => this.variables.Count; }

        /// <summary>
        /// 前置条件数
        /// </summary>
        public int ConditionCount { get => this.conditions.Count; }


        /// <summary>
        /// 动作条件
        /// </summary>
        public List<int> ActionConditions
        {
            get
            {
                return this.conditions.FindAll(c => owner[c].IsActionSensor());
            }
        }

        /// <summary>
        /// 取得输入基因
        /// </summary>
        /// <returns></returns>
        public override List<NodeGene> GetInputGenes()
        {
            List<NodeGene> r = new List<NodeGene>();
            if(this.conditions.Count>0)
                r.AddRange(this.conditions.ConvertAll(c=>owner[c]));
            if(this.variables.Count>0)
                r.AddRange(this.variables.ConvertAll(c => owner[c]));
            return r;
        }

        public override List<int> Dimensions
        {
            get
            {
                List<int> r = new List<int>(this.conditions);
                r.AddRange(this.variables);
                return r;
            }
        }


        #endregion

        #region 初始化和显示
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="genome"></param>
        public InferenceGene(NWSEGenome genome,int timediff,List<int> conditions,List<int> variables) : base(genome)
        {
            this.timediff = timediff;
            this.conditions.AddRange(conditions);
            this.variables.AddRange(variables);
            this.conditions.Sort();
            this.variables.Sort();
        }

        public void Sortdimension()
        {
            this.conditions.Sort();
            this.variables.Sort();
        }

        public override T clone<T>()
        {
            InferenceGene gene = new InferenceGene(this.owner,this.timediff,this.conditions,this.variables).copy<InferenceGene>(this);
            
            return (T)(Object)gene;
        }

        
        
        /// <summary>
        /// 显示文本
        /// </summary>
        public override String Text
        {
            get
            {
                String s1 = this.conditions
                    .ConvertAll(id => owner[id])
                    .ConvertAll(g => g.Text)
                    .Aggregate((x, y) => x + "," + y);
                String s2 = this.variables
                    .ConvertAll(id => owner[id])
                    .ConvertAll(g => g.Text)
                    .Aggregate((x, y) => x + "," + y);
                return s1 + "[t-"+timediff.ToString()+"]" + (timediff==0?"<=>":"=>") + s2 + "[t]";
            }
        }

        
        #endregion

        #region 读写
        public override string ToString()
        {
            String condstr = this.conditions.ConvertAll(c => c.ToString()).Aggregate((x, y) => x + "," + y);
            String varstr = this.variables.ConvertAll(c => c.ToString()).Aggregate((x, y) => x + "," + y);
            String dimensions = condstr + (timediff == 0 ? "<=>" : "=>") + varstr;
            return "InferenceGene:" + Text + ";dimensions:"+ dimensions+";timediff:"+timediff.ToString()+";info:" + base.ToString() + ";param:";
        }
        public static InferenceGene parse(NWSEGenome genome,String s)
        {
            int t1 = s.IndexOf("InferenceGene:")+ "InferenceGene:".Length;
            int t2 = s.IndexOf(";dimensions:") - 1;
            String text = s.Substring(t1, t2 - t1 + 1).Trim();

            t2 += ";dimensions:".Length+1;
            int t3 = s.IndexOf(";timediff:") -1;
            String dimensions = s.Substring(t2, t3 - t2 + 1);

            t3 += ";timediff:".Length;
            int t4 = s.IndexOf(";info:") - 1;
            String timediff = s.Substring(t3, t4 - t3 + 1);

            t4 += ";info:".Length;
            int t5 = s.IndexOf(";param:") -1;
            String info = s.Substring(t4, t5 - t4 + 1);

            

            //解析dimensions
            t5 = dimensions.IndexOf("=>");
            String condstr = dimensions.Substring(0, t5).Trim();
            if (condstr.EndsWith("<")) condstr = condstr.Substring(0, condstr.Length - 1);
            String varstr = dimensions.Substring(t5 + 2);
            List<int> conditions = condstr.Split(',').ToList().ConvertAll(str => int.Parse(str));
            List<int> variables = varstr.Split(',').ToList().ConvertAll(str => int.Parse(str));



            
            //解析info
            InferenceGene gene = new InferenceGene(genome,int.Parse(timediff),conditions,variables);
            gene.parseInfo(info);

            return gene;
        }

        #endregion

    }
}
