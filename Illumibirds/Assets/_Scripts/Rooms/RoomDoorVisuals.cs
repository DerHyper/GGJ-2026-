using UnityEngine;

namespace Rooms
{
    /// <summary>
    /// Controls the visual door sprites in a room prefab.
    /// Attach to room prefab root - automatically finds Door_Closed and Door_open children.
    /// </summary>
    public class RoomDoorVisuals : MonoBehaviour
    {
        [Header("Auto-detected (leave empty to auto-find)")]
        [SerializeField] private GameObject doorClosedSprite;
        [SerializeField] private GameObject doorOpenSprite;

        private bool isOpen;

        private void Awake()
        {
            // Auto-find door sprites if not assigned
            if (doorClosedSprite == null)
            {
                doorClosedSprite = FindChildContaining("Door_Closed");
            }
            if (doorOpenSprite == null)
            {
                doorOpenSprite = FindChildContaining("Door_open");
            }

            // Default state: closed
            SetDoorsOpen(false);
        }

        private GameObject FindChildContaining(string partialName)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child.name.Contains(partialName))
                {
                    return child.gameObject;
                }
            }
            return null;
        }

        public void SetDoorsOpen(bool open)
        {
            isOpen = open;

            if (doorClosedSprite != null)
            {
                doorClosedSprite.SetActive(!open);
            }
            if (doorOpenSprite != null)
            {
                doorOpenSprite.SetActive(open);
            }
        }

        public void OpenDoors()
        {
            SetDoorsOpen(true);
        }

        public void CloseDoors()
        {
            SetDoorsOpen(false);
        }

        public bool IsOpen => isOpen;
    }
}
