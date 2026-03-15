using System;
using System.Collections.Generic;
using System.Text;

namespace RuleForge
{
    /// <summary>
    /// NPC 대화 시스템 싱글톤.
    /// LLM(LlamaEngine)이 없을 때는 폴백 응답을 제공한다.
    /// </summary>
    public class NpcDialogueManager
    {
        private static NpcDialogueManager? _instance;
        public static NpcDialogueManager Instance => _instance ??= new NpcDialogueManager();

        private LlamaEngine? _engine;

        private NpcDialogueManager() { }

        /// <summary>LLM 엔진 주입 (Program.GameStartPreprocess에서 호출)</summary>
        public void SetEngine(LlamaEngine engine) => _engine = engine;

        public bool IsLLMAvailable => _engine != null;

        // ----------------------------------------------------------------
        // 대화 시작
        // ----------------------------------------------------------------

        /// <summary>
        /// NPC 대화 화면 진입.
        /// 첫 진입 시 LLM 세션을 초기화하고 NPC 인사를 생성한다.
        /// returnCallback: 대화 종료 후 되돌아갈 화면 액션
        /// </summary>
        public void StartDialogue(TrpgNPC npc, TrpgGameState state,
            Action<TrpgGameState> returnCallback)
        {
            // LLM 세션 초기화 (NPC당 1회)
            if (_engine != null && npc.ChatSession == null)
            {
                var systemPrompt = npc.BuildSystemPrompt(state.CurrentPlayer);
                npc.ChatSession = _engine.CreateNpcSession(systemPrompt);
            }

            // 첫 대화 시 NPC 인사 생성
            if (npc.DialogueHistory.Count == 0)
            {
                string greeting = GetResponse(npc, state.CurrentPlayer, "새로운 여행자가 인사를 건넨다. 짧게 인사로 응답해.");
                npc.DialogueHistory.Add((npc.Name, greeting));
            }

            // CustomData에 대화 상태 등록
            state.SetCustomData("npc_dialogue_active", true);
            state.SetCustomData("npc_dialogue_npc", npc);
            state.SetCustomData("npc_dialogue_return", returnCallback);

            RenderDialogueScreen(npc, state, returnCallback);
        }

        // ----------------------------------------------------------------
        // 플레이어 입력 처리 (TrpgGameLogic.ProcessInput에서 호출)
        // ----------------------------------------------------------------

        /// <summary>플레이어 자유 입력을 NPC에게 전달하고 응답을 화면에 반영한다.</summary>
        public void ProcessDialogueInput(string playerMessage, TrpgGameState state)
        {
            var npc = state.GetCustomData<TrpgNPC>("npc_dialogue_npc");
            var returnCallback = state.GetCustomData<Action<TrpgGameState>>("npc_dialogue_return");
            if (npc == null || returnCallback == null) return;

            npc.DialogueHistory.Add(("플레이어", playerMessage));

            string response = GetResponse(npc, state.CurrentPlayer, playerMessage);
            npc.DialogueHistory.Add((npc.Name, response));

            RenderDialogueScreen(npc, state, returnCallback);
        }

        // ----------------------------------------------------------------
        // 내부 헬퍼
        // ----------------------------------------------------------------

        private void RenderDialogueScreen(TrpgNPC npc, TrpgGameState state,
            Action<TrpgGameState> returnCallback)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"===== {npc.Name}와의 대화 =====");
            if (npc.Traits.Count > 0)
                sb.AppendLine($"[{string.Join(", ", npc.Traits)}]");
            sb.AppendLine();

            // 최근 대화 히스토리 (최대 6줄)
            int start = Math.Max(0, npc.DialogueHistory.Count - 6);
            for (int i = start; i < npc.DialogueHistory.Count; i++)
            {
                var (speaker, message) = npc.DialogueHistory[i];
                sb.AppendLine($"{speaker}: \"{message}\"");
            }

            sb.AppendLine();
            sb.Append("[하고 싶은 말을 자유롭게 입력하세요 / '0'을 입력하면 대화 종료]");

            state.NarrativeText = sb.ToString();
            state.ClearChoices();

            // 대화 종료 선택지 (번호 입력 대신 '0' 입력으로도 가능)
            state.AddChoice(new TrpgChoice("0", "0. 대화 종료")
            {
                OnSelect = (s) =>
                {
                    s.CustomData.Remove("npc_dialogue_active");
                    s.CustomData.Remove("npc_dialogue_npc");
                    s.CustomData.Remove("npc_dialogue_return");
                    returnCallback(s);
                }
            });
        }

        private string GetResponse(TrpgNPC npc, TrpgPlayer? player, string playerMessage)
        {
            if (_engine != null && npc.ChatSession != null)
            {
                try
                {
                    return npc.ChatSession.ChatAsync(playerMessage).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NpcDialogueManager] LLM 오류: {ex.Message}");
                }
            }
            return GetFallbackResponse(npc, playerMessage);
        }

        private static string GetFallbackResponse(TrpgNPC npc, string playerMessage)
        {
            // 인사 메시지 처리
            if (playerMessage.Contains("인사"))
                return $"어서오시게, 여행자여.";

            // 성격 기반 간단한 폴백
            if (npc.Traits.Count > 0)
            {
                return npc.Traits[0] switch
                {
                    "친절함" => "무엇이든 도와드릴게요, 여행자님.",
                    "지혜로움" => "음... 생각해볼 만한 말이군.",
                    "사무적임" => "용건이 있으시면 말씀하세요.",
                    "욕심많음" => "흠, 그런 이야기군요. 뭔가 도움이 될 물건이라도 있나요?",
                    _ => "그렇군요, 여행자여."
                };
            }

            return "...";
        }
    }
}
