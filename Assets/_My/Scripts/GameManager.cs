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

    [Header("속도 계산 시스템")]
    [Space(10)]
    public TextMeshProUGUI textSpeedInfo;     // 속도 정보 표시 텍스트 (선택사항)
    public TextMeshProUGUI textSpeedGrade;    // 속도 등급 표시 텍스트 (선택사항)
    private SpeedCalculator.SpeedCalculationResult currentSpeedResult; // 현재 속도 계산 결과
    
    [Header("속도 계산 디버그")]
    [Space(10)]
    public bool showSpeedDebug = true;        // 속도 계산 디버그 정보 표시 여부
    public bool logSpeedOnUpdate = false;     // 매 프레임 속도 계산 로그 출력 여부

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

    [Header("계단 시스템")]
    [Space(10)]
    public TextMeshProUGUI textLifeStage;         // 인생 단계 표시 텍스트 (선택사항)
    public TextMeshProUGUI textStairLevel;        // 계단 수준 표시 텍스트 (선택사항)
    public TextMeshProUGUI textStageProgress;     // 단계 진행률 표시 텍스트 (선택사항)
    private StairSystem.StairStatus currentStairStatus; // 현재 계단 상태 결과
    
    [Header("계단 시스템 디버그")]
    [Space(10)]
    public bool showStairDebug = true;        // 계단 시스템 디버그 정보 표시 여부
    public bool logStairOnUpdate = false;     // 매 프레임 계단 상태 로그 출력 여부

    [Header("직업 시스템")]
    [Space(10)]
    public TextMeshProUGUI textJobName;           // 직업 이름 표시 텍스트 (선택사항)
    public TextMeshProUGUI textJobDescription;   // 직업 설명 표시 텍스트 (선택사항)
    private JobSystem.JobResult currentJobResult; // 현재 직업 결정 결과
    
    [Header("직업 시스템 디버그")]
    [Space(10)]
    public bool showJobDebug = true;          // 직업 시스템 디버그 정보 표시 여부
    public bool logJobOnUpdate = false;       // 매 프레임 직업 결정 로그 출력 여부

    [Header("엔딩 시스템")]
    [Space(10)]
    public TextMeshProUGUI textEndingTitle;       // 엔딩 제목 표시 텍스트 (선택사항)
    public TextMeshProUGUI textEndingMessage;     // 엔딩 메시지 표시 텍스트 (선택사항)
    public TextMeshProUGUI textEndingSubtext;     // 엔딩 부제 표시 텍스트 (선택사항)
    public Image endingCardImage;                 // 엔딩 카드 이미지 (선택사항)
    public GameObject endingCardPanel;            // 엔딩 카드 패널 (선택사항)
    public Sprite[] jobCardSprites;               // 직업별 카드 스프라이트 배열 (14개)
    public TextMeshProUGUI textGameResult;        // 게임 결과 텍스트 (Clear/Over 표시)
    private EndingSystem.EndingResult finalEndingResult; // 최종 엔딩 결과
    
    [Header("엔딩 시스템 디버그")]
    [Space(10)]
    public bool showEndingDebug = true;       // 엔딩 시스템 디버그 정보 표시 여부

    [Header("한글 폰트 설정")]
    [Space(10)]
    public TMP_FontAsset koreanFontAsset;         // 한글 폰트 에셋
    public bool useKoreanFont = true;             // 한글 폰트 사용 여부

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
        
        // 한글 폰트 적용
        ApplyKoreanFont();
        
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
    /// 게임 클리어 처리
    /// </summary>
    private void GameClear()
    {
        isPlaying = false;
        isGameCleared = true;
        
        // 최종 속도 계산 결과 가져오기
        SpeedCalculator.SpeedCalculationResult finalSpeedResult = GetFinalSpeedResult();
        
        // 최종 계단 상태 결과 가져오기
        StairSystem.StairStatus finalStairStatus = GetFinalStairStatus();
        
        // 최종 직업 결정 결과 가져오기
        JobSystem.JobResult finalJobResult = GetFinalJobResult();
        
        // 최종 엔딩 생성
        finalEndingResult = EndingSystem.GenerateEnding(finalJobResult, finalSpeedResult, finalStairStatus, true);
        
        // 배경 음악 중지
        sound.Stop();
        
        // 최종 결과 로그 출력
        if (showSpeedDebug || showStairDebug || showJobDebug || showEndingDebug)
        {
            Debug.Log("=== 게임 클리어! 최종 결과 ===");
            
            if (showSpeedDebug)
            {
                SpeedCalculator.LogSpeedResult(finalSpeedResult);
            }
            
            if (showStairDebug)
            {
                StairSystem.LogStairStatus(finalStairStatus);
            }
            
            if (showJobDebug)
            {
                JobSystem.LogJobResult(finalJobResult);
            }
            
            if (showEndingDebug)
            {
                EndingSystem.LogEndingResult(finalEndingResult);
            }
        }
        
        // 게임 결과 UI 표시 (clearUI 대신 UI_GameOver 사용)
        StartCoroutine(ShowGameResult());
    }

    /// <summary>
    /// 게임 오버 처리
    /// </summary>
    public void GameOver()
    {
        isPlaying = false;
        
        // 게임오버 시 최종 속도 계산 결과 가져오기
        SpeedCalculator.SpeedCalculationResult finalSpeedResult = GetFinalSpeedResult();
        
        // 게임오버 시 최종 계단 상태 결과 가져오기
        StairSystem.StairStatus finalStairStatus = GetFinalStairStatus();
        
        // 게임오버 시 최종 직업 결정 결과 가져오기
        JobSystem.JobResult finalJobResult = GetFinalJobResult();
        
        // 게임오버 엔딩 생성
        finalEndingResult = EndingSystem.GenerateEnding(finalJobResult, finalSpeedResult, finalStairStatus, false);
        
        // 배경 음악 중지 및 사망 효과음 재생
        sound.loop = false;
        sound.Stop();
        sound.clip = dieSound;
        sound.Play();
        sound.volume = 1;

        // 게임오버 시 결과 로그 출력
        if (showSpeedDebug || showStairDebug || showJobDebug || showEndingDebug)
        {
            Debug.Log("=== 게임 오버! 최종 결과 ===");
            
            if (showSpeedDebug)
            {
                SpeedCalculator.LogSpeedResult(finalSpeedResult);
            }
            
            if (showStairDebug)
            {
                StairSystem.LogStairStatus(finalStairStatus);
            }
            
            if (showJobDebug)
            {
                JobSystem.LogJobResult(finalJobResult);
            }
            
            if (showEndingDebug)
            {
                EndingSystem.LogEndingResult(finalEndingResult);
            }
        }

        // 게임오버 UI 표시 코루틴 시작
        StartCoroutine(ShowGameResult());
    }

    /// <summary>
    /// 게임 결과 UI를 지연 표시하는 코루틴 (클리어/오버 공통)
    /// </summary>
    IEnumerator ShowGameResult()
    {
        // 1초 대기
        yield return new WaitForSeconds(1f);

        // 게임 결과 UI 활성화
        UI_GameOver.SetActive(true);

        // 최고 점수 갱신 확인
        if(nowScore > maxScore)
        {
            maxScore = nowScore;
        }

        // 점수 텍스트 업데이트
        textMaxScore.text = maxScore.ToString();
        textNowScore.text = nowScore.ToString();
        
        // 게임 결과 텍스트 업데이트
        if (textGameResult != null)
        {
            if (isGameCleared)
            {
                textGameResult.text = "🎉 GAME CLEAR! 🎉";
                textGameResult.color = Color.yellow;
            }
            else
            {
                textGameResult.text = "💀 GAME OVER 💀";
                textGameResult.color = Color.red;
            }
        }
        
        // 엔딩 UI 업데이트 (카드 포함)
        UpdateEndingUI();
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
    /// 속도 등급에 따른 결과 이미지 인덱스를 반환합니다
    /// </summary>
    /// <param name="speedGrade">속도 등급</param>
    /// <returns>결과 이미지 인덱스</returns>
    private int GetResultIndexBySpeedGrade(SpeedCalculator.SpeedGrade speedGrade)
    {
        switch (speedGrade)
        {
            case SpeedCalculator.SpeedGrade.VeryFast:
                return 0; // 매우 빠름
            case SpeedCalculator.SpeedGrade.Fast:
                return 1; // 빠름
            case SpeedCalculator.SpeedGrade.Normal:
                return 2; // 보통
            case SpeedCalculator.SpeedGrade.Slow:
                return 3; // 느림
            case SpeedCalculator.SpeedGrade.VerySlow:
                return 4; // 매우 느림
            default:
                return 2; // 기본값: 보통
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
            
            // 속도 계산 업데이트
            UpdateSpeedCalculation();
            
            // 계단 시스템 업데이트
            UpdateStairSystem();
            
            // 직업 시스템 업데이트
            UpdateJobSystem();
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
    /// 계단 시스템 업데이트
    /// </summary>
    private void UpdateStairSystem()
    {
        // 현재 계단 상태 계산
        currentStairStatus = StairSystem.CalculateStairStatus(nowScore);
        
        // 계단 시스템 UI 업데이트
        UpdateStairUI();
        
        // 디버그 로그 출력 (옵션)
        if (logStairOnUpdate)
        {
            StairSystem.LogStairStatus(currentStairStatus);
        }
    }
    
    /// <summary>
    /// 계단 관련 UI 업데이트
    /// </summary>
    private void UpdateStairUI()
    {
        // 인생 단계 텍스트 업데이트
        if (textLifeStage != null)
        {
            textLifeStage.text = $"{currentStairStatus.lifeStageName}";
            
            // 인생 단계에 따른 색상 변경
            UpdateLifeStageColor();
        }
        
        // 계단 수준 텍스트 업데이트
        if (textStairLevel != null)
        {
            textStairLevel.text = $"수준: {currentStairStatus.stairLevelName}";
            
            // 계단 수준에 따른 색상 변경
            UpdateStairLevelColor();
        }
        
        // 단계 진행률 텍스트 업데이트
        if (textStageProgress != null)
        {
            textStageProgress.text = $"단계 진행률: {currentStairStatus.stageProgressRatio:P1}\n" +
                                   $"다음까지: {currentStairStatus.stairsToNextStage}계단";
        }
    }
    
    /// <summary>
    /// 인생 단계에 따른 텍스트 색상 변경
    /// </summary>
    private void UpdateLifeStageColor()
    {
        if (textLifeStage == null) return;
        
        Color stageColor = Color.white;
        
        switch (currentStairStatus.lifeStage)
        {
            case StairSystem.LifeStage.Boy:
                stageColor = Color.cyan;       // 하늘색 - 소년
                break;
            case StairSystem.LifeStage.Teenager:
                stageColor = Color.yellow;     // 노란색 - 청소년
                break;
            case StairSystem.LifeStage.YoungAdult:
                stageColor = Color.green;      // 초록색 - 청년
                break;
            case StairSystem.LifeStage.Completed:
                stageColor = Color.magenta;    // 마젠타색 - 완료
                break;
        }
        
        textLifeStage.color = stageColor;
    }
    
    /// <summary>
    /// 계단 수준에 따른 텍스트 색상 변경
    /// </summary>
    private void UpdateStairLevelColor()
    {
        if (textStairLevel == null) return;
        
        Color levelColor = Color.white;
        
        switch (currentStairStatus.stairLevel)
        {
            case StairSystem.StairLevel.Low:
                levelColor = Color.red;        // 빨간색 - 낮음
                break;
            case StairSystem.StairLevel.Medium:
                levelColor = Color.yellow;     // 노란색 - 중간
                break;
            case StairSystem.StairLevel.High:
                levelColor = Color.green;      // 초록색 - 높음
                break;
        }
        
        textStairLevel.color = levelColor;
    }
    
    /// <summary>
    /// 현재 계단 상태 결과를 반환합니다
    /// </summary>
    /// <returns>현재 계단 상태 결과</returns>
    public StairSystem.StairStatus GetCurrentStairStatus()
    {
        return currentStairStatus;
    }
    
    /// <summary>
    /// 게임 종료 시 최종 계단 상태 결과를 반환합니다
    /// </summary>
    /// <returns>최종 계단 상태 결과</returns>
    public StairSystem.StairStatus GetFinalStairStatus()
    {
        return StairSystem.CalculateStairStatus(nowScore);
    }
    
    /// <summary>
    /// 계단 상태 결과를 콘솔에 출력합니다
    /// </summary>
    public void LogCurrentStairStatus()
    {
        StairSystem.LogStairStatus(currentStairStatus);
    }

    /// <summary>
    /// 속도 계산 시스템 업데이트
    /// </summary>
    private void UpdateSpeedCalculation()
    {
        // 현재 속도 계산
        currentSpeedResult = SpeedCalculator.CalculateSpeed(nowScore, playTime);
        
        // 속도 정보 UI 업데이트
        UpdateSpeedUI();
        
        // 디버그 로그 출력 (옵션)
        if (logSpeedOnUpdate)
        {
            SpeedCalculator.LogSpeedResult(currentSpeedResult);
        }
    }
    
    /// <summary>
    /// 속도 관련 UI 업데이트
    /// </summary>
    private void UpdateSpeedUI()
    {
        // 속도 정보 텍스트 업데이트
        if (textSpeedInfo != null)
        {
            textSpeedInfo.text = $"속도지수: {currentSpeedResult.speedIndex:F2}\n" +
                                $"계단비율: {currentSpeedResult.stairRatio:P1}";
        }
        
        // 속도 등급 텍스트 업데이트
        if (textSpeedGrade != null)
        {
            textSpeedGrade.text = currentSpeedResult.speedGradeText;
            
            // 등급에 따른 색상 변경
            UpdateSpeedGradeColor();
        }
    }
    
    /// <summary>
    /// 속도 등급에 따른 텍스트 색상 변경
    /// </summary>
    private void UpdateSpeedGradeColor()
    {
        if (textSpeedGrade == null) return;
        
        Color gradeColor = Color.white;
        
        switch (currentSpeedResult.speedGrade)
        {
            case SpeedCalculator.SpeedGrade.VeryFast:
                gradeColor = Color.red;      // 빨간색 - 매우 빠름
                break;
            case SpeedCalculator.SpeedGrade.Fast:
                gradeColor = Color.yellow;   // 노란색 - 빠름
                break;
            case SpeedCalculator.SpeedGrade.Normal:
                gradeColor = Color.green;    // 초록색 - 보통
                break;
            case SpeedCalculator.SpeedGrade.Slow:
                gradeColor = Color.cyan;     // 하늘색 - 느림
                break;
            case SpeedCalculator.SpeedGrade.VerySlow:
                gradeColor = Color.blue;     // 파란색 - 매우 느림
                break;
        }
        
        textSpeedGrade.color = gradeColor;
    }
    
    /// <summary>
    /// 현재 속도 계산 결과를 반환합니다
    /// </summary>
    /// <returns>현재 속도 계산 결과</returns>
    public SpeedCalculator.SpeedCalculationResult GetCurrentSpeedResult()
    {
        return currentSpeedResult;
    }
    
    /// <summary>
    /// 게임 종료 시 최종 속도 계산 결과를 반환합니다
    /// </summary>
    /// <returns>최종 속도 계산 결과</returns>
    public SpeedCalculator.SpeedCalculationResult GetFinalSpeedResult()
    {
        return SpeedCalculator.CalculateSpeed(nowScore, playTime);
    }
    
    /// <summary>
    /// 속도 계산 결과를 콘솔에 출력합니다
    /// </summary>
    public void LogCurrentSpeedResult()
    {
        SpeedCalculator.LogSpeedResult(currentSpeedResult);
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
    
    #region 속도 계산 시스템 테스트 메서드
    
    /// <summary>
    /// 속도 계산 시스템 테스트 - 다양한 시나리오로 속도 지수 확인
    /// </summary>
    [ContextMenu("속도 계산 시스템 테스트")]
    public void TestSpeedCalculationSystem()
    {
        Debug.Log("=== 속도 계산 시스템 테스트 시작 ===");
        
        // 테스트 시나리오들
        TestSpeedScenario(30, 60f, "30계단, 60초 (매우 빠름 예상)");
        TestSpeedScenario(60, 150f, "60계단, 150초 (보통 예상)");
        TestSpeedScenario(120, 300f, "120계단, 300초 (이상적)");
        TestSpeedScenario(40, 120f, "40계단, 120초 (빠름 예상)");
        TestSpeedScenario(80, 280f, "80계단, 280초 (느림 예상)");
        TestSpeedScenario(100, 500f, "100계단, 500초 (매우 느림 예상)");
        
        Debug.Log("=== 속도 계산 시스템 테스트 완료 ===");
    }
    
    /// <summary>
    /// 개별 속도 시나리오 테스트
    /// </summary>
    /// <param name="stairs">계단 수</param>
    /// <param name="time">시간</param>
    /// <param name="description">시나리오 설명</param>
    private void TestSpeedScenario(int stairs, float time, string description)
    {
        var result = SpeedCalculator.CalculateSpeed(stairs, time);
        Debug.Log($"[테스트] {description}\n결과: {result}");
    }
    
    /// <summary>
    /// 속도 등급별 목표 시간 가이드 출력
    /// </summary>
    [ContextMenu("속도 등급 가이드 출력")]
    public void ShowSpeedGradeGuide()
    {
        Debug.Log("=== 속도 등급별 가이드 (120계단 기준) ===");
        
        float totalIdealTime = SpeedCalculator.TOTAL_TIME_LIMIT; // 300초
        
        Debug.Log($"매우 빠름 (< {SpeedCalculator.VERY_FAST_THRESHOLD}): {totalIdealTime * SpeedCalculator.VERY_FAST_THRESHOLD:F1}초 미만");
        Debug.Log($"빠름 ({SpeedCalculator.VERY_FAST_THRESHOLD} ~ {SpeedCalculator.FAST_THRESHOLD}): {totalIdealTime * SpeedCalculator.VERY_FAST_THRESHOLD:F1}초 ~ {totalIdealTime * SpeedCalculator.FAST_THRESHOLD:F1}초");
        Debug.Log($"보통 ({SpeedCalculator.FAST_THRESHOLD} ~ {SpeedCalculator.NORMAL_THRESHOLD}): {totalIdealTime * SpeedCalculator.FAST_THRESHOLD:F1}초 ~ {totalIdealTime * SpeedCalculator.NORMAL_THRESHOLD:F1}초");
        Debug.Log($"느림 ({SpeedCalculator.NORMAL_THRESHOLD} ~ {SpeedCalculator.SLOW_THRESHOLD}): {totalIdealTime * SpeedCalculator.NORMAL_THRESHOLD:F1}초 ~ {totalIdealTime * SpeedCalculator.SLOW_THRESHOLD:F1}초");
        Debug.Log($"매우 느림 (≥ {SpeedCalculator.SLOW_THRESHOLD}): {totalIdealTime * SpeedCalculator.SLOW_THRESHOLD:F1}초 이상");
    }
    
    #endregion
    
    #region 계단 시스템 테스트 메서드
    
    /// <summary>
    /// 계단 시스템 테스트 - 다양한 시나리오로 계단 상태 확인
    /// </summary>
    [ContextMenu("계단 시스템 테스트")]
    public void TestStairSystem()
    {
        Debug.Log("=== 계단 시스템 테스트 시작 ===");
        
        // 테스트 시나리오들
        TestStairScenario(15, "15계단 (소년 중간)");
        TestStairScenario(30, "30계단 (소년 완료, 청소년 시작)");
        TestStairScenario(50, "50계단 (청소년 중간)");
        TestStairScenario(70, "70계단 (청소년 완료, 청년 시작)");
        TestStairScenario(100, "100계단 (청년 중간)");
        TestStairScenario(120, "120계단 (청년 완료)");
        
        // 구간별 경계값 테스트
        TestStairScenario(40, "40계단 (낮음 끝)");
        TestStairScenario(41, "41계단 (중간 시작)");
        TestStairScenario(80, "80계단 (중간 끝)");
        TestStairScenario(81, "81계단 (높음 시작)");
        
        Debug.Log("=== 계단 시스템 테스트 완료 ===");
    }
    
    /// <summary>
    /// 개별 계단 시나리오 테스트
    /// </summary>
    /// <param name="stairs">계단 수</param>
    /// <param name="description">시나리오 설명</param>
    private void TestStairScenario(int stairs, string description)
    {
        var result = StairSystem.CalculateStairStatus(stairs);
        Debug.Log($"[테스트] {description}\n결과: {result}");
    }
    
    /// <summary>
    /// 계단 시스템 정보 가이드 출력
    /// </summary>
    [ContextMenu("계단 시스템 정보 출력")]
    public void ShowStairSystemInfo()
    {
        StairSystem.LogStairSystemInfo();
    }
    
    /// <summary>
    /// 현재 계단 상태를 강제로 로그 출력
    /// </summary>
    [ContextMenu("현재 계단 상태 출력")]
    public void LogCurrentStairStatusDebug()
    {
        var status = StairSystem.CalculateStairStatus(nowScore);
        StairSystem.LogStairStatus(status);
    }
    
    #endregion

    /// <summary>
    /// 직업 시스템 업데이트
    /// </summary>
    private void UpdateJobSystem()
    {
        // 현재 속도 지수와 계단 수를 바탕으로 직업 결정
        currentJobResult = JobSystem.DetermineJob(currentSpeedResult.speedIndex, nowScore);
        
        // 직업 관련 UI 업데이트
        UpdateJobUI();
        
        // 디버그 로그 출력 (옵션)
        if (logJobOnUpdate)
        {
            JobSystem.LogJobResult(currentJobResult);
        }
    }

    /// <summary>
    /// 직업 관련 UI 업데이트
    /// </summary>
    private void UpdateJobUI()
    {
        // 직업 이름 텍스트 업데이트
        if (textJobName != null)
        {
            textJobName.text = $"직업: {currentJobResult.jobInfo.jobName}";
            
            // 직업에 따른 색상 변경 (선택사항)
            UpdateJobNameColor();
        }
        
        // 직업 설명 텍스트 업데이트
        if (textJobDescription != null)
        {
            // 설명이 너무 길면 잘라서 표시
            string shortDescription = currentJobResult.jobInfo.description;
            if (shortDescription.Length > 50)
            {
                shortDescription = shortDescription.Substring(0, 47) + "...";
            }
            textJobDescription.text = shortDescription;
        }
    }

    /// <summary>
    /// 직업에 따른 텍스트 색상 변경
    /// </summary>
    private void UpdateJobNameColor()
    {
        if (textJobName == null) return;
        
        Color jobColor = Color.white;
        
        switch (currentJobResult.jobInfo.jobType)
        {
            case JobSystem.JobType.President:
                jobColor = Color.red;      // 빨간색 - 대통령
                break;
            case JobSystem.JobType.Doctor:
            case JobSystem.JobType.Nurse:
                jobColor = Color.green;    // 초록색 - 의료진
                break;
            case JobSystem.JobType.Singer:
            case JobSystem.JobType.Dancer:
                jobColor = Color.magenta;  // 마젠타색 - 예술가
                break;
            case JobSystem.JobType.Professor:
            case JobSystem.JobType.Philosopher:
                jobColor = Color.blue;     // 파란색 - 학자
                break;
            case JobSystem.JobType.Beggar:
                jobColor = Color.gray;     // 회색 - 거지
                break;
            default:
                jobColor = Color.yellow;   // 노란색 - 기타 직업
                break;
        }
        
        textJobName.color = jobColor;
    }
    
    /// <summary>
    /// 현재 직업 결정 결과를 반환합니다
    /// </summary>
    /// <returns>현재 직업 결정 결과</returns>
    public JobSystem.JobResult GetCurrentJobResult()
    {
        return currentJobResult;
    }
    
    /// <summary>
    /// 게임 종료 시 최종 직업 결정 결과를 반환합니다
    /// </summary>
    /// <returns>최종 직업 결정 결과</returns>
    public JobSystem.JobResult GetFinalJobResult()
    {
        var finalSpeedResult = GetFinalSpeedResult();
        return JobSystem.DetermineJob(finalSpeedResult.speedIndex, nowScore);
    }
    
    /// <summary>
    /// 직업 결정 결과를 콘솔에 출력합니다
    /// </summary>
    public void LogCurrentJobResult()
    {
        JobSystem.LogJobResult(currentJobResult);
    }

    #region 직업 시스템 테스트 메서드
    
    /// <summary>
    /// 직업 시스템 테스트 - 다양한 시나리오로 직업 결정 확인
    /// </summary>
    [ContextMenu("직업 시스템 테스트")]
    public void TestJobSystem()
    {
        Debug.Log("=== 직업 시스템 테스트 시작 ===");
        
        // 테스트 시나리오들
        TestJobScenario(0.5f, 110, "매우 빠름, 최고 계단 (대통령 예상)");
        TestJobScenario(1.3f, 110, "보통, 최고 계단 (의사 예상)");
        TestJobScenario(0.8f, 60, "빠름, 중간 계단 (가수/무용가 예상)");
        TestJobScenario(1.1f, 90, "보통, 높은 계단 (간호사/공무원 예상)");
        TestJobScenario(1.8f, 90, "매우 느림, 높은 계단 (교수/철학자 예상)");
        TestJobScenario(2.0f, 30, "매우 느림, 낮은 계단 (거지/회상가 예상)");
        TestJobScenario(1.1f, 60, "보통, 중간 계단 (회사원/소방관 예상)");
        
        Debug.Log("=== 직업 시스템 테스트 완료 ===");
    }
    
    /// <summary>
    /// 개별 직업 시나리오 테스트
    /// </summary>
    /// <param name="speedIndex">속도 지수</param>
    /// <param name="stairs">계단 수</param>
    /// <param name="description">시나리오 설명</param>
    private void TestJobScenario(float speedIndex, int stairs, string description)
    {
        var result = JobSystem.DetermineJob(speedIndex, stairs);
        Debug.Log($"[테스트] {description}\n결과: {result.jobInfo.jobName} ({(result.isMatched ? "매칭 성공" : "매칭 실패")})");
    }
    
    /// <summary>
    /// 모든 직업 조건 정보 출력
    /// </summary>
    [ContextMenu("모든 직업 조건 출력")]
    public void ShowAllJobConditions()
    {
        JobSystem.LogAllJobConditions();
    }
    
    /// <summary>
    /// 현재 조건에 매칭되는 직업들 검색
    /// </summary>
    [ContextMenu("현재 조건 직업 검색")]
    public void SearchCurrentJobMatches()
    {
        var currentSpeed = GetCurrentSpeedResult();
        JobSystem.LogMatchingJobs(currentSpeed.speedIndex, nowScore);
    }
    
    /// <summary>
    /// 현재 직업 결과를 강제로 로그 출력
    /// </summary>
    [ContextMenu("현재 직업 상태 출력")]
    public void LogCurrentJobResultDebug()
    {
        if (currentJobResult.jobInfo.jobName != null)
        {
            JobSystem.LogJobResult(currentJobResult);
        }
        else
        {
            Debug.Log("아직 직업이 결정되지 않았습니다. 게임을 시작해주세요.");
        }
    }
    
    #endregion

    /// <summary>
    /// 엔딩 시스템 업데이트
    /// </summary>
    private void UpdateEndingUI()
    {
        // 엔딩 텍스트 UI 업데이트
        if (textEndingTitle != null)
        {
            textEndingTitle.text = finalEndingResult.endingInfo.endingTitle;
        }
        
        if (textEndingMessage != null)
        {
            textEndingMessage.text = finalEndingResult.endingInfo.endingMessage;
        }
        
        if (textEndingSubtext != null)
        {
            textEndingSubtext.text = finalEndingResult.endingInfo.endingSubtext;
        }
        
        // 엔딩 카드 표시
        UpdateEndingCard();
    }

    /// <summary>
    /// 엔딩 카드 UI 업데이트
    /// </summary>
    private void UpdateEndingCard()
    {
        if (endingCardImage == null)
        {
            // 카드 이미지가 없는 경우
            if (endingCardPanel != null)
                endingCardPanel.SetActive(false);
            return;
        }

        // 해당 직업의 카드 스프라이트 찾기
        Sprite jobCard = GetJobCardSprite(finalEndingResult.jobResult.jobInfo.jobType);
        
        if (jobCard != null)
        {
            // 카드 이미지 설정
            endingCardImage.sprite = jobCard;
            
            // 카드 패널 활성화
            if (endingCardPanel != null)
                endingCardPanel.SetActive(true);
            
            // 카드 표시 애니메이션 시작 (선택사항)
            StartEndingCardAnimation();
            
            if (showEndingDebug)
            {
                Debug.Log($"[엔딩 카드] {finalEndingResult.jobResult.jobInfo.jobName} 카드를 표시합니다.");
            }
        }
        else
        {
            if (showEndingDebug)
            {
                Debug.LogWarning($"[엔딩 카드] {finalEndingResult.jobResult.jobInfo.jobType} 직업의 카드 스프라이트를 찾을 수 없습니다.");
            }
            
            // 카드를 찾을 수 없는 경우 패널 비활성화
            if (endingCardPanel != null)
                endingCardPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 엔딩 카드 표시 애니메이션을 시작합니다
    /// </summary>
    private void StartEndingCardAnimation()
    {
        if (endingCardPanel != null)
        {
            // 간단한 페이드 인 애니메이션
            StartCoroutine(FadeInEndingCard());
        }
    }

    /// <summary>
    /// 엔딩 카드 페이드 인 애니메이션 코루틴
    /// </summary>
    private System.Collections.IEnumerator FadeInEndingCard()
    {
        if (endingCardImage == null) yield break;
        
        // 초기 투명도 설정
        Color originalColor = endingCardImage.color;
        Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        endingCardImage.color = transparentColor;
        
        // 페이드 인 애니메이션
        float fadeDuration = 1.0f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            
            endingCardImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            
            yield return null;
        }
        
        // 최종 투명도 설정
        endingCardImage.color = originalColor;
    }

    /// <summary>
    /// 최종 엔딩 결과를 반환합니다
    /// </summary>
    /// <returns>최종 엔딩 결과</returns>
    public EndingSystem.EndingResult GetFinalEndingResult()
    {
        return finalEndingResult;
    }
    
    /// <summary>
    /// 엔딩 결과를 콘솔에 출력합니다
    /// </summary>
    public void LogFinalEndingResult()
    {
        if (finalEndingResult.endingInfo.endingTitle != null)
        {
            EndingSystem.LogEndingResult(finalEndingResult);
        }
        else
        {
            Debug.Log("아직 엔딩이 생성되지 않았습니다. 게임을 완료해주세요.");
        }
    }
    
    #region 엔딩 시스템 테스트 메서드
    
    /// <summary>
    /// 엔딩 시스템 테스트 - 다양한 직업별 엔딩 확인
    /// </summary>
    [ContextMenu("엔딩 시스템 테스트")]
    public void TestEndingSystem()
    {
        Debug.Log("=== 엔딩 시스템 테스트 시작 ===");
        
        // 대표적인 직업들의 엔딩 테스트
        TestEndingScenario(JobSystem.JobType.President, 0.8f, 110, true, "대통령 - 게임 클리어");
        TestEndingScenario(JobSystem.JobType.Doctor, 1.3f, 105, true, "의사 - 게임 클리어");
        TestEndingScenario(JobSystem.JobType.Singer, 0.7f, 60, false, "가수 - 게임 오버");
        TestEndingScenario(JobSystem.JobType.Beggar, 2.0f, 25, false, "거지 - 게임 오버");
        TestEndingScenario(JobSystem.JobType.Teacher, 1.2f, 80, true, "교수 - 게임 클리어");
        
        Debug.Log("=== 엔딩 시스템 테스트 완료 ===");
    }
    
    /// <summary>
    /// 개별 엔딩 시나리오 테스트
    /// </summary>
    /// <param name="jobType">직업 유형</param>
    /// <param name="speedIndex">속도 지수</param>
    /// <param name="stairs">계단 수</param>
    /// <param name="isCleared">게임 클리어 여부</param>
    /// <param name="description">시나리오 설명</param>
    private void TestEndingScenario(JobSystem.JobType jobType, float speedIndex, int stairs, bool isCleared, string description)
    {
        // 테스트용 결과 생성
        var jobResult = JobSystem.DetermineJob(speedIndex, stairs);
        var speedResult = SpeedCalculator.CalculateSpeed(stairs, speedIndex * 150f); // 임시 시간 계산
        var stairStatus = StairSystem.CalculateStairStatus(stairs);
        
        // 엔딩 생성
        var endingResult = EndingSystem.GenerateEnding(jobResult, speedResult, stairStatus, isCleared);
        
        Debug.Log($"[테스트] {description}\n엔딩: {endingResult.endingInfo.endingTitle}");
    }
    
    /// <summary>
    /// 모든 엔딩 정보 출력
    /// </summary>
    [ContextMenu("모든 엔딩 정보 출력")]
    public void ShowAllEndingInfo()
    {
        EndingSystem.LogAllEndingInfo();
    }
    
    /// <summary>
    /// 특정 직업의 엔딩 정보 출력 (대통령 예시)
    /// </summary>
    [ContextMenu("대통령 엔딩 정보 출력")]
    public void ShowPresidentEndingInfo()
    {
        EndingSystem.LogEndingInfo(JobSystem.JobType.President);
        
        // 대통령 카드 스프라이트 확인
        var presidentCard = GetJobCardSprite(JobSystem.JobType.President);
        if (presidentCard != null)
        {
            Debug.Log($"[카드 확인] 대통령 카드 스프라이트가 준비되어 있습니다: {presidentCard.name}");
        }
        else
        {
            Debug.LogWarning("[카드 확인] 대통령 카드 스프라이트를 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 현재 게임 상태 기반 엔딩 미리보기
    /// </summary>
    [ContextMenu("현재 상태 엔딩 미리보기")]
    public void PreviewCurrentEnding()
    {
        if (isPlaying)
        {
            var currentSpeed = GetCurrentSpeedResult();
            var currentStair = GetCurrentStairStatus();
            var currentJob = GetCurrentJobResult();
            
            var previewEnding = EndingSystem.GenerateEnding(currentJob, currentSpeed, currentStair, nowScore >= TOTAL_STAIRS);
            
            Debug.Log($"[엔딩 미리보기] 현재 상태로 게임이 끝나면...\n{EndingSystem.FormatSimpleEnding(previewEnding)}");
            
            // 현재 직업의 카드 스프라이트 확인
            var jobCard = GetJobCardSprite(currentJob.jobInfo.jobType);
            if (jobCard != null)
            {
                Debug.Log($"[카드 미리보기] {currentJob.jobInfo.jobName} 카드가 표시될 예정입니다: {jobCard.name}");
            }
            else
            {
                Debug.LogWarning($"[카드 미리보기] {currentJob.jobInfo.jobName} 카드 스프라이트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.Log("게임이 진행 중이 아닙니다. 게임을 시작해주세요.");
        }
    }
    
    /// <summary>
    /// 모든 직업 카드 스프라이트 확인
    /// </summary>
    [ContextMenu("모든 직업 카드 확인")]
    public void CheckAllJobCards()
    {
        Debug.Log("=== 모든 직업 카드 스프라이트 확인 ===");
        
        if (jobCardSprites == null || jobCardSprites.Length == 0)
        {
            Debug.LogWarning("직업 카드 스프라이트 배열이 설정되지 않았습니다!");
            Debug.Log("Inspector에서 Job Card Sprites 배열을 14개로 설정하고 각 카드 이미지를 할당해주세요.");
            return;
        }
        
        Debug.Log($"카드 스프라이트 배열 크기: {jobCardSprites.Length}개 (권장: 14개)");
        
        string[] jobNames = {
            "가수", "간호사", "거지", "공무원", "교사", "교수", "대통령",
            "무용가", "소방관", "의사", "작가", "철학자", "회사원", "회상가"
        };
        
        for (int i = 0; i < jobNames.Length; i++)
        {
            if (i < jobCardSprites.Length)
            {
                if (jobCardSprites[i] != null)
                {
                    Debug.Log($"✓ [{i}] {jobNames[i]}: {jobCardSprites[i].name}");
                }
                else
                {
                    Debug.LogWarning($"✗ [{i}] {jobNames[i]}: 스프라이트가 설정되지 않음");
                }
            }
            else
            {
                Debug.LogWarning($"✗ [{i}] {jobNames[i]}: 배열 인덱스 부족");
            }
        }
    }

    /// <summary>
    /// 특정 직업의 카드 스프라이트를 반환합니다
    /// </summary>
    /// <param name="jobType">직업 유형</param>
    /// <returns>카드 스프라이트</returns>
    private Sprite GetJobCardSprite(JobSystem.JobType jobType)
    {
        if (jobCardSprites == null || jobCardSprites.Length == 0) return null;
        
        // 직업 타입에 해당하는 카드 배열 인덱스를 반환합니다
        int cardIndex = GetJobCardIndex(jobType);
        
        if (cardIndex >= 0 && cardIndex < jobCardSprites.Length)
        {
            return jobCardSprites[cardIndex];
        }
        
        return null;
    }

    /// <summary>
    /// 직업 타입에 해당하는 카드 배열 인덱스를 반환합니다
    /// </summary>
    /// <param name="jobType">직업 유형</param>
    /// <returns>카드 배열 인덱스 (Unknown이면 회사원 인덱스)</returns>
    private int GetJobCardIndex(JobSystem.JobType jobType)
    {
        switch (jobType)
        {
            case JobSystem.JobType.Singer: return 0;        // 가수
            case JobSystem.JobType.Nurse: return 1;         // 간호사
            case JobSystem.JobType.Beggar: return 2;        // 거지
            case JobSystem.JobType.CivilServant: return 3;  // 공무원
            case JobSystem.JobType.Teacher: return 4;       // 교사
            case JobSystem.JobType.Professor: return 5;     // 교수
            case JobSystem.JobType.President: return 6;     // 대통령
            case JobSystem.JobType.Dancer: return 7;        // 무용가
            case JobSystem.JobType.Firefighter: return 8;   // 소방관
            case JobSystem.JobType.Doctor: return 9;        // 의사
            case JobSystem.JobType.Writer: return 10;       // 작가
            case JobSystem.JobType.Philosopher: return 11;  // 철학자
            case JobSystem.JobType.Employee: return 12;     // 회사원
            case JobSystem.JobType.Reminiscer: return 13;   // 회상가
            case JobSystem.JobType.Unknown: 
            default: return 12; // Unknown일 때도 회사원 카드 표시
        }
    }

    /// <summary>
    /// Unknown 직업 카드 테스트 (회사원 카드가 표시되는지 확인)
    /// </summary>
    [ContextMenu("Unknown 직업 카드 테스트")]
    public void TestUnknownJobCard()
    {
        // Unknown 직업 결과 생성 (테스트)
        var testJobResult = new JobSystem.JobResult
        {
            jobInfo = new JobSystem.JobInfo
            {
                jobType = JobSystem.JobType.Unknown,
                jobName = "평범한 사람",
                description = "알 수 없는 직업",
                minSpeedIndex = 0f,
                maxSpeedIndex = float.MaxValue,
                minStairs = 0,
                maxStairs = int.MaxValue,
                priority = 99
            },
            speedIndex = 1.0f,
            stairCount = 45,
            isMatched = false,
            matchReason = "테스트용 Unknown"
        };

        var testSpeed = SpeedCalculator.CalculateSpeed(45, 120f);
        var testStair = StairSystem.CalculateStairStatus(45);
        
        // 테스트 엔딩 생성
        finalEndingResult = EndingSystem.GenerateEnding(testJobResult, testSpeed, testStair, false);
        finalEndingResult.jobResult = testJobResult; // Unknown 직업으로 강제 설정
        
        // 엔딩 UI 업데이트 (카드 포함)
        UpdateEndingUI();
        
        // 엔딩 카드 패널 활성화
        if (endingCardPanel != null)
        {
            endingCardPanel.SetActive(true);
        }
        
        Debug.Log($"[테스트] Unknown 직업에 대해 회사원 카드와 회사원 엔딩 메시지가 표시되었습니다.");
        Debug.Log($"카드 인덱스: {GetJobCardIndex(JobSystem.JobType.Unknown)}");
        Debug.Log($"엔딩 제목: {finalEndingResult.endingInfo.endingTitle}");
        Debug.Log($"엔딩 메시지: {finalEndingResult.endingInfo.endingMessage}");
    }

    /// <summary>
    /// 모든 직업 카드 스프라이트 상태 확인
    /// </summary>
    [ContextMenu("직업 카드 스프라이트 확인")]
    public void CheckJobCardSprites()
    {
        Debug.Log("=== 직업 카드 스프라이트 상태 확인 ===");
        
        if (jobCardSprites == null || jobCardSprites.Length == 0)
        {
            Debug.LogWarning("직업 카드 스프라이트 배열이 설정되지 않았습니다!");
            Debug.Log("Inspector에서 Job Card Sprites 배열을 14개로 설정하고 각 카드 이미지를 할당해주세요.");
            return;
        }
        
        Debug.Log($"카드 스프라이트 배열 크기: {jobCardSprites.Length}개 (권장: 14개)");
        
        string[] jobNames = {
            "가수", "간호사", "거지", "공무원", "교사", "교수", "대통령",
            "무용가", "소방관", "의사", "작가", "철학자", "회사원", "회상가"
        };
        
        for (int i = 0; i < jobNames.Length; i++)
        {
            if (i < jobCardSprites.Length)
            {
                if (jobCardSprites[i] != null)
                {
                    Debug.Log($"✓ [{i}] {jobNames[i]}: {jobCardSprites[i].name}");
                }
                else
                {
                    Debug.LogWarning($"✗ [{i}] {jobNames[i]}: 스프라이트가 설정되지 않음");
                }
            }
            else
            {
                Debug.LogWarning($"✗ [{i}] {jobNames[i]}: 배열 인덱스 부족");
            }
        }
    }

    #endregion

    /// <summary>
    /// 한글 폰트 적용
    /// </summary>
    private void ApplyKoreanFont()
    {
        if (useKoreanFont && koreanFontAsset != null)
        {
            // 모든 TextMeshPro 컴포넌트에 한글 폰트 적용
            if (textShowScore != null) textShowScore.font = koreanFontAsset;
            if (textMaxScore != null) textMaxScore.font = koreanFontAsset;
            if (textNowScore != null) textNowScore.font = koreanFontAsset;
            if (textPlayTime != null) textPlayTime.font = koreanFontAsset;
            if (textSpeedInfo != null) textSpeedInfo.font = koreanFontAsset;
            if (textSpeedGrade != null) textSpeedGrade.font = koreanFontAsset;
            if (textLifeStage != null) textLifeStage.font = koreanFontAsset;
            if (textStairLevel != null) textStairLevel.font = koreanFontAsset;
            if (textStageProgress != null) textStageProgress.font = koreanFontAsset;
            if (textJobName != null) textJobName.font = koreanFontAsset;
            if (textJobDescription != null) textJobDescription.font = koreanFontAsset;
            if (textEndingTitle != null) textEndingTitle.font = koreanFontAsset;
            if (textEndingMessage != null) textEndingMessage.font = koreanFontAsset;
            if (textEndingSubtext != null) textEndingSubtext.font = koreanFontAsset;
            if (textGameResult != null) textGameResult.font = koreanFontAsset;
            
            Debug.Log("한글 폰트가 모든 텍스트 컴포넌트에 적용되었습니다.");
        }
        else
        {
            Debug.LogWarning("한글 폰트 에셋이 설정되지 않았습니다. Inspector에서 Korean Font Asset을 설정해주세요.");
        }
    }

    /// <summary>
    /// 한글 폰트 설정 테스트
    /// </summary>
    [ContextMenu("한글 폰트 테스트")]
    public void TestKoreanFont()
    {
        // 한글 텍스트로 테스트
        if (textShowScore != null)
        {
            string originalText = textShowScore.text;
            textShowScore.text = "한글 테스트 ✓";
            
            Debug.Log("한글 폰트 테스트 완료!");
            Debug.Log("현재 폰트: " + (textShowScore.font != null ? textShowScore.font.name : "없음"));
            
            // 3초 후 원래 텍스트로 복원
            StartCoroutine(RestoreOriginalText(originalText));
        }
    }

    /// <summary>
    /// 원래 텍스트로 복원하는 코루틴
    /// </summary>
    private System.Collections.IEnumerator RestoreOriginalText(string originalText)
    {
        yield return new WaitForSeconds(3f);
        if (textShowScore != null)
        {
            textShowScore.text = originalText;
        }
    }

    /// <summary>
    /// 40계단 이하 거지 강제 할당 테스트
    /// </summary>
    [ContextMenu("40계단 이하 거지 할당 테스트")]
    public void Test40StairsBeggarRule()
    {
        Debug.Log("=== 40계단 이하 거지 강제 할당 테스트 ===");
        
        // 다양한 속도 지수로 40계단 이하 테스트
        TestJobScenario(0.5f, 20, "매우 빠름, 20계단 (거지 예상)");
        TestJobScenario(0.8f, 30, "빠름, 30계단 (거지 예상)");
        TestJobScenario(1.0f, 35, "보통, 35계단 (거지 예상)");
        TestJobScenario(1.5f, 40, "느림, 40계단 (거지 예상)");
        TestJobScenario(2.0f, 25, "매우 느림, 25계단 (거지 예상)");
        
        // 경계값 테스트 (40계단 vs 41계단)
        TestJobScenario(1.0f, 40, "보통, 40계단 (거지 예상)");
        TestJobScenario(1.0f, 41, "보통, 41계단 (기존 규칙 적용 예상)");
        
        Debug.Log("=== 40계단 이하 거지 강제 할당 테스트 완료 ===");
    }
}
