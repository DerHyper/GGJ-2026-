namespace GAS.Abilities
{
    /// <summary>
    /// Interface for ability behavior logic.
    /// Implement this interface to define what an ability does when activated.
    /// </summary>
    public interface IAbilityBehavior
    {
        /// <summary>
        /// Called when the ability is activated.
        /// </summary>
        void OnActivate(AbilityInstance ability, Core.AbilitySystemComponent owner);

        /// <summary>
        /// Called every frame while the ability is active (for channeled abilities).
        /// </summary>
        void OnTick(AbilityInstance ability, Core.AbilitySystemComponent owner, float deltaTime);

        /// <summary>
        /// Called when the ability ends (either naturally or cancelled).
        /// </summary>
        void OnEnd(AbilityInstance ability, Core.AbilitySystemComponent owner);

        /// <summary>
        /// Additional check for whether the ability can be activated.
        /// This is called after standard checks (cost, cooldown, tags).
        /// </summary>
        bool CanActivate(AbilityInstance ability, Core.AbilitySystemComponent owner);
    }
}
