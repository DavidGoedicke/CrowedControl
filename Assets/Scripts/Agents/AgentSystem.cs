// #define DEBUGDIRECT
//	#define DEBUGDIRTEXT

using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;




public partial class AgentSystem_IJobChunk: SystemBase
{


	BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
	protected override void OnCreate() {


		m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

	}

	EntityQueryDesc ActiveAgentQuery = new EntityQueryDesc {
		None = new ComponentType[]
		   {
			   ComponentType.ReadOnly<WasBornTag>(),
			   ComponentType.ReadOnly<ArrivedTag>()
		   },
		All = new ComponentType[]
		   {

			   ComponentType.ReadOnly<WalkingTag>()
		   }
	};

	protected override void OnUpdate() {

		var commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
		var temp = new Unity.Mathematics.Random(1);
		Entities
			.WithName("Initialize")
			.WithAny<WasBornTag>()
			.WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
			.ForEach((Entity entity, int entityInQueryIndex, ref PhysicsMass mass) => {

				commandBuffer.RemoveComponent<WasBornTag>(entityInQueryIndex, entity);
				commandBuffer.AddComponent<WalkingTag>(entityInQueryIndex, entity);
				commandBuffer.AddComponent<ApplyImpulse>(entityInQueryIndex, entity);
				mass.InverseInertia[0] = 0;
				//mass.InverseInertia[1] = 0;
				mass.InverseInertia[2] = 0;


			})
			.ScheduleParallel();
		m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

		EntityQuery m_AgentQuery = GetEntityQuery(ActiveAgentQuery);
		var agentCount = m_AgentQuery.CalculateEntityCount();

		if (agentCount == 0) { return; }

		var positionMemory = new NativeArray<float2>(agentCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		var directionMemory = new NativeArray<float2>(agentCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		var initialPosArrayJobHandle = Entities
					.WithAll<WalkingTag>()
					.WithName("InitPosMemorySeparationJob")
					.ForEach((int entityInQueryIndex, in LocalToWorld pos) => {

						positionMemory[entityInQueryIndex] = pos.Position.xz;
						directionMemory[entityInQueryIndex] = math.normalize(pos.Forward.xz);// pos.Forward;
					})
					.ScheduleParallel(Dependency);





		float deltaTime = Time.DeltaTime;
		var steerJob1 = Entities
			.WithName("Steer")
			.WithReadOnly(positionMemory)
			.WithReadOnly(directionMemory)
			.WithAll<WalkingTag>()
			.ForEach((ref ApplyImpulse impulse, in LocalToWorld pos) =>//in AgentInfo agent
			{
				//float avoidStrong = 1.5f;
				float minRaduis = 3f;
				float aviodWeight = 0.16f;

				float maxAlignRadius = 15f;
				float alignWeight = 0.1f;

				float maxGroupingRadius = 100.0f;
				float groupingWeight = 0.15f;

				uint avoidCount = 0;
				float2 avoidVector = float2.zero;
				uint alignCount = 0;
				float2 alignDirection = float2.zero;
				uint groupingCount = 0;
				float2 groupingCenter = float2.zero;
				for (int i = 0; i < positionMemory.Length; i++) {
					float distance = math.length(pos.Position.xz - positionMemory[i]);
					if (distance <= 0) { continue; } //cheap way of skipping our own




					#region AreTheyInFronOfMe
					bool inFront = AngleDiff(pos.Forward,math.normalizesafe(positionMemory[i].ExtendTo3(pos.Position.y) - pos.Position));
					#endregion


					#region DirectionAllignment
					bool inLine = AngleDiff(pos.Forward, math.normalizesafe(directionMemory[i].ExtendTo3()));
					#endregion

					float avoidAdjust = distance.MapRange(0, minRaduis, 2, 0.0001f);
#if DEBUGDIRECT
					Color c = Color.red;
					if (inLine && !inFront) {
						c = Color.green;
					} else if (inLine && inFront) {
						c = Color.cyan;
						
					} else if (! inLine && inFront) {
						c = Color.magenta;
					}
					//Debug.DrawLine(pos.Position, positionMemory[i].ExtendTo3(pos.Position.y), c);
					//if(avoidAdjust>0) Debug.DrawRay(pos.Position, new float3(0, avoidAdjust * 100, 0), Color.white);
#endif

					if (distance <=minRaduis) {
#if DEBUGDIRTEXT
						Debug.Log(distance.ToString() + "  " + avoidAdjust.ToString());
#endif
						if (inLine) {
							avoidVector += ((pos.Position.xz - positionMemory[i])* avoidAdjust);
							
						} else {
							avoidVector += ((pos.Position.xz - positionMemory[i])*0.5f);//if we go into the opposite direction it doesnt matter as much when they come close
						}
						avoidCount++;
					}
					if (distance < maxAlignRadius) {
						if (inLine) {
							alignDirection += directionMemory[i];
							alignCount++;
						}
					}
					if (distance < maxGroupingRadius) {
						
						if (inLine && inFront) { // Only if they go in the same direction do we want to go with them 
							groupingCenter += positionMemory[i];
							groupingCount++;
						}
					}
				}
				groupingCenter /= groupingCount;
				alignDirection /= alignCount;
				avoidVector /= avoidCount;

				Debug.Log(groupingCount.ToString() + " " + alignCount.ToString() + " " + avoidCount.ToString());

				float2 newValue = math.normalizesafe(
					(math.normalizesafe(avoidVector) * aviodWeight +
					math.normalizesafe(alignDirection) * alignWeight +
					math.normalizesafe(groupingCenter - pos.Position.xz) * groupingWeight));

#if DEBUGDIRECT
				
				if (groupingCount > 0) {
					Debug.DrawRay(pos.Position, (groupingCenter.ExtendTo3(pos.Position.y) - pos.Position) * groupingWeight, Color.black) ;
				}
				
				if (alignCount > 0) {
					Debug.DrawRay(pos.Position, alignDirection.ExtendTo3() * 10 * alignWeight, Color.green);
				}
				if (avoidCount > 0) {
					Debug.DrawRay(pos.Position, avoidVector.ExtendTo3() * 10 * aviodWeight, Color.red);
				}
				Debug.DrawRay(pos.Position, newValue.ExtendTo3() * 10 * aviodWeight, Color.white);
				
#endif
				impulse = new ApplyImpulse {
					Direction = newValue.ExtendTo3()
				};


			})
		   .ScheduleParallel(initialPosArrayJobHandle); //getGateDirection

		positionMemory.Dispose(steerJob1);
		directionMemory.Dispose(steerJob1);
		var steerJob2 = Entities
		   .WithName("SteerToGate")
		   .WithAll<WalkingTag, TargetGateEntity>()
		   .ForEach((ref ApplyImpulse impulse, in LocalToWorld IsPos, in TargetGatePosiition TargetPos) =>//in AgentInfo agent
		   {
			   float3 direction = TargetPos.value - IsPos.Position;
			   float3 newDirection;
			   if (math.length(impulse.Direction) > 0.01f) {
				   newDirection = (impulse.Direction * 0.3f) + (math.normalizesafe(direction) * 0.7f);
			   } else {
				   newDirection = direction;


			   }

			   impulse = new ApplyImpulse {
				   Direction = math.normalizesafe(newDirection)
			   };

		   }).ScheduleParallel(steerJob1);


		Dependency = steerJob2;


	}
	private static bool AngleDiff(float3 ThisForward, float3 OtherForward,float MaxAngle=math.PI/2) {
		float dotProduct = math.dot(math.normalize(ThisForward.xz), math.normalize(OtherForward.xz));
		float angleDiff = math.acos(dotProduct);
		angleDiff = math.cross(ThisForward, OtherForward).y < 0 ? -angleDiff : angleDiff;

		if (angleDiff < MaxAngle && angleDiff > -MaxAngle) {
			return true;
		} else {
			return false;
		}


	}
}

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

