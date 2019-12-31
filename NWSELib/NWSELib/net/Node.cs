using System;
using NWSELib.common;
using NWSELib.genome;

namespace NWSELib.net
{
    /// <summary>
    /// 网络节点
    /// </summary>
    public abstract class Node
    {
        #region 基本信息

        /// <summary>节点基因</summary>
        protected NodeGene gene;

        /// <summary>节点Id</summary>
        public int Id { get => this.gene.Id; }
        public String Name
        {
            get => gene.Name;
        }
        public string Cataory
        {
            get => gene.Cataory;
        }
        public String Group
        {
            get => gene.Group;
        }
        public NodeGene Gene
        {
            get => this.gene;
        }
        #endregion

        #region 状态信息
        /// <summary>当前值</summary>
        protected Vector curValue;
        /// <summary>当前时间</summary>
        protected int curTime;

        public Vector Value
        {
            get { return this.curValue; }
        }
        public int CurTime
        {
            get { return this.curTime; }
        }

        public Node(NodeGene gene)
        {
            this.gene = gene;
        }
        #endregion

        #region 评价信息
        /** 可靠度 */
        protected double realiability;
        #endregion

        #region 激活与重置
        /// <summary>
        /// 设置当前值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public virtual Object activate(Network net, int time, Object value = null)
        {
            if (this.curTime >= 0) return this.curValue;
            Object prev = this.curValue;
            this.curValue = (Vector)value;
            this.curTime = time;
            return prev;
        }
        /// <summary>
        /// 重置计算
        /// </summary>
        public void Reset()
        {
            this.curValue = 0;
            this.curTime = -1;
        }
        /// <summary>
        /// 是否已经完成过计算
        /// </summary>
        /// <returns></returns>
        public bool IsActivate()
        {
            return this.curTime >= 0;
        }
        #endregion
    }
}
