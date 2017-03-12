using System;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Utility
{
    public static readonly float MutationProbability = 0.01f;

    public static readonly Vector3 TileCenterOffset = new Vector3(0.5f, -0.5f);

    public static readonly List<Location> FixedFruitPositions = new List<Location>
    {
        new Location(1, 1),
        new Location(17, 1),
        new Location(1, 17),
        new Location(17, 17),
    };

    /// <summary>
    /// Calculates the distance between to locations
    /// </summary>
    public static float Distance(Location from, Location to)
    {
        return Mathf.Sqrt(Mathf.Pow(to.X - from.X, 2) + Mathf.Pow(to.Y - from.Y, 2));
    }

    /// <summary>
    ///  Returns the opposite cardinal direction
    /// </summary>
    public static Direction GetOpposite(this Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return Direction.South;
            case Direction.East:
                return Direction.West;
            case Direction.West:
                return Direction.East;
        }
        return Direction.North;
    }

    /// <summary>
    /// Relative world cardinal direction to another
    /// </summary>
    public static Direction GetRelative(this Direction direction, Direction relative)
    {
        switch (direction)
        {
            case Direction.East:
                switch (relative)
                {
                    case Direction.East:
                        return Direction.South;
                    case Direction.West:
                        return Direction.North;
                }
                break;
            case Direction.South:
                return relative.GetOpposite();
            case Direction.West:
                switch (relative)
                {
                    case Direction.East:
                        return Direction.North;
                    case Direction.West:
                        return Direction.South;
                }
                break;
        }
        return relative;
    }

    /// <summary>
    /// Returns a random floating point number
    /// </summary>
    public static float GetRandomFloat()
    {
        return Random.Range(0, 0.99f);
    }

    /// <summary>
    /// Returns random integer
    /// </summary>
    /// <param name="from">inclusive</param>
    /// <param name="to"> exclusive</param>
    /// <returns></returns>
    public static int GetRandomInt(int from = 0, int to = 100)
    {
        return Random.Range(from, to);
    }

    /// <summary>
    /// Rolls a random value and returns whether it is less than or equal to the given chance value 
    /// </summary>
    public static bool CheckProbability(float chance)
    {
        var roll = GetRandomFloat();
        return roll <= chance;
    }

    /// <summary>
    /// Selects a random element from the given list
    /// </summary>
    public static T SelectRandom<T>(List<T> population)
    {
        var randomIndex = UnityEngine.Random.Range(0, population.Count);
        return population[randomIndex];
    }

    public static ActionType GetRandomAction(ActionType except = ActionType.None)
    {
        var type = (ActionType)Random.Range(1, 4);
        return type != except ? type : GetRandomAction(except);
    }

    public static bool TypeMatch(this AgentType typeCombo, AgentType other)
    {
        return (typeCombo & other) == other;
    }
}

/// <summary>
/// Represents a type of agent
/// </summary>
[Flags]
public enum AgentType
{
    None    = 0,
    Pacman  = 1,
    Ghost   = 2,
    Fruit   = 4,
}

/// <summary>
/// Represents a cardinal direction in the world
/// </summary>
public enum Direction
{
    North,
    East,
    South,
    West
}

/// <summary>
/// Represents a type of action an agent can perform
/// </summary>
public enum ActionType
{
    None,
    Move,
    Turn,
    Look
}