using UnityEngine;

public class AlwaysLookAtCamera : MonoBehaviour
{
    private void OnEnable()
    {
        LookAtCamera();
    }

    private void LookAtCamera()
    {
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);

            Vector3 lookDirection = Camera.main.transform.position - transform.position;
            lookDirection.y = 0f;
            if (lookDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDirection);
        }
        else
        {
            Debug.LogWarning("Main Camera not found.");
        }
    }
}
