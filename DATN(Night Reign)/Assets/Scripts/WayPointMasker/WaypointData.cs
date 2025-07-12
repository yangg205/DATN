using UnityEngine;

public enum WaypointType
{
    CustomMarker,
    QuestLocation,
}

public class Waypoint
{
    public string id;
    public string name;
    public Vector3 worldPosition;
    public WaypointType waypointType;
    public Sprite icon;

    public Waypoint(string id, string name, Vector3 worldPosition, WaypointType type, Sprite icon = null)
    {
        this.id = id;
        this.name = name;
        this.worldPosition = worldPosition;
        this.waypointType = type;
        this.icon = icon;
    }
}
