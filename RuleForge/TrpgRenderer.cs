using System;
using System.Collections.Generic;

namespace RuleForge
{
    /// <summary>
    /// 게임 화면 렌더링을 담당하는 클래스
    /// 게임 상태를 받아서 콘솔에 출력
    /// </summary>
    public class TrpgRenderer
    {
        private static TrpgRenderer? _instance;

        public static TrpgRenderer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TrpgRenderer();
                }
                return _instance;
            }
        }

        private TrpgRenderer()
        {
        }

        /// <summary>
        /// 게임 상태를 렌더링
        /// </summary>
        public void Render(TrpgGameState state)
        {
            Console.Clear();

            // 씬에 따라 다르게 렌더링
            switch (state.CurrentScene)
            {
                case TrpgGameState.SceneType.MainMenu:
                    RenderMainMenu(state);
                    break;
                case TrpgGameState.SceneType.GameModeSelect:
                    RenderGameModeSelect(state);
                    break;
                case TrpgGameState.SceneType.PlayerSetup:
                    RenderPlayerSetup(state);
                    break;
                case TrpgGameState.SceneType.Exploration:
                    RenderExploration(state);
                    break;
                case TrpgGameState.SceneType.Combat:
                    RenderCombat(state);
                    break;
                case TrpgGameState.SceneType.Social:
                    RenderSocial(state);
                    break;
                case TrpgGameState.SceneType.Shop:
                    RenderShop(state);
                    break;
                case TrpgGameState.SceneType.Inventory:
                    RenderInventory(state);
                    break;
                case TrpgGameState.SceneType.Settings:
                    RenderSettings(state);
                    break;
                case TrpgGameState.SceneType.GameOver:
                    RenderGameOver(state);
                    break;
                case TrpgGameState.SceneType.GameClear:
                    RenderGameClear(state);
                    break;
                default:
                    Console.WriteLine($"Unknown scene: {state.CurrentScene}");
                    break;
            }

            // 하단 도움말
            RenderHelp();
        }

        /// <summary>
        /// 메인 메뉴 렌더링
        /// </summary>
        private void RenderMainMenu(TrpgGameState state)
        {
            RenderHeader("MAIN MENU");
            RenderNarrative(state);
            RenderChoices(state);
        }

        /// <summary>
        /// 게임 모드 선택 렌더링
        /// </summary>
        private void RenderGameModeSelect(TrpgGameState state)
        {
            RenderHeader("GAME MODE SELECT");
            RenderNarrative(state);
            RenderChoices(state);
        }

        /// <summary>
        /// 플레이어 설정 렌더링
        /// </summary>
        private void RenderPlayerSetup(TrpgGameState state)
        {
            RenderHeader("PLAYER SETUP");
            RenderNarrative(state);
            RenderChoices(state);
        }

        /// <summary>
        /// 탐험 화면 렌더링
        /// </summary>
        private void RenderExploration(TrpgGameState state)
        {
            RenderHeader("EXPLORATION");
            RenderPlayerStatus(state);
            RenderNarrative(state);
            RenderChoices(state);
        }

        /// <summary>
        /// 전투 화면 렌더링
        /// </summary>
        private void RenderCombat(TrpgGameState state)
        {
            RenderHeader("BATTLE");
            RenderPlayerStatus(state);
            RenderEnemyStatus(state);
            RenderNarrative(state);
            RenderChoices(state);
        }

        /// <summary>
        /// 적 상태 렌더링 (전투 중)
        /// </summary>
        private void RenderEnemyStatus(TrpgGameState state)
        {
            if (state.CurrentBattle != null)
            {
                var enemy = state.CurrentBattle.CurrentEnemy;
                var enemyHp = enemy.CommonAttributes.GetStatus("HP");
                var enemyAtk = enemy.CommonAttributes.GetStatus("ATK");
                var enemyDef = enemy.CommonAttributes.GetStatus("DEF");

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"VS [{enemy.Name}]  ");
                Console.ResetColor();

                if (enemyHp != null) Console.Write($"HP: {enemyHp.StatusValue} ");
                if (enemyAtk != null) Console.Write($"ATK: {enemyAtk.StatusValue} ");
                if (enemyDef != null) Console.Write($"DEF: {enemyDef.StatusValue}");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// 사회 활동 렌더링
        /// </summary>
        private void RenderSocial(TrpgGameState state)
        {
            RenderHeader("SOCIAL INTERACTION");
            RenderPlayerStatus(state);
            RenderNarrative(state);
            RenderChoices(state);
        }

        /// <summary>
        /// 상점 렌더링
        /// </summary>
        private void RenderShop(TrpgGameState state)
        {
            RenderHeader("SHOP");
            RenderPlayerStatus(state);
            RenderNarrative(state);
            RenderChoices(state);
        }

        /// <summary>
        /// 인벤토리 렌더링
        /// </summary>
        private void RenderInventory(TrpgGameState state)
        {
            RenderHeader("INVENTORY");
            RenderPlayerStatus(state);

            if (state.CurrentPlayer != null)
            {
                var bag = state.CurrentPlayer.playerItemBag;

                Console.WriteLine("\n[소비 아이템]");
                if (bag.ConsumableItems.Count > 0)
                {
                    foreach (var item in bag.ConsumableItems)
                    {
                        Console.WriteLine($"  - {item.ItemName} x{item.Quantity}");
                    }
                }
                else
                {
                    Console.WriteLine("  (없음)");
                }

                Console.WriteLine("\n[장비]");
                if (bag.EquipmentItems.Count > 0)
                {
                    foreach (var item in bag.EquipmentItems)
                    {
                        Console.WriteLine($"  - {item.ItemName}");
                    }
                }
                else
                {
                    Console.WriteLine("  (없음)");
                }

                Console.WriteLine("\n[키 아이템]");
                if (bag.KeyItems.Count > 0)
                {
                    foreach (var item in bag.KeyItems)
                    {
                        Console.WriteLine($"  - {item.ItemName}");
                    }
                }
                else
                {
                    Console.WriteLine("  (없음)");
                }
            }

            RenderChoices(state);
        }

        /// <summary>
        /// 설정 렌더링
        /// </summary>
        private void RenderSettings(TrpgGameState state)
        {
            RenderHeader("SETTINGS");
            RenderNarrative(state);
            RenderChoices(state);
        }

        /// <summary>
        /// 게임 오버 렌더링
        /// </summary>
        private void RenderGameOver(TrpgGameState state)
        {
            RenderHeader("GAME OVER");
            RenderNarrative(state);
            RenderChoices(state);
        }

        /// <summary>
        /// 게임 클리어 렌더링
        /// </summary>
        private void RenderGameClear(TrpgGameState state)
        {
            RenderHeader("GAME CLEAR");
            Console.WriteLine("\n축하합니다! 게임을 클리어했습니다!");
            RenderNarrative(state);
            RenderChoices(state);
        }

        /// <summary>
        /// 헤더 렌더링
        /// </summary>
        private void RenderHeader(string title)
        {
            int width = 60;
            string border = new string('=', width);
            string paddedTitle = title.PadLeft((width + title.Length) / 2).PadRight(width);

            Console.WriteLine(border);
            Console.WriteLine(paddedTitle);
            Console.WriteLine(border);
        }

        /// <summary>
        /// 플레이어 상태 렌더링
        /// </summary>
        private void RenderPlayerStatus(TrpgGameState state)
        {
            if (state.CurrentPlayer != null)
            {
                var player = state.CurrentPlayer;
                Console.WriteLine();
                Console.WriteLine($"[{player.Name}] Lv.{player.playerProfile.PlayerLevel} {player.playerProfile.job}");

                // HP/MP 표시
                var hp = player.CommonAttributes.GetStatus("HP");
                var mp = player.CommonAttributes.GetStatus("MP");
                var atk = player.CommonAttributes.GetStatus("ATK");
                var def = player.CommonAttributes.GetStatus("DEF");
                var spd = player.CommonAttributes.GetStatus("SPD");

                if (hp != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"HP: {hp.StatusValue} ");
                    Console.ResetColor();
                }
                if (mp != null)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"MP: {mp.StatusValue} ");
                    Console.ResetColor();
                }
                if (atk != null) Console.Write($"ATK: {atk.StatusValue} ");
                if (def != null) Console.Write($"DEF: {def.StatusValue} ");
                if (spd != null) Console.Write($"SPD: {spd.StatusValue}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"  Gold: {player.Gold}");
                Console.ResetColor();
                Console.WriteLine();

                // 스킬 보유 수 표시
                if (player.PlayerSkills.Count > 0)
                {
                    Console.Write($"스킬: {player.PlayerSkills.Count}개");
                    int usable = player.GetUsableSkills().Count;
                    if (usable < player.PlayerSkills.Count)
                        Console.Write($" (사용 가능: {usable}개)");
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// 내러티브 텍스트 렌더링
        /// </summary>
        private void RenderNarrative(TrpgGameState state)
        {
            if (!string.IsNullOrWhiteSpace(state.NarrativeText))
            {
                Console.WriteLine();
                Console.WriteLine(state.NarrativeText);
            }
        }

        /// <summary>
        /// 선택지 렌더링
        /// </summary>
        private void RenderChoices(TrpgGameState state)
        {
            if (state.AvailableChoices.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("─────────────────────────────────────────────────────────");

                foreach (var choice in state.AvailableChoices)
                {
                    string prefix = choice.IsEnabled ? "" : "[비활성] ";
                    Console.WriteLine($"{prefix}{choice.DisplayText}");

                    if (!string.IsNullOrWhiteSpace(choice.Description))
                    {
                        Console.WriteLine($"  └─ {choice.Description}");
                    }
                }

                Console.WriteLine("─────────────────────────────────────────────────────────");
            }
        }

        /// <summary>
        /// 하단 도움말 렌더링
        /// </summary>
        private void RenderHelp()
        {
            Console.WriteLine();
            Console.WriteLine("TIP: /help 명령어로 사용 가능한 명령어를 확인할 수 있습니다.");
        }

        /// <summary>
        /// 메시지 출력 (상태와 무관한 일반 메시지)
        /// </summary>
        public void RenderMessage(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// 에러 메시지 출력
        /// </summary>
        public void RenderError(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {errorMessage}");
            Console.ResetColor();
        }

        /// <summary>
        /// 경고 메시지 출력
        /// </summary>
        public void RenderWarning(string warningMessage)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARNING] {warningMessage}");
            Console.ResetColor();
        }

        /// <summary>
        /// 성공 메시지 출력
        /// </summary>
        public void RenderSuccess(string successMessage)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SUCCESS] {successMessage}");
            Console.ResetColor();
        }
    }
}
