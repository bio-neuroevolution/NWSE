using System;

namespace NWSELib.genome
{
    public abstract class NodeGene
    {
        #region 基本信息
        /// <summary>
        /// id
        /// </summary>
        protected int id;

        /// <summary>
        /// 名称
        /// </summary>
        protected String name;

        /// <summary>生成的进化年代</summary>
        protected int generation;


        /// <summary>
        /// 所属类别
        /// </summary>
        protected string cataory;

        /// <summary>
        /// 所属分组
        /// </summary>
        protected string group;

        public int Id { get => id; set => id = value; }
        /// <summary>
        /// 对应感受器名称
        /// </summary>
        public string Name { get => name; set => name = value; }
        /// <summary>
        /// 生成的进化年代
        /// </summary>
        public int Generation { get => generation; set => generation = value; }

        /// <summary>
        /// 对应感受器名称
        /// </summary>
        public string Cataory { get => cataory; set => cataory = value; }

        /// <summary>
        /// 所属分组
        /// </summary>
        public String Group
        {
            get => this.group;
        }
        #endregion

        #region 变异信息
        /// <summary>
        /// 分段数
        /// </summary>
        protected int sectionCount;

        /// <summary>
        /// 每层的分段数
        /// </summary>
        public int SectionCount { get => sectionCount; set => sectionCount = value; }

        #endregion

        #region 统计信息       
        /// <summary>
        /// 使用次数
        /// </summary>
        protected int usedCount;
        /// <summary>
        /// 使用次数
        /// </summary>
        public int UsedCount
        {
            get => usedCount; set => usedCount = value;
        }
        
        
        #endregion

        #region 初始化
        /// <summary>
        /// 克隆
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract T clone<T>() where T : NodeGene;
        /// <summary>
        /// 克隆
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gene"></param>
        /// <returns></returns>
        public T copy<T>(NodeGene gene) where T : NodeGene
        {
            this.id = gene.id;
            this.name = gene.name;
            this.cataory = gene.cataory;
            this.generation = gene.generation;
            this.group = gene.group;
            this.sectionCount = gene.sectionCount;
            return (T)this;

        }
        /// <summary>
        /// 转字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.GetType().Name + ": id=" + this.id.ToString() + ",name=" + this.name +
                ",generation=" + this.generation + ",cataory=" + this.cataory + ",group=" + this.group +
                ",sectionCount=" + this.sectionCount.ToString();
        }
        /// <summary>
        /// 字符串转
        /// </summary>
        /// <param name="str"></param>
        public void parse(String str)
        {
            int i1 = str.IndexOf("id");
            int i2 = str.IndexOf("=", i1 + 1);
            int i3 = str.IndexOf(",", i2 + 1);
            String s = str.Substring(i2 + 1, i3 - i2 - 1);
            id = int.Parse(s);

            i1 = str.IndexOf("name");
            i2 = str.IndexOf("=", i1 + 1);
            i3 = str.IndexOf(",", i2 + 1);
            s = str.Substring(i2 + 1, i3 - i2 - 1);
            name = s;

            i1 = str.IndexOf("generation");
            i2 = str.IndexOf("=", i1 + 1);
            i3 = str.IndexOf(",", i2 + 1);
            s = str.Substring(i2 + 1, i3 - i2 - 1);
            generation = int.Parse(s);

            i1 = str.IndexOf("cataory");
            i2 = str.IndexOf("=", i1 + 1);
            i3 = str.IndexOf(",", i2 + 1);
            s = str.Substring(i2 + 1, i3 - i2 - 1);
            cataory = s;

            i1 = str.IndexOf("group");
            i2 = str.IndexOf("=", i1 + 1);
            i3 = str.IndexOf(",", i2 + 1);
            s = str.Substring(i2 + 1, i3 - i2 - 1);
            group = s;

            i1 = str.IndexOf("sectionCount");
            i2 = str.IndexOf("=", i1 + 1);
            i3 = str.IndexOf(",", i2 + 1);
            s = str.Substring(i2 + 1, i3 - i2 - 1);
            sectionCount = int.Parse(s);
        }
        #endregion


    }
}
