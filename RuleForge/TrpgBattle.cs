using System;
using System.Collections.Generic;
using System.Text;

namespace RuleForge
{
    /// <summary>
    /// 전투 페이즈
    /// </summary>
    public enum BattlePhase
    {
        PlayerTurn,     // 플레이어 턴
        EnemyTurn,      // 적 턴
        Victory,        // 승리
        Defeat          // 패배
    }

    /// <summary>
    /// 전투 상태를 관리하는 클래스.
    /// 턴제 전투 루프를 TrpgGameState 기반으로 구동한다.
    /// </summary>
    public class TrpgBattle
    {
        public TrpgPlayer Player { get; set; }
        public TrpgEnemy CurrentEnemy { get; set; }
        public BattlePhase Phase { get; set; }
        public int TurnCount { get; set; }
        public bool IsPlayerDefending { get; set; }
        public List<string> BattleLog { get; set; }

        /// <summary>
        /// 전투 종료 후 호출되는 콜백.
        /// 인자: (GameState, 승리 여부)
        /// </summary>
        public Action<TrpgGameState, bool>? OnBattleEnd { get; set; }

        private static readonly Random _random = new Random();

        public TrpgBattle(TrpgPlayer player, TrpgEnemy enemy)
        {
            Player = player;
            CurrentEnemy = enemy;
            Phase = BattlePhase.PlayerTurn;
            TurnCount = 0;
            IsPlayerDefending = false;
            BattleLog = new List<string>();
        }

        // ─── 전투 시작 ───

        /// <summary>
        /// 전투를 시작하고 첫 플레이어 턴을 설정한다.
        /// </summary>
        public void StartBattle(TrpgGameState state)
        {
            TurnCount = 1;
            BattleLog.Clear();
            AddLog($"{CurrentEnemy.Name}이(가) 나타났다!");
            SetupPlayerTurn(state);
        }

        // ─── 플레이어 턴 ───

        /// <summary>
        /// 플레이어 턴 선택지를 GameState에 설정한다.
        /// </summary>
        public void SetupPlayerTurn(TrpgGameState state)
        {
            Phase = BattlePhase.PlayerTurn;
            IsPlayerDefending = false;

            state.NarrativeText = BuildBattleNarrative();
            state.ClearChoices();

            state.AddChoice(new TrpgChoice("1", "1. 공격")
            {
                OnSelect = (s) => PlayerAttack(s)
            });
            state.AddChoice(new TrpgChoice("2", "2. 방어")
            {
                OnSelect = (s) => PlayerDefend(s)
            });
            state.AddChoice(new TrpgChoice("3", "3. 아이템 사용")
            {
                OnSelect = (s) => ShowItemMenu(s)
            });
            state.AddChoice(new TrpgChoice("4", "4. 도망치기")
            {
                OnSelect = (s) => AttemptFlee(s)
            });
        }

        // ─── 플레이어 액션 ───

        /// <summary>
        /// 플레이어가 적을 공격한다.
        /// </summary>
        public void PlayerAttack(TrpgGameState state)
        {
            int playerAtk = GetStatValue(Player, "ATK");
            int enemyDef = GetStatValue(CurrentEnemy, "DEF");
            int damage = CalculateDamage(playerAtk, enemyDef);

            ApplyDamage(CurrentEnemy, damage);
            AddLog($"{Player.Name}의 공격! {CurrentEnemy.Name}에게 {damage}의 데미지!");

            if (GetStatValue(CurrentEnemy, "HP") <= 0)
            {
                Victory(state);
                return;
            }

            EnemyTurn(state);
        }

        /// <summary>
        /// 플레이어가 방어 자세를 취한다. 다음 적 공격의 데미지를 50% 감소시킨다.
        /// </summary>
        public void PlayerDefend(TrpgGameState state)
        {
            IsPlayerDefending = true;
            AddLog($"{Player.Name}은(는) 방어 자세를 취했다!");
            EnemyTurn(state);
        }

