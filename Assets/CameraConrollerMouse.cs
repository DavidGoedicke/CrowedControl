using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraConrollerMouse : MonoBehaviour
{
    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
        
       if( Input.GetKey(KeyCode.W)) transform.position += new Vector3(0, 0, 1);
       if( Input.GetKey(KeyCode.A)) transform.position += new Vector3(-1, 0, 0);
       if( Input.GetKey(KeyCode.S)) transform.position += new Vector3(0, 0, -1);
       if( Input.GetKey(KeyCode.D)) transform.position += new Vector3(1, 0, 0);
       if( Input.GetKey(KeyCode.F)) cam.orthographicSize += 0.5f;
       if( Input.GetKey(KeyCode.R)) cam.orthographicSize -= 0.5f;
        
    }
}
