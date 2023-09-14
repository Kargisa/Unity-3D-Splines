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

    public Color arrowColor = Color.black;

    public float arrowLength = 0.1f;

    public bool arrowDistributionByDistance = false;

    //normal distribution
    public int arrowDistribution = 40;

    //distribution by distance
    public int arrowResolution = 5000;
    public float arrowDistance = 0.05f;
}
