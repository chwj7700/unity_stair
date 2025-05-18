using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // Image 컴포넌트 사용을 위해 추가

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("계단")]
    [Space(10)]

    public GameObject[] Stairs;
    public bool[] isTurn;

    private enum State {Start,Left,Right};
    private State state;
    private Vector3 oldPosition;

    [Header("UI")]
    [Space(10)]
    public GameObject UI_GameOver;
    public TextMeshProUGUI textMaxScore;
    public TextMeshProUGUI textNowScore;
    public TextMeshProUGUI textShowScore;
    private int maxScore = 0;
    private int nowScore = 0;

    [Header("배경")]
    [Space(10)]
    public Image backgroundImage; // UI Canvas에 추가한 배경 이미지
    public Sprite[] backgroundSprites; // 사용할 배경 이미지들
    private int currentBackgroundIndex = 0;
    public int backgroundChangeInterval = 100; // 배경 변경 간격 (100점마다)

    [Header("캐릭터")]
    [Space(10)]
    public Player playerReference; // 플레이어 오브젝트 참조
    public Sprite[] characterSprites; // 캐릭터 스프라이트 배열
    private int currentCharacterIndex = 0;

    [Header("Audio")]
    [Space(10)]
    private AudioSource sound;
    public AudioClip bgmSound;
    public AudioClip dieSound;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        sound = GetComponent<AudioSource>();
        Init();
        InitStairs();
    }

    public void Init()
    {
        state = State.Start;
        oldPosition = Vector3.zero;

        isTurn = new bool[Stairs.Length];

        for (int i = 0; i < Stairs.Length; i++)
        {
            Stairs[i].transform.position = Vector3.zero;
            isTurn[i] = false;
        }

        nowScore = 0;
        currentBackgroundIndex = 0;
        currentCharacterIndex = 0;
        
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

        textShowScore.text = nowScore.ToString();

        UI_GameOver.SetActive(false);

        sound.clip = bgmSound;
        sound.Play();
        sound.loop = true;
        sound.volume = 0.4f;
    }

    public void InitStairs()
    {
        for (int i=0; i<Stairs.Length; i++)
        {
            switch (state)
            {
                case State.Start:
                    Stairs[i].transform.position = new Vector3(0.75f, -0.1f, 0);
                    state = State.Right;
                    break;
                case State.Left:
                    Stairs[i].transform.position = oldPosition + new Vector3(-0.75f, 0.5f, 0);
                    isTurn[i] = true;
                    break;
                case State.Right:
                    Stairs[i].transform.position = oldPosition + new Vector3(0.75f, 0.5f, 0);
                    isTurn[i] = false;
                    break;
            }

            oldPosition = Stairs[i].transform.position;

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

    public void SpawnStair(int cnt)
    {
        int ran = Random.Range(0, 5);

        if (ran < 2)
        {
            state = state == State.Left ? State.Right : State.Left;
        }

        switch (state)
        {
            case State.Left:
                Stairs[cnt].transform.position = oldPosition + new Vector3(-0.75f, 0.5f, 0);
                isTurn[cnt] = true;
                break;
            case State.Right:
                Stairs[cnt].transform.position = oldPosition + new Vector3(0.75f, 0.5f, 0);
                isTurn[cnt] = false;
                break;
        }

        oldPosition = Stairs[cnt].transform.position;
    }

    public void GameOver()
    {
        sound.loop = false;
        sound.Stop();
        sound.clip = dieSound;
        sound.Play();
        sound.volume = 1;

        StartCoroutine(ShowGameOver());
    }

    IEnumerator ShowGameOver()
    {
        yield return new WaitForSeconds(1f);

        UI_GameOver.SetActive(true);

        if(nowScore > maxScore)
        {
            maxScore = nowScore;
        }

        textMaxScore.text = maxScore.ToString();
        textNowScore.text = nowScore.ToString();
    }

    public void AddScore()
    {
        nowScore++;
        textShowScore.text = nowScore.ToString();
        
        // 일정 점수마다 배경과 캐릭터 변경
        if (nowScore % backgroundChangeInterval == 0)
        {
            ChangeBackground();
            ChangeCharacter();
        }
    }
    
    // 배경 변경 함수
    private void ChangeBackground()
    {
        if(backgroundImage == null || backgroundSprites.Length == 0)
            return;
            
        // 다음 배경 인덱스 계산 (순환)
        currentBackgroundIndex = (currentBackgroundIndex + 1) % backgroundSprites.Length;
        
        // 배경 이미지 변경
        backgroundImage.sprite = backgroundSprites[currentBackgroundIndex];
    }
    
    // 캐릭터 변경 함수
    private void ChangeCharacter()
    {
        if(playerReference == null || characterSprites.Length == 0)
            return;
            
        // 다음 캐릭터 인덱스 계산 (순환)
        currentCharacterIndex = (currentCharacterIndex + 1) % characterSprites.Length;
        
        // 캐릭터 스프라이트 변경
        playerReference.ChangeCharacterSprite(characterSprites[currentCharacterIndex]);
    }
}
