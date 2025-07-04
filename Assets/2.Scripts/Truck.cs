using UnityEngine;

public class Truck : MonoBehaviour
{
    public int hp = 100;

    public void TakeDamage(int amount)
    {
        hp -= amount;
        Debug.Log($"Truck took {amount} damage. HP: {hp}");

        if (hp <= 0)
        {
            OnDestroyed();
        }
    }

    private void OnDestroyed()
    {
        Debug.Log("Truck destroyed!");
        // TODO: Game Over 처리
    }
}