using UnityEngine;

public static class Finder
{
    public static Transform FindPlayer()
    {
        return GameObject.Find("Player").transform;
    }

    /// <summary>
    /// Tries to find a child GameObject by name within the given parent GameObject.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="name"></param>
    /// <param name="foundObject">The found GameObject, or null if not found.</param>
    /// <returns>True if the child GameObject was found, otherwise false.</returns>
    public static bool TryFindInChildren(Transform parent, string name, out Transform foundObject)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>();

        foreach (var item in children)
        {
            if (item.name.Equals(name))
            {
                foundObject = item;
                return true;
            }
        }

        foundObject = null;
        return false;
    }
}