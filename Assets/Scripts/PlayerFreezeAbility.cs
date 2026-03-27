using UnityEngine;
using System.Collections;

public class PlayerFreezeAbility : MonoBehaviour
{
    public bool hasFreeze = false;
    public float freezeDuration = 5f;

    public GameObject freezeIconUI;   // drag FreezeIcon here in Inspector

    bool freezeActive = false;

    void Start()
    {
        if (freezeIconUI != null)
            freezeIconUI.SetActive(false);
    }

    void Update()
    {
        if (hasFreeze && Input.GetKeyDown(KeyCode.T) && !freezeActive)
        {
            StartCoroutine(FreezeGuards());
        }
    }

    public void CollectFreeze()
    {
        hasFreeze = true;

        if (freezeIconUI != null)
            freezeIconUI.SetActive(true);   // show icon when collected
    }

    IEnumerator FreezeGuards()
    {
        freezeActive = true;
        hasFreeze = false;

        if (freezeIconUI != null)
            freezeIconUI.SetActive(false);  // hide icon after using it

        GuardPatrol[] guards = FindObjectsByType<GuardPatrol>(FindObjectsSortMode.None);

        foreach (GuardPatrol guard in guards)
            guard.Freeze(true);

        yield return new WaitForSeconds(freezeDuration);

        foreach (GuardPatrol guard in guards)
            guard.Freeze(false);

        freezeActive = false;
    }
}