using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NWSELib.genome
{
    public class EffectorGene : NodeGene
    {
        /// <summary>
        /// 所属分组
        /// </summary>
        protected string group;
        /// <summary>
        /// 所属分组
        /// </summary>
        public String Group
        {
            get => this.group;
        }
    }
}