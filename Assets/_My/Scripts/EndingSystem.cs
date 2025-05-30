using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 각 직업별로 고유한 엔딩 문구를 관리하는 시스템
/// </summary>
[System.Serializable]
public class EndingSystem
{
    #region 엔딩 데이터 구조체

    /// <summary>
    /// 엔딩 정보를 담는 구조체
    /// </summary>
    [System.Serializable]
    public struct EndingInfo
    {
        public JobSystem.JobType jobType;       // 직업 유형
        public string endingTitle;              // 엔딩 제목
        public string endingMessage;            // 엔딩 메시지
        public string endingSubtext;            // 엔딩 부제 (추가 메시지)

        /// <summary>
        /// 엔딩 정보의 문자열 표현
        /// </summary>
        public override string ToString()
        {
            return $"{endingTitle}\n{endingMessage}";
        }
    }

    /// <summary>
    /// 엔딩 결과를 담는 구조체
    /// </summary>
    [System.Serializable]
    public struct EndingResult
    {
        public EndingInfo endingInfo;           // 엔딩 정보
        public JobSystem.JobResult jobResult;   // 직업 결정 결과
        public SpeedCalculator.SpeedCalculationResult speedResult; // 속도 계산 결과
        public StairSystem.StairStatus stairStatus; // 계단 상태
        public bool isGameCleared;              // 게임 클리어 여부

        /// <summary>
        /// 엔딩 결과의 문자열 표현
        /// </summary>
        public override string ToString()
        {
            return $"{endingInfo.endingTitle}\n" +
                   $"직업: {jobResult.jobInfo.jobName}\n" +
                   $"결과: {(isGameCleared ? "게임 클리어" : "게임 오버")}";
        }
    }

    #endregion

    #region 엔딩 데이터베이스

