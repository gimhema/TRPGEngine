using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleForge
{
    class PlayerClass
    {
        public string ClassName { get; set; }

        public PlayerClass(string className)
        {
            ClassName = className;
        }
    }

    class TrpgPlayer : TrpgActor
    {
        // 클래스의 타입은 룰북에서 정의된것을 사용해야하기때문에 enum을 활용하지않는다.
        public PlayerClass playerClass { get; set; }
        public TrpgPlayer(string name, string description = "", string className = "") : base(name, description)
        {
            playerClass = new PlayerClass(className);
        }
    }
}
