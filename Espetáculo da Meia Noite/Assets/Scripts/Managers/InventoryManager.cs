using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Configuração")]
    public int maxSlots = 3; 

    private List<ItemData> items = new List<ItemData>();
    private int selectedSlot = -1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            items.Add(null);
        }
    }

    public bool AddItem(ItemData itemToAdd)
    {
        for (int i = 0; i < items.Count; i++) {
            if (items[i] == null) {
                items[i] = itemToAdd;
                if (selectedSlot == -1) 
                    SelectSlot(i);
                else
                    UpdateUI();
                return true;
            }
        }
        return false;
    }

    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= items.Count) return;

        selectedSlot = slotIndex;

        if (items[slotIndex] != null) {
            EquipItem(items[slotIndex]);
        } else {
            UnequipCurrentItem();
        }

        UpdateUI();
    }

    // Métodos de equipagem (integração com armas)
    private void EquipItem(ItemData item)
    {
        revolverObject.SetActive(false);
        facaObject.SetActive(false);
        lanternaObject.SetActive(false);

        // Ativa a correta baseado no item
        if (item.itemID == "revolver") {
            revolverObject.SetActive(true);
            WeaponSystem.Instance.SetCurrentWeapon(ItemType.Weapon);
        } else if (item.itemID == "faca") {
            facaObject.SetActive(true);
            WeaponSystem.Instance.SetCurrentWeapon(ItemType.Knife);
        } else if (item.itemID == "lanterna") {
            lanternaObject.SetActive(true);
            WeaponSystem.Instance.SetCurrentWeapon(ItemType.Flashlight);
        }
        // Para consumíveis, você pode não ativar nada, ou ativar um "uso" na mão.
    }
    // Exemplo no EquipmentManager
    private void UnequipCurrentItem()
    {
        Debug.Log("Desequipando item atual");
        // Aqui você desativa tudo na mão do player
        // Ex: EquipmentManager.Instance.UnequipAll();
    }

    // Getters para a UI
    public ItemData GetItemInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= items.Count) return null;
        return items[slotIndex];
    }

    public int GetSelectedSlot()
    {
        return selectedSlot;
    }

    private void UpdateUI()
    {
        if (HUDManager.Instance != null)
            HUDManager.Instance.UpdateInventoryUI();
    }
}