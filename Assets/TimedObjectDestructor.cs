using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class TimedObjectDestructor : MonoBehaviour
{
    public float time = 2f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, time);
    }


}
