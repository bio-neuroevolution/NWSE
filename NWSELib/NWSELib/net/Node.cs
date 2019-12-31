using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Text;

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
            
        /// <summary>当前值</summary>
        protected Object curValue;
        /// <summary>当前时间</summary>
        protected int curTime;

        public String Name
        {
            get => gene.Name;
        }
        public Object Value
        {
            get { return this.curValue; }
        }
        public int CurTime
        {
            get { return this.curTime; }
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
        public virtual Object activate(int time,Object value=null)
        {
            if (this.curTime >= 0) return this.curValue;
            Object prev = this.curValue;
            this.curValue = value;
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