    /// <summary>
    /// 모든 직업별 엔딩 정보를 담고 있는 데이터베이스
    /// </summary>
    private static readonly Dictionary<JobSystem.JobType, EndingInfo> endingDatabase = new Dictionary<JobSystem.JobType, EndingInfo>()
    {
        // 가수 엔딩
        {
            JobSystem.JobType.Singer,
            new EndingInfo
            {
                jobType = JobSystem.JobType.Singer,
                endingTitle = "무대 위의 별",
                endingMessage = "당신은 무대 위의 별처럼 빠르게 빛났습니다. 순간의 열정과 대중의 사랑 속에서 자유롭게 노래했지만, 안정된 길은 아니었지요. 앞으로도 도전은 계속됩니다.",
                endingSubtext = "빛나는 순간과 불안정한 삶이 공존하는 예술가의 운명"
            }
        },

        // 간호사 엔딩
        {
            JobSystem.JobType.Nurse,
            new EndingInfo
            {
                jobType = JobSystem.JobType.Nurse,
                endingTitle = "치유의 손길",
                endingMessage = "당신은 묵묵히 한 사람, 한 사람을 돌보았습니다. 꾸준한 헌신과 노력이 많은 이들의 삶을 지켰습니다. 진심 어린 손길은 세상을 따뜻하게 합니다.",
                endingSubtext = "헌신과 사랑으로 타인을 돌보는 천사"
            }
        },

        // 거지 엔딩
        {
            JobSystem.JobType.Beggar,
            new EndingInfo
            {
                jobType = JobSystem.JobType.Beggar,
                endingTitle = "방황의 끝에서",
                endingMessage = "삶의 방향을 잃고 방황했지만, 그 또한 당신의 이야기입니다. 멈추지 않고 다시 일어서길 바랍니다. 새로운 계단이 기다리고 있으니까요.",
                endingSubtext = "실패는 끝이 아닌 새로운 시작의 기회"
            }
        },

        // 공무원 엔딩
        {
            JobSystem.JobType.CivilServant,
            new EndingInfo
            {
                jobType = JobSystem.JobType.CivilServant,
                endingTitle = "사회의 기둥",
                endingMessage = "당신은 사회를 위해 성실히 일했습니다. 꾸준함과 책임감으로 안정된 삶을 쌓아갔습니다. 사람들에게 신뢰받는 자리에서 빛나고 있습니다.",
                endingSubtext = "안정과 신뢰로 사회를 지탱하는 든든한 기둥"
            }
        },

        // 교사 엔딩
        {
            JobSystem.JobType.Teacher,
            new EndingInfo
            {
                jobType = JobSystem.JobType.Teacher,
                endingTitle = "마음을 키우는 스승",
                endingMessage = "당신은 지식을 나누고 마음을 키웠습니다. 인내와 사랑으로 아이들의 꿈을 키워낸 훌륭한 스승입니다. 그 가르침은 오래도록 기억될 것입니다.",
                endingSubtext = "지식과 사랑으로 미래를 키우는 교육자"
            }
        },

        // 교수 엔딩
        {
            JobSystem.JobType.Professor,
            new EndingInfo
            {
                jobType = JobSystem.JobType.Professor,
                endingTitle = "지혜의 등불",
                endingMessage = "오랜 시간 깊은 연구와 사색에 잠겼습니다. 비록 천천히 걸었지만, 당신의 지혜는 빛나며 미래 세대를 밝혀줄 등불이 되었습니다.",
                endingSubtext = "깊은 연구와 사색으로 인류의 지식을 넓힌 학자"
            }
        },

        // 대통령 엔딩
        {
            JobSystem.JobType.President,
            new EndingInfo
            {
                jobType = JobSystem.JobType.President,
                endingTitle = "역사를 이끈 리더",
                endingMessage = "당신은 빠른 결단과 무거운 책임을 감당했습니다. 국민을 위해 헌신하며 큰 변화를 이끌었지요. 당신의 발자취는 역사의 한 페이지로 남을 것입니다.",
                endingSubtext = "빠른 결단력과 강한 리더십으로 역사를 만든 지도자"
            }
        },

        // 무용가 엔딩
        {
            JobSystem.JobType.Dancer,
            new EndingInfo
            {
                jobType = JobSystem.JobType.Dancer,
                endingTitle = "춤추는 영혼",
                endingMessage = "당신은 삶을 춤으로 표현했습니다. 순간의 열정과 빛나는 무대 위에서 자유로웠지만, 그 여정은 언제나 불안정했습니다.",
                endingSubtext = "몸짓으로 감정을 표현하는 자유로운 예술혼"
            }
        },

        // 소방관 엔딩
        {
            JobSystem.JobType.Firefighter,
            new EndingInfo
            {
                jobType = JobSystem.JobType.Firefighter,
                endingTitle = "진정한 영웅",
                endingMessage = "용기와 헌신으로 많은 생명을 구했습니다. 어둠 속에서도 두려움 없이 달려가는 당신은 진정한 영웅입니다.",
                endingSubtext = "용기와 희생정신으로 생명을 구하는 영웅"
            }
        },

        // 의사 엔딩
        {
            JobSystem.JobType.Doctor,
            new EndingInfo
            {
                jobType = JobSystem.JobType.Doctor,
                endingTitle = "생명을 살리는 손",
                endingMessage = "긴 시간 쌓은 지식과 노력이 많은 생명을 살렸습니다. 당신의 손길은 희망이자 치유입니다. 이 세상에 꼭 필요한 존재입니다.",
                endingSubtext = "의술과 헌신으로 생명을 구하는 치료자"
            }
        },

        // 작가 엔딩
        {
            JobSystem.JobType.Writer,
            new EndingInfo
            {
                jobType = JobSystem.JobType.Writer,
                endingTitle = "마음을 울리는 이야기꾼",
                endingMessage = "고독 속에서 글을 써내려갔습니다. 내면의 아픔과 기쁨이 이야기가 되어 많은 이들의 마음을 울렸습니다.",
                endingSubtext = "창작의 고통을 통해 감동을 만들어내는 문학가"
            }
        },

        // 철학자 엔딩
        {
            JobSystem.JobType.Philosopher,
            new EndingInfo
            {
                jobType = JobSystem.JobType.Philosopher,
                endingTitle = "진리를 탐구하는 사상가",
                endingMessage = "느리지만 깊은 사유로 진리를 탐구했습니다. 당신의 지혜는 세상을 이해하는 빛이 되어 오래도록 기억될 것입니다.",
                endingSubtext = "깊은 사색과 철학적 성찰로 지혜를 추구하는 사상가"
            }
        },

        // 회사원 엔딩
        {
            JobSystem.JobType.Employee,
            new EndingInfo
            {
                jobType = JobSystem.JobType.Employee,
                endingTitle = "성실한 일상의 힘",
                endingMessage = "꾸준한 노력으로 현실을 견뎌냈습니다. 평범하지만 흔들리지 않는 성실함이 당신의 가장 큰 힘입니다.",
                endingSubtext = "평범하지만 꾸준한 노력으로 안정을 만들어가는 사람"
            }
        },

        // 회상가 엔딩
        {
            JobSystem.JobType.Reminiscer,
            new EndingInfo
            {
                jobType = JobSystem.JobType.Reminiscer,
                endingTitle = "추억 속의 성찰",
                endingMessage = "지난 삶을 돌아보며 성찰했습니다. 과거의 추억과 아쉬움 속에서 내일을 준비하는 당신의 마음은 따뜻합니다.",
                endingSubtext = "과거를 되돌아보며 지혜를 얻는 성찰하는 삶"
            }
        }
    };

