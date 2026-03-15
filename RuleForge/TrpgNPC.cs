using System;
using System.Collections.Generic;
using System.Text;

namespace RuleForge
{
    public class TrpgNPC : TrpgActor
    {
        /// <summary>NPC 타입: Normal, Trader, Quest 등</summary>
        public string NpcType { get; set; } = "Normal";

        /// <summary>NPC 배경 스토리 (LLM 컨텍스트 주입용)</summary>
        public string Background { get; set; } = "";

        /// <summary>NPC 성격 특성 키워드 목록 (LLM 프롬프트 주입용)</summary>
        public List<string> Traits { get; set; } = new();

        /// <summary>상인 NPC의 판매 아이템 목록 (룰북에서 주입)</summary>
        public List<TrpgItem> TradeItems { get; set; } = new();

        /// <summary>퀘스트 NPC가 제공하는 퀘스트 목록 (룰북에서 주입)</summary>
        public List<Quest> Quests { get; set; } = new();

        /// <summary>대화 히스토리 (화자, 발언) 순서대로 기록</summary>
        public List<(string Speaker, string Message)> DialogueHistory { get; set; } = new();

        /// <summary>LLM 채팅 세션 (NpcDialogueManager에서 초기화)</summary>
        public NpcChatSession? ChatSession { get; set; }

        /// <summary>NPC 컨텍스트를 LLM 시스템 프롬프트로 빌드</summary>
        public string BuildSystemPrompt(TrpgPlayer? player)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"당신은 TRPG 게임의 NPC '{Name}'입니다. 반드시 한국어로만 대답하세요.");
            if (!string.IsNullOrEmpty(Background))
                sb.AppendLine($"배경: {Background}");
            if (Traits.Count > 0)
                sb.AppendLine($"성격: {string.Join(", ", Traits)}");
            if (player != null)
                sb.AppendLine($"대화 상대: {player.Name} ({player.playerProfile.job})");
            sb.AppendLine("캐릭터의 성격과 배경에 맞게 자연스럽게 대화하세요. 응답은 2~3문장 이내로.");
            return sb.ToString();
        }
    }
}