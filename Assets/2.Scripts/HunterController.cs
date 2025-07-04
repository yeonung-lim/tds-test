using UnityEngine;

public class HunterController : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float attackCooldown = 1.0f;
    public float attackRange = 4f;
    public LayerMask targetMask;
    public int attackDamage = 10;

    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;

        Collider2D target = Physics2D.OverlapCircle(transform.position, attackRange, targetMask);
        if (target != null && timer >= attackCooldown)
        {
            timer = 0f;
            Fire(target.transform.position);
        }
    }

    private void Fire(Vector3 targetPos)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetTarget(targetPos);
        bulletScript.SetDamage(attackDamage);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}