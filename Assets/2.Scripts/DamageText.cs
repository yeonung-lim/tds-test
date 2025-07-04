using UnityEngine;
using TMPro;
using System.Collections;

public class DamageText : MonoBehaviour
{
    public float duration = 1.0f;
    public float height = 1.5f;
    public float horizontalOffset = 0.5f;

    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void Show(int amount)
    {
        text.text = amount.ToString();
        StartCoroutine(MoveAlongCurve());
    }

    private IEnumerator MoveAlongCurve()
    {
        Vector3 start = transform.position;
        Vector3 control = start + Vector3.up * height + Vector3.right * horizontalOffset;
        Vector3 end = start + Vector3.right * 0.5f + Vector3.down * 0.2f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Quadratic Bezier Curve
            Vector3 a = Vector3.Lerp(start, control, t);
            Vector3 b = Vector3.Lerp(control, end, t);
            transform.position = Vector3.Lerp(a, b, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}