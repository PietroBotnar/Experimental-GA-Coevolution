using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;

/// <summary>
/// Manages maze creation, agents creation whithin the world
/// </summary>
public class Maze : MonoBehaviour
{
    private World _world;

    public CellController[,] Locations;

    public int Height;
    public int Width;

    /// <summary>
    /// Initializes the maze, mapping locations and agents
    /// </summary>
    public void InitMaze(World world, bool randomizeFruitPositions)
    {
        _world = world;

        DestroyAgents();

        Locations = new CellController[Width, Height];

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i).gameObject.GetComponent<CellController>();
            var location = new Location(child.transform.position);

            Locations[location.X, location.Y] = child;

            InitAgent(child, location, child.Agent);
        }

        PlaceFruits(_world.TotFruits, randomizeFruitPositions);
    }

    /// <summary>
    /// Called whithin the editor to draw a the base of the maze without any walls
    /// </summary>
    public void InitPlaneGrid()
    {
        if (_world == null)
            _world = FindObjectOfType<World>();

        ClearMaze();

        Locations = new CellController[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var tile = Instantiate(_world.TileTemplate);

                var location = new Location(x, y);

                tile.transform.SetParent(transform);
                tile.transform.position = new Vector3(x, -y);

                var cell = tile.GetComponent<CellController>();

                Locations[location.X, location.Y] = cell;

                cell.Init();

                //Set walls on map borders
                if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                {
                    cell.Wall = true;
                }

                cell.Refresh();

                InitAgent(cell, location, cell.Agent);
            }
        }
    }

    public void PlaceFruits(int count, bool random = false)
    {
        for (int i = 0; i < count; i++)
        {
            var loc = random ? GetRandomLocation() : Utility.FixedFruitPositions[i];
            var f = Instantiate(_world.FruitGO);
            f.transform.SetParent(_world.Agents);
            f.transform.position = loc.ToWorldPosition() + Utility.TileCenterOffset;
            _world.Fruits.Add(loc, f);
            _world.FruitGos.Add(f);
        }
    }

    public void ReplaceFruits(bool random = false)
    {
        _world.Fruits.Clear();

        int i = 0;
        foreach (var fruitGo in _world.FruitGos)
        {
            var loc = random ?  GetRandomLocation() : Utility.FixedFruitPositions[i];

            fruitGo.transform.position = loc.ToWorldPosition() + Utility.TileCenterOffset;
            fruitGo.SetActive(true);

            _world.Fruits.Add(loc, fruitGo);
            i++;
        }
    }

    /// <summary>
    /// Creates an agent at the given location
    /// </summary>
    void InitAgent(CellController cell, Location location, AgentType type)
    {
        var agentType = type;
        cell.DynamicAgent |= type;

        switch (agentType)
        {
            case AgentType.Pacman:

                var pac = Instantiate(_world.PacmanGO);
                pac.transform.SetParent(_world.Agents);
                _world.Pacman = new Agent(agentType, location, pac);

                break;
            case AgentType.Ghost:
                var g = Instantiate(_world.GhostGO);
                g.transform.SetParent(_world.Agents);
                _world.Ghosts.Add(new Agent(agentType, location, g));

                break;
        }
    }

    /// <summary>
    /// Destroys all agents
    /// </summary>
    public void DestroyAgents()
    {
        for (int i = _world.Agents.childCount - 1; i >= 0; i--)
        {

#if UNITY_EDITOR
            DestroyImmediate(_world.Agents.GetChild(i).gameObject);
#else
        Destroy(_world.Agents.GetChild(i).gameObject);
#endif

        }
        _world.Ghosts.Clear();
        _world.Fruits.Clear();
    }

    /// <summary>
    /// Retrieves a cell controller at the x y coordinates, if exists
    /// </summary>
    public CellController GetCell(int x, int y)
    {
        if (!InBounds(x, y))
            return null;

        var tile = Locations[x, y];
        return tile;
    }

    /// <summary>
    /// Retrieves a cell controller at location, if exists
    /// </summary>
    public CellController GetCell(Location location)
    {
        if (!InBounds(location.X, location.Y))
            return null;

        var tile = Locations[location.X, location.Y];
        return tile;
    }

    public Location GetRandomLocation()
    {
        var randomLocation = new Location(UnityEngine.Random.Range(0, Width), UnityEngine.Random.Range(0, Height));

        var cell = GetCell(randomLocation);
        return !cell.Wall && cell.Agent == AgentType.None && cell.DynamicAgent == AgentType.None && !_world.Fruits.ContainsKey(randomLocation) ? randomLocation : GetRandomLocation();
    }

    /// <summary>
    /// Checks whether coordinates are in bound
    /// </summary>
    public bool InBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    /// <summary>
    /// Checks whether location is in bound
    /// </summary>
    public bool InBounds(Location location)
    {
        var x = location.X;
        var y = location.Y;

        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    /// <summary>
    /// Destroys maze
    /// </summary>
    public void ClearMaze()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Refreshes the maze
    /// </summary>
    public void Refresh()
    {
        if (_world == null)
            _world = FindObjectOfType<World>();
        
        DestroyAgents();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var controller = transform.GetChild(i).gameObject.GetComponent<CellController>();
            controller.Refresh();

            if(controller.Agent == AgentType.Fruit)
                controller.Agent = AgentType.None;

            InitAgent(controller, new Location(controller.transform.position), controller.Agent);
        }
    }

    public void MoveAgent(Agent agent, Location newLocation)
    {
        var oldCell = GetCell(agent.Location);
        var newCell = GetCell(newLocation);

        oldCell.DynamicAgent ^= agent.Type;
        newCell.DynamicAgent |= agent.Type;

        agent.SetLocation(newLocation);
    }

    public void MoveAgent(Agent agent, CellController newCell)
    {
        var oldCell = GetCell(agent.Location);

        oldCell.DynamicAgent ^= agent.Type;
        newCell.DynamicAgent |= agent.Type;

        agent.SetLocation(newCell.Location());
    }

    /// <summary>
    /// Returns a location towards a direction, if exists
    /// </summary>
    public Location GetLocationTowards(Location from, Direction direction)
    {
        Location location = new Location();

        switch (direction)
        {
            case Direction.North:
                location = new Location(from.X, from.Y - 1);
                break;
            case Direction.East:
                location = new Location(from.X + 1, from.Y);
                break;
            case Direction.South:
                location = new Location(from.X, from.Y + 1);
                break;
            case Direction.West:
                location = new Location(from.X - 1, from.Y);
                break;
        }
        var cell = GetCell(location);

        return (cell != null && !cell.Wall) ? location : null;
    }

    /// <summary>
    /// Returns whether is possible to proceed towards a direction
    /// </summary>
    public bool CanGoInDirection(Location from, Direction direction)
    {
        return GetLocationTowards(from, direction) != null;
    }

    /// <summary>
    ///  Returns a list of pairs of direction to locations from a given position
    /// </summary>
    public List<DirectionLocation> GetAvailableLocationDirections(Location from)
    {
        var result = new List<DirectionLocation>();

        foreach (Direction direction in Enum.GetValues(typeof(Direction)))
        {
            var location = GetLocationTowards(from, direction);

            if(location != null)
                result.Add(new DirectionLocation(direction, location));
        }
        return result;
    }

    /// <summary>
    /// Returns the first visible agent towards a direction, until a wall is seen
    /// </summary>
    public AgentType LookAt(Location from, Direction direction)
    {
        var next = GetLocationTowards(from, direction);
        if (next == null) return AgentType.None;

        var isFruit = _world.Fruits.ContainsKey(next);

        var cell = GetCell(next);

        var result = cell.DynamicAgent;

        if(isFruit)
            result |= AgentType.Fruit;

        if (result != AgentType.None)
            return result;

        return LookAt(next, direction);
    }

    /// <summary>
    /// Returns what is visible in all directions
    /// </summary>
    public List<LookDirection> Look(Location from)
    {
        var result = new List<LookDirection>();

        foreach (Direction direction in Enum.GetValues(typeof(Direction)))
        {
            result.Add(new LookDirection(direction, LookAt(from, direction)));
        }

        return result;
    } 

}

/// <summary>
/// Pair of Direction and Location
/// </summary>
public class DirectionLocation
{
    public Direction    Direction;
    public Location     Location;

    public DirectionLocation(Direction direction, Location location)
    {
        Direction = direction;
        Location = location;
    }
}

/// <summary>
/// Pair of Direction and AgentType as a result of looking towards a direction
/// </summary>
public class LookDirection
{
    public Direction Direction;
    public AgentType Agent;

    public LookDirection(Direction direction, AgentType agent)
    {
        Direction = direction;
        Agent = agent;
    }
}
