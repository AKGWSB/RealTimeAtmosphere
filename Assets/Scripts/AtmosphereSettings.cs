using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[Serializable]
[CreateAssetMenu(fileName = "Atmosphere", menuName = "AtmosphereSettings")]
public class AtmosphereSettings : ScriptableObject
{
    [SerializeField]
    public float SeaLevel = 0.0f;

    [SerializeField]
    public float PlanetRadius = 6360000.0f;

    [SerializeField]
    public float AtmosphereHeight = 60000.0f;

    [SerializeField]
    public float SunLightIntensity = 31.4f;

    [SerializeField]
    public Color SunLightColor = Color.white;

    [SerializeField]
    public float SunDiskAngle = 9.0f;

    [SerializeField]
    public float RayleighScatteringScale = 1.0f;

    [SerializeField]
    public float RayleighScatteringScalarHeight = 8000.0f;

    [SerializeField]
    public float MieScatteringScale = 1.0f;

    [SerializeField]
    public float MieAnisotropy = 0.8f;

    [SerializeField]
    public float MieScatteringScalarHeight = 1200.0f;

    [SerializeField]
    public float OzoneAbsorptionScale = 1.0f;

    [SerializeField]
    public float OzoneLevelCenterHeight = 25000.0f;

    [SerializeField]
    public float OzoneLevelWidth = 15000.0f;

    [SerializeField]
    public float AerialPerspectiveDistance = 32000.0f;
}
