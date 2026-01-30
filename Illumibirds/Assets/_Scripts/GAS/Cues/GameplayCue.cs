using UnityEngine;

namespace GAS.Cues
{
    /// <summary>
    /// ScriptableObject wrapper for VFX/SFX that can be triggered by abilities/effects.
    /// </summary>
    [CreateAssetMenu(menuName = "GAS/Gameplay Cue", fileName = "Cue_")]
    public class GameplayCue : ScriptableObject
    {
        [Header("Visual Effects")]
        [SerializeField]
        private GameObject _vfxPrefab;

        [SerializeField]
        private bool _attachToTarget = true;

        [SerializeField]
        private Vector3 _positionOffset;

        [Header("Audio")]
        [SerializeField]
        private AudioClip _soundEffect;

        [SerializeField]
        [Range(0f, 1f)]
        private float _volume = 1f;

        [Header("Timing")]
        [SerializeField]
        private float _duration = 0f;

        public void Execute(Transform target)
        {
            if (target == null) return;

            // Spawn VFX
            if (_vfxPrefab != null)
            {
                var position = target.position + _positionOffset;
                var rotation = target.rotation;
                var parent = _attachToTarget ? target : null;

                var vfxInstance = Instantiate(_vfxPrefab, position, rotation, parent);

                if (_duration > 0f)
                {
                    Destroy(vfxInstance, _duration);
                }
            }

            // Play SFX
            if (_soundEffect != null)
            {
                AudioSource.PlayClipAtPoint(_soundEffect, target.position, _volume);
            }
        }

        public void Execute(Vector3 position)
        {
            // Spawn VFX
            if (_vfxPrefab != null)
            {
                var vfxInstance = Instantiate(_vfxPrefab, position + _positionOffset, Quaternion.identity);

                if (_duration > 0f)
                {
                    Destroy(vfxInstance, _duration);
                }
            }

            // Play SFX
            if (_soundEffect != null)
            {
                AudioSource.PlayClipAtPoint(_soundEffect, position, _volume);
            }
        }
    }
}
