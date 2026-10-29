using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite icon;
    public GameObject worldPrefab;
    public ItemType itemType;
}

public enum ItemType
{
    Weapon,
    Knife,
    Flashlight,
}