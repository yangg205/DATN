using UnityEngine;

// Không có namespace

public enum WaypointType
{
    Objective,     // Mục tiêu nhiệm vụ chưa hoàn thành
    Enemy,         // Vị trí kẻ thù
    CustomMarker,  // Điểm đánh dấu của người chơi
    QuestLocation, // Địa điểm nhiệm vụ đã hoàn thành / đạt được (m chết hả)
    Other          // Các loại khác
}

[System.Serializable]
public class Waypoint
{
    public string id;
    public string name;
    public Vector3 worldPosition;
    public WaypointType waypointType;
    public Sprite icon;

    public Waypoint(string id, string name, Vector3 worldPosition, WaypointType waypointType, Sprite icon)
    {
        this.id = id;
        this.name = name;
        this.worldPosition = worldPosition;
        this.waypointType = waypointType;
        this.icon = icon;
    }
}