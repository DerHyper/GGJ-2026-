using UnityEngine;

namespace GAS.Abilities
{
    /// <summary>
    /// Runtime state for a granted ability.
    /// </summary>
    public class AbilityInstance
    {
        public AbilityDefinition Definition { get; }
        public float CooldownRemaining;
        public bool IsOnCooldown => CooldownRemaining > 0f;
        public bool IsActive { get; private set; }
        public float ActiveDuration { get; private set; }

        public AbilityInstance(AbilityDefinition definition)
        {
            Definition = definition;
            CooldownRemaining = 0f;
            IsActive = false;
            ActiveDuration = 0f;
        }

        public void Tick(float deltaTime)
        {
            // Update cooldown
            if (CooldownRemaining > 0f)
            {
                CooldownRemaining -= deltaTime;
                if (CooldownRemaining < 0f)
                {
                    CooldownRemaining = 0f;
                }
            }

            // Update active duration for channeled abilities
            if (IsActive)
            {
                ActiveDuration += deltaTime;
            }
        }

        public void StartCooldown(float multiplier)
        {
            CooldownRemaining = Definition.Cooldown * multiplier;
        }

        public void Activate()
        {
            IsActive = true;
            ActiveDuration = 0f;
        }

        public void Deactivate()
        {
            IsActive = false;
            ActiveDuration = 0f;
        }

        public float GetCooldownPercent()
        {
            if (Definition.Cooldown <= 0f) return 0f;
            return CooldownRemaining / Definition.Cooldown;
        }
    }
}
