
#define DEBUGFLOOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Physics;
using Unity.Collections;
using Unity.Physics.Systems;



public struct IsFloorTag: IComponentData
{

}
[DisallowMultipleComponent]
public class FloorAuthoring: MonoBehaviour
{


}



class FloorAuthoringBaker : Baker<FloorAuthoring>
{
	public override void Bake(FloorAuthoring authoring)
	{
		AddComponent(new IsFloorTag());
		
	}
}


/*

public struct FloorVectorManager
{



	private CollisionFilter WallCollisionFilter;


	private BuildPhysicsWorld m_BuildPhysicsWorldSystem;
	private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;



	public void Init(BuildPhysicsWorld BuildPhysicsWorldSystem, EndSimulationEntityCommandBufferSystem EndSimulationEcbSystem) {
		if (Vectors == null) {
			Vectors = new Dictionary<int2, float2>();
		}
		m_BuildPhysicsWorldSystem = BuildPhysicsWorldSystem;
		m_EndSimulationEcbSystem = EndSimulationEcbSystem;

		WallCollisionFilter = new CollisionFilter {
			BelongsTo = 1u<<2,
			CollidesWith = 16u, //Wall and Ground
			GroupIndex = 0
		};
	}


	public float2 WallVector(float2 position) {
		

		int2 qPos = Quantize(position);

		if (Vectors.ContainsKey(qPos)) {
			return Vectors[qPos];
		} else {
			
			var collisionWorld = World.Coll;
			float2 qPosReal = new float2(qPos) / GameVals.WallSuperSampling;

			List<float2> wallList = new List<float2>();
			for (float x = qPosReal.x - GameVals.WallViewSize; x < (qPosReal.x + GameVals.WallViewSize); x += ((float)1.0f/GameVals.WallSuperSampling)) {
				for (float z = qPosReal.y - GameVals.WallViewSize; z < (qPosReal.y + GameVals.WallViewSize); z += ((float)1.0f/GameVals.WallSuperSampling)) {



					var raycastInput = new RaycastInput {
						Start = new float3(x, GameVals.WallHeight, z),
						End = new float3(x, -GameVals.WallHeight, z),
						Filter = WallCollisionFilter
					};

					var hit = world.CastRay(raycastInput, out var rayResult);

					if (hit) {
						wallList.Add(new float2(x, z));

#if DEBUGFLOOR
						Debug.DrawLine(raycastInput.Start, raycastInput.End, Color.red, 10);
#endif

					} else {
#if DEBUGFLOOR
						Debug.DrawLine(raycastInput.Start, raycastInput.End, Color.gray, 10);
#endif

					}


				}
			}
			float2 AvoidDirection = float2.zero;
			if (wallList.Count > 0) {
				foreach (float2 elem in wallList) {
					float2 vec = qPosReal - elem;
					AvoidDirection += math.normalizesafe(vec)* Util.MapRange(math.length(vec),
						0, math.length(new float2(GameVals.WallViewSize, GameVals.WallViewSize))
						,1,0.01f);
				}
				//AvoidDirection /= wallList.Count;
#if DEBUGFLOOR

				Debug.DrawRay(qPosReal.ExtendTo3(2), AvoidDirection.ExtendTo3(0), Color.blue, 20);
				//Debug.Log("The Last" + qPosReal.ExtendTo3(2)+ "  "+AvoidDirection.ExtendTo3(2));
#endif
			}
			Vectors[qPos] = AvoidDirection;
			return AvoidDirection;
		}
	}
	private int2 Quantize(float2 pos) {
		pos *= GameVals.WallSuperSampling;
		return new int2(math.round(pos));
	}

	private Dictionary<int2, float2> Vectors;
}

*/