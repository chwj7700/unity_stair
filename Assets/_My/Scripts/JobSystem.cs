using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 속도 지수와 계단 수에 따라 직업을 결정하는 시스템
/// </summary>
[System.Serializable]
public class JobSystem
{
    #region 열거형

    /// <summary>
    /// 직업 유형
    /// </summary>
    public enum JobType
    {
        Singer,      // 가수
        Nurse,       // 간호사
        Beggar,      // 거지
        CivilServant,// 공무원
        Teacher,     // 교사
        Professor,   // 교수
        President,   // 대통령
        Dancer,      // 무용가
        Firefighter, // 소방관
        Doctor,      // 의사
        Writer,      // 작가
        Philosopher, // 철학자
        Employee,    // 회사원
        Reminiscer,  // 회상가
        Unknown      // 알 수 없음 (기본값)
    }

    #endregion

    #region 직업 정보 데이터 구조체

    /// <summary>
    /// 직업 정보를 담는 구조체
    /// </summary>
    [System.Serializable]
    public struct JobInfo
    {
        public JobType jobType;                 // 직업 유형
        public string jobName;                  // 직업 이름
        public string description;              // 직업 해석 및 메시지
        public SpeedCalculator.SpeedGrade[] allowedSpeedGrades; // 허용되는 속도 등급들
        public int minStairs;                   // 최소 계단 수
        public int maxStairs;                   // 최대 계단 수
        public int priority;                    // 우선순위 (낮을수록 높은 우선순위)

        /// <summary>
        /// 직업 정보의 문자열 표현
        /// </summary>
        public override string ToString()
        {
            string speedGradeText = allowedSpeedGrades != null && allowedSpeedGrades.Length > 0 
                ? string.Join(", ", System.Array.ConvertAll(allowedSpeedGrades, g => SpeedCalculator.GetSpeedGradeText(g)))
                : "없음";
            return $"{jobName}: 속도등급 [{speedGradeText}], " +
                   $"계단 {minStairs}~{maxStairs}개";
        }
    }

    /// <summary>
    /// 직업 결정 결과를 담는 구조체
    /// </summary>
    [System.Serializable]
    public struct JobResult
    {
        public JobInfo jobInfo;                 // 결정된 직업 정보
        public float speedIndex;                // 플레이어의 속도 지수
        public int stairCount;                  // 플레이어의 계단 수
        public bool isMatched;                  // 조건 매칭 여부
        public string matchReason;              // 매칭 사유

        /// <summary>
        /// 직업 결정 결과의 문자열 표현
        /// </summary>
        public override string ToString()
        {
            return $"직업: {jobInfo.jobName}, " +
                   $"속도지수: {speedIndex:F2}, " +
                   $"계단: {stairCount}개, " +
                   $"매칭: {(isMatched ? "성공" : "실패")}";
        }
    }

    #endregion

    #region 직업 데이터베이스