    #endregion

    #region 공개 메서드

    /// <summary>
    /// 게임 결과를 바탕으로 엔딩을 생성합니다
    /// </summary>
    /// <param name="jobResult">직업 결정 결과</param>
    /// <param name="speedResult">속도 계산 결과</param>
    /// <param name="stairStatus">계단 상태</param>
    /// <param name="isGameCleared">게임 클리어 여부</param>
    /// <returns>엔딩 결과</returns>
    public static EndingResult GenerateEnding(JobSystem.JobResult jobResult, 
                                            SpeedCalculator.SpeedCalculationResult speedResult,
                                            StairSystem.StairStatus stairStatus,
                                            bool isGameCleared)
    {
        EndingResult result = new EndingResult();
        result.jobResult = jobResult;
        result.speedResult = speedResult;
        result.stairStatus = stairStatus;
        result.isGameCleared = isGameCleared;

        // 직업에 따른 엔딩 정보 가져오기
        result.endingInfo = GetEndingInfo(jobResult.jobInfo.jobType);

        return result;
    }

    /// <summary>
    /// 특정 직업의 엔딩 정보를 반환합니다
    /// </summary>
    /// <param name="jobType">직업 유형</param>
    /// <returns>엔딩 정보</returns>
    public static EndingInfo GetEndingInfo(JobSystem.JobType jobType)
    {
        // Unknown인 경우 회사원 엔딩 반환
        if (jobType == JobSystem.JobType.Unknown)
        {
            return endingDatabase[JobSystem.JobType.Employee];
        }
        
        if (endingDatabase.ContainsKey(jobType))
        {
            return endingDatabase[jobType];
        }
        
        // 해당 직업의 엔딩이 없으면 회사원 엔딩 반환 (기본값)
        return endingDatabase[JobSystem.JobType.Employee];
    }

    /// <summary>
    /// 모든 엔딩 정보의 목록을 반환합니다
    /// </summary>
    /// <returns>모든 엔딩 정보 리스트</returns>
    public static List<EndingInfo> GetAllEndings()
    {
        List<EndingInfo> allEndings = new List<EndingInfo>();
        foreach (var endingPair in endingDatabase)
        {
            allEndings.Add(endingPair.Value);
        }
        return allEndings;
    }

