using UnityEngine;
using Unity.Entities;


public class SpawingArchetypeAuthoring : MonoBehaviour
{
    public GameObject AgentPrefab;
    public class MyBaker : Baker<SpawingArchetypeAuthoring>
    {
        public override void Bake(SpawingArchetypeAuthoring authoring)
        {
            AddComponent(new AgentPrefab { Value = GetEntity(authoring.AgentPrefab) });
        }
    }
}

public struct AgentPrefab : IComponentData
{
    public Entity Value;
}