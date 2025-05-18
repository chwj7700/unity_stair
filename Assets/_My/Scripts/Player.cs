using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private Vector3 oldPosition;
    private bool isTurn = false;

    private int moveCnt = 0;
    private int turnCnt = 0;
    private int spawnCnt = 0;
    private bool isDie = false;

    private AudioSource sound;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        sound = GetComponent<AudioSource>();

        startPosition = transform.position;
        Init();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            CharTurn();
        }else if (Input.GetMouseButtonDown(0))
        {
            CharMove();
        }
    }

    private void Init()
    {
        anim.SetBool("Die", false);
        transform.position = startPosition;
        oldPosition = startPosition;
        moveCnt = 0;
        spawnCnt = 0;
        turnCnt = 0;
        isTurn = false;
        spriteRenderer.flipX = isTurn;
        isDie = false;
    }

    public void CharTurn()
    {
        isTurn = isTurn == true ? false : true;

        spriteRenderer.flipX = isTurn;
        CharMove();
    }

    public void CharMove()
    {
        if (isDie)
        {
            return;
        }

        sound.Play();

        moveCnt++;
        MoveDirection();
        if(isFailTurn())
        {
            CharDie();
            anim.SetBool("Die", true);
            return;
        }

        if(moveCnt > 7)
        {
            RespawnStair();

        }

        GameManager.Instance.AddScore();
    }

    private void MoveDirection()
    {
        if (isTurn)
        {
            oldPosition += new Vector3(-0.75f, 0.5f, 0);
        }
        else
        {
            oldPosition += new Vector3(0.75f, 0.5f, 0);
        }

        transform.position = oldPosition;
        anim.SetTrigger("Move");
    }

    private bool isFailTurn()
    {
        bool result = false;

        if (GameManager.Instance.isTurn[turnCnt] != isTurn)
        {
            result = true;
        }

        turnCnt++;

        if (turnCnt > GameManager.Instance.Stairs.Length - 1)// 0~19 Length == 20
        {
            turnCnt = 0;
        }
        return result;
    }

    private void RespawnStair()
    {
        GameManager.Instance.SpawnStair(spawnCnt);

        spawnCnt++;

        if(spawnCnt > GameManager.Instance.Stairs.Length - 1)
        {
            spawnCnt = 0;
        }
    }

    private void CharDie()
    {
        GameManager.Instance.GameOver();

        anim.SetBool("Die", true);
        isDie = true;
    }

    public void ButtonRestart()
    {
        Init();
        GameManager.Instance.Init();
        GameManager.Instance.InitStairs();
    }
    
    // 캐릭터 스프라이트 변경 메서드
    public void ChangeCharacterSprite(Sprite newSprite)
    {
        if(spriteRenderer != null && newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
        }
    }
}