    /// <summary>
    /// 엔딩 제목으로 직업 유형을 찾습니다
    /// </summary>
    /// <param name="endingTitle">엔딩 제목</param>
    /// <returns>직업 유형</returns>
    public static JobSystem.JobType GetJobTypeByEndingTitle(string endingTitle)
    {
        foreach (var endingPair in endingDatabase)
        {
            if (endingPair.Value.endingTitle == endingTitle)
            {
                return endingPair.Key;
            }
        }
        return JobSystem.JobType.Employee;
    }

    #endregion

    #region 엔딩 포맷팅 메서드

    /// <summary>
    /// 엔딩 결과를 완전한 형태로 포맷팅합니다
    /// </summary>
    /// <param name="endingResult">엔딩 결과</param>
    /// <returns>포맷팅된 엔딩 텍스트</returns>
    public static string FormatCompleteEnding(EndingResult endingResult)
    {
        string result = "";
        
        // 게임 결과
        result += $"=== {(endingResult.isGameCleared ? "게임 클리어!" : "게임 오버")} ===\n\n";
        
        // 엔딩 제목과 메시지
        result += $"『 {endingResult.endingInfo.endingTitle} 』\n\n";
        result += $"{endingResult.endingInfo.endingMessage}\n\n";
        
        // 게임 통계
        result += "─── 당신의 여정 ───\n";
        result += $"직업: {endingResult.jobResult.jobInfo.jobName}\n";
        result += $"도달한 계단: {endingResult.stairStatus.currentStairs}개 / {StairSystem.TOTAL_STAIRS}개\n";
        result += $"인생 단계: {endingResult.stairStatus.lifeStageName}\n";
        result += $"속도 등급: {endingResult.speedResult.speedGradeText}\n";
        result += $"속도 지수: {endingResult.speedResult.speedIndex:F2}\n\n";
        
        // 엔딩 부제
        result += $"※ {endingResult.endingInfo.endingSubtext}";
        
        return result;
    }

    /// <summary>
    /// 간단한 엔딩 메시지만 포맷팅합니다
    /// </summary>
    /// <param name="endingResult">엔딩 결과</param>
    /// <returns>간단한 엔딩 텍스트</returns>
    public static string FormatSimpleEnding(EndingResult endingResult)
    {
        return $"『 {endingResult.endingInfo.endingTitle} 』\n" +
               $"{endingResult.endingInfo.endingMessage}";
    }

    #endregion

    #region 디버그 및 유틸리티 메서드

    /// <summary>
    /// 엔딩 결과를 Unity 콘솔에 출력합니다
    /// </summary>
    /// <param name="endingResult">출력할 엔딩 결과</param>
    public static void LogEndingResult(EndingResult endingResult)
    {
        Debug.Log($"[엔딩 시스템 결과]\n{FormatCompleteEnding(endingResult)}");
    }

    /// <summary>
    /// 모든 엔딩 정보를 출력합니다
    /// </summary>
    public static void LogAllEndingInfo()
    {
        Debug.Log("=== 모든 엔딩 정보 ===");
        
        foreach (var endingPair in endingDatabase)
        {
            var ending = endingPair.Value;
            Debug.Log($"[{ending.jobType}] {ending.endingTitle}: {ending.endingMessage}");
        }
    }

    /// <summary>
    /// 특정 직업의 엔딩 정보를 상세히 출력합니다
    /// </summary>
    /// <param name="jobType">직업 유형</param>
    public static void LogEndingInfo(JobSystem.JobType jobType)
    {
        var ending = GetEndingInfo(jobType);
        Debug.Log($"[엔딩 정보 - {jobType}]\n" +
                  $"제목: {ending.endingTitle}\n" +
                  $"메시지: {ending.endingMessage}\n" +
                  $"부제: {ending.endingSubtext}");
    }

    #endregion
} 