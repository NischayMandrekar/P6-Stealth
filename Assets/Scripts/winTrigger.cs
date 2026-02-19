using UnityEngine;

public class winTrigger : MonoBehaviour
{
    public bool GameWon;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") )
        {
            GameWon = true;
        }
    }
}
