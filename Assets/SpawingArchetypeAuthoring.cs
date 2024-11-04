using UnityEngine;
using Unity.Entities;


public class SpawingArchetypeAuthoring : MonoBehaviour
{
    public GameObject AgentPrefab;
    public class MyBaker : Baker<SpawingArchetypeAuthoring>
    {
        public override void Bake(SpawingArchetypeAuthoring authoring)
        {
            //var prefabEntity = GetEntity(authoring.AgentPrefab);
            
            AddComponent(new AgentPrefab { Value = GetEntity(authoring.AgentPrefab) });
            
            // Add singleton to reference the prefab
         //   AddComponent(new AgentPrefab { Value = prefabEntity });

            // Add components for the archetype behavior
           
           
           
        }
    }
}

public struct AgentPrefab : IComponentData
{
    public Entity Value;
}