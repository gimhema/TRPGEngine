using System;
using System.Collections.Generic;

namespace RuleForge
{
    /// <summary>
    /// 스킬 타겟 타입
    /// </summary>
    public enum SkillTargetType
    {
        Self,       // 자기 자신
        Enemy       // 적 대상
    }

    /// <summary>
    /// 스킬 효과 타입
    /// </summary>
    public enum SkillEffectType
    {
        Damage,     // 데미지
        Heal,       // HP 회복
        MpRestore,  // MP 회복
        Buff,       // 버프 (스탯 증가)
        Debuff      // 디버프 (스탯 감소)
    }

    /// <summary>
    /// 스킬 효과 정의. 하나의 스킬이 여러 효과를 가질 수 있다.
    /// </summary>
    public class SkillEffect
    {
        public SkillEffectType EffectType { get; set; }

        /// <summary>
        /// 효과 대상 스탯 이름 (Buff/Debuff 시 사용, 예: "ATK", "DEF")
        /// </summary>
        public string TargetStat { get; set; }

        /// <summary>
        /// 효과 수치 (데미지량, 회복량, 버프/디버프 증감치)
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// 버프/디버프 지속 턴 수 (0이면 즉시 효과)
        /// </summary>
        public int Duration { get; set; }

        public SkillEffect(SkillEffectType effectType, int value, string targetStat = "", int duration = 0)
        {
            EffectType = effectType;
            Value = value;
            TargetStat = targetStat;
            Duration = duration;
        }
    }

    /// <summary>
    /// TRPG 스킬 클래스
    /// </summary>
    public class TrpgSkill
    {
        public string SkillName { get; set; }
        public string Description { get; set; }
        public int MpCost { get; set; }
        public SkillTargetType TargetType { get; set; }
        public List<SkillEffect> Effects { get; set; }

        /// <summary>
        /// 스킬 습득에 필요한 최소 레벨 (0이면 제한 없음)
        /// </summary>
        public int RequiredLevel { get; set; }

        public TrpgSkill(string name, string description, int mpCost, SkillTargetType targetType)
        {
            SkillName = name;
            Description = description;
            MpCost = mpCost;
            TargetType = targetType;
            Effects = new List<SkillEffect>();
            RequiredLevel = 0;
        }

        /// <summary>
        /// 효과 추가 (빌더 패턴)
        /// </summary>
        public TrpgSkill AddEffect(SkillEffect effect)
        {
            Effects.Add(effect);
            return this;
        }

        /// <summary>
        /// 스킬 사용 가능 여부 확인 (MP 충분한지)
        /// </summary>
        public bool CanUse(TrpgActor caster)
        {
            int currentMp = caster.CommonAttributes.GetStatus("MP")?.StatusValue ?? 0;
            return currentMp >= MpCost;
        }

        /// <summary>
        /// 스킬을 사용한다. MP를 소모하고 모든 효과를 적용한다.
        /// </summary>
        /// <returns>스킬 사용 결과 로그 메시지 목록</returns>
        public List<string> Use(TrpgActor caster, TrpgActor target)
        {
            var logs = new List<string>();

            // MP 소모
            int currentMp = caster.CommonAttributes.GetStatus("MP")?.StatusValue ?? 0;
            caster.CommonAttributes.UpdateStatus("MP", currentMp - MpCost);

            foreach (var effect in Effects)
            {
                var effectTarget = (TargetType == SkillTargetType.Self) ? caster : target;

                switch (effect.EffectType)
                {
                    case SkillEffectType.Damage:
                    {
                        int casterAtk = caster.CommonAttributes.GetStatus("ATK")?.StatusValue ?? 0;
                        int targetDef = effectTarget.CommonAttributes.GetStatus("DEF")?.StatusValue ?? 0;
                        // 스킬 데미지: 기본 공식 + 스킬 고유 위력
                        int damage = TrpgBattle.CalculateDamage(casterAtk + effect.Value, targetDef);
                        int currentHp = effectTarget.CommonAttributes.GetStatus("HP")?.StatusValue ?? 0;
                        effectTarget.CommonAttributes.UpdateStatus("HP", Math.Max(0, currentHp - damage));
                        logs.Add($"{effectTarget.Name}에게 {damage}의 데미지!");
                        break;
                    }
                    case SkillEffectType.Heal:
                    {
                        int currentHp = effectTarget.CommonAttributes.GetStatus("HP")?.StatusValue ?? 0;
                        int newHp = currentHp + effect.Value;
                        effectTarget.CommonAttributes.UpdateStatus("HP", newHp);
                        logs.Add($"{effectTarget.Name}의 HP가 {effect.Value} 회복!");
                        break;
                    }
                    case SkillEffectType.MpRestore:
                    {
                        int mp = effectTarget.CommonAttributes.GetStatus("MP")?.StatusValue ?? 0;
                        int newMp = mp + effect.Value;
                        effectTarget.CommonAttributes.UpdateStatus("MP", newMp);
                        logs.Add($"{effectTarget.Name}의 MP가 {effect.Value} 회복!");
                        break;
                    }
                    case SkillEffectType.Buff:
                    {
                        int currentVal = effectTarget.CommonAttributes.GetStatus(effect.TargetStat)?.StatusValue ?? 0;
                        effectTarget.CommonAttributes.UpdateStatus(effect.TargetStat, currentVal + effect.Value);
                        logs.Add($"{effectTarget.Name}의 {effect.TargetStat}이(가) {effect.Value} 증가!");
                        break;
                    }
                    case SkillEffectType.Debuff:
                    {
                        int currentVal = effectTarget.CommonAttributes.GetStatus(effect.TargetStat)?.StatusValue ?? 0;
                        effectTarget.CommonAttributes.UpdateStatus(effect.TargetStat, Math.Max(0, currentVal - effect.Value));
                        logs.Add($"{effectTarget.Name}의 {effect.TargetStat}이(가) {effect.Value} 감소!");
                        break;
                    }
                }
            }

            return logs;
        }
    }
}
