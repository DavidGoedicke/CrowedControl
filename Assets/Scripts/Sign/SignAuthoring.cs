using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;



[DisallowMultipleComponent]
public class SignAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public GateNums[] PublicSignNumber;
    public bool StartActive;
    /// <param name="conversionSystem">Used for more advanced conversion features. Not used here.</param>
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Here we add all of the components needed to start the guard off in the "Patrol" state
        // i.e. We add TargetPosition, and don't add IdleTimer or IsChasing tag
        //dstManager.AddComponents(entity, new ComponentTypes(
        //    new ComponentType[] {
        //        typeof(SignNumbers)
        //    }));
        if (StartActive)
        {
            dstManager.AddComponents(entity, new ComponentTypeSet(
           new ComponentType[] {
                typeof(ActiveSign)
           }));
        }
        foreach (GateNums temp in PublicSignNumber)
        {
            dstManager.AddComponentData(entity, new ActiveSign() { value = temp });
        }

    }





    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
