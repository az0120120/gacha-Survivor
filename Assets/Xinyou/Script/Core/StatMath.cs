using UnityEngine;

public static class StatMath
{
    public static int FloorToInt(float value)
    {
        return Mathf.FloorToInt(value);
    }

    public static float PercentToRatio(float percent)
    {
        return percent * 0.01f;
    }

    public static float ClampRatio(float ratio)
    {
        return Mathf.Clamp01(ratio);
    }
}
