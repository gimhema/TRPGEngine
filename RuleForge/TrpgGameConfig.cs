using System;
using System.Collections.Generic;

namespace RuleForge
{
    /// <summary>
    /// 전투 밸런스 설정. 룰북에서 파싱하여 덮어쓸 수 있다.
    /// </summary>
    public class BattleConfig
    {
        /// <summary>방어력 적용 비율 (기본: 0.5 → DEF의 50%만 데미지 감소에 적용)</summary>
        public double DefenseRatio { get; set; } = 0.5;

        /// <summary>데미지 편차 최솟값 (기본: 0.8 → -20%)</summary>
        public double DamageVarianceMin { get; set; } = 0.8;

        /// <summary>데미지 편차 최댓값 (기본: 1.2 → +20%)</summary>
        public double DamageVarianceMax { get; set; } = 1.2;

        /// <summary>최소 데미지 (기본: 1)</summary>
        public int MinDamage { get; set; } = 1;

        /// <summary>방어 시 데미지 감소 비율 (기본: 0.5 → 50% 감소)</summary>
        public double DefendDamageReduction { get; set; } = 0.5;

        /// <summary>도망 기본 확률 % (기본: 50)</summary>
        public int FleeBaseChance { get; set; } = 50;

        /// <summary>SPD 차이당 도망 확률 변동 % (기본: 5)</summary>
        public int FleeChancePerSpd { get; set; } = 5;

        /// <summary>도망 최소 확률 % (기본: 10)</summary>
        public int FleeMinChance { get; set; } = 10;

        /// <summary>도망 최대 확률 % (기본: 90)</summary>
        public int FleeMaxChance { get; set; } = 90;

        /// <summary>전투 로그 최대 표시 수 (기본: 5)</summary>
        public int MaxVisibleBattleLog { get; set; } = 5;
    }

    /// <summary>
    /// 플레이어 기본 설정. 룰북에서 파싱하여 덮어쓸 수 있다.
    /// </summary>
    public class PlayerDefaultConfig
    {
        /// <summary>초기 스탯 (스탯명 → 값)</summary>
        public Dictionary<string, int> InitialStats { get; set; } = new Dictionary<string, int>();

        /// <summary>기본 레벨</summary>
        public int DefaultLevel { get; set; } = 1;

        /// <summary>기본 성별</summary>
        public string DefaultGender { get; set; } = "Not Specified";

        /// <summary>기본 성격</summary>
        public string DefaultPersonality { get; set; } = "Neutral";

        /// <summary>기본 직업</summary>
        public string DefaultJob { get; set; } = "Adventurer";

        /// <summary>기본 배경스토리</summary>
        public string DefaultBackgroundStory { get; set; } = "A mysterious past.";
    }

    /// <summary>
    /// 게임 설정 중앙 관리. 룰북 리소스에서 파싱된 데이터로 덮어쓸 수 있다.
    /// TODO: RulebookParser 구현 시 LoadFromRulebook()에서 각 Config를 설정
    /// </summary>
    public static class TrpgGameConfig
    {
        public static BattleConfig Battle { get; private set; } = new BattleConfig();
        public static PlayerDefaultConfig PlayerDefault { get; private set; } = new PlayerDefaultConfig();

        /// <summary>
        /// 전투 설정을 룰북 데이터로 교체한다.
        /// </summary>
        public static void SetBattleConfig(BattleConfig config)
        {
            Battle = config;
        }

        /// <summary>
        /// 플레이어 기본 설정을 룰북 데이터로 교체한다.
        /// </summary>
        public static void SetPlayerDefaultConfig(PlayerDefaultConfig config)
        {
            PlayerDefault = config;
        }

        /// <summary>
        /// 모든 설정을 기본값으로 초기화한다.
        /// </summary>
        public static void Reset()
        {
            Battle = new BattleConfig();
            PlayerDefault = new PlayerDefaultConfig();
        }
    }
}
