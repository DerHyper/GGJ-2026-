using UnityEngine;

public static class Finder
{
    public static Transform FindPlayer()
    {
        return GameObject.Find("Player").transform;
    }
}