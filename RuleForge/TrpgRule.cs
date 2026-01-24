using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    class RulebookParser
    {
        public List<Chapter> ParseRulebook(string filePath)
        {
            // 파일 읽기 및 파싱
            List<Chapter> chapters = new List<Chapter>();

            // Parse . . .

            return chapters;
        }
    }

    class TrpgRule
    {
        private Dictionary<string, TrpgGameAction> GameActions = new Dictionary<string, TrpgGameAction>();

        public void InitializeGameActionsByRulebook(string filePath)
        {
            
        }

        public TrpgRule()
        {
            
        }

        public void DoAction(string actionName)
        {
            if (GameActions.ContainsKey(actionName))
            {
                GameActions[actionName].ExecuteAction();
            }
            else
            {
                Console.WriteLine($"Action '{actionName}' not found.");
            }
        }

    }
}
