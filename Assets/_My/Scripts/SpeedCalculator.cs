using System;
using UnityEngine;

/// <summary>
/// 플레이어의 게임 진행 속도를 계산하고 판정하는 시스템
/// </summary>
[System.Serializable]
public class SpeedCalculator
{
    #region 상수
    
    /// <summary>
    /// 총 계단 수
    /// </summary>
    public const int TOTAL_STAIRS = 120;
    
    /// <summary>
    /// 게임 제한 시간 (초)
    /// </summary>
    public const float TOTAL_TIME_LIMIT = 30f;
    
    #endregion

    #region 속도 지수 구간 상수
    
    public const float VERY_FAST_THRESHOLD = 0.6f;
    public const float FAST_THRESHOLD = 0.7f;
    public const float NORMAL_THRESHOLD = 0.9f;
    public const float SLOW_THRESHOLD = 1.1f;
    
    #endregion

    #region 열거형
    
    /// <summary>
    /// 속도 판정 등급
    /// </summary>
    public enum SpeedGrade
    {
        VeryFast,   // 매우 빠름
        Fast,       // 빠름
        Normal,     // 보통
        Slow,       // 느림
        VerySlow    // 매우 느림
    }
    
    #endregion

    #region 계산 결과 데이터 구조체
    
    /// <summary>
    /// 속도 계산 결과를 담는 구조체
    /// </summary>
    [System.Serializable]
    public struct SpeedCalculationResult
    {
        public int climbedStairs;           // 올라간 계단 수
        public float actualPlayTime;        // 실제 플레이 시간
        public float stairRatio;            // 도달한 계단 비율
        public float idealTime;             // 이상적 시간
        public float speedIndex;            // 속도 지수
        public SpeedGrade speedGrade;       // 속도 등급
        public string speedGradeText;       // 속도 등급 텍스트
        
        /// <summary>
        /// 계산 결과의 문자열 표현
        /// </summary>
        public override string ToString()
        {
            return $"계단: {climbedStairs}/{TOTAL_STAIRS}, " +
                   $"시간: {actualPlayTime:F1}s, " +
                   $"속도지수: {speedIndex:F2}, " +
                   $"등급: {speedGradeText}";
        }
    }
    
    #endregion

    #region 공개 메서드
    
    /// <summary>
    /// 속도를 계산하고 판정 결과를 반환합니다
    /// </summary>
    /// <param name="climbedStairs">플레이어가 올라간 계단 수</param>
    /// <param name="actualPlayTime">실제 플레이 시간 (초)</param>
    /// <returns>속도 계산 결과</returns>
    public static SpeedCalculationResult CalculateSpeed(int climbedStairs, float actualPlayTime)
    {
        SpeedCalculationResult result = new SpeedCalculationResult();
        
        // 기본 데이터 설정
        result.climbedStairs = climbedStairs;
        result.actualPlayTime = actualPlayTime;
        
        // 도달한 계단 비율 계산
        result.stairRatio = CalculateStairRatio(climbedStairs);
        
        // 이상적 시간 계산
        result.idealTime = CalculateIdealTime(result.stairRatio);
        
        // 속도 지수 계산
        result.speedIndex = CalculateSpeedIndex(actualPlayTime, result.idealTime);
        
        // 속도 등급 판정
        result.speedGrade = DetermineSpeedGrade(result.speedIndex);
        result.speedGradeText = GetSpeedGradeText(result.speedGrade);
        
        return result;
    }
    
    /// <summary>
    /// 도달한 계단 비율을 계산합니다
    /// </summary>
    /// <param name="climbedStairs">올라간 계단 수</param>
    /// <returns>계단 비율 (0.0 ~ 1.0)</returns>
    public static float CalculateStairRatio(int climbedStairs)
    {
        return Mathf.Clamp01((float)climbedStairs / TOTAL_STAIRS);
    }
    
    /// <summary>
    /// 이상적 시간을 계산합니다
    /// </summary>
    /// <param name="stairRatio">도달한 계단 비율</param>
    /// <returns>이상적 시간 (초)</returns>
    public static float CalculateIdealTime(float stairRatio)
    {
        return stairRatio * TOTAL_TIME_LIMIT;
    }
    
