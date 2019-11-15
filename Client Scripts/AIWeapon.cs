using UnityEngine;

public class AIWeapon : MonoBehaviour
{
    [SerializeField] AI ai;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Player p = other.GetComponent<Player>();
            if (p != null)
                ai.Attack(p);
            AI a = other.GetComponent<AI>();
            if (a != null)
                ai.Attack(a);
        }
    }
}