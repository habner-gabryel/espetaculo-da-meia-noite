using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("UI Elements")]
    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public GameObject emptyPanel;

    public Image[] slotHighlight;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateInventoryUI();
    }

    public void UpdateInventoryUI()
    {
        if (InventoryManager.Instance == null) return;

        int selected = InventoryManager.Instance.GetSelectedSlot();
        ItemData currentItem = null;

        if (selected != -1) {
            currentItem = InventoryManager.Instance.GetItemInSlot(selected);
        }

        if (currentItem != null) {
            itemIcon.sprite = currentItem.icon;
            itemIcon.color = Color.white;
            if (itemName != null) itemName.text = currentItem.itemName;
            if (emptyPanel != null) emptyPanel.SetActive(false);
        } else {
            itemIcon.sprite = null;
            itemIcon.color = Color.clear;
            if (itemName != null) itemName.text = "Vazio";
            if (emptyPanel != null) emptyPanel.SetActive(true);
        }

        // if (slotHighlight != null && slotHighlight.Length == 3)
        // {
        //     for (int i = 0; i < slotHighlight.Length; i++)
        //     {
        //         if (slotHighlight[i] != null)
        //         {
        //             slotHighlight[i].color = (i == selected) ? Color.yellow : Color.gray;
        //         }
        //     }
        // }
    }
}