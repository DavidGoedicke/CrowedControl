//#define DEBUGFLOOR
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Physics;
using Unity.Collections;
using Unity.Physics.Systems;
using Unity.Transforms;


public struct IsFloorTag : IComponentData
{
}

[DisallowMultipleComponent]
public class FloorAuthoring : MonoBehaviour
{
    class FloorAuthoringBaker : Baker<FloorAuthoring>
    {
        public override void Bake(FloorAuthoring authoring)
        {
            AddComponent(new IsFloorTag());
        }
    }
}


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(AgentSystem))]
public partial class FloorVectorManager : SystemBase
{
    private CollisionFilter WallCollisionFilter;


    protected override void OnCreate()
    {
        Vectors = new Dictionary<int2, float2>();


        WallCollisionFilter = new CollisionFilter
        {
            BelongsTo = 1u << 2,
            CollidesWith = 16u, //Wall and Ground
            GroupIndex = 0
        };
    }

    protected override void OnUpdate()
    {
        PhysicsWorldSingleton physicsWorldSingleton = GetSingleton<PhysicsWorldSingleton>();
        Entities.WithAll<WalkingTag>().WithoutBurst().ForEach((ref WallAvoidVector wav,in Translation pos) =>
        {
            int2 qPos = Quantize(pos.Value);

            if (Vectors.ContainsKey(qPos))
            {
                wav = new WallAvoidVector { Value = Vectors[qPos] };
            }
            else
            {
                float2 qPosReal = new float2(qPos) / FixedGameValues.WallSuperSampling;

                List<float2> wallList = new List<float2>();
                for (float x = qPosReal.x - FixedGameValues.WallViewSize;
                     x < (qPosReal.x + FixedGameValues.WallViewSize);
                     x += ((float)1.0f / FixedGameValues.WallSuperSampling))
                {
                    for (float z = qPosReal.y - FixedGameValues.WallViewSize;
                         z < (qPosReal.y + FixedGameValues.WallViewSize);
                         z += ((float)1.0f / FixedGameValues.WallSuperSampling))
                    {
                        var raycastInput = new RaycastInput
                        {
                            Start = new float3(x, FixedGameValues.WallHeight, z),
                            End = new float3(x, -FixedGameValues.WallHeight, z),
                            Filter = WallCollisionFilter
                        };

                        var hit = physicsWorldSingleton.CastRay(raycastInput, out var rayResult);

                        if (hit)
                        {
                            wallList.Add(new float2(x, z));

#if DEBUGFLOOR
                            Debug.DrawLine(raycastInput.Start, raycastInput.End, Color.red, 10);
#endif
                        }
                        else
                        {
#if DEBUGFLOOR
                            Debug.DrawLine(raycastInput.Start, raycastInput.End, Color.gray, 10);
#endif
                        }
                    }
                }

                float2 AvoidDirection = float2.zero;
                if (wallList.Count > 0)
                {
                    foreach (float2 elem in wallList)
                    {
                        float2 vec = qPosReal - elem;
                        AvoidDirection += math.normalizesafe(vec) * Util.MapRange(math.length(vec),
                            0, math.length(new float2(FixedGameValues.WallViewSize, FixedGameValues.WallViewSize))
                            , 1, 0.01f);
                    }
                    //AvoidDirection /= wallList.Count;
#if DEBUGFLOOR

                    Debug.DrawRay(qPosReal.ExtendTo3(2), AvoidDirection.ExtendTo3(0), Color.blue, 20);
                    //Debug.Log("The Last" + qPosReal.ExtendTo3(2)+ "  "+AvoidDirection.ExtendTo3(2));
#endif
                }

                Vectors.Add(qPos, AvoidDirection);
                wav = new WallAvoidVector { Value = AvoidDirection };
            }
        }).Run();
    }

    private static int2 Quantize(float3 pos)
    {
        pos *= FixedGameValues.WallSuperSampling;
        return new int2(math.round(pos.xz));
    }

    private Dictionary<int2, float2> Vectors;
}