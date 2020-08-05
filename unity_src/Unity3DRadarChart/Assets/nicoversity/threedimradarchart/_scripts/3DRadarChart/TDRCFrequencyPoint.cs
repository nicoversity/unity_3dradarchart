using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class and data structure to represent an individual data item (i.e., data point) in a Frequency Polygon.
/// </summary>
public class TDRCFrequencyPoint
{
    [Header("Properties")]
    public string dimension;        // dimension (key) the data item belongs to
    public string time;             // time indicator of the data item
    public int value;               // value of the data item
    public Vector3 pos;             // position of the data item within the Frequency Polygon
    public Color color;             // color associated to the data item within the Frequency Polygon


    #region DATA_HANDLER

    /// <summary>
    /// Method to get textual insights of the properties.
    /// </summary>
    /// <returns>String representing the core properties of the data item.</returns>
    public string getPropertiesString()
    {
        return dimension + " " + time + " " + value;
    }

    /// <summary>
	/// Update properties.
	/// </summary>
	/// <param name="fp">TDRCFrequencyPoint used to copy its values to this data structure.</param>
    public void updateData(TDRCFrequencyPoint fp)
    {
        dimension = fp.dimension;
        time = fp.time;
        value = fp.value;
        pos = fp.pos;
        color = fp.color;
    }

    #endregion
}
