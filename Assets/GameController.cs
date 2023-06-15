using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct AgentSimParams
{
    public bool MotionIsRunning; //ToDo This Wnats to Be loaded in from a config file...
    
}
public class GameController : MonoBehaviour
{

    private static GameController _singelton;
    private AgentSimParams _agentSimParams;


    [Range(0, 1)] public float GroupDistance = 0.5f;

    private List<AgentAuthoring> Agents;
    
    public GameController()
    {
        _agentSimParams = SimParams;
    }

    public static GameController Singelton
    {
        get { return _singelton; }
    }

    public AgentSimParams SimParams
    {
        get { return _agentSimParams; }

    }

   

    private void Start()
    {
        
    }

    private void Update()
    {
        
       
    }
    

    private void Awake()
    {
        if (_singelton != null && _singelton != this)
        {
            Destroy(gameObject);
            return;
        }

        _singelton = this;
        DontDestroyOnLoad(gameObject);

        _agentSimParams = new AgentSimParams()
        {
            MotionIsRunning = true,


        };
    }

    public void ToggleSim()
    {
        _agentSimParams.MotionIsRunning = !_agentSimParams.MotionIsRunning;

    }

    public void SetSimPause(bool val)
    {
        _agentSimParams.MotionIsRunning = val;
    }
}