        /// <summary>
        /// 소비 아이템 사용 메뉴를 표시한다.
        /// </summary>
        public void ShowItemMenu(TrpgGameState state)
        {
            var consumables = Player.playerItemBag.ConsumableItems;

            if (consumables.Count == 0)
            {
                AddLog("사용할 수 있는 아이템이 없다!");
                SetupPlayerTurn(state);
                return;
            }

            state.NarrativeText = BuildBattleNarrative() + "\n\n[아이템 선택]";
            state.ClearChoices();

            int index = 1;
            for (int i = 0; i < consumables.Count; i++)
            {
                int itemIndex = i;
                var item = consumables[i];
                state.AddChoice(new TrpgChoice(index.ToString(), $"{index}. {item.ItemName} (x{item.Quantity})")
                {
                    OnSelect = (s) =>
                    {
                        Player.playerItemBag.UseConsumable(itemIndex);
                        AddLog($"{Player.Name}은(는) {item.ItemName}을(를) 사용했다!");
                        EnemyTurn(s);
                    }
                });
                index++;
            }

            state.AddChoice(new TrpgChoice(index.ToString(), $"{index}. 돌아가기")
            {
                OnSelect = (s) => SetupPlayerTurn(s)
            });
        }

        /// <summary>
        /// 도망치기를 시도한다. SPD 차이에 따라 성공 확률이 변동된다.
        /// </summary>
        public void AttemptFlee(TrpgGameState state)
        {
            int playerSpd = GetStatValue(Player, "SPD");
            int enemySpd = GetStatValue(CurrentEnemy, "SPD");

            // 기본 50% + SPD 차이당 ±5%, 최소 10% ~ 최대 90%
            int fleeChance = 50 + (playerSpd - enemySpd) * 5;
            fleeChance = Math.Clamp(fleeChance, 10, 90);

            if (_random.Next(100) < fleeChance)
            {
                AddLog("도망에 성공했다!");
                EndBattle(state, false);
                return;
            }

            AddLog("도망에 실패했다!");
            EnemyTurn(state);
        }

        // ─── 적 턴 ───

        /// <summary>
        /// 적의 턴을 처리한다. 기본 공격을 수행한다.
        /// </summary>
        public void EnemyTurn(TrpgGameState state)
        {
            Phase = BattlePhase.EnemyTurn;

            int enemyAtk = GetStatValue(CurrentEnemy, "ATK");
            int playerDef = GetStatValue(Player, "DEF");
            int damage = CalculateDamage(enemyAtk, playerDef);

            // 방어 시 데미지 50% 감소
            if (IsPlayerDefending)
            {
                damage = Math.Max(1, damage / 2);
                AddLog($"{CurrentEnemy.Name}의 공격! {Player.Name}은(는) 방어하여 {damage}의 데미지!");
            }
            else
            {
                AddLog($"{CurrentEnemy.Name}의 공격! {Player.Name}에게 {damage}의 데미지!");
            }

            ApplyDamage(Player, damage);

            if (GetStatValue(Player, "HP") <= 0)
            {
                Defeat(state);
                return;
            }

            TurnCount++;
            SetupPlayerTurn(state);
        }

        // ─── 전투 결과 ───

        /// <summary>
        /// 승리 처리. 보상 지급 후 계속 진행 선택지를 보여준다.
        /// </summary>
        private void Victory(TrpgGameState state)
        {
            Phase = BattlePhase.Victory;
            AddLog($"\n{CurrentEnemy.Name}을(를) 처치했다!");

            // 적 처치 보상 지급
            GiveEnemyReward();

            state.NarrativeText = BuildBattleNarrative();
            state.ClearChoices();
            state.AddChoice(new TrpgChoice("1", "1. 계속")
            {
                OnSelect = (s) => EndBattle(s, true)
            });
        }

