using NWSELib.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace NWSELib
{
    [XmlRoot(ElementName = "configuration")]
    public class Configuration
    {
        [XmlElement(ElementName = "agent")]
        public Agent agent = new Agent();

        public class Agent
        {
            [XmlAttribute]
            public int shorttermcapacity;
            [XmlElement]
            public Receptors receptors = new Receptors();
        }
        public class Receptors
        {
            [XmlArray(ElementName = "env")]
            [XmlArrayItem(Type = typeof(Sensor), ElementName = "sensor")]
            public List<Sensor> env = new List<Sensor>();

            [XmlArray(ElementName = "gestures")]
            [XmlArrayItem(Type = typeof(Sensor), ElementName = "sensor")]
            public List<Sensor> gestures = new List<Sensor>();

            [XmlArray(ElementName = "actions")]
            [XmlArrayItem(Type = typeof(Sensor), ElementName = "sensor")]
            public List<Sensor> actions = new List<Sensor>();

            public Sensor GetEnvSensor(String name)
            {
                return env.FirstOrDefault<Sensor>(s => s.name == name);
            }
            public Sensor GetGestureSensor(String name)
            {
                return gestures.FirstOrDefault<Sensor>(s => s.name == name);
            }
            public Sensor GetActionSensor(String name)
            {
                return actions.FirstOrDefault<Sensor>(s => s.name == name);
            }
            public Sensor GetSensor(String name)
            {
                Sensor r = GetEnvSensor(name);
                if (r == null) r = GetGestureSensor(name);
                if (r == null) r = GetActionSensor(name);
                return r;
            }

        }
        public class Sensor
        {
            [XmlAttribute]
            public String name;
            [XmlAttribute]
            public String cataory;
            [XmlAttribute]
            public String group;
            [XmlAttribute]
            public String range;

            public ValueRange Range { get => null; }
        }
    }
}
