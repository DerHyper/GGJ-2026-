using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAS.Tags
{
    /// <summary>
    /// Runtime collection of gameplay tags with events for changes.
    /// </summary>
    [Serializable]
    public class GameplayTagContainer
    {
        [SerializeField]
        private List<GameplayTag> _tags = new();

        public event Action<GameplayTag> OnTagAdded;
        public event Action<GameplayTag> OnTagRemoved;

        public IReadOnlyList<GameplayTag> Tags => _tags;
        public int Count => _tags.Count;

        public void AddTag(GameplayTag tag)
        {
            if (tag == null || _tags.Contains(tag)) return;

            _tags.Add(tag);
            OnTagAdded?.Invoke(tag);
        }

        public void RemoveTag(GameplayTag tag)
        {
            if (tag == null) return;

            if (_tags.Remove(tag))
            {
                OnTagRemoved?.Invoke(tag);
            }
        }

        public bool HasTag(GameplayTag tag)
        {
            if (tag == null) return false;
            return _tags.Contains(tag);
        }

        /// <summary>
        /// Check if container has a tag that matches (including hierarchy).
        /// </summary>
        public bool HasTagMatching(GameplayTag tag)
        {
            if (tag == null) return false;

            foreach (var t in _tags)
            {
                if (t.MatchesTag(tag)) return true;
            }
            return false;
        }

        /// <summary>
        /// Check if container has any of the given tags.
        /// </summary>
        public bool HasAnyTag(IEnumerable<GameplayTag> tags)
        {
            foreach (var tag in tags)
            {
                if (HasTag(tag)) return true;
            }
            return false;
        }

        /// <summary>
        /// Check if container has any tag matching any of the given tags (including hierarchy).
        /// </summary>
        public bool HasAnyTagMatching(IEnumerable<GameplayTag> tags)
        {
            foreach (var tag in tags)
            {
                if (HasTagMatching(tag)) return true;
            }
            return false;
        }

        /// <summary>
        /// Check if container has all of the given tags.
        /// </summary>
        public bool HasAllTags(IEnumerable<GameplayTag> tags)
        {
            foreach (var tag in tags)
            {
                if (!HasTag(tag)) return false;
            }
            return true;
        }

        /// <summary>
        /// Check if container has all tags matching (including hierarchy).
        /// </summary>
        public bool HasAllTagsMatching(IEnumerable<GameplayTag> tags)
        {
            foreach (var tag in tags)
            {
                if (!HasTagMatching(tag)) return false;
            }
            return true;
        }

        public void Clear()
        {
            var tagsToRemove = new List<GameplayTag>(_tags);
            foreach (var tag in tagsToRemove)
            {
                RemoveTag(tag);
            }
        }

        public void AddTags(IEnumerable<GameplayTag> tags)
        {
            foreach (var tag in tags)
            {
                AddTag(tag);
            }
        }

        public void RemoveTags(IEnumerable<GameplayTag> tags)
        {
            foreach (var tag in tags)
            {
                RemoveTag(tag);
            }
        }
    }
}
