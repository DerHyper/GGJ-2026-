using System.Collections.Generic;
using GAS.Pickups;
using UnityEngine;

public class RandomAbilityPickup : MonoBehaviour
{
    public List<Pickup> possiblePickups;

    void Start()
    {
        SpawnRandomAbility();
    }

    public void SpawnRandomAbility()
    {
        int rnd = UnityEngine.Random.Range(0, possiblePickups.Count);
        Pickup pickup = possiblePickups[rnd];

        Instantiate(pickup, transform.position, Quaternion.identity);
        Destroy(this.gameObject,0.1f);
    }

}
