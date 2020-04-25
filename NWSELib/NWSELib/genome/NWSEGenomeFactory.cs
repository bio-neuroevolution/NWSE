using NWSELib.net.handler;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NWSELib.genome
{
    /// <summary>
    /// 染色体工厂
    /// </summary>
    public class NWSEGenomeFactory
    {
        private List<ReceptorGene> createReceptors(NWSEGenome genome,Session session)
        {
            
            List<Configuration.Sensor> sensors = Session.GetConfiguration().agent.receptors.GetAllSensor();
            for (int i = 0; i < sensors.Count; i++)
            {
                ReceptorGene receptorGene = new ReceptorGene(genome);
                receptorGene.Cataory = sensors[i].cataory;
                receptorGene.Generation = session.Generation;
                receptorGene.Group = sensors[i].group;
                receptorGene.Name = sensors[i].name;
                receptorGene.Description = sensors[i].desc;
                receptorGene.Id = session.GetIdGenerator().getGeneId(receptorGene);
                genome.receptorGenes.Add(receptorGene);
            }
            return genome.receptorGenes;

        }
        public NWSEGenome createOriginGenome(Session session)
        {
            NWSEGenome genome = new NWSEGenome();
            //生成感受器
            this.createReceptors(genome, session);


            //生成缺省推理节点
            int[] varids = { genome["d1"].Id, genome["d2"].Id,
                             genome["d3"].Id, genome["d4"].Id,
                             genome["d5"].Id, genome["d6"].Id,
                             genome["b"].Id,
                             genome["heading"].Id};
            for (int i = 0; i < varids.Length; i++)
            {
                List<int> conditions = new List<int>();
                conditions.Add(genome["_a2"].Id);
                List<int> variables = new List<int>();
                variables.Add(varids[i]);
                InferenceGene inferenceGene = new InferenceGene(genome,1,conditions, variables);
                inferenceGene.Generation = session.Generation;
                inferenceGene.Id = Session.idGenerator.getGeneId(inferenceGene);
                genome.inferenceGenes.Add(inferenceGene);
            }

            genome.id = Session.idGenerator.getGenomeId();
            genome.generation = 1;
            genome.computeNodeDepth();
            return genome;
        }
        public NWSEGenome createSimpleGenome(Session session)
        {
            NWSEGenome genome = new NWSEGenome();
            //生成感受器
            this.createReceptors(genome,session);


            //生成缺省推理节点
            List<int> conditions = new List<int>();
            List<int> variables = new List<int>();
            conditions.Add(genome["heading"].Id);
            conditions.Add(genome["_a2"].Id);
            variables.Add(genome["heading"].Id);
            InferenceGene inferenceGene = new InferenceGene(genome,1,conditions,variables);
            inferenceGene.Generation = session.Generation;
            inferenceGene.Id = Session.idGenerator.getGeneId(inferenceGene);
            genome.inferenceGenes.Add(inferenceGene);

            genome.id = Session.idGenerator.getGenomeId();
            genome.generation = 1;
            genome.computeNodeDepth();
            return genome;
        }

        public NWSEGenome createReabilityGenome(Session session)
        {
            NWSEGenome genome = new NWSEGenome();
            //生成感受器
            this.createReceptors(genome, session);

            InferenceGene inferenceGene = null;
            //生成推理节点:1 
            List<int> conditions = new List<int>();
            List<int> variables = new List<int>();
            conditions.Add(genome["heading"].Id);
            conditions.Add(genome["_a2"].Id);
            variables.Add(genome["heading"].Id);
            inferenceGene = new InferenceGene(genome,1,conditions,variables);
            inferenceGene.Generation = session.Generation;
            inferenceGene.Id = Session.idGenerator.getGeneId(inferenceGene);
            genome.inferenceGenes.Add(inferenceGene);

            
            

            //生成推理节点:4
            int[] ids = new int[] { genome["d1"].Id, genome["d2"].Id,
                                    genome["d3"].Id, genome["d4"].Id,
                                    genome["d5"].Id, genome["d6"].Id
                                   };
            foreach (int did in ids)
            {
                conditions = new List<int>();
                variables = new List<int>();
                conditions.Add(did);
               // conditions.Add(genome["heading"].Id);
                conditions.Add(genome["_a2"].Id);
                variables.Add(did);
                inferenceGene = new InferenceGene(genome,1,conditions,variables);
                inferenceGene.Generation = session.Generation;
                inferenceGene.Id = Session.idGenerator.getGeneId(inferenceGene);
                genome.inferenceGenes.Add(inferenceGene);
            }

            inferenceGene = new InferenceGene(genome, 1,
                new int[] { genome["d3"].Id, genome["_a2"].Id }.ToList(),
                new int[] { genome["b"].Id }.ToList());
            inferenceGene.Generation = session.Generation;
            inferenceGene.Id = Session.idGenerator.getGeneId(inferenceGene);
            genome.inferenceGenes.Add(inferenceGene);

            //生成推理节点:4
            /*ids = new int[] { genome["g1"].Id, genome["gd"].Id};
            foreach (int did in ids)
            {
                conditions = new List<int>();
                variables = new List<int>();
                conditions.Add(genome["g1"].Id);
                conditions.Add(genome["gd"].Id);
                conditions.Add(genome["heading"].Id);
                conditions.Add(genome["_a2"].Id);
                variables.Add(did);
                inferenceGene = new InferenceGene(genome,1,conditions,variables);
                inferenceGene.Generation = session.Generation;
                inferenceGene.Id = Session.idGenerator.getGeneId(inferenceGene);
                genome.inferenceGenes.Add(inferenceGene);
            }*/

            genome.id = Session.idGenerator.getGenomeId();
            genome.generation = 1;
            genome.computeNodeDepth();
            return genome;
        }
        public NWSEGenome createFullGenome(Session session)
        {
            NWSEGenome genome = new NWSEGenome();
            //生成感受器
            this.createReceptors(genome, session);

            InferenceGene inferenceGene = null;
            //生成推理节点:1 
            List<int> conditions = new List<int>();
            List<int> variables = new List<int>();
            conditions.Add(genome["heading"].Id);
            conditions.Add(genome["_a2"].Id);
            variables.Add(genome["heading"].Id);
            inferenceGene = new InferenceGene(genome,1,conditions,variables);
            inferenceGene.Generation = session.Generation;
            inferenceGene.Id = Session.idGenerator.getGeneId(inferenceGene);
            genome.inferenceGenes.Add(inferenceGene);

            conditions = new List<int>();
            conditions.Add(genome["d1"].Id);
            conditions.Add(genome["d2"].Id);
            conditions.Add(genome["d3"].Id);
            conditions.Add(genome["d4"].Id);
            conditions.Add(genome["d5"].Id);
            conditions.Add(genome["d6"].Id);
            conditions.Add(genome["heading"].Id);
            conditions.Add(genome["_a2"].Id);

            int[] varIds = { genome["d1"].Id, genome["d2"].Id,
                             genome["d3"].Id, genome["d4"].Id,
                             genome["d5"].Id, genome["d6"].Id,
                             //["g1"].Id, genome["gd"].Id,
                             //genome["g3"].Id, genome["g4"].Id,
                             //genome["b"].Id,genome["heading"].Id

            };
            
            //生成推理节点
            for (int i = 0; i < varIds.Length; i++)
            {
                inferenceGene = new InferenceGene(genome,1,new List<int>(conditions),new int[] { varIds[i]}.ToList());
                inferenceGene.Generation = session.Generation;
                inferenceGene.Id = Session.idGenerator.getGeneId(inferenceGene);
                genome.inferenceGenes.Add(inferenceGene);
            }

            inferenceGene = new InferenceGene(genome, 1, 
                new int[] { genome["d3"].Id, genome["heading"].Id, genome["_a2"].Id }.ToList(), 
                new int[] { genome["b"].Id }.ToList());
            inferenceGene.Generation = session.Generation;
            inferenceGene.Id = Session.idGenerator.getGeneId(inferenceGene);
            genome.inferenceGenes.Add(inferenceGene);


            genome.id = Session.idGenerator.getGenomeId();
            genome.generation = 1;
            genome.computeNodeDepth();
            return genome;
        }
        
    }
}
