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
    public float GetLength(BezierResolution resolution);

    /// <summary>
    /// Calculates the estiamted length of the Bezier
    /// </summary>
    /// <param name="resolution">custom resolution for the calculation</param>
    /// <returns>Estimated length of Bezier</returns>
    public float GetLength(int resolution);

    /// <summary>
    /// Get the point on the Bezier <c>t</c>
    /// </summary>
    /// <param name="t">value on the Bezier</param>
    /// <returns>Vector3 Point on Bezier</returns>
    public Vector3 GetPoint(float t);
}
