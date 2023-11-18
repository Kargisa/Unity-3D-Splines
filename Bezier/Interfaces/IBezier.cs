using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBezier
{
    /// <summary>
    /// Calculates the estiamted length of the Bezier
    /// </summary>
    /// <param name="resolution">resolution for the calculation</param>
    /// <returns>Estimated length of Bezier</returns>
    //public float GetEstimatedLength(BezierResolution resolution);

    /// <summary>
    /// Calculates the estiamted length of the Bezier
    /// </summary>
    /// <param name="resolution">custom resolution for the calculation</param>
    /// <returns>Estimated length of Bezier</returns>
    public float GetEstimatedLength(int resolution);

    /// <summary>
    /// Get the point on the Bezier <c>t</c>
    /// </summary>
    /// <param name="t">value on the Bezier</param>
    /// <returns>Vector3 Point on Bezier</returns>
    public Vector3 GetPoint(float t);

    /// <summary>
    /// Calculates the point <c>t</c> that is <c>distance</c> away from <c>start</c>
    /// </summary>
    /// <param name="resolution"></param>
    /// <param name="start">point on the beziere to start from</param>
    /// <param name="distance">distance from the start</param>
    /// <returns>point that is <c>distance</c> away from <c>start</c></returns>
    public float PointFromDistance(int resolution, float start, float distance);

    /// <summary>
    /// Calculates all points <c>t</c> where the distance between the points is <c>distance</c>
    /// </summary>
    /// <param name="distance">distance between each point</param>
    /// <param name="resolution"></param>
    /// <returns>all points with <c>distance</c> apart </returns>
    public float[] EqualDistancePoints(float distance, int resolution);
}
