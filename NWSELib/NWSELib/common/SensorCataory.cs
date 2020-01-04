using System;
using System.Collections.Generic;

namespace NWSELib.genome
{
    /// <summary>
    /// 感知器类型
    /// type of sensor(perception)
    /// </summary>
    public enum SensorType
    {
        /// <summary>
        /// 任意 any
        /// </summary>
        Any = 0,
        /// <summary>
        /// 环境
        /// enviorment
        /// </summary>
        Env = 1,
        /// <summary>
        /// 自身身体姿态
        /// The perception of agent's own posture
        /// </summary>
        Body,
        /// <summary>
        /// 自身行为
        /// The perception of agent's own behaviour
        /// </summary>
        Action,

    }
    /// <summary>
    /// 感知信息分类
    /// cataory of sensor
    /// We categorize perception to avoid irrational processing in evolution, 
    /// such as calculating the average speed and direction of an object, 
    /// that would not occur in real creature.
    /// The classification is based on physical units such as velocity, angular velocity, l
    /// ength, area, etc.
    /// </summary>
    public class SensorCataory
    {
        #region 基本信息
        /// <summary>
        /// 类型
        /// type of sensor
        /// <see cref="SensorType"/>
        /// </summary>
        private SensorType sensorType;
        /// <summary>
        /// 名称
        /// name, such as velocity, length, etc.
        /// </summary>
        private String name;
        /// <summary>
        /// 单位
        /// physical unit
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
            Unit = "cm"
        };

        /// <summary>
        /// 方向感知
        /// </summary>
        public readonly static SensorCataory DIRECTION = new SensorCataory()
        {
            SensorType = SensorType.Any,
            name = "direction",
            Unit = "rad"
        };

        /// <summary>
        /// 所有感知类别
        /// </summary>
        public readonly static List<SensorCataory> Instances = new List<SensorCataory>(new SensorCataory[] { DISTANCE, DIRECTION });

        #endregion



    }
}
