using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
        Vector3 move = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
        {
            move += new Vector3(0, 0, 1);
        }
        if (Keyboard.current.aKey.isPressed)
        {
            move += new Vector3(-1, 0, 0);
        }
        if (Keyboard.current.sKey.isPressed)
        {
            move += new Vector3(0, 0, -1);
        }
        if (Keyboard.current.dKey.isPressed)
        {
            move += new Vector3(1, 0, 0);
        }
        transform.position += move * Time.deltaTime; // Optional: multiply by deltaTime for smooth movement

        if (Keyboard.current.rKey.isPressed)
        {
            cam.orthographicSize -= 0.5f;
        }
        if (Keyboard.current.fKey.isPressed)
        {
            cam.orthographicSize += 0.5f;
        }
    }
}
