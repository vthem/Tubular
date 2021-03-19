using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TestSample : MonoBehaviour
{
    [SerializeField] float round = 1f;
    [SerializeField] float input = 1f;
    [SerializeField] float output = 1f;


    // Update is called once per frame
    void Update()
    {
        var r = input % round;
        output = input - r;
    }
}
