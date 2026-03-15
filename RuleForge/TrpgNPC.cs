using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public List<TrpgItem> TradeItems { get; set; } = new List<TrpgItem>();

        /// <summary>퀘스트 NPC가 제공하는 퀘스트 목록 (룰북에서 주입)</summary>
        public List<Quest> Quests { get; set; } = new List<Quest>();

        public void InterAction()
        {

        }

        public void Trade()
        {

        }

        public void Communicate()
        {

        }
    }

}