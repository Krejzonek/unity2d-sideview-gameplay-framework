
using UnityEngine;

public class WeaponVisualEquipment : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder;

    private GameObject currentWeaponObject;
    private ItemData currentItem;

    public void RefreshFromCharacter(CharacterData character)
    {
        ItemData weapon = null;

        if (character != null && character.equippedItems != null)
        {
            foreach (ItemData item in character.equippedItems)
            {
                if (item != null && item.equipmentType == EquipmentType.Weapon)
                {
                    weapon = item;
                    break;
                }
            }
        }

        Equip(weapon);
    }

    public void Equip(ItemData item)
    {
        if (currentItem == item && currentWeaponObject != null)
            return;

        ClearWeapon();

        if (item == null || item.weaponVisualProfile == null)
            return;

        if (weaponHolder == null)
        {
            Debug.LogError("WeaponVisualEquipment: Weapon Holder is not assigned.", this);
            return;
        }

        WeaponVisualProfile profile = item.weaponVisualProfile;

        if (profile.weaponPrefab == null)
        {
            Debug.LogWarning("WeaponVisualEquipment: Weapon prefab is missing for " + item.name, this);
            return;
        }

        currentWeaponObject = Instantiate(profile.weaponPrefab, weaponHolder, false);
        currentWeaponObject.transform.localPosition = Vector3.zero;
        currentWeaponObject.transform.localRotation = Quaternion.identity;
        currentWeaponObject.transform.localScale = Vector3.one;

        WeaponVisualController controller = currentWeaponObject.GetComponent<WeaponVisualController>();

        if (controller == null)
        {
            Debug.LogError("WeaponVisualEquipment: Weapon prefab needs a WeaponVisualController.", currentWeaponObject);
            Destroy(currentWeaponObject);
            currentWeaponObject = null;
            currentItem = null;
            return;
        }

        currentItem = item;
        controller.ApplyProfile(profile);
    }

    public void ClearWeapon()
    {
        if (currentWeaponObject != null)
            Destroy(currentWeaponObject);

        currentWeaponObject = null;
        currentItem = null;
    }
}
