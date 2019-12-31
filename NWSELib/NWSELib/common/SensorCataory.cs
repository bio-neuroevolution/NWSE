using System;
using System.Collections.Generic;

namespace NWSELib.genome
{
    /// <summary>
    /// 感知器类型
    /// </summary>
    public enum SensorType
    {
        /// <summary>
        /// 任意
        /// </summary>
        Any = 0,
        /// <summary>
        /// 环境
        /// </summary>
        Env = 1,
        /// <summary>
        /// 自身身体姿态
        /// </summary>
        Body,
        /// <summary>
        /// 自身行为
        /// </summary>
        Action,

    }
    /// <summary>
    /// 感知信息分类
    /// </summary>
    public class SensorCataory
    {
        #region 基本信息
        /// <summary>
        /// 类型
        /// </summary>
        private SensorType sensorType;
        /// <summary>
        /// 名称
        /// </summary>
        private String name;
        /// <summary>
        /// 单位
        /// </summary>
        private String unit;

        /// <summary>
        /// 类型
        /// </summary>
        public SensorType SensorType { get => sensorType; set => sensorType = value; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get => name; set => name = value; }
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get => unit; set => unit = value; }


        #endregion

        #region 常用类型
        /// <summary>
        /// 距离感知
        /// </summary>
        public readonly static SensorCataory DISTANCE = new SensorCataory()
        {
            SensorType = SensorType.Env,
            name = "distance",
            Unit = "m"
        };

        /// <summary>
        /// 方向感知
        /// </summary>
        public readonly static SensorCataory DIRECTION = new SensorCataory()
        {
            SensorType = SensorType.Any,
            name = "direction",
            Unit = "d"
        };

        /// <summary>
        /// 所有感知类别
        /// </summary>
        public readonly static List<SensorCataory> Instances = new List<SensorCataory>(new SensorCataory[] { DISTANCE, DIRECTION });

        #endregion



    }
}
