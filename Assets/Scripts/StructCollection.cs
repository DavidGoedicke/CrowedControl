//Gate and sign strucut managment.

using Unity.Entities;


//General:
// This is used as a querey filter to identefy anything a agent might look at (sign or gate, etc) :
// public struct VisibleFollowPoint : IComponentData {}
// Similarly the Gate associate with that.
public struct GateNumber : IComponentData { public GateNums value; }

public enum GateNums { A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, NONE }



//Agent:
//What the agent wants to go to;

public struct TargetGateEntity : IComponentData { public Entity value; }

//Gate:
// Details a Gate or
public struct ActiveGate : IComponentData { }

//Gate:
// Details a Gate or
public struct ActiveSign : IComponentData { }
