using UnityEngine;
using UnityEngine.UI;

public class UIProgressTeamHolder : MonoBehaviour
{
    public Team team;
    [SerializeField] private Image image;

    public void SetImage(Color color)
    {
        if (image != null)
        {
            image.color = color;
        }
    }
}
