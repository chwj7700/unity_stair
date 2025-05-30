using System;
using UnityEngine;

/// <summary>
/// 플레이어가 오른 계단 수에 따른 상태 판단 시스템
/// </summary>
[System.Serializable]
public class StairSystem
{
    #region 상수

    /// <summary>
    /// 총 계단 수
    /// </summary>
    public const int TOTAL_STAIRS = 120;

    #endregion

    #region 단계별 계단 개수 상수

    /// <summary>
    /// 소년 단계 계단 수
    /// </summary>
    public const int BOY_STAIRS = 30;

    /// <summary>
    /// 청소년 단계 계단 수
    /// </summary>
    public const int TEENAGER_STAIRS = 40;

    /// <summary>
    /// 청년 단계 계단 수
    /// </summary>
    public const int YOUNG_ADULT_STAIRS = 50;

    #endregion

    #region 구간별 해석 상수

    /// <summary>
    /// 낮음 구간 최대값
    /// </summary>
    public const int LOW_LEVEL_MAX = 40;

    /// <summary>
    /// 중간 구간 최소값
    /// </summary>
    public const int MEDIUM_LEVEL_MIN = 41;

    /// <summary>
    /// 중간 구간 최대값
    /// </summary>
    public const int MEDIUM_LEVEL_MAX = 80;

    /// <summary>
    /// 높음 구간 최소값
    /// </summary>
    public const int HIGH_LEVEL_MIN = 81;

    /// <summary>
    /// 높음 구간 최대값
    /// </summary>
    public const int HIGH_LEVEL_MAX = 120;

    #endregion

    #region 열거형

    /// <summary>
    /// 인생 단계 (나이 단계)
    /// </summary>
    public enum LifeStage
    {
        Boy,        // 소년 (0-30)
        Teenager,   // 청소년 (31-70)
        YoungAdult, // 청년 (71-120)
        Completed   // 완료 (120)
    }

    /// <summary>
    /// 계단 수준 (상태 해석)
    /// </summary>
    public enum StairLevel
    {
        Low,    // 낮음 (0-40)
        Medium, // 중간 (41-80)
        High    // 높음 (81-120)
    }

    #endregion

    #region 계단 상태 데이터 구조체

    /// <summary>
    /// 계단 상태 결과를 담는 구조체
    /// </summary>
    [System.Serializable]
    public struct StairStatus
    {
        public int currentStairs;           // 현재 계단 수
        public LifeStage lifeStage;         // 현재 인생 단계
        public StairLevel stairLevel;       // 계단 수준
        public string lifeStageName;        // 인생 단계 이름
        public string stairLevelName;       // 계단 수준 이름
        public float progressRatio;         // 전체 진행률 (0.0 ~ 1.0)
        public float stageProgressRatio;    // 현재 단계 진행률 (0.0 ~ 1.0)
        public int stairsToNextStage;       // 다음 단계까지 남은 계단 수
        
        /// <summary>
        /// 계단 상태의 문자열 표현
        /// </summary>
        public override string ToString()
        {
            return $"계단: {currentStairs}/{TOTAL_STAIRS}, " +
                   $"단계: {lifeStageName}, " +
                   $"수준: {stairLevelName}, " +
                   $"진행률: {progressRatio:P1}";
        }
    }

    #endregion

    #region 공개 메서드

    /// <summary>
    /// 현재 계단 수에 따른 상태를 계산하고 반환합니다
    /// </summary>
    /// <param name="currentStairs">현재 계단 수</param>
    /// <returns>계단 상태 결과</returns>
    public static StairStatus CalculateStairStatus(int currentStairs)
    {
        StairStatus status = new StairStatus();
        
        // 기본 데이터 설정
        status.currentStairs = Mathf.Clamp(currentStairs, 0, TOTAL_STAIRS);
        
        // 전체 진행률 계산
        status.progressRatio = (float)status.currentStairs / TOTAL_STAIRS;
        
        // 인생 단계 판정
        status.lifeStage = DetermineLifeStage(status.currentStairs);
        status.lifeStageName = GetLifeStageName(status.lifeStage);
        
        // 계단 수준 판정
        status.stairLevel = DetermineStairLevel(status.currentStairs);
        status.stairLevelName = GetStairLevelName(status.stairLevel);
        
        // 현재 단계 진행률 및 다음 단계까지 계단 수 계산
        CalculateStageProgress(status.currentStairs, out status.stageProgressRatio, out status.stairsToNextStage);
        
        return status;
    }

