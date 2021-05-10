//Gate and sign strucut managment.
using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;


//General:

// This is used as a querey filter to identefy anything a agent might look at (sign or gate, etc) :
// public struct VisibleFollowPoint : IComponentData {}
// Similarly the Gate associate with that.
public struct GateNumber : IComponentData { public GateNums value; }
public struct SignNumber : IComponentData { public GateNums value; } /// <summary>
///  We allow multiple of this component.       
/// </summary>
public enum GateNums { A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, NONE }

public struct GateColor
{
    public static readonly Dictionary<GateNums, float4> val =
    new Dictionary<GateNums, float4> {
        { GateNums.A, new float4(1f,0f,0f,0)},
        { GateNums.B,  new float4(0f,1f,0f,0) },    
        { GateNums.C, new float4(0f,0f,1f,0)},
        { GateNums.D, new float4(0f,.75f,.75f,0) },
        { GateNums.E, new float4(.25f,.25f,.25f,0) } };
}


//Agent:
//What the agent wants to go to;

public struct TargetGateEntity : IComponentData { public Entity value; }
public struct TargetGatePosiition : IComponentData { public float3 value; }
public struct ColorRecieved : IComponentData { }

//Gate:
// Details a Gate or
public struct ActiveGate : IComponentData { }

//Gate:
// Details a Sign 
public struct ActiveSign : IComponentData { }
