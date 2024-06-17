using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    Player player;
    private void Start()
    {
        StartCoroutine(GetPlayer());
    }

    private void LateUpdate()
    {
        if (player == null) return;

        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
    }

    IEnumerator GetPlayer()
    {
        while (player == null)
        {
            player = FindObjectOfType<Player>();
            if (player != null && !player.IsOwner) player = null;
            else if (player != null) break;

            yield return null;
        }
    }

}
