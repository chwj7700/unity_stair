using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // Image 컴포넌트 사용을 위해 추가

/// <summary>
/// 게임의 전체적인 상태와 로직을 관리하는 클래스
/// 싱글톤 패턴으로 구현되어 다른 스크립트에서 쉽게 접근 가능
/// </summary>
public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager Instance;

    [Header("계단")]
    [Space(10)]
    public const int TOTAL_STAIRS = 120;  // 총 계단 수
    public bool isGameCleared = false;   // 게임 클리어 여부

    [Header("게임 클리어")]
    [Space(10)]
    public GameObject clearUI;            // 클리어 UI 오브젝트
    public Image gameClearImage;         // 게임 클리어 이미지
    public Sprite[] gameClearSprites;    // 게임 클리어 이미지 배열

    [Header("계단")]
    [Space(10)]
    public GameObject[] Stairs;       // 계단 오브젝트 배열
    public bool[] isTurn;             // 각 계단의 방향 정보 (false: 오른쪽, true: 왼쪽)

    private enum State {Start,Left,Right};  // 계단 생성 상태 열거형
    private State state;              // 현재 계단 생성 상태
    private Vector3 oldPosition;      // 마지막 계단 위치 저장용

    [Header("UI")]
    [Space(10)]
    public GameObject UI_GameOver;    // 게임오버 UI 오브젝트
    public TextMeshProUGUI textMaxScore;  // 최고 점수 텍스트
    public TextMeshProUGUI textNowScore;  // 현재 점수 텍스트 (게임오버 화면)
    public TextMeshProUGUI textShowScore; // 현재 점수 텍스트 (게임 진행 중)
    public TextMeshProUGUI textPlayTime;  // 플레이 시간 텍스트
    private int maxScore = 0;         // 최고 점수 저장
    private int nowScore = 0;         // 현재 점수 저장
    private float playTime = 0f;      // 플레이 시간 저장
    private bool isPlaying = false;   // 게임 진행 상태

    [Header("배경")]
    [Space(10)]
    public Image backgroundImage;     // UI Canvas에 추가한 배경 이미지
    public Sprite[] backgroundSprites; // 사용할 배경 이미지들
    private int currentBackgroundIndex = 0; // 현재 배경 인덱스
    public int backgroundChangeInterval = 100; // 배경 변경 간격 (100점마다)
    private Vector2 initialBackgroundPosition; // 초기 배경 이미지 위치 저장

    [Header("캐릭터")]
    [Space(10)]
    public Player playerReference;    // 플레이어 오브젝트 참조
    public Sprite[] characterSprites; // 캐릭터 스프라이트 배열
    private int currentCharacterIndex = 0;  // 현재 캐릭터 인덱스
    
    [Header("캐릭터 애니메이션")]
    [Space(10)]
    public Sprite[] characterIdleSprites; // 가만히 서있을 때의 스프라이트 배열 (1_1, 2_1, 3_1)
    public Sprite[] characterMoveSprites; // 이동할 때의 스프라이트 배열 (1_2, 2_2, 3_2)

    [Header("Audio")]
    [Space(10)]
    private AudioSource sound;        // 오디오 소스 컴포넌트
    public AudioClip bgmSound;        // 배경 음악
    public AudioClip dieSound;        // 사망 효과음

    /// <summary>
    /// 시작 시 초기화 및 싱글톤 인스턴스 설정
    /// </summary>
    void Start()
    {
        // 싱글톤 인스턴스 설정
        Instance = this;

        // 오디오 소스 컴포넌트 가져오기
        sound = GetComponent<AudioSource>();
        
        // 초기 배경 위치 저장
        if (backgroundImage != null && backgroundImage.rectTransform != null)
        {
            initialBackgroundPosition = backgroundImage.rectTransform.anchoredPosition;
        }
        
        // 게임 초기화
        Init();
        InitStairs();
    }

    /// <summary>
    /// 게임 상태 초기화
    /// </summary>
    public void Init()
    {
        // 계단 생성 상태 초기화
        state = State.Start;
        oldPosition = Vector3.zero;

        // 계단 방향 배열 초기화
        isTurn = new bool[Stairs.Length];

        // 모든 계단 초기화
        for (int i = 0; i < Stairs.Length; i++)
        {
            Stairs[i].transform.position = Vector3.zero;
            isTurn[i] = false;
        }

        // 점수 및 인덱스 초기화
        nowScore = 0;
        currentBackgroundIndex = 0;
        currentCharacterIndex = 0;
        playTime = 0f;
        isPlaying = true;
        isGameCleared = false;
        
        // 초기 배경 설정
        if(backgroundImage != null && backgroundSprites.Length > 0)
        {
            backgroundImage.sprite = backgroundSprites[0];
            
            // 배경 이미지 위치를 초기 좌표로 리셋
            if (backgroundImage.rectTransform != null)
            {
                backgroundImage.rectTransform.anchoredPosition = initialBackgroundPosition;
            }
        }
        
        // 초기 캐릭터 스프라이트 설정 (Idle 스프라이트 사용)
        if(playerReference != null && characterIdleSprites != null && characterIdleSprites.Length > 0)
        {
            playerReference.ChangeCharacterSprite(characterIdleSprites[0]);
        }

        // UI 초기화
        if (clearUI != null)
            clearUI.SetActive(false);
        if (UI_GameOver != null)
            UI_GameOver.SetActive(false);

        // UI 텍스트 초기화
        textShowScore.text = nowScore.ToString();
        textPlayTime.text = "00:00";

        // 배경 음악 설정 및 재생
        sound.clip = bgmSound;
        sound.Play();
        sound.loop = true;
        sound.volume = 0.4f;
    }

    /// <summary>
    /// 초기 계단들 생성 및 배치
    /// </summary>
    public void InitStairs()
    {
        for (int i=0; i<Stairs.Length; i++)
        {
            switch (state)
            {
                case State.Start:
                    // 첫 번째 계단은 특정 위치에 배치
                    Stairs[i].transform.position = new Vector3(0.75f, -0.1f, 0);
                    state = State.Right;
                    break;
                case State.Left:
                    // 왼쪽으로 올라가는 계단 배치
                    Stairs[i].transform.position = oldPosition + new Vector3(-0.75f, 0.5f, 0);
                    isTurn[i] = true;
                    break;
                case State.Right:
                    // 오른쪽으로 올라가는 계단 배치
                    Stairs[i].transform.position = oldPosition + new Vector3(0.75f, 0.5f, 0);
                    isTurn[i] = false;
                    break;
            }

            // 현재 위치 저장
            oldPosition = Stairs[i].transform.position;

            // 첫 번째 계단이 아니면 일정 확률로 방향 전환
            if(i != 0)
            {
                int ran = Random.Range(0, 5);

                if(ran < 2 && i < Stairs.Length - 1)
                {
                    state = state == State.Left ? State.Right : State.Left;
                }
            }
        }
    }

    /// <summary>
    /// 새로운 계단 생성 및 배치
    /// </summary>
    /// <param name="cnt">생성할 계단의 인덱스</param>
    public void SpawnStair(int cnt)
    {
        // 일정 확률로 방향 전환
        int ran = Random.Range(0, 5);

        if (ran < 2)
        {
            state = state == State.Left ? State.Right : State.Left;
        }

        switch (state)
        {
            case State.Left:
                // 왼쪽으로 향하는 계단 생성
                Stairs[cnt].transform.position = oldPosition + new Vector3(-0.75f, 0.5f, 0);
                isTurn[cnt] = true;
                break;
            case State.Right:
                // 오른쪽으로 향하는 계단 생성
                Stairs[cnt].transform.position = oldPosition + new Vector3(0.75f, 0.5f, 0);
                isTurn[cnt] = false;
                break;
        }

        // 현재 위치 저장
        oldPosition = Stairs[cnt].transform.position;
    }

    /// <summary>
    /// 게임 오버 처리
    /// </summary>
    public void GameOver()
    {
        isPlaying = false;
        // 배경 음악 중지 및 사망 효과음 재생
        sound.loop = false;
        sound.Stop();
        sound.clip = dieSound;
        sound.Play();
        sound.volume = 1;

        // 게임오버 UI 표시 코루틴 시작
        StartCoroutine(ShowGameOver());
    }

    /// <summary>
    /// 게임오버 UI를 지연 표시하는 코루틴
    /// </summary>
    IEnumerator ShowGameOver()
    {
        // 1초 대기
        yield return new WaitForSeconds(1f);

        // 게임오버 UI 활성화
        UI_GameOver.SetActive(true);

        // 최고 점수 갱신 확인
        if(nowScore > maxScore)
        {
            maxScore = nowScore;
        }

        // 점수 텍스트 업데이트
        textMaxScore.text = maxScore.ToString();
        textNowScore.text = nowScore.ToString();
    }

    /// <summary>
    /// 점수 증가 및 일정 점수마다 배경과 캐릭터 변경
    /// </summary>
    public void AddScore()
    {
        if (isGameCleared) return;

        nowScore++;
        textShowScore.text = nowScore.ToString();
        
        // 마지막 계단에 도달하면 게임 클리어
        if (nowScore >= TOTAL_STAIRS)
        {
            GameClear();
            return;
        }

        // 일정 점수마다 배경과 캐릭터 변경
        if (nowScore % backgroundChangeInterval == 0)
        {
            ChangeBackground();
            ChangeCharacter();
        }
    }
    
    /// <summary>
    /// 배경 이미지 변경 함수
    /// </summary>
    private void ChangeBackground()
    {
        // 배경 이미지나 스프라이트가 없으면 리턴
        if(backgroundImage == null || backgroundSprites.Length == 0)
            return;
            
        // 다음 배경 인덱스 계산 (순환)
        currentBackgroundIndex = (currentBackgroundIndex + 1) % backgroundSprites.Length;
        
        // 배경 이미지 변경
        backgroundImage.sprite = backgroundSprites[currentBackgroundIndex];
        
        // 배경 이미지 위치를 초기 좌표로 리셋
        if (backgroundImage != null && backgroundImage.rectTransform != null)
        {
            // 배경 이미지의 위치를 초기 저장해둔 좌표로 리셋
            backgroundImage.rectTransform.anchoredPosition = initialBackgroundPosition;
        }
    }
    
    /// <summary>
    /// 캐릭터 스프라이트 변경 함수
    /// </summary>
    private void ChangeCharacter()
    {
        // 플레이어 참조나 스프라이트가 없으면 리턴
        if(playerReference == null || characterIdleSprites == null || characterIdleSprites.Length == 0)
            return;
            
        // 다음 캐릭터 인덱스 계산 (순환)
        currentCharacterIndex = (currentCharacterIndex + 1) % characterIdleSprites.Length;
        
        // 캐릭터 스프라이트 변경 (Idle 스프라이트 사용)
        playerReference.ChangeCharacterSprite(characterIdleSprites[currentCharacterIndex]);
    }
    
    /// <summary>
    /// 게임 클리어 처리
    /// </summary>
    private void GameClear()
    {
        isPlaying = false;
        isGameCleared = true;
        
        // 배경 음악 중지
        sound.Stop();
        
        // 클리어 UI 표시
        if (clearUI != null)
        {
            clearUI.SetActive(true);
            
            // 시간에 따른 결과 이미지 출력
            if (gameClearImage != null && gameClearSprites != null && gameClearSprites.Length >= 4)
            {
                int resultIndex;
                if (playTime <= 10f)
                    resultIndex = 0;
                else if (playTime <= 15f)
                    resultIndex = 1;
                else if (playTime <= 20f)
                    resultIndex = 2;
                else
                    resultIndex = 3;
                    
                gameClearImage.sprite = gameClearSprites[resultIndex];
            }
        }
    }

    /// <summary>
    /// 플레이 시간 업데이트
    /// </summary>
    void Update()
    {
        if (isPlaying)
        {
            playTime += Time.deltaTime;
            UpdatePlayTimeDisplay();
        }
    }

    /// <summary>
    /// 플레이 시간 표시 업데이트
    /// </summary>
    private void UpdatePlayTimeDisplay()
    {
        int minutes = Mathf.FloorToInt(playTime / 60);
        int seconds = Mathf.FloorToInt(playTime % 60);
        textPlayTime.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    /// <summary>
    /// 배경 이미지 위치를 이동시키는 메서드
    /// </summary>
    /// <param name="direction">이동 방향 (true: 왼쪽, false: 오른쪽)</param>
    public void MoveBackground(bool direction)
    {
        if (backgroundImage == null)
            return;
            
        // 배경의 RectTransform 가져오기
        RectTransform rectTransform = backgroundImage.rectTransform;
        
        // 현재 위치 가져오기
        Vector2 currentPosition = rectTransform.anchoredPosition;
        
        // 이동 방향에 따라 배경 이동 (플레이어 이동과 반대 방향으로 이동)
        // 플레이어가 오른쪽으로 이동하면 배경은 왼쪽으로 이동
        float moveAmountX = 5.0f; // 좌우 이동 거리 조정
        float moveAmountY = 3.0f; // 상하 이동 거리 조정 (음수 값으로 설정하여 아래로 이동)
        
        if (direction) // 플레이어가 왼쪽으로 이동
        {
            // 배경은 오른쪽으로 이동하고 위로 이동
            rectTransform.anchoredPosition = new Vector2(
                currentPosition.x + moveAmountX, 
                currentPosition.y - moveAmountY); // 위로 이동하기 위해 음수 부호 변경
        }
        else // 플레이어가 오른쪽으로 이동
        {
            // 배경은 왼쪽으로 이동하고 위로 이동
            rectTransform.anchoredPosition = new Vector2(
                currentPosition.x - moveAmountX, 
                currentPosition.y - moveAmountY); // 위로 이동하기 위해 음수 부호 변경
        }
    }
    
    /// <summary>
    /// 캐릭터를 이동 스프라이트로 잠시 변경하는 메서드
    /// </summary>
    public void PlayCharacterMoveAnimation()
    {
        if (playerReference != null && characterMoveSprites != null && characterMoveSprites.Length > currentCharacterIndex)
        {
            // 현재 캐릭터 인덱스에 맞는 이동 스프라이트로 변경
            playerReference.ChangeCharacterSprite(characterMoveSprites[currentCharacterIndex]);
            
            // 잠시 후 원래 스프라이트로 돌아가는 코루틴 시작
            StartCoroutine(RestoreCharacterIdleSprite());
        }
    }
    
    /// <summary>
    /// 잠시 후 캐릭터를 원래 Idle 스프라이트로 돌려놓는 코루틴
    /// </summary>
    private IEnumerator RestoreCharacterIdleSprite()
    {
        // 0.2초 대기 (애니메이션 시간)
        yield return new WaitForSeconds(0.2f);
        
        // 원래 Idle 스프라이트로 복원
        if (playerReference != null && characterIdleSprites != null && characterIdleSprites.Length > currentCharacterIndex)
        {
            playerReference.ChangeCharacterSprite(characterIdleSprites[currentCharacterIndex]);
        }
    }
    
    /// <summary>
    /// 현재 캐릭터를 Idle 스프라이트로 설정하는 메서드
    /// </summary>
    public void SetCharacterToIdle()
    {
        if (playerReference != null && characterIdleSprites != null && characterIdleSprites.Length > currentCharacterIndex)
        {
            playerReference.ChangeCharacterSprite(characterIdleSprites[currentCharacterIndex]);
        }
    }
}
