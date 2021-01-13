using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextHandler : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 0.5f);
    }

    private void FixedUpdate()
    {
        transform.rotation = Quaternion.Euler(Vector3.up * 0);
    }
}
