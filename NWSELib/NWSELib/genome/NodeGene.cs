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


        /// <summary>
        /// 分段数
        /// </summary>
        protected int sectionCount;


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

        /// <summary>
        /// 每层的分段数
        /// </summary>
        public int SectionCount { get => sectionCount; set => sectionCount = value; }

        

        #endregion

        #region 字符串转换
        public override string ToString()
        {
            return this.GetType().Name + ": id=" + this.id.ToString() + ",name=" + this.name +
                ",generation=" + this.generation + ",cataory=" + this.cataory + ",group=" + this.group +
                ",sectionCount=" + this.sectionCount.ToString();
        }
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
