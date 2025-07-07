using UnityEngine;

public enum WaypointType // Loại điểm đánh dấu
{
    QuestObjective,
    CustomMarker,
    OtherNPC
}

[System.Serializable] // Cho phép hiển thị trong Inspector (nếu là List)
public class Waypoint
{
    public string id; // ID duy nhất cho mỗi waypoint (GUID hoặc tên nhiệm vụ)
    public string displayName; // Tên hiển thị (ví dụ: "Mục tiêu nhiệm vụ: Giết Goblin", "Điểm đánh dấu của tôi")
    public Vector3 worldPosition; // Vị trí trong thế giới 3D
    public WaypointType type; // Loại waypoint
    public GameObject associatedObject; // Tham chiếu đến GameObject (ví dụ: NPC mục tiêu, quái mục tiêu) nếu có
    public Sprite uiIcon; // Icon để hiển thị trên UI (minimap/la bàn)

    public Waypoint(string id, string name, Vector3 position, WaypointType type, Sprite icon = null, GameObject obj = null)
    {
        this.id = id;
        this.displayName = name;
        this.worldPosition = position;
        this.type = type;
        this.uiIcon = icon;
        this.associatedObject = obj;
    }
}