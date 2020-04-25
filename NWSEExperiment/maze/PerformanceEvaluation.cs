using NWSELib;
using NWSELib.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.IO;
using NWSELib.net;

namespace NWSEExperiment.maze
{
    public class PerformanceEvaluation
    {
        private ILog logger = LogManager.GetLogger("PerformanceEvaluation");
        private DirectoryInfo netDirectoryInfo;
        private FileInfo[] netFileInfos;
        private DirectoryInfo mazeDirectionInfo;
        private FileInfo[] mazeFileInfos;

        public void Execute(String networkPaths, String mazePath, NWSELib.EventHandler handler,int runcount = 1200)
        {
            netDirectoryInfo = new DirectoryInfo(networkPaths);
            mazeDirectionInfo = new DirectoryInfo(mazePath);

            netFileInfos = netDirectoryInfo.GetFiles("*.ind");
            mazeFileInfos = mazeDirectionInfo.GetFiles("*.xml");

            for (int i = 0; i < netFileInfos.Length; i++)
            {
                Network net = null;
                List<double> performances = new List<double>();

                for (int j = 0; j < mazeFileInfos.Length; j++)
                {
                    net = Network.Load(netFileInfos[i].FullName);
                    HardMaze env = HardMaze.loadEnvironment(mazeFileInfos[j].FullName);
                    double value = DoEvaluation(net, env, runcount);
                    performances.Add(value);
                }

                String log = netFileInfos[i].Name + "," + net.Id.ToString() + "," + performances.Average().ToString() + "," + Utility.toString(performances);
                int g = int.Parse(DateTime.Now.ToString("HHmmss"));
                handler(Session.EVT_LOG, g, log);
                //logger.Info(log);
            }
        }

        public double DoEvaluation(Network net, HardMaze env, int runcount = 1000)
        {


            Session session = new Session(env,
                new FitnessHandler(env.compute_fitness),
                new NWSELib.EventHandler(doEventHandler),
                new InstinctActionHandler(HardMaze.createInstinctAction),
                new OptimaGestureHandler(env.GetOptimaGestureHandler),
                new TaskBeginHandler(env.TaskBeginHandler));

            (List<double> obs, List<double> gesture) = env.reset(net);
            net.Reset();
            double reward = 0.0;
            bool end = false;

            for (int time = 0; time < runcount; time++)
            {
                List<double> inputs = new List<double>(obs);
                inputs.AddRange(gesture);
                List<double> actions = net.Activate(inputs, time, session, reward);
                (obs, gesture, actions, reward, end) = env.action(net, actions);
                if (end) break;
            }

            net.Fitness = Session.fitnessHandler == null ? 0 : Session.fitnessHandler(net, session);
            net.TaskCompleted = end;

            return net.Fitness;
        }

        public void doEventHandler(String eventName, int generation, params Object[] states)
        {

        }
    }
}