    /// <summary>
    /// 속도 지수를 계산합니다
    /// </summary>
    /// <param name="actualTime">실제 플레이 시간</param>
    /// <param name="idealTime">이상적 시간</param>
    /// <returns>속도 지수</returns>
    public static float CalculateSpeedIndex(float actualTime, float idealTime)
    {
        // 이상적 시간이 0이면 무한대 반환 방지
        if (idealTime <= 0f)
        {
            return float.MaxValue;
        }
        
        return actualTime / idealTime;
    }
    
    /// <summary>
    /// 속도 지수에 따른 등급을 판정합니다
    /// </summary>
    /// <param name="speedIndex">속도 지수</param>
    /// <returns>속도 등급</returns>
    public static SpeedGrade DetermineSpeedGrade(float speedIndex)
    {
        if (speedIndex < VERY_FAST_THRESHOLD)
            return SpeedGrade.VeryFast;
        else if (speedIndex < FAST_THRESHOLD)
            return SpeedGrade.Fast;
        else if (speedIndex < NORMAL_THRESHOLD)
            return SpeedGrade.Normal;
        else if (speedIndex < SLOW_THRESHOLD)
            return SpeedGrade.Slow;
        else
            return SpeedGrade.VerySlow;
    }
    
    /// <summary>
    /// 속도 등급의 한국어 텍스트를 반환합니다
    /// </summary>
    /// <param name="grade">속도 등급</param>
    /// <returns>등급 텍스트</returns>
    public static string GetSpeedGradeText(SpeedGrade grade)
    {
        switch (grade)
        {
            case SpeedGrade.VeryFast:
                return "매우 빠름";
            case SpeedGrade.Fast:
                return "빠름";
            case SpeedGrade.Normal:
                return "보통";
            case SpeedGrade.Slow:
                return "느림";
            case SpeedGrade.VerySlow:
                return "매우 느림";
            default:
                return "알 수 없음";
        }
    }
    
    /// <summary>
    /// 특정 계단 수에서의 목표 시간을 계산합니다 (참고용)
    /// </summary>
    /// <param name="targetStairs">목표 계단 수</param>
    /// <returns>해당 계단까지의 목표 시간</returns>
    public static float GetTargetTimeForStairs(int targetStairs)
    {
        float ratio = CalculateStairRatio(targetStairs);
        return CalculateIdealTime(ratio);
    }
    
    /// <summary>
    /// 속도 지수가 허용된 속도 등급들 중 하나와 일치하는지 확인합니다
    /// </summary>
    /// <param name="speedIndex">확인할 속도 지수</param>
    /// <param name="allowedGrades">허용된 속도 등급 배열</param>
    /// <returns>일치 여부</returns>
    public static bool IsSpeedGradeAllowed(float speedIndex, SpeedGrade[] allowedGrades)
    {
        if (allowedGrades == null || allowedGrades.Length == 0)
            return false;
            
        SpeedGrade currentGrade = DetermineSpeedGrade(speedIndex);
        
        foreach (SpeedGrade allowedGrade in allowedGrades)
        {
            if (currentGrade == allowedGrade)
                return true;
        }
        
        return false;
    }
    
    #endregion

    #region 디버그 및 유틸리티 메서드
    
    /// <summary>
    /// 속도 계산 결과를 Unity 콘솔에 출력합니다
    /// </summary>
    /// <param name="result">출력할 계산 결과</param>
    public static void LogSpeedResult(SpeedCalculationResult result)
    {
        Debug.Log($"[속도 계산 결과]\n" +
                  $"올라간 계단: {result.climbedStairs}/{TOTAL_STAIRS} ({result.stairRatio:P1})\n" +
                  $"실제 시간: {result.actualPlayTime:F2}초\n" +
                  $"이상적 시간: {result.idealTime:F2}초\n" +
                  $"속도 지수: {result.speedIndex:F3}\n" +
                  $"속도 등급: {result.speedGradeText}");
    }
    
    #endregion
} 