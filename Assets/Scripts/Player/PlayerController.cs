using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public TileClass selectedTile;

    public int playerRange;
    public Vector2Int mousePos;
    
    public float moveSpeed;
    public float jumpForce;
    public bool onGround;

    public bool hit;
    public bool place;

    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    [HideInInspector]
    public Vector2 spawnPos;
    public TerrainGeneration terrainGenerator;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void Spawn()
    {
        GetComponent<Transform>().position = spawnPos;
    }
    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Ground"))
            onGround = true;
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Ground"))
            onGround = false;
    }
    private void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float jump = Input.GetAxis("Jump");
        float vertical = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        hit = Input.GetMouseButton(0);
        place = Input.GetMouseButton(1);

        if (Vector2.Distance(transform.position, mousePos) <= playerRange && Vector2.Distance(transform.position, mousePos) > 0.7f)
        {
            if (hit)
                terrainGenerator.RemoveTile(mousePos.x, mousePos.y);
            else if (place)
                terrainGenerator.CheckTile(selectedTile, mousePos.x, mousePos.y, false);
        }

        if (horizontal > 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (horizontal < 0)
            transform.localScale = new Vector3(1, 1, 1);

        if (vertical > 0.1f || jump > 0.1f)
        {
            if (onGround)
                movement.y = jumpForce;
        }
        rb.velocity = movement;
        animator.SetFloat("Walk", Mathf.Abs(horizontal));
        animator.SetBool("hit", hit || place);
    }
    private void Update()
    {
        mousePos.x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 0.5f);
        mousePos.y = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y - 0.5f);
    }
}
