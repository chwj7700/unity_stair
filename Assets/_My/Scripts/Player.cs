using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 캐릭터를 제어하는 클래스
/// 이동, 회전, 사망 처리 및 계단 생성 로직을 담당
/// </summary>
public class Player : MonoBehaviour
{
    private Animator anim;            // 캐릭터 애니메이션 제어용 애니메이터
    private SpriteRenderer spriteRenderer;  // 캐릭터 스프라이트 렌더러
    private Vector3 startPosition;    // 캐릭터 시작 위치
    private Vector3 oldPosition;      // 이동 계산을 위한 이전 위치
    private bool isTurn = false;      // 현재 방향 상태 (false: 오른쪽, true: 왼쪽)

    private int moveCnt = 0;          // 이동 횟수 카운터
    private int turnCnt = 0;          // 턴 인덱스 카운터
    private int spawnCnt = 0;         // 계단 스폰 인덱스 카운터
    private bool isDie = false;       // 사망 상태 플래그

    private AudioSource sound;        // 효과음 재생용 오디오 소스

    /// <summary>
    /// 시작 시 초기화 작업
    /// </summary>
    void Start()
    {
        // 컴포넌트 참조 가져오기
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        sound = GetComponent<AudioSource>();

        // 시작 위치 저장 및 초기화
        startPosition = transform.position;
        Init();
    }