        /// <summary>
        /// 패배 처리. 패배 메시지 후 계속 진행 선택지를 보여준다.
        /// </summary>
        private void Defeat(TrpgGameState state)
        {
            Phase = BattlePhase.Defeat;
            AddLog($"\n{Player.Name}은(는) 쓰러졌다...");

            state.NarrativeText = BuildBattleNarrative();
            state.ClearChoices();
            state.AddChoice(new TrpgChoice("1", "1. 계속")
            {
                OnSelect = (s) => EndBattle(s, false)
            });
        }

        /// <summary>
        /// 전투를 종료하고 OnBattleEnd 콜백을 호출한다.
        /// 콜백이 없으면 이전 씬으로 복귀한다.
        /// </summary>
        private void EndBattle(TrpgGameState state, bool isVictory)
        {
            state.CurrentBattle = null;

            if (OnBattleEnd != null)
            {
                OnBattleEnd(state, isVictory);
            }
            else
            {
                state.ReturnToPreviousScene();
            }
        }

        // ─── 데미지 계산 ───

        /// <summary>
        /// 데미지 계산 공식: ATK - DEF/2, ±20% 랜덤 편차, 최소 1
        /// </summary>
        public static int CalculateDamage(int attackPower, int defensePower)
        {
            int baseDamage = attackPower - (defensePower / 2);
            // ±20% 랜덤 편차 (0.8 ~ 1.2)
            double variance = 0.8 + _random.NextDouble() * 0.4;
            int finalDamage = (int)(baseDamage * variance);
            return Math.Max(1, finalDamage);
        }

        // ─── 보상 처리 ───

        /// <summary>
        /// 적 처치 보상을 플레이어에게 지급한다. 결과를 BattleLog에 기록한다.
        /// </summary>
        private void GiveEnemyReward()
        {
            if (CurrentEnemy.BattleReward == null || CurrentEnemy.BattleReward.Count == 0)
            {
                return;
            }

            int rewardCount = _random.Next(1, CurrentEnemy.BattleReward.Count + 1);
            AddLog($"\n보상 {rewardCount}개를 획득!");

            HashSet<int> selectedIndices = new HashSet<int>();
            for (int i = 0; i < rewardCount; i++)
            {
                int randomIndex;
                do
                {
                    randomIndex = _random.Next(0, CurrentEnemy.BattleReward.Count);
                } while (selectedIndices.Contains(randomIndex));

                selectedIndices.Add(randomIndex);
                var rewardItem = CurrentEnemy.BattleReward[randomIndex];

                if (rewardItem is Consumable consumable)
                {
                    Player.playerItemBag.AcquireConsumable(consumable);
                    AddLog($"  - {consumable.ItemName} 획득!");
                }
                else if (rewardItem is Equipment equipment)
                {
                    Player.playerItemBag.AcquireEquipment(equipment);
                    AddLog($"  - {equipment.ItemName} 획득!");
                }
                else if (rewardItem is KeyItem keyItem)
                {
                    Player.playerItemBag.KeyItems.Add(keyItem);
                    AddLog($"  - {keyItem.ItemName} 획득!");
                }
            }
        }

        // ─── 유틸리티 ───

        private void AddLog(string message)
        {
            BattleLog.Add(message);
        }

        private static int GetStatValue(TrpgActor actor, string statName)
        {
            var status = actor.CommonAttributes.GetStatus(statName);
            return status?.StatusValue ?? 0;
        }

        private static void ApplyDamage(TrpgActor actor, int damage)
        {
            int currentHp = GetStatValue(actor, "HP");
            actor.CommonAttributes.UpdateStatus("HP", Math.Max(0, currentHp - damage));
        }

        /// <summary>
        /// 전투 내러티브를 구성한다. (턴 정보 + 최근 전투 로그)
        /// </summary>
        private string BuildBattleNarrative()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"─── 턴 {TurnCount} ───");
            sb.AppendLine();

            // 최근 전투 로그 (최대 5개)
            int start = Math.Max(0, BattleLog.Count - 5);
            for (int i = start; i < BattleLog.Count; i++)
            {
                sb.AppendLine(BattleLog[i]);
            }

            return sb.ToString();
        }
    }
}
