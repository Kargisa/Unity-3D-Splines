using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SplineCustomizer 
{
    public Color splineColor = Color.green;
    public Color selectedColor = Color.yellow;
    public Color connectionColor = Color.yellow;

    public Color anchorColor = Color.red;
    public Color controlColor = Color.gray;

    public Color arrowColor = Color.blue;

    public float arrowLength = 0.1f;
    public bool alwaysShowArrows = false;

    public bool useArrowDistanceDistribution = false;

    public int arrowDistribution = 40;
    public float arrowDistance = 0.1f;
}
