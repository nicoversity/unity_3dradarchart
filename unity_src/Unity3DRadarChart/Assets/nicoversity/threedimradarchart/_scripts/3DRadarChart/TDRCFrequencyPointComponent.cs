/*
 * TDRCFrequencyPointComponent.cs
 *
 * Supported Unity version: 2019.2.17f1 Personal (tested)
 *
 * Author: Nico Reski
 * Web: https://reski.nicoversity.com
 * Twitter: @nicoversity
 * GitHub: https://github.com/nicoversity
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used as a wrapper within the context of the Unity engine in order to hold a Frequency Point data structure (and thus make it attachable to GameObjects).
/// </summary>
public class TDRCFrequencyPointComponent : MonoBehaviour
{
    public TDRCFrequencyPoint fp;       // reference to this component's linked Frequency Point data structure
}
