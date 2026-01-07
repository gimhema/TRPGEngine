using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    class PlayerBasicInfo
    {
        public int Id { get; set; }
        public PlayerBasicInfo() { }

        public string Name { get; set; }
        public string Description { get; set; }
    }

    class TrpgPlayer
    {
        public PlayerBasicInfo PlayerBasicInfo { get; set; }
        public TrpgActorStatus PlayeStatus { get; set; }

        public TrpgPlayer()
        {
            PlayerBasicInfo = new PlayerBasicInfo();
        }

        public void Init(PlayerBasicInfo playerBasicInfo, TrpgActorStatus playerStatus)
        {
            PlayerBasicInfo = playerBasicInfo;
            PlayeStatus = playerStatus;
        }
    }
}