    /// <summary>
    /// 현재 계단 수에 따른 인생 단계를 판정합니다
    /// </summary>
    /// <param name="currentStairs">현재 계단 수</param>
    /// <returns>인생 단계</returns>
    public static LifeStage DetermineLifeStage(int currentStairs)
    {
        if (currentStairs >= TOTAL_STAIRS)
            return LifeStage.Completed;
        else if (currentStairs >= BOY_STAIRS + TEENAGER_STAIRS)
            return LifeStage.YoungAdult;
        else if (currentStairs >= BOY_STAIRS)
            return LifeStage.Teenager;
        else
            return LifeStage.Boy;
    }

    /// <summary>
    /// 현재 계단 수에 따른 계단 수준을 판정합니다
    /// </summary>
    /// <param name="currentStairs">현재 계단 수</param>
    /// <returns>계단 수준</returns>
    public static StairLevel DetermineStairLevel(int currentStairs)
    {
        if (currentStairs >= HIGH_LEVEL_MIN)
            return StairLevel.High;
        else if (currentStairs >= MEDIUM_LEVEL_MIN)
            return StairLevel.Medium;
        else
            return StairLevel.Low;
    }

    /// <summary>
    /// 인생 단계의 한국어 이름을 반환합니다
    /// </summary>
    /// <param name="stage">인생 단계</param>
    /// <returns>단계 이름</returns>
    public static string GetLifeStageName(LifeStage stage)
    {
        switch (stage)
        {
            case LifeStage.Boy:
                return "소년";
            case LifeStage.Teenager:
                return "청소년";
            case LifeStage.YoungAdult:
                return "청년";
            case LifeStage.Completed:
                return "완료";
            default:
                return "알 수 없음";
        }
    }

    /// <summary>
    /// 계단 수준의 한국어 이름을 반환합니다
    /// </summary>
    /// <param name="level">계단 수준</param>
    /// <returns>수준 이름</returns>
    public static string GetStairLevelName(StairLevel level)
    {
        switch (level)
        {
            case StairLevel.Low:
                return "낮음";
            case StairLevel.Medium:
                return "중간";
            case StairLevel.High:
                return "높음";
            default:
                return "알 수 없음";
        }
    }

    /// <summary>
    /// 현재 단계의 진행률과 다음 단계까지 남은 계단 수를 계산합니다
    /// </summary>
    /// <param name="currentStairs">현재 계단 수</param>
    /// <param name="stageProgress">현재 단계 진행률</param>
    /// <param name="stairsToNext">다음 단계까지 남은 계단 수</param>
    private static void CalculateStageProgress(int currentStairs, out float stageProgress, out int stairsToNext)
    {
        LifeStage currentStage = DetermineLifeStage(currentStairs);
        
        switch (currentStage)
        {
            case LifeStage.Boy:
                stageProgress = (float)currentStairs / BOY_STAIRS;
                stairsToNext = BOY_STAIRS - currentStairs;
                break;
                
            case LifeStage.Teenager:
                int teenagerProgress = currentStairs - BOY_STAIRS;
                stageProgress = (float)teenagerProgress / TEENAGER_STAIRS;
                stairsToNext = (BOY_STAIRS + TEENAGER_STAIRS) - currentStairs;
                break;
                
            case LifeStage.YoungAdult:
                int youngAdultProgress = currentStairs - (BOY_STAIRS + TEENAGER_STAIRS);
                stageProgress = (float)youngAdultProgress / YOUNG_ADULT_STAIRS;
                stairsToNext = TOTAL_STAIRS - currentStairs;
                break;
                
            case LifeStage.Completed:
                stageProgress = 1.0f;
                stairsToNext = 0;
                break;
                
            default:
                stageProgress = 0f;
                stairsToNext = BOY_STAIRS;
                break;
        }
        
        // 진행률은 0~1 범위로 제한
        stageProgress = Mathf.Clamp01(stageProgress);
        stairsToNext = Mathf.Max(0, stairsToNext);
    }

