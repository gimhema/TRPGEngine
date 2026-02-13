using System;
using System.Collections.Generic;

namespace RuleForge
{
    /// <summary>
    /// 게임의 현재 상태를 나타내는 클래스
    /// 모든 게임 상태 정보를 중앙에서 관리
    /// </summary>
    public class TrpgGameState
    {
        /// <summary>
        /// 게임 씬(화면) 타입
        /// </summary>
        public enum SceneType
        {
            MainMenu,       // 메인 메뉴
            GameModeSelect, // 게임 모드 선택
            PlayerSetup,    // 플레이어 설정
            Exploration,    // 탐험
            Combat,         // 전투
            Social,         // 사회 활동 (NPC 대화, 상점 등)
            Shop,           // 상점
            Inventory,      // 인벤토리
            Settings,       // 설정
            GameOver,       // 게임 오버
            GameClear       // 게임 클리어
        }

        /// <summary>
        /// 현재 씬
        /// </summary>
        public SceneType CurrentScene { get; set; }

        /// <summary>
        /// 씬 히스토리 스택 (다단계 되돌아가기 지원)
        /// </summary>
        public Stack<SceneType> SceneHistory { get; set; }

        /// <summary>
        /// 현재 사용 가능한 선택지 목록
        /// </summary>
        public List<TrpgChoice> AvailableChoices { get; set; }

        /// <summary>
        /// 현재 내러티브 텍스트 (게임 상황 설명)
        /// </summary>
        public string NarrativeText { get; set; }

        /// <summary>
        /// 현재 플레이어
        /// </summary>
        public TrpgPlayer? CurrentPlayer { get; set; }

        /// <summary>
        /// 현재 챕터
        /// </summary>
        public Chapter? CurrentChapter { get; set; }

        /// <summary>
        /// 현재 퀘스트
        /// </summary>
        public Quest? CurrentQuest { get; set; }

        /// <summary>
        /// 현재 플레이어가 위치한 월드 유닛 (마을, 필드, 던전 등)
        /// </summary>
        public WorldUnit? CurrentLocation { get; set; }

        /// <summary>
        /// 게임 종료 플래그
        /// </summary>
        public bool ShouldExit { get; set; }

        /// <summary>
        /// 게임 클리어 여부
        /// </summary>
        public bool IsGameCleared { get; set; }

        /// <summary>
        /// 추가 상태 정보를 저장하는 딕셔너리 (유연성 제공)
        /// </summary>
        public Dictionary<string, object> CustomData { get; set; }

        public TrpgGameState()
        {
            CurrentScene = SceneType.MainMenu;
            SceneHistory = new Stack<SceneType>();
            AvailableChoices = new List<TrpgChoice>();
            NarrativeText = "";
            CurrentPlayer = null;
            CurrentChapter = null;
            CurrentQuest = null;
            CurrentLocation = null;
            ShouldExit = false;
            IsGameCleared = false;
            CustomData = new Dictionary<string, object>();
        }

        /// <summary>
        /// 씬 전환 (현재 씬을 히스토리에 push)
        /// </summary>
        public void ChangeScene(SceneType newScene)
        {
            SceneHistory.Push(CurrentScene);
            CurrentScene = newScene;
        }

        /// <summary>
        /// 이전 씬으로 되돌아가기 (히스토리에서 pop)
        /// </summary>
        public void ReturnToPreviousScene()
        {
            if (SceneHistory.Count > 0)
            {
                CurrentScene = SceneHistory.Pop();
            }
        }

        /// <summary>
        /// 이전 씬이 존재하는지 확인
        /// </summary>
        public bool HasPreviousScene => SceneHistory.Count > 0;

        /// <summary>
        /// 선택지 초기화 및 설정
        /// </summary>
        public void SetChoices(List<TrpgChoice> choices)
        {
            AvailableChoices = choices;
        }

        /// <summary>
        /// 선택지 추가
        /// </summary>
        public void AddChoice(TrpgChoice choice)
        {
            AvailableChoices.Add(choice);
        }

        /// <summary>
        /// 선택지 초기화
        /// </summary>
        public void ClearChoices()
        {
            AvailableChoices.Clear();
        }

        /// <summary>
        /// ID로 선택지 찾기
        /// </summary>
        public TrpgChoice? FindChoice(string id)
        {
            return AvailableChoices.Find(c => c.Id == id);
        }

        /// <summary>
        /// 커스텀 데이터 설정
        /// </summary>
        public void SetCustomData(string key, object value)
        {
            CustomData[key] = value;
        }

        /// <summary>
        /// 커스텀 데이터 가져오기
        /// </summary>
        public T? GetCustomData<T>(string key)
        {
            if (CustomData.ContainsKey(key))
            {
                return (T)CustomData[key];
            }
            return default;
        }
    }

    /// <summary>
    /// 선택지 클래스
    /// 플레이어가 선택할 수 있는 옵션 하나를 나타냄
    /// </summary>
    public class TrpgChoice
    {
        /// <summary>
        /// 선택지 식별자 (예: "1", "2", "attack", "flee")
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 화면에 표시될 텍스트 (예: "1. 전투")
        /// </summary>
        public string DisplayText { get; set; }

        /// <summary>
        /// 선택지 설명 (선택적)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 이 선택지를 선택할 수 있는지 여부
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 선택지를 선택했을 때 실행될 액션
        /// </summary>
        public Action<TrpgGameState>? OnSelect { get; set; }

        public TrpgChoice(string id, string displayText, Action<TrpgGameState>? onSelect = null)
        {
            Id = id;
            DisplayText = displayText;
            Description = null;
            IsEnabled = true;
            OnSelect = onSelect;
        }

        public TrpgChoice(string id, string displayText, string description, Action<TrpgGameState>? onSelect = null)
        {
            Id = id;
            DisplayText = displayText;
            Description = description;
            IsEnabled = true;
            OnSelect = onSelect;
        }

        /// <summary>
        /// 선택지 실행
        /// </summary>
        public void Execute(TrpgGameState state)
        {
            if (IsEnabled && OnSelect != null)
            {
                OnSelect(state);
            }
        }
    }
}
