/// <summary>
/// The resolution of the Bezier curve. 
/// <br />
/// Higher options are best for longer curves
/// </summary>
public enum BezierResolution
{
    /// <summary>
    /// Low resolution with 100 segments
    /// </summary>
    Low = 100,
    /// <summary>
    /// Middle resolution with 1000 segments
    /// </summary>
    Middle = 1000,
    /// <summary>
    /// <b>DEFAULT</b> High resolution with 5000 segments
    /// </summary>
    High = 5000,
    /// <summary>
    /// Ultra resolution with 15_000 segments
    /// </summary>
    Ultra = 15_000,
    /// <summary>
    /// <b>EXPERIMENTAL</b> Automatically calculate the resolution fpr the curve
    /// </summary>
    Auto = -1 //TODO: implementation
}
