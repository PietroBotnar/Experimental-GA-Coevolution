using System.Collections.Generic;

namespace Assets.Scripts
{
/// <summary>
/// Base Action class, defines an action that an agent can perform
/// </summary>
    public abstract class Action
    {
        public ActionType Type;

        protected Action(ActionType type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }

    /// <summary>
    /// Defines a movement action
    /// </summary>
    public class MoveAction : Action
    {
        public MoveAction() : base(ActionType.Move)
        {
        }
    }

    /// <summary>
    /// Allows the agent to see (be aware) of what it is around him whithin line of sight
    /// </summary>
    public class LookAction : Action
    {
        private AgentType _good;    
        private AgentType _bad;

        public LookAction(AgentType good, AgentType bad) : base(ActionType.Look)
        {
            _good = good;
            _bad = bad;
        }

        public Direction Reaction(Direction defaultDir, List<LookDirection> lookResult)
        {
            foreach (var lookDirection in lookResult)
            {
                var agentType = lookDirection.Agent;
                if ((agentType & _good) == _good && _good != AgentType.None)
                {
                    //always move towards the good
                    return lookDirection.Direction;
                }

                if ((agentType & _bad) == _bad && _bad != AgentType.None)
                {
                    //always run away from the bad
                    return lookDirection.Direction.GetOpposite();
                }
            }
            return defaultDir;
        }

        public override string ToString()
        {
            return base.ToString();//+ string.Format("TurnChance: {0}", _turnReactionChance.ToString("p0"));
        }
    }

    /// <summary>
    /// Defines the action of turning towards a direction
    /// </summary>
    public class TurnAction : Action
    {
        private readonly float _left;

        public TurnAction() : base(ActionType.Turn)
        {
            _left = Utility.GetRandomFloat();
        }

        public Direction GetDirection()
        {
            return Utility.CheckProbability(_left) ? Direction.West : Direction.East;
        }

        public override string ToString()
        {
            return base.ToString() + string.Format("Left: {0}", _left.ToString("p0"));
        }
    }
}