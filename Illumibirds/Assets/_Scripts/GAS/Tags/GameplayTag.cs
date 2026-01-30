using UnityEngine;

namespace GAS.Tags
{
    /// <summary>
    /// ScriptableObject representing a gameplay tag for categorization and filtering.
    /// Tags can be hierarchical (e.g., Status.Stunned, Ability.Attack).
    /// </summary>
    [CreateAssetMenu(menuName = "GAS/Gameplay Tag", fileName = "Tag_")]
    public class GameplayTag : ScriptableObject
    {
        [Tooltip("Optional parent tag for hierarchical matching")]
        public GameplayTag Parent;

        /// <summary>
        /// Check if this tag matches another tag or is a child of it.
        /// </summary>
        public bool MatchesTag(GameplayTag other)
        {
            if (other == null) return false;
            if (this == other) return true;

            // Check if this tag is a descendant of the other tag
            var current = Parent;
            while (current != null)
            {
                if (current == other) return true;
                current = current.Parent;
            }

            return false;
        }

        /// <summary>
        /// Check if this tag is exactly the given tag (no hierarchy check).
        /// </summary>
        public bool MatchesTagExact(GameplayTag other)
        {
            return this == other;
        }
    }
}
