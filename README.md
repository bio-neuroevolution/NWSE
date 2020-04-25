Code for the paper "A Non-Weight Structural Evolution Neural Network for Knowledge Reasoning".

### Enviornment Requirment
  Net Framework4.7 is needed to run NWSE project and Visual studio2019 is needed to view source code.
  
### Experiment
  Run NWSEEXperiment.exe, click “Evolution：Run” in the toolbar. You can observe the evolution process in the evolution panel tab (on the right side of the window). The evolution tree and the optima individuals of every generation are stored in session directory. The name of the session directory starts with "session" and ends with beginning time, such as "session20200101120000".
  Copy all optima individuals in the session directory to path "genome", restart NWSEEXperiment.exe and click "evaluation" in the toolbar. The log file of evaluationis generated in the same directory of exe, starting with "session...".
  
### How to compare with the methods of paper "Learning Behavior Characterizations for Novelty Search"
  The source code of mazes is come from "Learning Behavior Characterizations for Novelty Search", and have been modified to suit our experiment. To complete the comparasion you need to run the modifiyed version(in path "orgin"), and click "Simulation-> performanceEvaulation" in menubar. The results would be stored in session log file.
  
  
  




