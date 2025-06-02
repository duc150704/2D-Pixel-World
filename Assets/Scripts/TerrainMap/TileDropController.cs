using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDropController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            //them vao tui do
            Destroy(this.gameObject);
        }
    }
}
