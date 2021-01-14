using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusIndicator : MonoBehaviour
{
    [SerializeField]
    private RectTransform healthBarRect;
    [Header("Optional: ")]
    [SerializeField]
    private Text text;
    public Gradient gradient;

    private void Start()
    {
        if (healthBarRect == null)
            Debug.LogError("STATUS INDICATOR: No health bar object referenced");
    }

    public void SetHealth(int _currentHealth, int _maxHealth)
    {
        float _value = (float)_currentHealth / _maxHealth;
        healthBarRect.GetComponentInChildren<Image>().color = gradient.Evaluate(_value);
        healthBarRect.localScale = new Vector3(_value, healthBarRect.localScale.y, healthBarRect.localScale.z);

        if (text != null)
            text.text = "HP: " + _currentHealth.ToString();
    }

    private void FixedUpdate()
    {
        transform.rotation = Quaternion.Euler(Vector3.up * 0);
    }
}