    /// <summary>
    /// 모든 직업 정보를 담고 있는 데이터베이스
    /// </summary>
    private static readonly Dictionary<JobType, JobInfo> jobDatabase = new Dictionary<JobType, JobInfo>()
    {
        // === 매우 빠름(VeryFast) + 빠름(Fast) 조합 직업들 ===
        
        // 대통령: 매우 빠름~빠름, 매우 높음 (100~120) - 최우선
        {
            JobType.President,
            new JobInfo
            {
                jobType = JobType.President,
                jobName = "대통령",
                description = "당신은 빠른 결단력과 뛰어난 리더십으로 국가의 최고 지도자가 되었습니다. 매우 빠른 속도로 최고의 높이에 도달하여 큰 책임과 영향력을 가진 삶을 살고 있습니다.",
                allowedSpeedGrades = new SpeedCalculator.SpeedGrade[] { 
                    SpeedCalculator.SpeedGrade.VeryFast, 
                    SpeedCalculator.SpeedGrade.Fast 
                },
                minStairs = 100,
                maxStairs = 120,
                priority = 1
            }
        },

        // 가수: 매우 빠름~빠름, 중간~낮음 (41~80) - 40 이하는 거지이므로 41부터
        {
            JobType.Singer,
            new JobInfo
            {
                jobType = JobType.Singer,
                jobName = "가수",
                description = "당신은 무대 위의 별처럼 빠르게 빛났습니다. 빠른 성공과 도전정신으로 많은 이들에게 감동을 주었지만, 불안정한 삶 속에서도 열정을 잃지 않았습니다.",
                allowedSpeedGrades = new SpeedCalculator.SpeedGrade[] { 
                    SpeedCalculator.SpeedGrade.VeryFast, 
                    SpeedCalculator.SpeedGrade.Fast 
                },
                minStairs = 41,
                maxStairs = 80,
                priority = 2
            }
        },

        // 무용가: 매우 빠름~빠름, 중간 (41~80)
        {
            JobType.Dancer,
            new JobInfo
            {
                jobType = JobType.Dancer,
                jobName = "무용가",
                description = "당신은 열정과 순간의 빛으로 사람들을 감동시키는 삶을 살고 있습니다. 빠른 움직임과 중간 정도의 성취로 불안정함과 아름다움이 공존하는 예술가의 삶을 선택했습니다.",
                allowedSpeedGrades = new SpeedCalculator.SpeedGrade[] { 
                    SpeedCalculator.SpeedGrade.VeryFast, 
                    SpeedCalculator.SpeedGrade.Fast 
                },
                minStairs = 41,
                maxStairs = 80,
                priority = 3
            }
        },

        // === 보통(Normal) + 느림(Slow) 조합 직업들 ===

        // 의사: 보통~느림, 매우 높음 (100~120)
        {
            JobType.Doctor,
            new JobInfo
            {
                jobType = JobType.Doctor,
                jobName = "의사",
                description = "당신은 긴 노력과 전문성으로 사람들의 생명을 구하는 숭고한 삶을 살고 있습니다. 꾸준한 걸음으로 최고의 높이에 도달하여 생명을 다루는 막중한 책임을 지고 있습니다.",
                allowedSpeedGrades = new SpeedCalculator.SpeedGrade[] { 
                    SpeedCalculator.SpeedGrade.Normal, 
                    SpeedCalculator.SpeedGrade.Slow 
                },
                minStairs = 100,
                maxStairs = 120,
                priority = 4
            }
        },

        // 간호사: 보통~느림, 높음 (81~120)
        {
            JobType.Nurse,
            new JobInfo
            {
                jobType = JobType.Nurse,
                jobName = "간호사",
                description = "당신은 헌신과 노력으로 타인의 아픔을 치유하는 삶을 선택했습니다. 꾸준하고 성실한 걸음으로 높은 계단을 올라 안정적이고 존경받는 삶을 살고 있습니다.",
                allowedSpeedGrades = new SpeedCalculator.SpeedGrade[] { 
                    SpeedCalculator.SpeedGrade.Normal, 
                    SpeedCalculator.SpeedGrade.Slow 
                },
                minStairs = 81,
                maxStairs = 120,
                priority = 5
            }
        },

        // 교사: 보통~느림, 중간~높음 (61~120)
        {
            JobType.Teacher,
            new JobInfo
            {
                jobType = JobType.Teacher,
                jobName = "교사",
                description = "당신은 인내와 가르침으로 보람 있는 삶을 살고 있습니다. 꾸준한 걸음으로 높은 곳에 도달하여 다음 세대를 키우는 숭고한 일을 하고 있습니다.",
                allowedSpeedGrades = new SpeedCalculator.SpeedGrade[] { 
                    SpeedCalculator.SpeedGrade.Normal, 
                    SpeedCalculator.SpeedGrade.Slow 
                },
                minStairs = 61,
                maxStairs = 120,
                priority = 6
            }
        },

        // === 보통(Normal) 전용 직업들 ===

        // 공무원: 보통, 중간~높음 (61~120)
        {
            JobType.CivilServant,
            new JobInfo
            {
                jobType = JobType.CivilServant,
                jobName = "공무원",
                description = "당신은 안정적이고 꾸준한 노력으로 사회에 봉사하는 삶을 선택했습니다. 적당한 속도로 높은 계단에 도달하여 사회적으로 인정받는 안정된 직업을 갖게 되었습니다.",
                allowedSpeedGrades = new SpeedCalculator.SpeedGrade[] { SpeedCalculator.SpeedGrade.Normal },
                minStairs = 61,
                maxStairs = 120,
                priority = 7
            }
        },

        // 소방관: 보통, 중간~높음 (61~120)
        {
            JobType.Firefighter,
            new JobInfo
            {
                jobType = JobType.Firefighter,
                jobName = "소방관",
                description = "당신은 용기와 헌신으로 생명을 구하는 숭고한 삶을 살고 있습니다. 꾸준한 노력으로 높은 곳에 도달하여 위험한 순간에도 타인을 위해 자신을 희생할 준비가 되어 있습니다.",
                allowedSpeedGrades = new SpeedCalculator.SpeedGrade[] { SpeedCalculator.SpeedGrade.Normal },
                minStairs = 61,
                maxStairs = 120,
                priority = 8
            }
        },

        // 회사원: 보통, 중간 (41~80)
        {
            JobType.Employee,
            new JobInfo
            {
                jobType = JobType.Employee,
                jobName = "회사원",
                description = "당신은 현실적이고 꾸준한 노력으로 안정을 추구하는 삶을 살고 있습니다. 적당한 속도와 중간 정도의 성취로 평범하지만 의미 있는 일상을 만들어가고 있습니다.",
                allowedSpeedGrades = new SpeedCalculator.SpeedGrade[] { SpeedCalculator.SpeedGrade.Normal },
                minStairs = 41,
                maxStairs = 80,
                priority = 9
            }
        },

        // === 느림(Slow) + 매우 느림(VerySlow) 조합 직업들 ===

        // 교수: 느림~매우 느림, 높음 (81~120)
        {
            JobType.Professor,
            new JobInfo
            {
                jobType = JobType.Professor,
                jobName = "교수",
                description = "당신은 깊은 연구와 오랜 노력으로 학문의 정상에 올랐습니다. 느리지만 확실한 걸음으로 높은 계단에 도달하여 존경받는 학자가 되었습니다.",
                allowedSpeedGrades = new SpeedCalculator.SpeedGrade[] { 
                    SpeedCalculator.SpeedGrade.Slow, 
                    SpeedCalculator.SpeedGrade.VerySlow 
                },
                minStairs = 81,
                maxStairs = 120,
                priority = 10
            }
        },

        // 작가: 느림~매우 느림, 중간~높음 (61~120)
        {
            JobType.Writer,
            new JobInfo
            {
                jobType = JobType.Writer,
                jobName = "작가",
                description = "당신은 창작의 고통과 내면 성찰을 통해 깊이 있는 작품을 만들어내는 삶을 살고 있습니다. 느린 걸음이지만 상당한 높이에 도달하여 문학과 사상을 통해 사람들의 마음을 움직이고 있습니다.",
                allowedSpeedGrades = new SpeedCalculator.SpeedGrade[] { 
                    SpeedCalculator.SpeedGrade.Slow, 
                    SpeedCalculator.SpeedGrade.VerySlow 
                },
                minStairs = 61,
                maxStairs = 120,
                priority = 11
            }
        },

        // === 매우 느림(VerySlow) 전용 직업들 ===

        // 철학자: 매우 느림, 높음 (81~120)
        {
            JobType.Philosopher,
            new JobInfo
            {
                jobType = JobType.Philosopher,
                jobName = "철학자",
                description = "당신은 깊은 사유와 느린 지혜의 삶을 선택했습니다. 매우 느린 걸음이지만 높은 정신적 경지에 도달하여 인생과 세상의 본질을 탐구하고 있습니다.",
                allowedSpeedGrades = new SpeedCalculator.SpeedGrade[] { SpeedCalculator.SpeedGrade.VerySlow },
                minStairs = 81,
                maxStairs = 120,
                priority = 12
            }
        },

        // 회상가: 매우 느림, 낮음~중간 (41~60)
        {
            JobType.Reminiscer,
            new JobInfo
            {
                jobType = JobType.Reminiscer,
                jobName = "회상가",
                description = "당신은 인생을 되돌아보고 성찰하는 후반기 삶을 살고 있습니다. 느린 걸음과 중간 정도의 성취이지만, 지나온 시간들을 돌아보며 깊은 깨달음을 얻고 있습니다.",
                allowedSpeedGrades = new SpeedCalculator.SpeedGrade[] { SpeedCalculator.SpeedGrade.VerySlow },
                minStairs = 41,
                maxStairs = 60,
                priority = 13
            }
        },

        // === 특수 케이스 ===

        // 거지: 느림~매우 느림, 낮음 (0~40) - 40계단 이하는 무조건 거지로 처리됨
        {
            JobType.Beggar,
            new JobInfo
            {
                jobType = JobType.Beggar,
                jobName = "거지",
                description = "당신은 목표를 잃고 방황하는 삶을 살고 있습니다. 낮은 성취로 인해 현재는 어려운 시간을 보내고 있지만, 이것이 끝이 아닙니다. 다시 일어설 기회는 언제나 존재합니다.",
                allowedSpeedGrades = new SpeedCalculator.SpeedGrade[] { 
                    SpeedCalculator.SpeedGrade.Slow, 
                    SpeedCalculator.SpeedGrade.VerySlow 
                },
                minStairs = 0,
                maxStairs = 40,
                priority = 14
            }
        }
    };

