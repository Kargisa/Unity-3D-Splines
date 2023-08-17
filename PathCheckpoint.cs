using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class PathCheckpoint
{
#if UNITY_EDITOR
    [HideInInspector]
    public bool foldOut;  
#endif

    [HideInInspector]
    public List<Vector3> points;

    [HideInInspector]
    public List<Quaternion> rotations;

    [HideInInspector]
    public bool isClosed;

    [HideInInspector]
    public bool is2D;

    public PathCheckpoint(List<Vector3> points, List<Quaternion> rotations, bool isClosed, bool is2D)
    {
        this.points = points;
        this.rotations = rotations;
        this.isClosed = isClosed;
        this.is2D = is2D;
        
        foldOut = false;
    }
}
