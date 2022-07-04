using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilingWall : MonoBehaviour
{
    Material m;

    void Start()
    {
        m = GetComponent<MeshRenderer>().material;
        m.mainTextureScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }
}
