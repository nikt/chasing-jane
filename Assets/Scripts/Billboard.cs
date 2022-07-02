using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    Camera cam;

    void Update()
    {
        if (cam == null)
        {
            // look for the active camera in the scene
            cam = FindObjectOfType<Camera>();
        }

        if (cam == null)
        {
            // no camera found, don't bother trying to rotate
            return;
        }

        transform.LookAt(cam.transform);
    }
}
