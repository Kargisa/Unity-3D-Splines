using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CubicBezier
{
    public Vector3 anchor1 { get; set; }
    public Vector3 controler1 { get; set; }
    public Vector3 controler2 { get; set; }
    public Vector3 anchor2 { get; set; }

    public CubicBezier(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        anchor1 = p1;
        controler1 = p2;
        controler2 = p3;
        anchor2 = p4;
    }
}