    #endregion

    #region 유틸리티 메서드

    /// <summary>
    /// 각 인생 단계의 시작 계단 수를 반환합니다
    /// </summary>
    /// <param name="stage">인생 단계</param>
    /// <returns>시작 계단 수</returns>
    public static int GetStageStartStairs(LifeStage stage)
    {
        switch (stage)
        {
            case LifeStage.Boy:
                return 0;
            case LifeStage.Teenager:
                return BOY_STAIRS;
            case LifeStage.YoungAdult:
                return BOY_STAIRS + TEENAGER_STAIRS;
            case LifeStage.Completed:
                return TOTAL_STAIRS;
            default:
                return 0;
        }
    }

    /// <summary>
    /// 각 인생 단계의 종료 계단 수를 반환합니다
    /// </summary>
    /// <param name="stage">인생 단계</param>
    /// <returns>종료 계단 수</returns>
    public static int GetStageEndStairs(LifeStage stage)
    {
        switch (stage)
        {
            case LifeStage.Boy:
                return BOY_STAIRS;
            case LifeStage.Teenager:
                return BOY_STAIRS + TEENAGER_STAIRS;
            case LifeStage.YoungAdult:
            case LifeStage.Completed:
                return TOTAL_STAIRS;
            default:
                return BOY_STAIRS;
        }
    }

    /// <summary>
    /// 계단 상태 결과를 Unity 콘솔에 출력합니다
    /// </summary>
    /// <param name="status">출력할 계단 상태</param>
    public static void LogStairStatus(StairStatus status)
    {
        Debug.Log($"[계단 시스템 상태]\n" +
                  $"현재 계단: {status.currentStairs}/{TOTAL_STAIRS} ({status.progressRatio:P1})\n" +
                  $"인생 단계: {status.lifeStageName} (진행률: {status.stageProgressRatio:P1})\n" +
                  $"계단 수준: {status.stairLevelName}\n" +
                  $"다음 단계까지: {status.stairsToNextStage}계단");
    }

    /// <summary>
    /// 계단 시스템의 모든 구간 정보를 출력합니다
    /// </summary>
    public static void LogStairSystemInfo()
    {
        Debug.Log("=== 계단 시스템 정보 ===");
        Debug.Log($"총 계단 수: {TOTAL_STAIRS}개");
        Debug.Log("");
        Debug.Log("[인생 단계별 계단 수]");
        Debug.Log($"소년: 0 ~ {BOY_STAIRS}개 ({BOY_STAIRS}개)");
        Debug.Log($"청소년: {BOY_STAIRS + 1} ~ {BOY_STAIRS + TEENAGER_STAIRS}개 ({TEENAGER_STAIRS}개)");
        Debug.Log($"청년: {BOY_STAIRS + TEENAGER_STAIRS + 1} ~ {TOTAL_STAIRS}개 ({YOUNG_ADULT_STAIRS}개)");
        Debug.Log("");
        Debug.Log("[계단 수준별 구간]");
        Debug.Log($"낮음: 0 ~ {LOW_LEVEL_MAX}개");
        Debug.Log($"중간: {MEDIUM_LEVEL_MIN} ~ {MEDIUM_LEVEL_MAX}개");
        Debug.Log($"높음: {HIGH_LEVEL_MIN} ~ {HIGH_LEVEL_MAX}개");
    }

    #endregion
} 