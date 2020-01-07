using NWSELib.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Reflection;

namespace NWSELib
{
    [XmlRoot(ElementName = "configuration")]
    public class Configuration
    {
        [XmlElement(ElementName = "agent")]
        public Agent agent = new Agent();

        [XmlElement(ElementName ="learning")]
        public Learning learning = new Learning();

        [XmlElement(ElementName = "evaluation")]
        public Evaluation evaluation = new Evaluation();

        [XmlElement(ElementName = "evolution")]
        public Evolution evolution = new Evolution();

        [XmlArray(ElementName ="handlers")]
        [XmlArrayItem(ElementName ="handler",Type =typeof(Handler))]
        public List<Handler> handlers = new List<Handler>();

        [XmlElement(ElementName ="view")]
        public View view = new View();

        public Handler Find(String name)
        {
            return handlers.FirstOrDefault<Handler>(h => h.name == name);
        }

        public Handler random_handler(double[] distribution = null)
        {
            if (distribution == null)
            {
                distribution = new double[handlers.Count];
                for (int i = 0; i < distribution.Length; i++) distribution[i] = 1.0 / handlers.Count;
            }
            int index = new Random().Next(0, handlers.Count);
            return handlers[index];
        }

        

        public class Agent
        {
            [XmlAttribute]
            public int shorttermcapacity;
            [XmlAttribute]
            public int inferencesamples;
            [XmlElement]
            public Receptors receptors = new Receptors();
            [XmlElement]
            public Noise noise;
            
            
        }

        public class Noise
        {
            [XmlAttribute]
            public double sensorNoise;
            [XmlAttribute]
            public double effectorNoise;
            [XmlAttribute]
            public double headingNoise;
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

            public List<Sensor> GetAllSensor()
            {
                List<Sensor> r = new List<Sensor>();
                r.AddRange(env);
                r.AddRange(gestures);
                r.AddRange(actions);
                return r;
            }
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
            public String desc;
            [XmlAttribute]
            public String cataory;
            [XmlAttribute]
            public String group;
            [XmlAttribute]
            public String range;
            [XmlAttribute]
            public String level;

            [XmlIgnore]
            private ValueRange _range;
            [XmlIgnore]
            private ValueRange _level;

            [XmlIgnore]
            public ValueRange Range { get => _range==null?_range=new ValueRange(range):_range; }
            [XmlIgnore]
            public ValueRange Level { get => _level==null?_level=new ValueRange(level):_level; }
        }

        public class Learning
        {
            [XmlElement]
            public LearningInfernece inference = new LearningInfernece();
        }
        public class LearningInfernece
        {
            [XmlAttribute]
            public double accept_prob;
            [XmlAttribute]
            public int accept_max_count;
        }

        
        public class Evaluation
        {
            [XmlAttribute]
            public double timeStep;

            [XmlAttribute(AttributeName = "gene_reability_range")]
            public String gene_reability_range_str;
            [XmlIgnore]
            public ValueRange gene_reability_range
            {
                get => new ValueRange(gene_reability_range_str);
            }
            [XmlAttribute]
            public int run_count;
            [XmlAttribute]
            public int max_reward;
        }
        public class Evolution
        {
            [XmlAttribute]
            public int propagate_base_count;
            [XmlAttribute]
            public int iter_count;

            [XmlElement]
            public EvolutionSelection selection = new EvolutionSelection();

            [XmlElement]
            public EvolutionMutate mutate = new EvolutionMutate();

        }
        public class EvolutionSelection
        {
            [XmlAttribute]
            public double reability_lowlimit;
        }
     
        public class EvolutionMutate
        {
            [XmlAttribute]
            public String handlerprob;
            [XmlIgnore]
            private List<double> _handlerprob;
            [XmlIgnore]
            public List<double> Handlerprob 
            {
                get => _handlerprob == null ? _handlerprob = Utility.parse(handlerprob) : _handlerprob;
            }
        }

        public class Handler
        {
            [XmlAttribute]
            public String name;
            [XmlAttribute]
            public int paramCount;
            [XmlAttribute]
            public String paramRange;
            [XmlIgnore]
            private List<ValueRange> _paramRange;
            [XmlIgnore]
            public List<ValueRange> ParamRange
            {
                get
                {
                    if (_paramRange != null) return _paramRange;
                    if (paramRange == null || paramRange.Trim() == "") return _paramRange;
                    String[] s1 = paramRange.Trim().Split(';');
                    if (s1 == null || s1.Length <= 0) return null;
                    _paramRange = new List<ValueRange>();
                    for (int i=0;i<s1.Length;i++)
                    {
                        _paramRange.Add(new ValueRange(s1[i].Trim()));
                    }
                    return _paramRange;
                }
            }
            [XmlAttribute]
            public String typename;
            [XmlIgnore]
            public Type HandlerType { get => Type.GetType(typename); }
            [XmlAttribute]
            public String selection_prob_range;

            public ValueRange Selection_prob_range
            {
                get => new ValueRange(selection_prob_range);
            }
            public double[] randomParam()
            {
                double[] r = new double[this.paramCount];
                for(int i=0;i<r.Length;i++)
                {
                    r[i] = ParamRange[i].random();
                }
                return r;
            }


        }

        public class View
        {
            [XmlAttribute]
            public int width;
            [XmlAttribute]
            public int height;
        }

    }
}
