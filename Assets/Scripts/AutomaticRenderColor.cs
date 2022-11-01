using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;

using Unity.Rendering;



public partial class AutomaticRenderColor : SystemBase
{
    EntityCommandBufferSystem ecbS;
    
    protected override void OnCreate() {

    }
    protected override void OnUpdate()
    {/* ToDo: styart here r : https://docs.unity3d.com/Packages/com.unity.entities@0.17/manual/shared_component_data.html?q=RenderMesh
        var buff = ecbS.CreateCommandBuffer();
       Dictionary<GateNums, float4> Colors;
        Colors = GateColor.val;
        Entities
           .WithName("SettingTheColor")
           .WithNone<ColorRecieved>()
           .WithReadOnly(Colors)
           .WithoutBurst()
           .ForEach((Entity entity, RenderMesh col, in AgentConfiguration agentConfiguration) =>
           {
               UnityEngine.Debug.Log("Tried to updateColor" + Colors[agentConfiguration.TargetGate].ToString());
               RenderMesh newmesh = col;
               newmesh.material.color = new UnityEngine.Color(Colors[agentConfiguration.TargetGate].x,
                   Colors[agentConfiguration.TargetGate].y,
                   Colors[agentConfiguration.TargetGate].z);


               /*buff.SetComponent(entity, new RenderMesh {
                   layer= newCol.layer,
                   material= newCol.material,
                   mesh= newCol.mesh,
                   receiveShadows= newCol.receiveShadows,
                   needMotionVectorPass= newCol.needMotionVectorPass,
                   castShadows= newCol.castShadows
               });
        buff.SetSharedComponent<RenderMesh>(entity, newmesh);
               buff.AddComponent<ColorRecieved>(entity);
           }).Run();

        ecbS.AddJobHandleForProducer(Dependency);
       */
    } 
}
