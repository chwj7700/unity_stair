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

    [Header("캐릭터")]
    [Space(10)]
    public Player playerReference;    // 플레이어 오브젝트 참조
    public Sprite[] characterSprites; // 캐릭터 스프라이트 배열
    private int currentCharacterIndex = 0;  // 현재 캐릭터 인덱스

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
        
        // 초기 배경 설정
        if(backgroundImage != null && backgroundSprites.Length > 0)
        {
            backgroundImage.sprite = backgroundSprites[0];
        }
        
        // 초기 캐릭터 스프라이트 설정
        if(playerReference != null && characterSprites.Length > 0)
        {
            playerReference.ChangeCharacterSprite(characterSprites[0]);
        }

        // UI 텍스트 초기화
        textShowScore.text = nowScore.ToString();
        textPlayTime.text = "00:00";

        // 게임오버 UI 비활성화
        UI_GameOver.SetActive(false);

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
        // 점수 증가
        nowScore++;
        textShowScore.text = nowScore.ToString();
        
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
    }
    
    /// <summary>
    /// 캐릭터 스프라이트 변경 함수
    /// </summary>
    private void ChangeCharacter()
    {
        // 플레이어 참조나 스프라이트가 없으면 리턴
        if(playerReference == null || characterSprites.Length == 0)
            return;
            
        // 다음 캐릭터 인덱스 계산 (순환)
        currentCharacterIndex = (currentCharacterIndex + 1) % characterSprites.Length;
        
        // 캐릭터 스프라이트 변경
        playerReference.ChangeCharacterSprite(characterSprites[currentCharacterIndex]);
    }

    void Update()
    {
        if (isPlaying)
        {
            playTime += Time.deltaTime;
            UpdatePlayTimeDisplay();
        }
    }

    private void UpdatePlayTimeDisplay()
    {
        int minutes = Mathf.FloorToInt(playTime / 60);
        int seconds = Mathf.FloorToInt(playTime % 60);
        textPlayTime.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
