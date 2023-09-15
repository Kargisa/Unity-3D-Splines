using UnityEngine;

public struct QuadraticBezier
{
    public Vector3 anchor1 { get; set; }
    public Vector3 controler { get; set; }
    public Vector3 anchor2 { get; set; }

    public QuadraticBezier(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        anchor1 = p1;
        controler = p2;
        anchor2 = p3;
    }
}
