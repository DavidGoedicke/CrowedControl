using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


public struct  AgentDebugData
{
    public float3 start;
    public float3[] dir;
    public float3[] col;
    public float[] circlesize;
    public float3[] circleCol;
}



//With help from: https://gist.github.com/runewake2/b4b7bf73485222f7c5a0fe0c91dd1322
public class AgentAlgorythmDebug : MonoBehaviour
{
    

    public Material linemat;
    public static AgentDebugData[] renderData;
    public static bool newData = false;

  
    void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
    }
    void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
    }
    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        OnPostRender();
    }

    void OnPostRender()
    {
        if (renderData != null && newData)
        {
           
            RenderLines(renderData);
            newData = false;
        }
    }

  

    public void RenderLines(AgentDebugData[] DataToRender)
    {
        foreach (var d in DataToRender)
        {
            RenderAgentDebug(d.start, d.dir, d.col, d.circlesize, d.circleCol);
        }
        
        
    }
    
    void RenderAgentDebug(float3 start,float3[] dir,float3[] col,float[] circlesize,float3[] circleCol)
    {
        if (dir!=null && dir.Length > 0 && dir.Length == col.Length)
        {
            for (int i = 0; i < dir.Length; i++)
            {
                GameViewRender.DrawLine(linemat, start, start + dir[i], col[i]);
               
            }
        }
       
        if (circlesize!=null && circlesize.Length > 0 && circlesize.Length == circleCol.Length)
        {
            for (int i = 0; i < circlesize.Length; i++)
            {
                GameViewRender.DrawCircle(linemat, start, circlesize[i], circleCol[i]);

            }
        }
        
        
        
            
    }

}

public static class GameViewRender
{

    public const int circleSegments = 16;
    public static void DrawLine(Material mat, float3 start,float3 stop,float3 c)
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(new Color(c.x,c.y,c.z));
        GL.Vertex(start);
        GL.Vertex(stop);
        GL.End();   
        GL.PopMatrix();
    }

    public static void DrawCircle(Material mat, float3 center,float radius,float3 c)
    {
        
        float angleStep = 2f * Mathf.PI / circleSegments;
        
        
        GL.PushMatrix();
        mat.SetPass(0);
       // GL.LoadOrtho();
        GL.Begin(GL.LINES);
        GL.Color(new Color(c.x,c.y,c.z));
        
        for (int i = 0; i < circleSegments; i++)
        {
            // Calculate the angles for the current segment
            float angle1 = angleStep * i;
            float angle2 = angleStep * (i + 1);

            // Calculate the positions of the points on the inner and outer circle
            float x1 = center.x + Mathf.Cos(angle1) * (radius );
            float z1 = center.z + Mathf.Sin(angle1) * (radius );
            float x2 = center.x + Mathf.Cos(angle2) * (radius );
            float z2 = center.z + Mathf.Sin(angle2) * (radius );

            // Draw a line for the current segment
            GL.Vertex3(x1, center.y,z1 );
            GL.Vertex3(x2, center.y,z2 );
        }
        GL.End();   
        GL.PopMatrix();
    }

// Calculate the angle between each segment
  
}