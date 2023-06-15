//#define DEBUGSimValues

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;
using Text = UnityEngine.UI.Text;



public class AgentDebugUI : MonoBehaviour
{

    public Toggle SimStop;

    public GameObject SliderPrefab;
    
    
    
    
    
    #if DEBUGSimValues
    public  Slider Slider_minRaduis;
    public  Slider Slider_maxAlignRadiu;
    public  Slider Slider_maxGroupingRadius;
    
    public  Slider Slider_aviodWeight;
    public  Slider Slider_alignWeight;
    public  Slider Slider_groupingWeight;
    
    public  Slider Slider_aviodWeight_OP;
    public  Slider Slider_alignWeight_OPP;
    public  Slider Slider_groupingWeight_OP;
    
    public  Slider Slider_gateWeight;
    public  Slider Slider_wallWeight;
    #endif
    
    
    // Start is called before the first frame update
    void Start()
    {

        SimStop.onValueChanged.AddListener(ToggleSimChange);
        
#if DEBUGSimValues
       int count = 0;
        Slider_minRaduis = Instantiate(SliderPrefab, transform).GetComponent<Slider>();//.GetComponentInChildren<Label>().text = "minR";
        Slider_minRaduis.GetComponentInChildren<Text>().text = "Av";
        Slider_minRaduis.GetComponent<RectTransform>().localPosition -= new Vector3(0, 20 * count++, 0);
        Slider_minRaduis.minValue = 0;
        Slider_minRaduis.maxValue = 10;
        Slider_minRaduis.value = SimVal.minRaduis;
        
        Slider_maxAlignRadiu = Instantiate(SliderPrefab, transform).GetComponent<Slider>();
        Slider_maxAlignRadiu.GetComponentInChildren<Text>().text = "Al";
        Slider_maxAlignRadiu.GetComponent<RectTransform>().localPosition -= new Vector3(0, 20 * count++, 0);
        Slider_maxAlignRadiu.minValue = 0;
        Slider_maxAlignRadiu.maxValue = 10;
        Slider_maxAlignRadiu.value = SimVal.maxAlignRadius;
        
        Slider_maxGroupingRadius = Instantiate(SliderPrefab, transform).GetComponent<Slider>();
        Slider_maxGroupingRadius.GetComponentInChildren<Text>().text = "Gr";
        Slider_maxGroupingRadius.GetComponent<RectTransform>().localPosition -= new Vector3(0, 20 * count++, 0);
        Slider_maxGroupingRadius.minValue = 0;
        Slider_maxGroupingRadius.maxValue = 50;
        Slider_maxGroupingRadius.value = SimVal.maxGroupingRadius;
        
        Slider_aviodWeight = Instantiate(SliderPrefab, transform).GetComponent<Slider>();
        Slider_aviodWeight.GetComponentInChildren<Text>().text = "av";
        Slider_aviodWeight.GetComponent<RectTransform>().localPosition -= new Vector3(0, 20 * count++, 0);
        Slider_aviodWeight.minValue = 0;
        Slider_aviodWeight.maxValue = 1;
        Slider_aviodWeight.value = SimVal.aviodWeight;
        
        Slider_alignWeight = Instantiate(SliderPrefab, transform).GetComponent<Slider>();
        Slider_alignWeight.GetComponentInChildren<Text>().text = "al";
        Slider_alignWeight.GetComponent<RectTransform>().localPosition -= new Vector3(0, 20 * count++, 0);
        Slider_alignWeight.minValue = 0;
        Slider_alignWeight.maxValue = 1;
        Slider_alignWeight.value = SimVal.alignWeight;
        
        Slider_groupingWeight = Instantiate(SliderPrefab, transform).GetComponent<Slider>();
        Slider_groupingWeight.GetComponentInChildren<Text>().text = "gr";
        Slider_groupingWeight.GetComponent<RectTransform>().localPosition -= new Vector3(0, 20 * count++, 0);
        Slider_groupingWeight.minValue = 0;
        Slider_groupingWeight.maxValue = 1;
        Slider_groupingWeight.value = SimVal.groupingWeight;
#endif
    }

   

