using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Assets.Scripts
{
    /// <summary>
    /// Controls the construction and application of a genetic algorith pattern to an agent
    /// </summary>
    public class GeneticAlgorithm
    {
        public List<Gene> Population;

        private AgentType _objectiveAgent;
        private AgentType _escapeAgent;

        private readonly int _populationSize;
        private readonly int _geneSize;
        private readonly bool _useMutation;

        public GeneticAlgorithm(Settings settings, AgentType objectiveAgent, AgentType escapeAgent)
        {
            Population = new List<Gene>();

            _objectiveAgent = objectiveAgent;
            _escapeAgent = escapeAgent;

            _populationSize = settings.Population;
            _useMutation    = settings.Mutation;
            _geneSize       = settings.GeneSize;

            InitPopulation(_populationSize);
        }

        void InitPopulation(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Population.Add(new Gene(_objectiveAgent, _escapeAgent, _geneSize));
            }
        }

        public Gene GetGene(int index)
        {
            return (Population.Count > index) ? Population[index] : null;
        }

        /// <summary>
        /// Returns 2 offsprings using Uniform UniformCrossover, by default using 0.5 ratio
        /// </summary>
        public Gene[] UniformCrossover(Gene parent1, Gene parent2, float crossoverRatio = 0.5f)
        {
            var offspring1 = new List<Action>();
            var offspring2 = new List<Action>();

            for (int chromosome = 0; chromosome < _geneSize; chromosome++)
            {
                var p1 = parent1[chromosome];
                var p2 = parent2[chromosome];

                var swap = Utility.CheckProbability(crossoverRatio);
                if (swap)
                {
                    offspring2.Add(p1);
                    offspring1.Add(p2);
                }
                else
                {
                    offspring1.Add(p1);
                    offspring2.Add(p2);
                }
            }

            var child1 = new Gene(offspring1);
            var child2 = new Gene(offspring2);

            if (!child1.IsValid() || !child2.IsValid())
                return UniformCrossover(parent1, parent2, crossoverRatio);

            return new Gene[]
            {
                child1,
                child2
            };
        }

        /// <summary>
        /// Returns the offspring genes resulting from a single point crossover chosen at random
        /// </summary>
        public Gene[] SinglePointCrossover(Gene parent1, Gene parent2)
        {
            var crossoverPosition = UnityEngine.Random.Range(0, _geneSize);

            var offspring1 = new List<Action>();
            var offspring2 = new List<Action>();

            for (int chromosome = 0; chromosome < _geneSize; chromosome++)
            {
                var p1 = parent1[chromosome];
                var p2 = parent2[chromosome];

                var swap = crossoverPosition >= chromosome;
                if (swap)
                {
                    offspring2.Add(p1);
                    offspring1.Add(p2);
                }
                else
                {
                    offspring1.Add(p1);
                    offspring2.Add(p2);
                }
            }

            var child1 = new Gene(offspring1);
            var child2 = new Gene(offspring2);

            if (!child1.IsValid() || !child2.IsValid())
                return SinglePointCrossover(parent1, parent2);

            return new Gene[]
            {
                child1,
                child2
            };
        }

        /// <summary>
        /// Apply random mutation using a mutation factor, default is 0.01
        /// </summary>
        public Gene Mutate(Gene gene, float mutationFactor = 0.01f)
        {
            var backup = new Gene(gene);

            for (int i = 0; i < gene.Count; i++)
            {
                var mutate = Utility.CheckProbability(mutationFactor);
                if (mutate)
                {
                    var randomType = Utility.GetRandomAction();
                    Action action = null;

                    switch (randomType)
                    {
                        case ActionType.Move:
                        action = new MoveAction();
                            break;
                        case ActionType.Turn:
                        action = new TurnAction();
                            break;
                        case ActionType.Look:
                        action = new LookAction(_objectiveAgent, _escapeAgent);
                            break;
                    }
                    gene[i] = action;
                }
            }

            //discard mutation if not valid
            return !gene.IsValid() ? backup : gene;
        }

        public void ApplyMutation()
        {
            for (int i = 0; i < Population.Count; i++)
            {
                Population[i] = Mutate(Population[i]);
            }
        }

        /// <summary>
        /// Divides the population depending on the proportion of their fitness value
        /// </summary>
        Dictionary<int, List<Gene>> ProportionatePopulation()
        {
            var result = new Dictionary<int, List<Gene>>();
            var sumFitness = (int)Population.Sum(gene => gene.FitnessValue);

            foreach (var gene in Population)
            {
                var chance = (int)(gene.FitnessValue*100/sumFitness);

                if (!result.ContainsKey(chance))
                {
                    result.Add(chance, new List<Gene> {gene});
                }
                else
                {
                    result[chance].Add(gene);
                }
            }

            var refinedValue = new Dictionary<int, List<Gene>>();

            var singleSum = (int)result.Sum(pair => pair.Value[0].FitnessValue);

            foreach (var proportion in result)
            {
                var portion = (int)(proportion.Value[0].FitnessValue *100 / singleSum);

                if (refinedValue.ContainsKey(portion))
                {
                    refinedValue[portion].AddRange(proportion.Value);
                }
                else
                {
                    refinedValue.Add(portion, proportion.Value);
                }
            }

            return refinedValue;
        }

        /// <summary>
        /// Selects a gene using roulette wheel selection
        /// </summary>
        Gene RouletteWheelSelection(int sum, Dictionary<int, List<Gene>> proportionalPopulation, Gene except = null)
        {
                var roll = Utility.GetRandomInt(0, sum);
                var previous = 0;
                foreach (var rank in proportionalPopulation)
                {
                    if (roll >= previous && roll <= rank.Key + previous)
                    {
                        var random = Utility.SelectRandom(rank.Value);
                        return random.Equals(except) ? RouletteWheelSelection(sum, proportionalPopulation, except) : random;
                    }
                    previous += rank.Key;
                }
            return null;
        }

        Gene[] GetGenes(int sum, Dictionary<int, List<Gene>> proportionalPopulation)
        {
            var result = new Gene[2];

            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                {
                    result[i] = RouletteWheelSelection(sum, proportionalPopulation);
                }
                else
                {
                    result[i] = RouletteWheelSelection(sum, proportionalPopulation, result[0]);
                }
            }

            return result;
        }

        /// <summary>
        /// Evolves population using roulette wheel selection and single point crossover, also mantains elitest genes from previous generation
        /// </summary>
        public void Evolve(bool keepBest = true)
        {
            //keep all best genes for next generation
            var newPopulation = new List<Gene>();

            var proportionatePopulation = ProportionatePopulation();

            int sum = proportionatePopulation.Sum(pair => pair.Key);

            var count = (_populationSize - newPopulation.Count)/2;

            //generate new population
            for (int i = 0; i < count + 1; i++)
            {
                var selectedGenes = GetGenes(sum, proportionatePopulation);
                newPopulation.AddRange(SinglePointCrossover(selectedGenes[0], selectedGenes[1]));
            }

            if (newPopulation.Count > _populationSize)
            {
                var toRemove = newPopulation.Count - _populationSize;
                newPopulation.RemoveRange(newPopulation.Count - toRemove - 1, toRemove);
            }

            Population = newPopulation;

            newPopulation.ForEach(gene => gene.ResetFitness());

            if (_useMutation)
            {
                ApplyMutation();
            }
        }

        public float CalculateAverageFitness()
        {
            return (Population.Sum(gene => gene.FitnessValue)/Population.Count);
        }
    }

    /// <summary>
    /// Defines a gene that can be applied to an agent.
    /// Contains a behaviour expressed in a list of actions in order to complete an objective.
    /// </summary>
    public class Gene : List<Action>
    {
        public float FitnessValue;
        public int ActionsExecuted;

        public Gene(Gene other)
        {
            ActionsExecuted = other.ActionsExecuted;
            FitnessValue = other.FitnessValue;
            AddRange(other.ToList());
        }

        public Gene(List<Action> _actions)
        {
            AddRange(_actions.ToList());
        }

        public Gene(AgentType objectiveAgent, AgentType badAgent, int actionCount)
        {
            var moveAction = new MoveAction();
            
            Add(moveAction);
            Add(new TurnAction());

            var lastType = ActionType.None;
            for (int i = 0; i < actionCount-2; i++)
            {
                var randomType = Utility.GetRandomAction(lastType);
                switch (randomType)
                {
                    case ActionType.Move:
                        Add(moveAction);
                        break;
                    case ActionType.Turn:
                       // Add(turnAction);
                        Add(new TurnAction());
                        break;
                    case ActionType.Look:
                     //   Add(lookAction);
                        Add(new LookAction(objectiveAgent, badAgent));
                        break;
                }
                lastType = randomType;
            }

            Shuffle();
        }

        public void Shuffle()
        {
            Random r = new Random();
            var shuffled = this.OrderBy(action => r.Next()).ToList();
           Clear();
           AddRange(shuffled);
        }

        /// <summary>
        /// A gene to be valid must contain at least one move and turn action
        /// </summary>
        public bool IsValid()
        {
            return this.Count(action => action.Type == ActionType.Move) >= 1 &&
                   this.Count(action => action.Type == ActionType.Turn) >= 1;
        }

        public void ResetFitness()
        {
            FitnessValue = 0;
            ActionsExecuted = 0;
        }

        public override string ToString()
        {
            var result = "";

            foreach (var action in this)
            {
                result += action + " -> ";
            }

            return result;
        }

        protected bool Equals(Gene other)
        {
            return FitnessValue.Equals(other.FitnessValue) && ActionsExecuted == other.ActionsExecuted;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Gene) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (FitnessValue.GetHashCode()*397) ^ ActionsExecuted;
            }
        }
    }


}
