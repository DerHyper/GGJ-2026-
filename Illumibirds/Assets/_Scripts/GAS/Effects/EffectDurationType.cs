namespace GAS.Effects
{
    /// <summary>
    /// Determines how long an effect lasts.
    /// </summary>
    public enum EffectDurationType
    {
        /// <summary>
        /// Applied once immediately, then removed.
        /// </summary>
        Instant,

        /// <summary>
        /// Lasts for a specified duration.
        /// </summary>
        Duration,

        /// <summary>
        /// Lasts until explicitly removed.
        /// </summary>
        Infinite
    }
}