    private void ToggleSimChange(bool arg0)
    {
        GameController.Singelton.SetSimPause(arg0);
    }

    // Update is called once per frame
    void Update()
    {
        
        
        
        #if DEBUGSimValues
        if (Slider_minRaduis.value != SimVal.minRaduis)
        {

            SimVal.minRaduis = Slider_minRaduis.value;
            Slider_minRaduis.GetComponentInChildren<Text>().text = "Av" + Slider_minRaduis.value.ToString("F2");
        }

        if (Slider_maxAlignRadiu.value != SimVal.maxAlignRadius)
        {

            SimVal.maxAlignRadius = Slider_maxAlignRadiu.value;
            
            Slider_maxAlignRadiu.GetComponentInChildren<Text>().text = "Al" + Slider_maxAlignRadiu.value.ToString("F2");
        }

        if (Slider_maxGroupingRadius.value != SimVal.maxGroupingRadius)
        {

            SimVal.maxGroupingRadius = Slider_maxGroupingRadius.value;
            Slider_maxGroupingRadius.GetComponentInChildren<Text>().text = "Gr" + Slider_maxGroupingRadius.value.ToString("F2");
        }
        
        if (Slider_aviodWeight.value != SimVal.aviodWeight)
        {

            SimVal.aviodWeight = Slider_aviodWeight.value;
            Slider_aviodWeight.GetComponentInChildren<Text>().text = "av" + Slider_aviodWeight.value.ToString("F2");
        }
        
        if (Slider_alignWeight.value != SimVal.alignWeight)
        {

            SimVal.alignWeight = Slider_alignWeight.value;
            Slider_alignWeight.GetComponentInChildren<Text>().text = "al" + Slider_alignWeight.value.ToString("F2");
        }
        
        if (Slider_groupingWeight.value != SimVal.groupingWeight)
        {

            SimVal.groupingWeight = Slider_groupingWeight.value;
            Slider_groupingWeight.GetComponentInChildren<Text>().text = "gr" + Slider_groupingWeight.value.ToString("F2");
        }
        
        
        #endif
    }

}


public static class SimVal
{
#if DEBUGSimValues
    public const int WallHeight = 3;
    public const int WallSuperSampling = 1;
    public const float WallViewSize = 2f;
    public const float SpawnDelay = 0.25f;
    public const int MaxAgents = 100;
    
    
    
    public static float minRaduis = 1.8f;
    public static float maxAlignRadius = 4.8f;
    public static float maxGroupingRadius = 20.0f;
    
    public static float aviodWeight = 0.5f;
    public static float alignWeight = 0.46f;
    public static float groupingWeight = 0.2f;
    
    public static float aviodWeight_OPP = 0.0f;
    public static float alignWeight_OPP = -0.01f;
    public static float groupingWeight_OPP = groupingWeight/2;
    
    public static float gateWeight = 0.8f;
    public static float wallWeight = 0f;
    
#else
    
public const int WallHeight = 3;
    public const int WallSuperSampling = 1;
    public const float WallViewSize = 2f;
    public const float SpawnDelay = 0.25f;
    public const int MaxAgents = 5;
    
   public const float minRaduis = 1.8f;
    public const float maxAlignRadius = 4.8f;
    public const float maxGroupingRadius = 20.0f;
    
    public const float aviodWeight = 0.5f;
    public const float alignWeight = 0.46f;
    public const float groupingWeight = 0.2f;
    
    public const float aviodWeight_OPP = 0.0f;
    public const float alignWeight_OPP = -0.01f;
    public const float groupingWeight_OPP = groupingWeight/2;
    
    public const float gateWeight = 0.8f;
    public const float wallWeight = 0f;
    #endif
}
