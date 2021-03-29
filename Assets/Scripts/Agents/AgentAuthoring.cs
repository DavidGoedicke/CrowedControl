using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;


public struct WasBornTag : IComponentData{ }
public struct WalkingTag : IComponentData{ }
public struct ArrivedTag : IComponentData{ }
public struct AgentSpeed : IComponentData { public float Value; }
public struct AgentPosition : IComponentData {  public float3 Value; }
public struct AgentDirection : IComponentData { public float3 Value; }



[DisallowMultipleComponent]
public class AgentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    /// <summary>
    /// We can refer directly to GameObject transforms in the authoring component
    /// </summary>
    public List<Transform> Waypoints;

    // Fields are used to populate Entity data
    public float speed = 3.0f;


    /// <summary>
    /// A function which converts our Guard authoring GameObject to a more optimized Entity representation
    /// </summary>
    /// <param name="entity">A reference to the entity this GameObject will become</param>
    /// <param name="dstManager">The EntityManager is used to make changes to Entity data.</param>
    /// <param name="conversionSystem">Used for more advanced conversion features. Not used here.</param>
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Here we add all of the components needed to start the guard off in the "Patrol" state
        // i.e. We add TargetPosition, and don't add IdleTimer or IsChasing tag
        dstManager.AddComponents(entity, new ComponentTypes(
            new ComponentType[] {
                typeof(WasBornTag),
                typeof(AgentSpeed),
                typeof(AgentPosition),
                typeof(AgentDirection)
            }));
        dstManager.SetComponentData(entity, new AgentSpeed { Value = speed });
    }
}