    #endregion

    #region 공개 메서드

    /// <summary>
    /// 속도 지수와 계단 수를 바탕으로 직업을 결정합니다
    /// </summary>
    /// <param name="speedIndex">플레이어의 속도 지수</param>
    /// <param name="stairCount">플레이어가 오른 계단 수</param>
    /// <returns>직업 결정 결과</returns>
    public static JobResult DetermineJob(float speedIndex, int stairCount)
    {
        JobResult result = new JobResult();
        result.speedIndex = speedIndex;
        result.stairCount = stairCount;
        result.isMatched = false;

        // 40계단 이하일 때는 속도등급에 관계없이 거지로 결정
        // (표에 따르면 거지는 느림~매우 느림 + 0~40계단이지만, 40계단 이하는 모든 속도에서 거지로 강제 할당)
        if (stairCount <= 40)
        {
            result.jobInfo = GetJobInfo(JobType.Beggar);
            result.isMatched = true;
            result.matchReason = "40계단 이하로 인한 강제 거지 할당";
            return result;
        }

        // 속도 지수를 속도 등급으로 변환
        SpeedCalculator.SpeedGrade playerSpeedGrade = SpeedCalculator.DetermineSpeedGrade(speedIndex);

        // 매칭되는 직업들을 찾아서 우선순위 순으로 정렬
        List<JobInfo> matchedJobs = new List<JobInfo>();

        foreach (var jobPair in jobDatabase)
        {
            JobInfo job = jobPair.Value;
            
            // 속도 등급과 계단 수 조건 모두 만족하는지 확인
            bool speedMatch = SpeedCalculator.IsSpeedGradeAllowed(speedIndex, job.allowedSpeedGrades);
            bool stairMatch = stairCount >= job.minStairs && stairCount <= job.maxStairs;

            if (speedMatch && stairMatch)
            {
                matchedJobs.Add(job);
            }
        }

        // 매칭된 직업이 있으면 우선순위가 가장 높은 것 선택
        if (matchedJobs.Count > 0)
        {
            matchedJobs.Sort((a, b) => a.priority.CompareTo(b.priority));
            result.jobInfo = matchedJobs[0];
            result.isMatched = true;
            result.matchReason = $"속도등급({SpeedCalculator.GetSpeedGradeText(playerSpeedGrade)})과 계단수 조건 매칭";
        }
        else
        {
            // 매칭되는 직업이 없으면 기본 직업 할당
            result.jobInfo = GetDefaultJob();
            result.isMatched = false;
            result.matchReason = $"조건 불일치(속도등급: {SpeedCalculator.GetSpeedGradeText(playerSpeedGrade)}, 계단: {stairCount}개)로 기본 직업 할당";
        }

        return result;
    }

