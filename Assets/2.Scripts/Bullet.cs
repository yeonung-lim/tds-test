using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 5f;
    public LayerMask targetMask;

    private Vector3 direction;
    private int damage = 1;

    public void SetTarget(Vector3 targetPos)
    {
        direction = (targetPos - transform.position).normalized;
        Destroy(gameObject, lifetime);
    }
    
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & targetMask) != 0)
        {
            Monster monster = collision.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}