    /// <summary>
    /// 매 프레임마다 입력 감지
    /// </summary>
    void Update()
    {
        // 사망 상태일 때 스페이스바로 재시작
        if (isDie || GameManager.Instance.isGameCleared)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ButtonRestart();
            }
            return;
        }

        HandleInput();
    }

    /// <summary>
    /// 플레이어 입력 처리
    /// </summary>
    private void HandleInput()
    {
        switch (true)
        {
            // 좌측 방향키: 왼쪽으로 캐릭터 이동
            case bool _ when Input.GetKeyDown(KeyCode.LeftArrow):
                MoveLeft();
                break;
            // 우측 방향키: 오른쪽으로 캐릭터 이동
            case bool _ when Input.GetKeyDown(KeyCode.RightArrow):
                MoveRight();
                break;
        }
    }

    /// <summary>
    /// 캐릭터와 게임 상태 초기화
    /// </summary>
    private void Init()
    {
        // 사망 애니메이션 비활성화
        anim.SetBool("Die", false);
        
        // 위치 초기화
        transform.position = startPosition;
        oldPosition = startPosition;
        
        // 카운터 초기화
        moveCnt = 0;
        spawnCnt = 0;
        turnCnt = 0;
        
        // 방향 초기화 (오른쪽)
        isTurn = false;
        spriteRenderer.flipX = isTurn;
        
        // 사망 상태 해제
        isDie = false;
    }

    /// <summary>
    /// 캐릭터 방향 전환 및 이동 처리
    /// </summary>
    public void CharTurn()
    {
        // 방향 상태 반전
        isTurn = isTurn == true ? false : true;

        // 스프라이트 방향 변경
        spriteRenderer.flipX = isTurn;
        
        // 배경 이미지 이동 (GameManager를 통해)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MoveBackground(isTurn);
        }
        
        // 방향 전환 후 이동
        CharMove();
    }

    /// <summary>
    /// 캐릭터 이동 처리
    /// </summary>
    public void CharMove()
    {
        // 사망 상태면 이동 불가
        if (isDie)
        {
            return;
        }

        // 이동 효과음 재생
        sound.Play();

        // 이동 카운트 증가
        moveCnt++;
        
        // 실제 이동 처리
        MoveDirection();
        
        // 캐릭터 이동 애니메이션 재생
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayCharacterMoveAnimation();
        }
        
        // 잘못된 방향으로 이동했는지 확인
        if(isFailTurn())
        {
            CharDie();
            anim.SetBool("Die", true);
            return;
        }

        // 일정 횟수 이상 이동 시 새 계단 생성
        if(moveCnt > 7)
        {
            RespawnStair();
        }

        // 점수 증가
        GameManager.Instance.AddScore();
    }

    /// <summary>
    /// 왼쪽으로 이동
    /// </summary>
    private void MoveLeft()
    {
        if (!isDie)
        {
            isTurn = true;
            spriteRenderer.flipX = isTurn;
            
            // 배경 이미지 이동 (GameManager를 통해)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.MoveBackground(isTurn);
            }
            
            CharMove();
        }
    }

    /// <summary>
    /// 오른쪽으로 이동
    /// </summary>
    private void MoveRight()
    {
        if (!isDie)
        {
            isTurn = false;
            spriteRenderer.flipX = isTurn;
            
            // 배경 이미지 이동 (GameManager를 통해)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.MoveBackground(isTurn);
            }
            
            CharMove();
        }
    }

    /// <summary>
    /// 현재 방향에 따른 이동 위치 계산
    /// </summary>
    private void MoveDirection()
    {
        // 왼쪽 방향이면 왼쪽 위로 이동
        if (isTurn)
        {
            oldPosition += new Vector3(-0.75f, 0.5f, 0);
        }
        // 오른쪽 방향이면 오른쪽 위로 이동
        else
        {
            oldPosition += new Vector3(0.75f, 0.5f, 0);
        }

        // 캐릭터 위치 업데이트
        transform.position = oldPosition;
        
        // 이동 애니메이션 트리거
        anim.SetTrigger("Move");
    }

    /// <summary>
    /// 현재 계단의 방향과 플레이어 방향이 일치하는지 확인
    /// </summary>
    /// <returns>실패 여부 (true: 실패, false: 성공)</returns>
    private bool isFailTurn()
    {
        bool result = false;

        // 현재 계단의 방향과 플레이어 방향이 다르면 실패
        if (GameManager.Instance.isTurn[turnCnt] != isTurn)
        {
            result = true;
        }

        // 다음 계단 인덱스로 이동
        turnCnt++;

        // 인덱스가 범위를 벗어나면 처음으로 돌아감
        if (turnCnt > GameManager.Instance.Stairs.Length - 1)// 0~19 Length == 20
        {
            turnCnt = 0;
        }
        return result;
    }

    /// <summary>
    /// 새로운 계단 생성 요청
    /// </summary>
    private void RespawnStair()
    {
        // TOTAL_STAIRS 이상의 계단은 생성하지 않음
        if (moveCnt >= GameManager.TOTAL_STAIRS - 12)
        {
            return;
        }

        // GameManager에 새 계단 생성 요청
        GameManager.Instance.SpawnStair(spawnCnt);

        // 다음 스폰 인덱스로 이동
        spawnCnt++;

        // 인덱스가 범위를 벗어나면 처음으로 돌아감
        if(spawnCnt > GameManager.Instance.Stairs.Length - 1)
        {
            spawnCnt = 0;
        }
    }

    /// <summary>
    /// 캐릭터 사망 처리
    /// </summary>
    private void CharDie()
    {
        // 게임오버 처리 요청
        GameManager.Instance.GameOver();

        // 사망 애니메이션 재생
        anim.SetBool("Die", true);
        
        // 사망 상태 설정
        isDie = true;
    }

    /// <summary>
    /// 재시작 버튼 클릭 시 호출되는 메서드
    /// </summary>
    public void ButtonRestart()
    {
        // 플레이어 초기화
        Init();
        
        // 게임매니저 초기화
        GameManager.Instance.Init();
        GameManager.Instance.InitStairs();
        
        // 캐릭터를 Idle 스프라이트로 설정
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCharacterToIdle();
        }
    }
    
    /// <summary>
    /// 캐릭터 스프라이트 변경 메서드
    /// </summary>
    /// <param name="newSprite">적용할 새 스프라이트</param>
    public void ChangeCharacterSprite(Sprite newSprite)
    {
        // 스프라이트 렌더러와 새 스프라이트가 유효한지 확인
        if(spriteRenderer != null && newSprite != null)
        {
            // 스프라이트 변경
            spriteRenderer.sprite = newSprite;
        }
    }
}
