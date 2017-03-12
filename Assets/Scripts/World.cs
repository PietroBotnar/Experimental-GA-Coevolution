using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    /// <summary>
    /// Controls object initialisation and execution within the space world
    /// </summary>
    public class World : MonoBehaviour
    {
        //game object prefabs
        public GameObject TileTemplate;
        public GameObject PacmanGO;
        public GameObject GhostGO;
        public GameObject FruitGO;

        public Transform Agents;

        public Transform LeftPanel;
        public Transform RightPanel;

        public Transform ExecutionLogPanel;

        public Text LineTemplate;

        public Text FruitsCollectedText;
        public Text ActionsText;
        public Text GhostActionsText;

        public Text PropertiesText;
        public Text GenerationText;

        public Maze Maze;

        public List<Agent> Ghosts = new List<Agent>();

        public List<GameObject> FruitGos = new List<GameObject>(); 
        public Dictionary<Location, GameObject> Fruits = new Dictionary<Location, GameObject>();

        public Agent Pacman;
        public Agent Ghost;

        int _fruitsCollected = 0;
        public int TotFruits = 4;

        private bool _isPlaying = false;

        public int _pacmanGeneIndex = -1;
        public int _ghostGeneIndex = -1;

        private Settings _settings;

        private bool _skipping = false;

        // Use this for initialization
        void Start()
        {
            _settings = IOHandler.ParseSettings();

            SetSettingsText();
            SetGenerationText(0);

            LogExection("Initialise environment");

            LogExection(string.Format("Initial population: {0} - Chromosomes per gene: {1}", _settings.Population, _settings.GeneSize));

            Maze.InitMaze(this, _settings.RandomFruitPositions);

            Pacman.SetAlgorithm(new GeneticAlgorithm(_settings, AgentType.Fruit, AgentType.Ghost));

            Ghost = Ghosts[0];
            
            Ghost.SetAlgorithm(new GeneticAlgorithm(_settings, AgentType.Pacman, AgentType.None));

            SetNextGenes();

            _fruitsCollected = 0;
            SetFruitsCollected(0);

            IOHandler.DeleteOldLogs();

            LogExection("Start population fitness measurement");
        }

        // Update is called once per frame
        void Update()
        {
            if (_isPlaying && _settings.Visual && !_skipping)
            {
                PlayGame();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                _skipping = true;
                RunFast(5);
            }
        }

        void PlayGame()
        {
            if (!PlayAgent(Pacman) || !PlayAgent(Ghost))
            {
                _isPlaying = false; 
                
                Reset();
            }
        }

        bool PlayAgent(Agent agent)
        {
            if (agent.ActionsExecuted == 10000)
                return false;

            if (agent.CurrentAction != null)
            {
                Execute(agent, agent.CurrentAction);
            }
            else if (!agent.NextAction())
            {
                //no more actions
                return false;
            }
            return true;
        }

        /// <summary>
        /// Play/Stop button listener
        /// </summary>
        public void Play(bool value)
        {
            LogExection(string.Format("Execution {0}", value ? "started" : "stopped"));
            _isPlaying = value;

            while (_isPlaying && !_settings.Visual && Pacman.Evolutions <= _settings.Evolutions)
            {
                PlayGame();
            }
        }

        void RunFast(int gens)
        {
            _isPlaying = true;

            var stopAt = Pacman.Evolutions + gens;
            while (_isPlaying && Pacman.Evolutions <= stopAt)
            {
                PlayGame();
            }

            _isPlaying = false;
            _skipping = false;
        }

        /// <summary>
        /// Prints current Gene values and resets the environment with new genes selected
        /// </summary>
        public void Reset()
        {
            AddLine(AgentType.Pacman, string.Format("\nGeneration: {3}/{0} Actions: {2} Fitness: {1}", _pacmanGeneIndex, GetFitnessValue(Pacman), Pacman.GetActionCount(), Pacman.Evolutions), true);
            AddLine(AgentType.Ghost, string.Format("\nGeneration: {3}/{0} Actions: {2} Fitness: {1}", _ghostGeneIndex, GetFitnessValue(Ghost), Ghost.GetActionCount(), Ghost.Evolutions), true);
            
            Maze.MoveAgent(Pacman, Pacman.Reset());
            Maze.MoveAgent(Ghost, Ghost.Reset());

            Maze.ReplaceFruits(_settings.RandomFruitPositions);

            SetNextGenes();

            _fruitsCollected = 0;
            SetFruitsCollected(0);

            SetActionsText(AgentType.Ghost, 0);
            SetActionsText(AgentType.Pacman, 0);

            _isPlaying = true;
        }

        /// <summary>
        /// Sets next genes from current population, otherwise evolves the agents
        /// </summary>
        public void SetNextGenes()
        {
            _pacmanGeneIndex++;
            _ghostGeneIndex++;

            if (_pacmanGeneIndex >= Pacman.GeneticAlgorithm.Population.Count)
            {
                EvolveAgent(Pacman);
                _pacmanGeneIndex = 0;

                SetGenerationText(Pacman.Evolutions);
            }

            if (_ghostGeneIndex >= Ghost.GeneticAlgorithm.Population.Count)
            {
                EvolveAgent(Ghost);
                _ghostGeneIndex = 0;
            }

            Pacman.Moves = 0;
            Ghost.Moves = 0;
            var pacG = Pacman.SetGene(_pacmanGeneIndex);
            var ghostG = Ghost.SetGene(_ghostGeneIndex);

            //AddLine(AgentType.Pacman, pacG);
            //AddLine(AgentType.Ghost, ghostG);
        }

        bool CheckPacmanCollision()
        {
            var location = Pacman.Location;

            if (Fruits.ContainsKey(location))
            {
                _fruitsCollected++;
                SetFruitsCollected(_fruitsCollected);

                Fruits[location].SetActive(false);

                Fruits.Remove(location);
            }

            if (Ghost.Location.Equals(location) || _fruitsCollected == TotFruits)
            {
                //game over
                return false;
            }

            return true;
        }

        /// <summary>
        ///  Moves the agent forward on a given direction
        /// </summary>
        public bool MoveAgent(Agent agent, Direction direction)
        {
            var agentLocation = agent.Location;
            var nextLocation = Maze.GetLocationTowards(agentLocation, direction);

            if (nextLocation == null)
                return false;

            Maze.MoveAgent(agent, nextLocation);

            //update how close the ghost gets to the pacman
            if (agent.Type == AgentType.Ghost)
            {
                agent.Moves++;
                
                var distanceToGoal = agent.Location.DistanceTo(Pacman.Location);

                agent.Gene.FitnessValue += distanceToGoal;
            }

            if (!CheckPacmanCollision())
            {
                _isPlaying = false;
                Reset();
            }

            return true;
        }

        /// <summary>
        /// Execute an action on an agent
        /// </summary>
        public void Execute(Agent agent, Action action)
        {
            if (action == null)
                return;

            var executed = false;
            
            switch (action.Type)
            {
                case ActionType.Move:
                    executed |= MoveAgent(agent, agent.Direction);
                    break;
                case ActionType.Turn:
                    var turnAction = action as TurnAction;
                    var turnDirection = agent.Direction.GetRelative(turnAction.GetDirection());

                    if (Maze.CanGoInDirection(agent.Location, turnDirection))
                    {
                        agent.Direction = turnDirection;
                        executed = true;
                    }
                    break;
                case ActionType.Look:
                    var lookAction = action as LookAction;
                    var reaction = lookAction.Reaction(agent.Direction, Maze.Look(agent.Location));

                    agent.Direction = Maze.CanGoInDirection(agent.Location, reaction)
                            ? reaction
                            : Maze.GetAvailableLocationDirections(agent.Location)
                                .First(location => location.Direction != agent.Direction)
                                .Direction;

                    executed = true;
                    break;
            }
            agent.CurrentAction = null;

            if(executed)
                agent.ActionsExecuted++;

            SetActionsText(agent.Type, agent.GetActionCount());
        }

        /// <summary>
        /// Evolves the agent using EliteSelection - Uniform UniformCrossover - Mutation
        /// </summary>
        void EvolveAgent(Agent agent)
        {
            LogExection(string.Format("Gen {0} average fitness: {1}", agent.Evolutions, agent.GeneticAlgorithm.CalculateAverageFitness()));

            IOHandler.LogAverage(agent);

            LogExection(string.Format("Evolving agent: {0}", agent.Type));

            agent.GeneticAlgorithm.Evolve(agent.Type == AgentType.Pacman);

            var evolution = agent.Evolutions++;
            LogExection(string.Format("Evolution {0} complete", evolution));

            switch(agent.Type)
            {
                case AgentType.Pacman:
                ClearPanel(LeftPanel);
                    break;

                case AgentType.Ghost:
                ClearPanel(RightPanel);
                    break;
            }
        }

        /// <summary>
        /// Logs to the specific agent panel
        /// </summary>
        void AddLine(AgentType agent, string info, bool highlight = false)
        {
            if(!_settings.Logs)
                return;

            var newLine = Instantiate(LineTemplate);
            newLine.text = info;

            switch (agent)
            {
                case AgentType.Pacman:
                    newLine.transform.SetParent(LeftPanel);
                    break;
                case AgentType.Ghost:
                    newLine.transform.SetParent(RightPanel);
                    break;
            }

            if (highlight)
                newLine.color = Color.red;

            newLine.transform.SetAsFirstSibling();
            newLine.gameObject.SetActive(true);
        }

        void ClearPanel(Transform panel)
        {
            for (int i = panel.childCount-1; i >= 0; i--)
            {
                Destroy(panel.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Logs to the main log panel
        /// </summary>
        void LogExection(string text)
        {
            var newLine = Instantiate(LineTemplate);
            newLine.text = text;

            newLine.transform.SetParent(ExecutionLogPanel);
            newLine.gameObject.SetActive(true);
        }

        /// <summary>
        /// Sets fruits collected label value
        /// </summary>
        void SetFruitsCollected(int value)
        {
            FruitsCollectedText.text = string.Format("Fruits collected: {0}/{1}", value, TotFruits);
        }

        /// <summary>
        /// Sets the action count value for the specific agent
        /// </summary>
        void SetActionsText(AgentType agent, int value)
        {
            switch (agent)
            {
                case AgentType.Pacman:
                    ActionsText.text = string.Format("Actions: {0}", value);
                    break;
                case AgentType.Ghost:
                    GhostActionsText.text = string.Format("Actions: {0}", value);
                    break;
            }
        }

        void SetSettingsText()
        {
            PropertiesText.text = string.Format("Popolation size:{0} \t Gene size:{1} \t Mutation:{2}",
                _settings.Population, _settings.GeneSize, _settings.Mutation ? "ON" : "OFF");
        }

        void SetGenerationText(int value)
        {
            GenerationText.text = string.Format("Generation: {0}", value);
        }

        /// <summary>
        /// Calculates the fitness value of the given agent
        /// </summary>
        float GetFitnessValue(Agent agent)
        {
            agent.Gene.ActionsExecuted = agent.ActionsExecuted;

            switch (agent.Type)
            {
                case AgentType.Pacman:
                    var objectiveAchievement = ((float) _fruitsCollected*1000/TotFruits) / agent.ActionsExecuted;
                    agent.Gene.FitnessValue = objectiveAchievement;
                    return objectiveAchievement;

                case AgentType.Ghost:
                    //average distance to pacman
                    var fitness = agent.Gene.FitnessValue / agent.Moves;
                    //percentage in relation to longest distance possible
                    var result = (17-fitness)*1000/17;
                    result /= agent.ActionsExecuted;
                    agent.Gene.FitnessValue = result;
                    return result;
            }
            return -1;
        }

    }
}