    /// <summary>
    /// 특정 직업의 정보를 반환합니다
    /// </summary>
    /// <param name="jobType">직업 유형</param>
    /// <returns>직업 정보</returns>
    public static JobInfo GetJobInfo(JobType jobType)
    {
        if (jobDatabase.ContainsKey(jobType))
        {
            return jobDatabase[jobType];
        }
        return GetDefaultJob();
    }

    /// <summary>
    /// 모든 직업의 목록을 반환합니다
    /// </summary>
    /// <returns>모든 직업 정보 리스트</returns>
    public static List<JobInfo> GetAllJobs()
    {
        List<JobInfo> allJobs = new List<JobInfo>();
        foreach (var jobPair in jobDatabase)
        {
            allJobs.Add(jobPair.Value);
        }
        return allJobs;
    }

    /// <summary>
    /// 직업 이름으로 직업 유형을 찾습니다
    /// </summary>
    /// <param name="jobName">직업 이름</param>
    /// <returns>직업 유형</returns>
    public static JobType GetJobTypeByName(string jobName)
    {
        foreach (var jobPair in jobDatabase)
        {
            if (jobPair.Value.jobName == jobName)
            {
                return jobPair.Key;
            }
        }
        return JobType.Unknown;
    }

    /// <summary>
    /// 기본 직업 정보를 반환합니다 (매칭 실패 시 사용)
    /// </summary>
    /// <returns>기본 직업 정보</returns>
    private static JobInfo GetDefaultJob()
    {
        return new JobInfo
        {
            jobType = JobType.Unknown,
            jobName = "평범한 사람",
            description = "당신은 평범하지만 소중한 삶을 살고 있습니다. 특별하지 않을지라도 당신만의 가치와 의미가 있습니다. 매일의 작은 행복과 노력이 모여 당신만의 독특한 인생 이야기를 만들어가고 있습니다.",
            allowedSpeedGrades = new SpeedCalculator.SpeedGrade[] { 
                SpeedCalculator.SpeedGrade.VeryFast, 
                SpeedCalculator.SpeedGrade.Fast, 
                SpeedCalculator.SpeedGrade.Normal, 
                SpeedCalculator.SpeedGrade.Slow, 
                SpeedCalculator.SpeedGrade.VerySlow 
            },
            minStairs = 0,
            maxStairs = int.MaxValue,
            priority = 99
        };
    }

