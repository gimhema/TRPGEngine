using System;
using System.Collections.Generic;
using System.Text;

namespace RuleForge
{
    // ----------------------------------------------------------------
    // 나레이터 이벤트 타입
    // ----------------------------------------------------------------
    public enum NarrativeEventType
    {
        GameStart,      // 게임 시작 도입부
        BattleVictory,  // 전투 승리
        BattleDefeat,   // 전투 패배
        DungeonClear,   // 던전 클리어
        DungeonFailed,  // 던전 실패
        LocationEnter,  // 장소 진입
    }

    // ----------------------------------------------------------------
    // 나레이티브 컨텍스트 - 이벤트 발생 시 LLM에 전달할 상황 데이터
    // ----------------------------------------------------------------
    public class NarrativeContext
    {
        public NarrativeEventType EventType { get; set; }
        public string PlayerName { get; set; } = "";
        /// <summary>관련 대상 이름 (적, 던전, 장소 등)</summary>
        public string? TargetName { get; set; }
        /// <summary>추가 정보 (예: 획득 보상, 현재 챕터 제목 등)</summary>
        public string? Extra { get; set; }
    }

    // ----------------------------------------------------------------
    // 챕터 개요 (TR_0_Overview.md 파싱 결과)
    // ----------------------------------------------------------------
    public class ChapterOverview
    {
        public int ChapterNumber { get; set; }
        public bool IsFinal { get; set; }
        public string Story { get; set; } = "";
        public string Goal { get; set; } = "";
    }

    // ----------------------------------------------------------------
    // NarratorManager - 싱글톤
    // ----------------------------------------------------------------
    /// <summary>
    /// 게임 상황을 LLM 나레이터에 전달해 내러티브 묘사를 생성한다.
    /// LLM이 없으면 상황에 맞는 폴백 텍스트를 반환한다.
    /// </summary>
    public class NarratorManager : IDisposable
    {
        private static NarratorManager? _instance;
        public static NarratorManager Instance => _instance ??= new NarratorManager();

        private NpcChatSession? _session;
        private string _mainStory = "";
        private readonly List<ChapterOverview> _chapters = new();

        private NarratorManager() { }

        public void Dispose() => _session?.Dispose();

        // ----------------------------------------------------------------
        // 초기화
        // ----------------------------------------------------------------

        /// <summary>LLM 엔진 주입 및 나레이터 세션 생성</summary>
        public void SetEngine(LlamaEngine engine)
        {
            _session?.Dispose();
            _session = engine.CreateNpcSession(BuildNarratorSystemPrompt());
        }

        /// <summary>TR_0_Overview.md 파싱 결과 주입 (RulebookParser에서 호출)</summary>
        public void LoadStory(string mainStory, List<ChapterOverview> chapters)
        {
            _mainStory = mainStory;
            _chapters.Clear();
            _chapters.AddRange(chapters);
        }

        // ----------------------------------------------------------------
        // 나레이션 생성
        // ----------------------------------------------------------------

        /// <summary>
        /// 이벤트 컨텍스트를 바탕으로 나레이션 텍스트를 반환한다.
        /// LLM이 있으면 LLM 생성, 없으면 폴백 텍스트.
        /// </summary>
        public string Narrate(NarrativeContext ctx)
        {
            string prompt = BuildEventPrompt(ctx);

            if (_session != null)
            {
                try
                {
                    return ConsoleSpinner.Run("나레이터가 상황을 묘사하는 중...",
                        () => _session.ChatAsync(prompt).GetAwaiter().GetResult());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"LLM 오류: {ex.Message}");
                }
            }

            return GetFallbackNarration(ctx);
        }

        // ----------------------------------------------------------------
        // 내부 헬퍼
        // ----------------------------------------------------------------

        private string BuildNarratorSystemPrompt()
        {
            var sb = new StringBuilder();
            sb.AppendLine("당신은 TRPG 게임의 나레이터입니다. 반드시 한국어로만 답하세요.");
            sb.AppendLine("각 이벤트 상황을 2~3문장으로 간결하고 생생하게 묘사하세요. 과장하지 마세요.");

            if (!string.IsNullOrWhiteSpace(_mainStory))
            {
                sb.AppendLine();
                sb.AppendLine("[세계관]");
                sb.AppendLine(_mainStory);
            }
            else
            {
                sb.AppendLine("[세계관] 평범한 판타지 세계를 배경으로 한다.");
            }

            return sb.ToString();
        }

        private string BuildEventPrompt(NarrativeContext ctx)
        {
            return ctx.EventType switch
            {
                NarrativeEventType.GameStart =>
                    $"용사 {ctx.PlayerName}이(가) 모험을 시작한다. 도입부를 묘사해.",

                NarrativeEventType.BattleVictory =>
                    $"{ctx.PlayerName}이(가) {ctx.TargetName}을(를) 쓰러뜨렸다. 승리의 순간을 묘사해." +
                    (ctx.Extra != null ? $" ({ctx.Extra})" : ""),

                NarrativeEventType.BattleDefeat =>
                    $"{ctx.PlayerName}이(가) {ctx.TargetName}에게 패배했다. 쓰러지는 순간을 묘사해.",

                NarrativeEventType.DungeonClear =>
                    $"{ctx.PlayerName}이(가) {ctx.TargetName} 던전을 완전히 클리어했다. 클리어의 순간을 묘사해.",

                NarrativeEventType.DungeonFailed =>
                    $"{ctx.PlayerName}이(가) {ctx.TargetName} 던전에서 쓰러져 돌아왔다. 실패의 순간을 묘사해.",

                NarrativeEventType.LocationEnter =>
                    $"{ctx.PlayerName}이(가) {ctx.TargetName}에 도착했다. 장소의 첫인상을 묘사해.",

                _ => $"상황을 묘사해."
            };
        }

        private static string GetFallbackNarration(NarrativeContext ctx)
        {
            return ctx.EventType switch
            {
                NarrativeEventType.GameStart =>
                    $"{ctx.PlayerName}은(는) 새로운 모험을 향해 첫 발을 내딛었다.",

                NarrativeEventType.BattleVictory =>
                    $"{ctx.PlayerName}은(는) 숨을 고르며 쓰러진 {ctx.TargetName}을(를) 바라보았다.",

                NarrativeEventType.BattleDefeat =>
                    $"{ctx.PlayerName}은(는) 힘이 빠져 무릎을 꿇었다...",

                NarrativeEventType.DungeonClear =>
                    $"{ctx.PlayerName}은(는) 마지막 적을 쓰러뜨리고 던전의 정적 속에 섰다.",

                NarrativeEventType.DungeonFailed =>
                    $"{ctx.PlayerName}은(는) 간신히 던전을 빠져나왔다...",

                NarrativeEventType.LocationEnter =>
                    $"{ctx.PlayerName}은(는) {ctx.TargetName}에 발을 들였다.",

                _ => ""
            };
        }
    }
}
