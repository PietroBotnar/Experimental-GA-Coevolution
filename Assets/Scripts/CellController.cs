using System;
using UnityEngine;
using Assets.Scripts;

public class CellController : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;

    public  bool       Wall;
    public  AgentType  Agent;
    public AgentType DynamicAgent { get; set; }

    public void Init()
    {
        Wall                = false;
        Agent               = AgentType.None;
    }

    public void Refresh()
    {
        SpriteRenderer.color = Wall ? Color.blue : Color.black;
    }

    public Location Location()
    {
        return new Location(transform.position);
    }
}
