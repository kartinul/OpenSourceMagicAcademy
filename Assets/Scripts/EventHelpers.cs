using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine.UI;

public class EventHelpers : MonoBehaviour
{
    /// <summary>
    /// Generates a default camera shake based on the impulse source attached to the Player.
    /// </summary>
    public void ShakeScreen()
    {
        CinemachineImpulseSource impulseSource = GetImpulseSource();

        if (impulseSource != null)
        {
            if (impulseSource.DefaultVelocity == Vector3.zero)
            {
                // Kickstart it with a default force if it was left at (0,0,0)
                impulseSource.GenerateImpulse(new Vector3(1f, 1f, 0f));
            }
            else
            {
                impulseSource.GenerateImpulse();
            }
        }
        else
        {
            Debug.LogWarning("EventHelpers: No CinemachineImpulseSource found in Player children for ShakeScreen.");
        }
    }

    /// <summary>
    /// Generates a camera shake with a specific force.
    /// </summary>
    public void ShakeScreenWithForce(float force)
    {
        CinemachineImpulseSource impulseSource = GetImpulseSource();

        if (impulseSource != null)
        {
            impulseSource.GenerateImpulseWithForce(force);
        }
    }

    private CinemachineImpulseSource GetImpulseSource()
    {
        if (Player.Instance != null)
        {
            return Player.Instance.GetComponentInChildren<CinemachineImpulseSource>();
        }

        return null;
    }

    /// <summary>
    /// Logs a custom message to the console. Useful for debugging UnityEvents.
    /// </summary>
    public void LogMessage(string message)
    {
        Debug.Log($"[EventHelpers] {message}");
    }

    /// <summary>
    /// Spawns a prefab at the position of this GameObject.
    /// </summary>
    public void SpawnPrefab(GameObject prefab)
    {
        if (prefab != null)
        {
            Instantiate(prefab, transform.position, transform.rotation);
        }
    }

    /// <summary>
    /// Flashes the screen white briefly.
    /// </summary>
    public void FlashScreenWhite()
    {
        StartCoroutine(FlashRoutine(Color.white, 0.5f));
    }

    /// <summary>
    /// Flashes the screen red briefly.
    /// </summary>
    public void FlashScreenRed()
    {
        StartCoroutine(FlashRoutine(Color.red, 0.5f));
    }

    /// <summary>
    /// Adds a GameObject to the Player's CinemachineTargetGroup so the camera focuses on it.
    /// </summary>
    public void FocusCameraOn(GameObject target)
    {
        if (target == null || Player.Instance == null) return;

        CinemachineTargetGroup targetGroup = Player.Instance.GetComponentInChildren<CinemachineTargetGroup>();
        if (targetGroup != null)
        {
            // First check if it's already in the group
            foreach (var t in targetGroup.Targets)
            {
                if (t.Object == target.transform) return;
            }

            targetGroup.Targets.Add(new CinemachineTargetGroup.Target { Object = target.transform, Weight = 1f, Radius = 1f });
        }
        else
        {
            Debug.LogWarning("EventHelpers: No CinemachineTargetGroup found in Player children for FocusCameraOn.");
        }
    }

    /// <summary>
    /// Removes all extra targets from the Player's CinemachineTargetGroup, leaving only the Player.
    /// </summary>
    public void ClearCameraFocus()
    {
        if (Player.Instance == null) return;

        CinemachineTargetGroup targetGroup = Player.Instance.GetComponentInChildren<CinemachineTargetGroup>();
        if (targetGroup != null)
        {
            targetGroup.Targets.Clear();
            targetGroup.Targets.Add(new CinemachineTargetGroup.Target { Object = Player.Instance.transform, Weight = 1f, Radius = 1f });
        }
    }

    private IEnumerator FlashRoutine(Color color, float duration)
    {
        GameObject canvasObj = new GameObject("ScreenFlashCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; 

        Image image = canvasObj.AddComponent<Image>();
        image.color = color;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(color.a, 0f, elapsed / duration);
            Color newColor = image.color;
            newColor.a = alpha;
            image.color = newColor;
            yield return null;
        }

        Destroy(canvasObj);
    }
}
