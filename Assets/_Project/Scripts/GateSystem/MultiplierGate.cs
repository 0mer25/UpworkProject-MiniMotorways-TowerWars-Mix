using TMPro;
using UnityEngine;

public class MultiplierGate : MonoBehaviour
{
    [SerializeField] private int multiplier = 2;
    [SerializeField] private TextMeshPro multiplierText;

    void Start()
    {
        multiplierText.text = $"x{multiplier}";
    }

    public int GetMultiplier()
    {
        return multiplier;
    }
}
