using UnityEngine;
using TMPro;

/// <summary>
/// 한글 폰트 설정을 도와주는 헬퍼 클래스
/// </summary>
public class KoreanFontHelper : MonoBehaviour
{
    [Header("한글 폰트 설정 도우미")]
    [Space(10)]
    public Font sourceFont;                    // 소스 폰트 (ttf/ttc 파일)
    public TMP_FontAsset generatedFontAsset;   // 생성된 TextMeshPro 폰트 에셋
    
    [TextArea(5, 10)]
    public string koreanCharacters = "가나다라마바사아자차카타파하 " +
                                   "게임 오버 클리어 점수 시간 속도 " +
                                   "계단 직업 엔딩 회상가 대통령 의사 " +
                                   "간호사 교사 교수 작가 철학자 " +
                                   "무용가 가수 소방관 거지 공무원 " +
                                   "회사원 매우 빠름 보통 느림 " +
                                   "낮음 중간 높음 소년 청소년 청년 " +
                                   "수준 단계 진행률 다음까지 " +
                                   "0123456789 .,!?:%-/()[] " +
                                   "abcdefghijklmnopqrstuvwxyz " +
                                   "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    
    /// <summary>
    /// 한글 폰트 에셋 생성 가이드를 콘솔에 출력
    /// </summary>
    [ContextMenu("한글 폰트 에셋 생성 가이드")]
    public void ShowFontAssetCreationGuide()
    {
        Debug.Log("=== TextMeshPro 한글 폰트 에셋 생성 가이드 ===");
        Debug.Log("");
        Debug.Log("1️⃣ Window > TextMeshPro > Font Asset Creator 열기");
        Debug.Log("");
        Debug.Log("2️⃣ Font Asset Creator 설정:");
        Debug.Log("   • Source Font File: Assets/_My/Fonts/AppleSDGothicNeo.ttc");
        Debug.Log("   • Sampling Point Size: 32 (또는 원하는 크기)");
        Debug.Log("   • Padding: 5");
        Debug.Log("   • Packing Method: Optimum");
        Debug.Log("   • Atlas Resolution: 1024 x 1024 (필요시 더 큰 크기)");
        Debug.Log("   • Character Set: Custom Characters");
        Debug.Log("");
        Debug.Log("3️⃣ Custom Character List에 다음 텍스트 입력:");
        Debug.Log("   " + koreanCharacters);
        Debug.Log("");
        Debug.Log("4️⃣ 'Generate Font Atlas' 버튼 클릭");
        Debug.Log("");
        Debug.Log("5️⃣ 'Save' 또는 'Save as...' 버튼으로 폰트 에셋 저장");
        Debug.Log("   권장 경로: Assets/_My/Fonts/AppleSDGothicNeo_Korean SDF");
        Debug.Log("");
        Debug.Log("6️⃣ 생성된 폰트 에셋을 GameManager의 Korean Font Asset 필드에 할당");
        Debug.Log("");
        Debug.Log("=== 완료! ===");
    }
    
    /// <summary>
    /// 한글 폰트 적용 상태 확인
    /// </summary>
    [ContextMenu("한글 폰트 상태 확인")]
    public void CheckKoreanFontStatus()
    {
        var gameManager = FindObjectOfType<GameManager>();
        
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager를 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log("=== 한글 폰트 상태 확인 ===");
        
        if (gameManager.koreanFontAsset != null)
        {
            Debug.Log($"✅ 한글 폰트 에셋 설정됨: {gameManager.koreanFontAsset.name}");
            Debug.Log($"✅ 한글 폰트 사용 여부: {gameManager.useKoreanFont}");
            
            // 폰트 에셋의 문자 지원 확인
            var characterTable = gameManager.koreanFontAsset.characterTable;
            bool hasKorean = false;
            
            foreach (var character in characterTable)
            {
                if (character.unicode >= 0xAC00 && character.unicode <= 0xD7AF) // 한글 유니코드 범위
                {
                    hasKorean = true;
                    break;
                }
            }
            
            if (hasKorean)
            {
                Debug.Log("✅ 한글 문자 지원 확인됨");
            }
            else
            {
                Debug.LogWarning("⚠️ 한글 문자가 폰트 에셋에 포함되지 않은 것 같습니다. 폰트 에셋을 다시 생성해주세요.");
            }
        }
        else
        {
            Debug.LogWarning("❌ 한글 폰트 에셋이 설정되지 않았습니다!");
            Debug.Log("Inspector에서 GameManager > Korean Font Asset 필드에 한글 폰트 에셋을 할당해주세요.");
        }
    }
    
    /// <summary>
    /// 게임에서 사용되는 모든 한글 텍스트 추출
    /// </summary>
    [ContextMenu("게임 한글 텍스트 추출")]
    public void ExtractGameKoreanText()
    {
        Debug.Log("=== 게임에서 사용되는 한글 텍스트 ===");
        Debug.Log("");
        Debug.Log("UI 텍스트:");
        Debug.Log("• 게임 오버, 게임 클리어");
        Debug.Log("• 점수, 최고 점수, 현재 점수");
        Debug.Log("• 플레이 시간");
        Debug.Log("• 속도 정보, 속도 등급");
        Debug.Log("• 인생 단계, 계단 수준, 단계 진행률");
        Debug.Log("• 직업 이름, 직업 설명");
        Debug.Log("• 엔딩 제목, 엔딩 메시지");
        Debug.Log("");
        Debug.Log("속도 등급:");
        Debug.Log("• 매우 빠름, 빠름, 보통, 느림, 매우 느림");
        Debug.Log("");
        Debug.Log("계단 수준:");
        Debug.Log("• 낮음, 중간, 높음");
        Debug.Log("");
        Debug.Log("인생 단계:");
        Debug.Log("• 소년, 청소년, 청년, 완료");
        Debug.Log("");
        Debug.Log("직업명:");
        Debug.Log("• 가수, 간호사, 거지, 공무원, 교사, 교수");
        Debug.Log("• 대통령, 무용가, 소방관, 의사, 작가, 철학자");
        Debug.Log("• 회사원, 회상가");
    }
} 