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
    public float GetEstimatedLength();
    /// <summary>
    /// VERY rough estimation of the length of the Bezier without segmenting it.<br />
    /// <b>Do not use for accurate calculations</b>
    /// </summary>
    /// <returns>rough estimation of the length</returns>
    public float FastLengthEstimation();

    /// <summary>
    /// Automatically calculates the point <c>t</c> that is <c>distance</c> away from <c>start</c>
    /// </summary>
    /// <param name="start">point on the beziere to start from</param>
    /// <param name="distance">distance from the start</param>
    /// <returns>point that is <c>distance</c> away from <c>start</c></returns>
    public float PointFromDistance(float start, float distance);

    /// <summary>
    /// Automatically calculates all points <c>t</c> where the distance between the points is <c>distance</c>
    /// </summary>
    /// <param name="distance">distance between each point</param>
    /// <returns>all points with <c>distance</c> apart </returns>
    public float[] EqualDistancePoints(float distance);
}
