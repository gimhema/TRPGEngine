using System.IO;

namespace RuleForge
{
    public class GameSetting
    {
        public string ModelPath { get; private set; } = "";

        private static readonly string IniFilePath =
            Path.Combine(AppContext.BaseDirectory, "GameSetting", "GameSetting.ini");

        public void LoadINI()
        {
            if (!File.Exists(IniFilePath))
            {
                Console.WriteLine($"[GameSetting] 설정 파일 없음: {IniFilePath}");
                return;
            }

            string currentSection = "";

            foreach (var raw in File.ReadAllLines(IniFilePath))
            {
                var line = raw.Trim();

                // 빈 줄, 주석 skip
                if (string.IsNullOrEmpty(line) || line.StartsWith(';') || line.StartsWith('#'))
                    continue;

                // 섹션 헤더
                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    currentSection = line[1..^1].Trim();
                    continue;
                }

                // key=value
                var sep = line.IndexOf('=');
                if (sep < 0) continue;

                var key   = line[..sep].Trim();
                var value = line[(sep + 1)..].Trim();

                if (currentSection == "Model" && key == "path" && !string.IsNullOrWhiteSpace(value))
                    ModelPath = Path.Combine(AppContext.BaseDirectory, "GameSetting", "model", value);
            }
        }
    }
}
