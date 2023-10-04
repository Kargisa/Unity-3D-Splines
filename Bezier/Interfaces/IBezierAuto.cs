using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBezierAuto
{
    /// <summary>
    /// Resolution used to calculate 1 meter
    /// </summary>
    public int ResPerMeter { get; }

    /// <summary>
    /// Automatically estimates the resolution for the Length calculations
    /// </summary>
    /// <returns>Estimated Length of the Bezier</returns>
    public float GetLength();
    /// <summary>
    /// VERY rough estimation of the length of the Bezier without segmenting it.<br />
    /// <b>Do not use for accurate calculations</b>
    /// </summary>
    /// <returns>rough estimation of the length</returns>
    public float FastLengthEstimation();
}
