using UnityEngine;

namespace GAS.Abilities
{
    /// <summary>
    /// Runtime state for a granted ability.
    /// </summary>
    public class AbilityInstance
    {
        public AbilityDefinition Definition { get; }
        public float CooldownRemaining { get; private set; }
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

        public void StartCooldown()
        {
            CooldownRemaining = Definition.Cooldown;
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
