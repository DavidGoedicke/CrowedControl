//Gate and sign strucut managment.
using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;


//General:

// This is used as a query filter to identefy anything a agent might look at (sign or gate, etc) :
// public struct VisibleFollowPoint : IComponentData {}
// Similarly the Gate associate with that.
//public struct GateNumber : IComponentData {}


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

public struct TargetGateEntity : IComponentData
{
	public Entity value;
	public float3 pos;
}
public struct ColorRecieved : IComponentData { }

//Gate:
// Details a Gate or
public struct ActiveGate : IComponentData { public GateNums value;  }

//Gate:
// Details a Sign 
public struct ActiveSign : IComponentData { public GateNums value;  }


public static class Util
{
	public static float3 ExtendTo3(this float2 x) {
		return new float3(x.x, 0, x.y);
	}
	public static float3 ExtendTo3(this float2 x, float y) {
		return new float3(x.x, y, x.y);
	}
	public static float MapRange(this float val, float in_min, float in_max, float out_min, float out_max) //https://gist.github.com/nadavmatalon/71ccaf154bc4bd71f811289e78c65918
   {
		return (val - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}
}


public static class GameVals
{

	public static readonly int WallHeight = 3;
	public static readonly int WallSuperSampling =2;
	public static readonly float WallViewSize = 1.5f;
}
