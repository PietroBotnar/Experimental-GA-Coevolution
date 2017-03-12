using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Defines an agent
    /// </summary>
    public class Agent
    {
        private Location _initLocation;
        private Direction _direction;
        private int _nextActionIndex;
        public Action CurrentAction;
        public GameObject GameObject;
        public Gene Gene;
        public GeneticAlgorithm GeneticAlgorithm;
        public Location Location;
        public AgentType Type;

        public int ActionsExecuted = 0;
        public int Evolutions;

        public int Moves;

        public Agent(AgentType type, Location location, GameObject gameObject)
        {
            Type = type;
            GameObject = gameObject;

            _initLocation = location;

            SetLocation(location);
        }

        public Direction Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;

                UpdateGORotation();
            }
        }

        public void SetAlgorithm(GeneticAlgorithm algorithm)
        {
            GeneticAlgorithm = algorithm;
        }

        public string SetGene(int generation)
        {
            Gene = GeneticAlgorithm.GetGene(generation);

            _nextActionIndex = -1;
            ActionsExecuted = 0;

            return string.Format("{2}/{0} - {1}", generation, Gene, Evolutions);
        }

        public int GetActionCount()
        {
            return ActionsExecuted;
        }

        public bool NextAction()
        {
            _nextActionIndex++;
            if (Gene.Count == _nextActionIndex)
            {
                _nextActionIndex = 0;
            }

            CurrentAction = Gene[_nextActionIndex];
            return true;
        }

        public void SetLocation(Location location)
        {
            if (location == null)
                return;

            Location = location;

            GameObject.transform.position = Location.ToWorldPosition() + Utility.TileCenterOffset;
        }

        public Location Reset()
        {
            GameObject.SetActive(true);

            return _initLocation;
        }

        public float GetDistanceTo(Location other)
        {
            return Utility.Distance(Location, other);
        }

        public float GetDistanceTo(Agent other)
        {
            return GetDistanceTo(other.Location);
        }

        private void UpdateGORotation()
        {
            switch (_direction)
            {
                case Direction.North:
                    GameObject.transform.rotation = Quaternion.AngleAxis(90, Vector3.forward);
                    break;
                case Direction.East:
                    GameObject.transform.rotation = Quaternion.identity;
                    break;
                case Direction.South:
                    GameObject.transform.rotation = Quaternion.AngleAxis(270, Vector3.forward);
                    break;
                case Direction.West:
                    GameObject.transform.rotation = Quaternion.AngleAxis(180, Vector3.forward);
                    break;
            }
        }
    }
}