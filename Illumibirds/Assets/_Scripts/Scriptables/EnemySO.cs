using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Enemy", order = 1)]
public class EnemySO : ScriptableObject
{
    public enum AttackType
    {
        ranged,
        brawl
    }
    public string Name;
    public AttackType attackType;
    public float attackDistance;
    public float movementSpeed;
    public float attackSpeed;
}