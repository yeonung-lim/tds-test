using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject damageTextPrefab;
    public Transform canvasTransform;

    private void OnEnable()
    {
        Monster.OnDamageDealt += ShowDamageText;
    }

    private void OnDisable()
    {
        Monster.OnDamageDealt -= ShowDamageText;
    }

    public void ShowDamageText(Vector3 worldPos, int amount)
    {
        GameObject go = Instantiate(damageTextPrefab, worldPos, Quaternion.identity, canvasTransform);
        go.GetComponent<DamageText>()?.Show(amount);
    }
}