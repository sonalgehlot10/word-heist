using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class PlayerFreezeAbility : MonoBehaviour
{
    [Header("Freeze Use")]
    [SerializeField] private KeyCode useKey = KeyCode.T;
    [SerializeField] private float freezeDuration = 5f;
    public static bool FreezeActive { get; private set; }
    private Collider2D[] frozenGuardColliders;
    private Collider2D playerCollider;

    [Header("HUD")]
    [SerializeField] private TMP_Text freezeCountText;

    private int freezeCount = 0;
    private bool freezeActive = false;

    private void Start()
    {
        playerCollider = GetComponent<Collider2D>();
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

        List<Collider2D> colliders = new List<Collider2D>();

        foreach (GuardPatrol guard in guards)
        {
            guard.Freeze(true);

            Collider2D guardCol = guard.GetComponent<Collider2D>();
            if (guardCol != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, guardCol, true);
                colliders.Add(guardCol);
            }
        }

        yield return new WaitForSeconds(freezeDuration);

        foreach (GuardPatrol guard in guards)
        {
            guard.Freeze(false);
        }

        foreach (Collider2D guardCol in colliders)
        {
            if (guardCol != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, guardCol, false);
            }
        }

        freezeActive = false;
    } 

    private void RefreshFreezeUI()
    {
        if (freezeCountText != null)
            freezeCountText.text = freezeCount.ToString();
    }
}