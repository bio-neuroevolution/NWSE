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

        [XmlElement(ElementName = "evolution")]
        public Evolution evolution = new Evolution();


        public class Agent
        {
            [XmlAttribute]
            public int shorttermcapacity;
            [XmlAttribute]
            public int inferencesamples;
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
    
        public class Evolution
        {
            [XmlAttribute]
            public int propagate_base_count;
            [XmlAttribute(AttributeName = "inference_reability_range")]
            public String inference_reability_range_str;
            public ValueRange Inference_reability_range
            {
                get => new ValueRange(inference_reability_range_str);
            }
        }
    }
}
