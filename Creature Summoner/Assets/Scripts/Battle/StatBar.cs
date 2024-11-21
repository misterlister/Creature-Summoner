using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    [SerializeField] GameObject barValue;
    [SerializeField] Image barBackground;

    private const float BAR_SPEED = 5f;
    private const float THRESHOLD = 0.01f;

    public void SetBar(float hpNormalized)
    {
        barValue.transform.localScale = new Vector3(hpNormalized, 1f);
    }

    public IEnumerator SetBarSmooth(float newVal)
    {
        float currVal = barValue.transform.localScale.x;

        while (Mathf.Abs(currVal - newVal) > THRESHOLD) // Use threshold to stop the loop
        {
            // Gradually move the bar towards its target
            currVal = Mathf.Lerp(currVal, newVal, BAR_SPEED * Time.deltaTime);

            // Apply the new length to the scale of the bar
            barValue.transform.localScale = new Vector3(currVal, 1f);

            yield return null;
        }

        // Set the final value precisely to the target to avoid any floating-point precision issues
        barValue.transform.localScale = new Vector3(newVal, 1f);
    }

    public void Hide()
    {
        barValue.gameObject.SetActive(false);
        barBackground.gameObject.SetActive(false);
    }

    public void Show()
    {
        barValue.gameObject.SetActive(true);
        barBackground.gameObject.SetActive(true);
    }
}
