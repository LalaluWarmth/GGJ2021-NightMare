using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldNoteRenderer : MonoBehaviour
{
    private LineRenderer _lr;
    public Transform h0;
    public Transform h1;
    private Vector3[] _vector3s=new Vector3[2];
    public Material defaultMaterial;
    public Material interruptMaterial;
    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _vector3s[0] = h0.transform.position;
        _vector3s[1] = h1.transform.position;
    }

    
    void Update()
    {
        _vector3s[0] = h0.transform.position;
        _vector3s[1] = h1.transform.position;
        _lr.SetPositions(_vector3s);
    }
}
