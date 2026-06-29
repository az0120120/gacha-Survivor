using System;
using UnityEngine;

[Serializable]
public class VictoryIncomingPropSettings
{
    [Tooltip("物体贴图，可自定义")]
    public Sprite sprite;
    public Color tintColor = Color.white;
    public float scale = 1f;
    public int sortingOrder = 20;

    [Tooltip("飞入起点（视口坐标，>1 或 <0 表示屏幕外）")]
    public Vector2 spawnViewport = new Vector2(1.15f, 0.75f);

    [Tooltip("到达后相对摄像头的视口锚点")]
    public Vector2 targetViewport = new Vector2(0.72f, 0.58f);

    [Tooltip("飞入加速度")]
    public float rushAcceleration = 22f;

    [Tooltip("飞入最大速度")]
    public float maxRushSpeed = 17f;

    [Tooltip("判定到达目标点的距离")]
    public float arriveDistance = 0.35f;
}

[Serializable]
public class VictoryIncomingPropSequenceEntry
{
    [Tooltip("在上一个步骤之后等待的秒数（物体 A 表示到达速度 A 之后）")]
    public float delaySeconds = 10f;
    public VictoryIncomingPropSettings settings = new VictoryIncomingPropSettings();
}
