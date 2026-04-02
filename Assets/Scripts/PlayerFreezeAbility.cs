using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerFreezeAbility : MonoBehaviour
{
    [Header("Freeze Use")]
    [SerializeField] private KeyCode useKey = KeyCode.T;
    [SerializeField] private float freezeDuration = 5f;

    [Header("HUD")]
    [SerializeField] private TMP_Text freezeCountText;

    private int freezeCount = 0;
    private bool freezeActive = false;

    private void Start()
    {
        RefreshFreezeUI();
    }

    private void Update()
    {
        if (freezeCount > 0 && Input.GetKeyDown(useKey) && !freezeActive)
        {
            StartCoroutine(FreezeGuards());
        }
    }

    public void CollectFreeze()
    {
        freezeCount++;
        RefreshFreezeUI();
    }

    private IEnumerator FreezeGuards()
    {
        freezeActive = true;
        freezeCount--;
        RefreshFreezeUI();

        GuardPatrol[] guards = FindObjectsByType<GuardPatrol>(FindObjectsSortMode.None);

        foreach (GuardPatrol guard in guards)
            guard.Freeze(true);

        yield return new WaitForSeconds(freezeDuration);

        foreach (GuardPatrol guard in guards)
            guard.Freeze(false);

        freezeActive = false;
    }

    private void RefreshFreezeUI()
    {
        if (freezeCountText != null)
            freezeCountText.text = freezeCount.ToString();
    }
}