    #endregion

    #region 디버그 및 유틸리티 메서드

    /// <summary>
    /// 직업 결정 결과를 Unity 콘솔에 출력합니다
    /// </summary>
    /// <param name="result">출력할 직업 결정 결과</param>
    public static void LogJobResult(JobResult result)
    {
        Debug.Log($"[직업 시스템 결과]\n" +
                  $"결정된 직업: {result.jobInfo.jobName}\n" +
                  $"플레이어 속도지수: {result.speedIndex:F3}\n" +
                  $"플레이어 계단수: {result.stairCount}개\n" +
                  $"매칭 상태: {(result.isMatched ? "성공" : "실패")} ({result.matchReason})\n" +
                  $"직업 설명: {result.jobInfo.description}");
    }

    /// <summary>
    /// 모든 직업의 조건을 출력합니다
    /// </summary>
    public static void LogAllJobConditions()
    {
        Debug.Log("=== 모든 직업 조건 ===");
        
        var sortedJobs = GetAllJobs();
        sortedJobs.Sort((a, b) => a.priority.CompareTo(b.priority));
        
        foreach (var job in sortedJobs)
        {
            string speedRange = job.allowedSpeedGrades != null && job.allowedSpeedGrades.Length > 0 
                ? string.Join(", ", System.Array.ConvertAll(job.allowedSpeedGrades, g => SpeedCalculator.GetSpeedGradeText(g)))
                : "없음";
                
            Debug.Log($"{job.jobName}: 속도등급 [{speedRange}], " +
                      $"계단 {job.minStairs}~{job.maxStairs}개 " +
                      $"(우선순위: {job.priority})");
        }
    }

    /// <summary>
    /// 특정 조건에 매칭되는 모든 직업을 찾아 출력합니다
    /// </summary>
    /// <param name="speedIndex">속도 지수</param>
    /// <param name="stairCount">계단 수</param>
    public static void LogMatchingJobs(float speedIndex, int stairCount)
    {
        Debug.Log($"=== 조건 매칭 직업 검색 ===");
        Debug.Log($"검색 조건: 속도지수 {speedIndex:F2}, 계단 {stairCount}개");
        
        List<JobInfo> matchedJobs = new List<JobInfo>();
        
        foreach (var jobPair in jobDatabase)
        {
            JobInfo job = jobPair.Value;
            bool speedMatch = SpeedCalculator.IsSpeedGradeAllowed(speedIndex, job.allowedSpeedGrades);
            bool stairMatch = stairCount >= job.minStairs && stairCount <= job.maxStairs;
            
            if (speedMatch && stairMatch)
            {
                matchedJobs.Add(job);
            }
        }
        
        if (matchedJobs.Count > 0)
        {
            matchedJobs.Sort((a, b) => a.priority.CompareTo(b.priority));
            Debug.Log($"매칭된 직업 {matchedJobs.Count}개:");
            
            for (int i = 0; i < matchedJobs.Count; i++)
            {
                Debug.Log($"{i + 1}. {matchedJobs[i].jobName} (우선순위: {matchedJobs[i].priority})");
            }
        }
        else
        {
            Debug.Log("매칭되는 직업이 없습니다. 기본 직업이 할당됩니다.");
        }
    }

    #endregion
} 