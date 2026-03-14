# CLAUDE.md

이 파일은 Claude Code가, 이 저장소에서 작업을 할 때 참고하는 가이드 문서입니다.

## 프로젝트 개요

RuleForge는 .NET 9.0과 C#으로 구축된 TRPG (테이블탑 롤플레잉 게임) 엔진입니다. 싱글플레이어와 멀티플레이어 모드를 모두 지원하며, LLamaSharp를 통한 LLM 통합으로 향상된 내러티브 생성 기능을 제공합니다.

## 빌드 및 실행 명령어

### 기본 명령어
```bash
# 프로젝트 빌드
dotnet build

# 애플리케이션 실행
dotnet run

# 인자와 함께 실행 (예: 멀티플레이어 호스트용 커스텀 포트)
dotnet run -- 8080

# 빌드 결과물 정리
dotnet clean
```

### 의존성
프로젝트는 LLM 통합을 위해 LLamaSharp를 사용합니다. 패키지 추가가 필요한 경우:
```bash
dotnet add package LLamaSharp
dotnet add package LLamaSharp.Backend.Cpu
```

## 아키텍처 개요

### 게임 루프 패턴
프로젝트는 명확한 관심사 분리를 가진 모듈형 아키텍처를 사용합니다:

1. **TrpgGameController** ([TrpgGameController.cs:11](TrpgGameController.cs#L11)) - 메인 게임 루프 조정자 (싱글톤)
   - Input → Logic → State → Render 사이클 조율
   - 약 20Hz로 비동기 게임 루프 실행 (프레임당 50ms 딜레이)
   - 씬 전환 및 특수 명령어 관리

2. **TrpgGameState** ([TrpgGameState.cs:10](TrpgGameState.cs#L10)) - 중앙 상태 컨테이너
   - 씬, 선택지, 내러티브 텍스트, 플레이어, 챕터, 퀘스트를 포함한 모든 게임 상태 보유
   - `CurrentLocation`: 현재 플레이어가 위치한 WorldUnit 추적
   - `CurrentBattle`: 현재 진행 중인 전투 상태 (TrpgBattle 인스턴스, 전투 중이 아니면 null)
   - `SceneHistory`: Stack 기반 다단계 씬 히스토리 (push/pop으로 다단계 되돌아가기 지원)
   - 확장 가능한 상태 저장을 위한 CustomData 딕셔너리 사용

3. **TrpgGameLogic** ([TrpgGameLogic.cs:122](TrpgGameLogic.cs#L122)) - 비즈니스 로직 (싱글톤)
   - 챕터, 퀘스트, 활동 관리
   - 플레이어 생성 및 관리 처리 (초기 스탯은 `TrpgGameConfig.PlayerDefault.InitialStats`에서 로드)
   - WorldManager를 내장하여 월드 로드/조회/위치 진입 관리
   - `StartBattle()`: 전투 개시 (TrpgBattle 생성, Combat 씬 전환, 콜백 기반 종료 처리)
   - 현재 씬 타입에 따른 입력 처리

4. **TrpgRenderer** - 콘솔 출력 렌더링
   - 게임 상태를 콘솔에 렌더링
   - 내러티브 텍스트, 선택지, 상태 표시

5. **TrpgInputHandler** - 사용자 입력 처리
   - 입력 유효성 검사 및 정제
   - 특수 명령어 처리 ("exit", "help" 등)
   - 입력을 선택지 또는 게임 액션에 매핑

### 멀티플레이어 아키텍처

멀티플레이어 시스템은 길이 접두사(length-prefixed) 메시징을 사용하는 커스텀 TCP 서버를 사용합니다:

1. **TcpServer** ([TcpServer.cs:10](TcpServer.cs#L10)) - 고성능 TCP 서버
   - 기본적으로 최대 10,000개의 동시 연결 지원
   - 확장성을 위한 async/await 패턴 사용
   - 구성 가능한 버퍼 크기, 백로그, 소켓 옵션

2. **Connection** ([Connection.cs:9](Connection.cs#L9)) - 클라이언트별 연결 핸들러
   - 길이 접두사 프로토콜 구현: `[u32 빅엔디안 길이][페이로드]`
   - 효율적인 메모리 관리를 위한 `ArrayPool<byte>` 사용
   - `SemaphoreSlim`을 사용한 스레드 안전 전송

3. **Protocol** ([Protocol.cs:5](Protocol.cs#L5)) - 바이너리 프로토콜 유틸리티
   - 메시지 프레이밍을 위한 빅엔디안 u32 읽기/쓰기
   - 기본 최대 메시지 크기: 1MB

4. **현재 제약사항**: TCP 서버와 GameController 통합이 불완전함 ([Program.cs:75](Program.cs#L75) 참조)
   - 서버가 게임 루프와 병렬로 실행되지만 상태를 동기화하지 않음
   - 멀티플레이어 클라이언트 모드가 아직 구현되지 않음

### LLM 통합

**LlamaEngine** ([LlamaInterface.cs:66](LlamaInterface.cs#L66))은 로컬 LLM 추론을 위해 LLamaSharp를 래핑합니다:
- 디스크에서 GGUF 모델 파일 로드
- 컨텍스트 크기: 4096 토큰
- GPU 레이어 수: 0 (기본적으로 CPU 전용)
- 한국어 응답으로 구성됨
- 비동기 열거를 통한 스트리밍 토큰 생성 사용

**ModelDescription** ([LlamaInterface.cs:12](LlamaInterface.cs#L12))은 모델 생명주기를 관리합니다:
- 모델 로딩, 컨텍스트 생성, 실행자 설정 처리
- 시스템 프롬프트와 함께 채팅 히스토리 유지
- 역순으로 리소스를 올바르게 해제

### 게임 콘텐츠 구조

1. **Chapter** ([TrpgGameLogic.cs:11](TrpgGameLogic.cs#L11)) - 스토리 컨테이너
   - 여러 퀘스트(메인 및 서브) 포함
   - 현재 퀘스트 인덱스 추적
   - 메인 퀘스트 상태를 기반으로 챕터 완료 여부 결정

2. **Quest** ([TrpgGameLogic.cs:59](TrpgGameLogic.cs#L59)) - 퀘스트 시스템
   - 타입: Main (필수) 또는 Sub (선택)
   - 완료 상태 추적
   - 내러티브 설명 제공

3. **Activity** ([TrpgGameLogic.cs:88](TrpgGameLogic.cs#L88)) - 게임 액션 카테고리
   - 타입: Combat (전투), Exploration (탐험), Social (사회 활동)
   - 상황에 맞는 내러티브 생성

### 플레이어 및 아이템

1. **TrpgPlayer** ([TrpgPlayer.cs:136](TrpgPlayer.cs#L136)) - TrpgActor를 확장
   - PlayerProfile: 나이, 성별, 성격, 직업, 배경 스토리, 레벨
   - PlayerClass: 캐릭터 클래스 시스템
   - PlayerItemBag: 소비템, 장비, 키 아이템 인벤토리
   - PlayerEquipments: 장착된 아이템 관리
   - Gold: 플레이어 소지금
   - PlayerSkills: 습득한 스킬 목록

2. **TrpgActor** - 캐릭터(플레이어, NPC, 적)의 기본 클래스
   - CommonAttributes: 동적 상태 속성 (HP, MP, ATK, DEF, SPD)

3. **아이템 시스템** ([TrpgItem.cs](TrpgItem.cs)) - 세 가지 아이템 타입
   - Equipment (장비): 장착/해제 가능
   - Consumable (소비템): 일회용 또는 수량 기반
   - KeyItem (키 아이템): 퀘스트 관련 아이템
   - 모든 아이템에 `Price` 속성 (구매가) 지원

### 씬 시스템

게임은 여러 씬을 통해 흐릅니다 ([TrpgGameState.cs:15](TrpgGameState.cs#L15)):
- MainMenu → GameModeSelect → PlayerSetup → Exploration
- Exploration에서 Combat, Social, Shop, Inventory로 전환 가능
- GameOver 또는 GameClear가 종료 상태
- 대부분의 씬에서 Settings 접근 가능

## 게임 모드

애플리케이션은 게임 모드 선택 프롬프트로 시작합니다 ([Program.cs:11](Program.cs#L11)):

1. **싱글 플레이어** - 네트워킹 없는 로컬 게임
2. **멀티 플레이어 호스트** - 포트 7777에서 TCP 서버 실행 (인자로 구성 가능)
3. **멀티 플레이어 클라이언트** - 아직 구현되지 않음

## 코드 컨벤션

1. 코드베이스 전반에 걸쳐 한국어 주석 사용
2. 주요 게임 시스템에 싱글톤 패턴 사용 (Instance 속성)
3. 게임 루프 및 네트워킹에 Async/await 사용
4. 우아한 종료를 위한 CancellationToken 지원
5. 리소스 관리를 위한 IDisposable 패턴 (모델, 소켓)

## 구현된 시스템

### 아이템 및 인벤토리 시스템 ✅
**위치**: [TrpgPlayer.cs](TrpgPlayer.cs), [TrpgItem.cs](TrpgItem.cs), [TrpgInterface.cs](TrpgInterface.cs)

플레이어 인벤토리 시스템이 완전히 데이터 기반으로 동작합니다:
- **PlayerItemBag**: 소비템(Consumable), 장비(Equipment), 키 아이템(KeyItem) 관리
- **PlayerEquipments**: 장착된 아이템 관리
- **UseItem**: 플레이어의 실제 소비 아이템 목록을 표시하고 사용
- **SellItem**: 플레이어의 실제 장비 목록을 표시하고 판매
- **DropItem**: 플레이어의 모든 아이템을 카테고리별로 표시하고 제거

모든 아이템 관련 로직은 하드코딩 없이 실제 플레이어 데이터를 기반으로 동작합니다.

### 전투 시스템 ✅
**위치**: [TrpgBattle.cs](TrpgBattle.cs), [TrpgEnemy.cs](TrpgEnemy.cs), [TrpgGameLogic.cs](TrpgGameLogic.cs)

턴제 전투 시스템이 TrpgGameState 기반으로 완전히 구현되었습니다:

**핵심 클래스**:
- **TrpgBattle** ([TrpgBattle.cs](TrpgBattle.cs)) - 전투 상태 컨테이너 및 전투 루프
  - `BattlePhase`: PlayerTurn, EnemyTurn, Victory, Defeat 4단계
  - `BattleLog`: 전투 로그 기록 (최근 5개 표시)
  - `OnBattleEnd`: 전투 종료 콜백 (승리/패배 후처리용)

**플레이어 액션** (4가지):
- **공격**: ATK vs DEF 기반 데미지 계산
- **방어**: 다음 적 공격 데미지 50% 감소
- **아이템 사용**: 소비 아이템 목록에서 선택하여 사용 (전투 중)
- **도망치기**: SPD 차이 기반 성공 확률 (기본 50%, ±5%/SPD 차이, 10~90% 범위)

**데미지 계산**:
- 공식: `ATK - DEF/2`, ±20% 랜덤 편차 (0.8~1.2배), 최소 데미지 1
- `TrpgBattle.CalculateDamage(attackPower, defensePower)` 정적 메서드

**적 시스템**:
- **TrpgEnemy.InitCombatStats(hp, atk, def, spd)**: 전투 스탯 일괄 초기화
- **적 AI**: 기본 공격 수행 (EnemyTurn에서 처리)
- **보상 시스템**: 적 처치 시 BattleReward에서 랜덤 보상 지급 (BattleLog에 기록)

**전투 흐름**:
1. `TrpgGameLogic.StartBattle(enemy, state, onBattleEnd)` 호출
2. Combat 씬 전환 → TrpgBattle.StartBattle() → 플레이어 턴 선택지 표시
3. 플레이어 액션 → 적 HP 확인 → 적 턴 → 플레이어 HP 확인 → 다음 턴
4. 승리/패배 → "계속" 선택 → OnBattleEnd 콜백 실행
5. 연속 전투 지원: 이미 Combat 씬이면 씬 전환 없이 새 전투 시작

**렌더링**:
- `TrpgRenderer.RenderCombat()`: 플레이어 상태 + 적 상태(HP/ATK/DEF) + 전투 로그 + 선택지
- `RenderEnemyStatus()`: 적 이름, HP, ATK, DEF를 빨간색 강조 표시

### 월드/던전 시스템 ✅
**위치**: [TrpgWorld.cs](TrpgWorld.cs), [TrpgEnemy.cs](TrpgEnemy.cs)

월드 시스템과 던전-전투 통합이 구현되었습니다:

**월드 구조**:
- **WorldManager**: TrpgGameLogic에 내장, 월드 등록/조회 담당
- **World**: 월드 전체를 관리하는 컨테이너
- **WorldUnit**: 모든 월드 요소의 추상 기본 클래스
  - `Action(TrpgGameState)`: CurrentLocation 갱신 후 OnAction() 호출 (템플릿 메서드 패턴)
  - `OnAction(TrpgGameState)`: 하위 클래스에서 구현할 실제 진입 동작
  - `ConnectedLocations`: 모든 WorldUnit의 연결 관계 관리 (통일된 단일 리스트)
  - **Village**: 마을 (Establishment 포함)
  - **Establishment**: 상호작용 가능한 건물 (상점, 길드, 여관 등)
  - **Field**: 지역 간 연결 영역
  - **Dungeon**: 적 인카운터 및 보상이 있는 던전

**Village 시스템** ✅:
- **Village.Action()**: 마을 내 시설(Establishment) 목록을 TrpgChoice 선택지로 표시, "마을을 떠나기"로 이전 씬 복귀
- **Establishment.Action()**: 시설 내 NPC 목록을 선택지로 표시, "돌아가기"로 마을 시설 목록 복귀
- 마을 → 시설 → NPC 순의 계층적 탐색과 각 단계에서의 돌아가기 지원

**Field 시스템** ✅:
- **Field.Action()**: 채집하기, 주변 탐색, 돌아가기 선택지 표시
- **Field.Explore()**: ConnectedLocations에 연결된 WorldUnit 목록을 선택지로 표시, 선택 시 해당 유닛의 Action() 진입, "돌아가기"로 필드 메뉴 복귀
- **Field.Gathering()**: TODO (플레이어 액션 시스템 구현 후 연결 예정)

**적 및 인카운터 시스템**:
- **TrpgEnemy** ([TrpgEnemy.cs:10](TrpgEnemy.cs#L10))
  - `InitCombatStats(hp, atk, def, spd)`: 전투 스탯 일괄 초기화
  - `BattleReward`: 전투 보상 아이템 리스트 (생성자에서 초기화)
  - `GiveReward()`: 랜덤 개수의 보상을 랜덤 선택하여 지급
  - `Death()`: 적 처치 시 보상 지급 처리

- **TrpgEnemyGroup** ([TrpgEnemy.cs:87](TrpgEnemy.cs#L87))
  - `Queue<TrpgEnemy>` 기반 순차 인카운터 시스템
  - `Encount()`: 다음 적을 Dequeue하여 반환
  - 중복 없는 순차적 적 등장 보장

**던전 시스템** ✅ (전투 통합 완료):
- **Dungeon.OnAction()**: 던전 진입 화면 (적 수 표시, 클리어 여부 확인)
- **Dungeon.MakeDungeon()**: EnemyGroupInstance 초기화 (기존 큐 클리어 후 재등록)
- **Dungeon.StartDungeonExploration()**: 첫 적과 전투 개시, 콜백 기반 연쇄 전투
- **Dungeon.ContinueDungeonExploration()**: 전투 종료 콜백 - 승리 시 다음 적 또는 클리어, 패배 시 실패
- **Dungeon.Clear(TrpgGameState)**: 던전 클리어 + 보상 지급 (GameState 기반, 내러티브로 표시)
- **Dungeon.Failed(TrpgGameState)**: 던전 실패 + HP 1 회복 + 필드 복귀 (GameState 기반)

**미완성 부분**:
- Chapter/Quest와의 연동 미구현
- NPC 대화 시스템 미구현 (Establishment에서 NPC 선택까지만 가능)
- Field.Gathering() 구현 (플레이어 액션 시스템 필요)

### 스킬 시스템 ✅
**위치**: [TrpgSkill.cs](TrpgSkill.cs), [TrpgBattle.cs](TrpgBattle.cs), [TrpgPlayer.cs](TrpgPlayer.cs)

턴제 전투에 통합된 스킬 시스템이 구현되었습니다:

**핵심 클래스**:
- **TrpgSkill** ([TrpgSkill.cs:61](TrpgSkill.cs#L61)) - 스킬 정의 (이름, 설명, MP 소모, 타겟 타입, 효과 목록, 필요 레벨)
- **SkillEffect** ([TrpgSkill.cs:30](TrpgSkill.cs#L30)) - 스킬 효과 (Damage, Heal, MpRestore, Buff, Debuff)
- **TrpgSkillData** ([TrpgSkill.cs:171](TrpgSkill.cs#L171)) - 스킬 데이터 저장소 (룰북 파싱 대응)
  - `RegisterStarterSkill()` / `RegisterAdvancedSkill()`: 룰북에서 파싱된 스킬 등록
  - `GetStarterSkills()` / `GetAdvancedSkills()`: 등록된 스킬 조회

**전투 통합**:
- 전투 메뉴에서 "스킬" 선택 → 스킬 목록 표시 (MP 확인) → 사용 → 효과 적용 → 적 턴
- `TrpgBattle.ShowSkillMenu()`, `TrpgBattle.UseSkill()` 구현 완료

**플레이어 통합**:
- `TrpgPlayer.PlayerSkills`: 습득한 스킬 목록
- `LearnSkill()`, `ForgetSkill()`, `GetUsableSkills()` 구현 완료
- 플레이어 생성 시 `TrpgSkillData.GetStarterSkills()`에서 기본 스킬 자동 습득

**향후 확장 가능 사항**:
- 직업별/레벨별 스킬 습득 시스템
- 스킬 업그레이드/강화 시스템
- Buff/Debuff 지속 턴 처리

### 상점 시스템 ✅
**위치**: [TrpgShop.cs](TrpgShop.cs), [TrpgItem.cs](TrpgItem.cs), [TrpgPlayer.cs](TrpgPlayer.cs)

TrpgGameState/TrpgChoice 기반 상점 시스템이 구현되었습니다:

**핵심 클래스**:
- **TrpgShop** ([TrpgShop.cs](TrpgShop.cs)) - 상점 로직 (구매/판매 UI)
  - `Merchandise`: 상점 판매 아이템 목록
  - `SellRatio`: 판매 시 가격 비율 (기본 0.5 → 구매가의 50%)
  - `Enter(state)`: 상점 진입 (Shop 씬 전환)
  - `ShowBuyMenu()` → `BuyItem()`: 상품 목록에서 구매 (골드 차감, 아이템 복제 지급)
  - `ShowSellMenu()` → `SellConsumable()` / `SellEquipment()`: 보유 아이템 판매 (골드 획득)

**연관 변경사항**:
- `TrpgItem.Price`: 모든 아이템에 가격 속성 추가
- `TrpgPlayer.Gold`: 플레이어 소지금 속성 추가
- `TrpgRenderer.RenderPlayerStatus()`: 골드 표시 (노란색)

**사용법**:
- Village의 Establishment에서 `shop.Enter(state)` 호출로 상점 진입
- Shop 씬 렌더링은 기존 `TrpgRenderer.RenderShop()` 활용

**향후 확장 가능 사항**:
- 상점 인벤토리 재고 제한 / 리필 시스템
- 할인, 흥정 시스템
- 전투 보상에 골드 추가

### 게임 설정 시스템 (데이터 기반 전환) ✅
**위치**: [TrpgGameConfig.cs](TrpgGameConfig.cs)

하드코딩된 게임 밸런스 값을 중앙 관리하는 설정 시스템이 구현되었습니다:

- **BattleConfig**: 데미지 공식 파라미터, 도망 확률, 방어 효과, 전투 로그 표시 수
- **PlayerDefaultConfig**: 플레이어 초기 스탯(`InitialStats` Dictionary), 기본 프로필 값
- `TrpgGameConfig.SetBattleConfig()` / `SetPlayerDefaultConfig()`: 룰북 파싱 결과로 설정 교체
- 모든 전투 계산과 플레이어 생성이 `TrpgGameConfig`를 참조

**TODO**: RulebookParser 구현 시 `TrpgGameConfig`에 파싱 결과를 주입

## 미구현 시스템 (우선순위별)

### 핵심 시스템

#### 1. 룰북 리소스 파서 🔴
**현재 상태**: 룰북 파일 포맷이 **혼합 방식(Markdown + JSON)** 으로 확정되었으며, [RuleBook/](RuleBook/) 디렉토리에 스키마 정의 완료

**룰북 리소스 파일 구조** ([RuleBook/](RuleBook/)):
- `TR_0_Overview.md` - **Markdown 형식** - 메인 스토리 및 챕터 구조 (LLM 입력용, 목표 타입: 몬스터처치/아이템획득/마스터자율)
- `TR_1_0_World.json` - **JSON 형식** - 월드 계층 구조 (World → Field → Village/Dungeon)
- `TR_1_1_Village.json` - **JSON 형식** - 마을 목록 (VillageName, NPCs)
- `TR_1_2_Dungeon.json` - **JSON 형식** - 던전 목록 (DungeonName, EncounterEnemys)
- `TR_2_Skill.json` - **JSON 형식** - 스킬 정의 (SkillName, SkillTargetType, SkillEffectType, Stat)
- `TR_3_Item.json` - **JSON 형식** - 아이템 정의 (Id, Name, Type, Stat/ItemEffect)
- `TR_4_NPC.json` - **JSON 형식** - NPC 정의 (Name, Type, Personality, TradeItems/QuestIds)
- `TR_5_Enemy.json` - **JSON 형식** - 적 정의 (EnemyName, Stat, RewardItemIds)
- `TR_6_Quest.json` - **JSON 형식** (**신규**) - 퀘스트 정의 (QuestId, QuestTitle, QuestDescription, QuestRewardItemIds)

**현재 상태**: 모든 JSON 파일의 스키마 구조는 정의됨. 실제 게임 데이터는 비어있음. `RulebookParser`는 골격만 존재하며 JSON 파싱 로직 미구현. `RuleForge.csproj`에 RuleBook 파일 빌드 포함 미설정.

**필요 작업**:
- `RulebookParser`를 JSON 파서로 구현 ([TrpgRule.cs](TrpgRule.cs) 재작성 필요, `System.Text.Json` 활용)
- 파싱 결과를 각 시스템에 주입:
  - `TR_1_0/1/2_World*.json` → `WorldManager.LoadWorldInfoByRuleBook()`
  - `TR_2_Skill.json` → `TrpgSkillData` (RegisterStarterSkill/RegisterAdvancedSkill)
  - `TR_3_Item.json` → 아이템 인스턴스 생성, 상점 Merchandise 등록
  - `TR_4_NPC.json` → TrpgNPC 인스턴스 생성, Establishment 배치
  - `TR_5_Enemy.json` → TrpgEnemy 인스턴스 생성, 던전 EnemyList 등록
  - `TR_6_Quest.json` → Quest 인스턴스 생성, Chapter에 등록
- `TR_0_Overview.md` Markdown 파싱 (챕터 목표 파싱: `목표:[몬스터:이름]을 [처치]` 등)
- `RuleForge.csproj`에 RuleBook/*.json, *.md 파일을 `Content/CopyToOutputDirectory` 설정

#### 2. 전투 시스템 확장 🟡
**향후 확장 가능 사항**:
- 경험치 및 레벨업 시스템
- 장비 스탯 반영 (Equipment의 EquipmentStatuses를 전투 계산에 반영)
- 크리티컬 히트, 회피 등 추가 전투 메커니즘
- 소비 아이템의 실제 효과 구현 (HP/MP 회복 등)
- 전투 보상에 골드 추가

### 스토리 및 상호작용 시스템

#### 3. NPC 대화 시스템 🟡 (LLM 통합과 함께 구현 예정)
**필요 작업**:
- NPC 클래스 확장 (TrpgActor 기반)
- 대화 트리 구조 (DialogueNode, 선택지 분기)
- LLM 통합 (동적 대화 생성)
- NPC별 성격/태도 시스템
- 호감도/평판 시스템

**참고**: [TrpgInterface.cs:217-220](TrpgInterface.cs#L217-L220) HandleSocialInput "Talk" 케이스

#### 4. 퀘스트 수락/관리 시스템 🟡 (LLM 통합과 함께 구현 예정)
**필요 작업**:
- NPC와 퀘스트 연결 (NPC가 제공하는 퀘스트 목록)
- 퀘스트 수락 조건 확인 (레벨, 선행 퀘스트, 아이템 보유 등)
- 퀘스트 진행 상황 추적 UI
- 퀘스트 보상 시스템

**참고**:
- Quest 클래스는 [TrpgGameLogic.cs:59](TrpgGameLogic.cs#L59)에 이미 존재
- [TrpgInterface.cs:229-233](TrpgInterface.cs#L229-L233) "Accept Quest" 케이스

### 게임 시스템

#### 5. 저장/로드 시스템 ✅
**위치**: [TrpgGameData.cs](TrpgGameData.cs)

`GameSaveManager` 정적 클래스로 구현 완료:
- `HasSaveData()`: `save.json` 존재 여부 확인
- `Save(state, worldMgr)`: 플레이어 스탯/아이템/스킬, 던전 클리어 상태, 수락/완료 퀘스트 ID → JSON 직렬화
- `Load(state, worldMgr)`: 역직렬화 후 게임 상태 복원 (위치는 WorldUnit ID로 조회)
- 필드/마을에서 "저장하기" 선택지, `InitializeGame()`에 "이어하기" 선택지 (세이브 파일 존재 시)

**미구현 (향후)**:
- 세이브 슬롯 관리 (다중 세이브)
- 자동 저장

#### 6. 휴식 시스템 ✅
**위치**: [TrpgWorld.cs](TrpgWorld.cs) `Field.Rest()`

필드에서 "야영하기" 선택지로 구현 완료:
- HP/MP 최대치의 50% 회복
- `Math.Min(maxHp / 2, maxHp - currentHp)` 방식으로 과회복 방지

#### 7. 설정 시스템 🟢
**필요 작업**:
- 옵션 관리 (음량, 난이도, 텍스트 속도 등)
- 키 바인딩 설정
- 그래픽/디스플레이 옵션
- 설정 저장/불러오기

**참고**: [TrpgInterface.cs:104-107](TrpgInterface.cs#L104-L107) Settings 메뉴

### 기타 미완성 영역

1. **Agent.cs** ([Agent.cs:9](Agent.cs#L9)) - 빈 클래스, 용도 불명확
2. **OllamaAPI.cs** - 대체 LLM API 통합 (현재 사용되지 않음)
3. **TrpgRule** ([TrpgRule.cs:22](TrpgRule.cs#L22)) - 룰 시스템 골격만 존재, Markdown 룰북 파서 구현 필요
4. **TrpgNPC** ([TrpgNPC.cs](TrpgNPC.cs)) - TrpgActor 확장, InterAction/Trade/Communicate 메서드 비어있음
5. **TrpgGameAction** ([TrpgGameAction.cs](TrpgGameAction.cs)) - 기본 액션 구조만 존재 (Name, Description, Target, Cost)
6. **GameStartPreprocess** ([Program.cs:110](Program.cs#L110)) - LLM 모델 로딩 및 룰북 파싱/설정 미구현
7. 멀티플레이어 클라이언트 모드 구현 누락
8. TCP 서버와 게임 상태 통합 미완료
9. WorldManager.LoadWorldInfoByRuleBook() - 룰북 기반 월드 자동 로딩 미구현
10. `RuleForge.csproj`에 RuleBook/ 리소스 파일 빌드 포함 설정 누락

## 개발 우선순위 가이드

### ✅ 구현 완료
전투 시스템, 스킬 시스템, 월드/던전 시스템, 아이템/인벤토리 시스템, 상점 시스템, 게임 설정 시스템 (데이터 기반 전환)

### ✅ 룰북 파일 스키마 정의 완료
JSON 스키마 구조 정의 완료 (TR_1_0~TR_6). 실제 데이터 입력 및 파서 구현 필요.

### 🔴 높음 (핵심 시스템)
- 룰북 JSON 파서(`RulebookParser`) 구현 - 스키마 정의됨, 파싱 로직 미구현
- 룰북 파일에 실제 게임 데이터 입력 (현재 전부 빈 값)
- `RuleForge.csproj`에 RuleBook 파일 빌드 포함 설정 누락

### 🟡 중간 (컨텐츠 확장)
NPC 대화, 퀘스트 관리 - LLM 로컬 모델 통합과 함께 구현 예정

### 🟢 낮음 (편의 기능)
저장/로드, 휴식, 설정 - 사용자 경험 개선

## 기타 미완성 영역 (업데이트)

- `TR_6_Quest.json` 신규 추가됨 - 퀘스트 파싱 및 Chapter 연동 미구현
- `TR_1_0/1/2_World*.json` - 월드를 3개 파일로 분리 (World/Village/Dungeon), 파서 미구현
- `RulebookParser.ParseRulebook()` - 현재 Markdown 파서 시그니처만 있음, JSON 방식으로 재작성 필요
- `WorldManager.LoadWorldInfoByRuleBook()` - 빈 메서드, 구현 필요
- `GameStartPreprocess()` - LLM 로딩 및 룰북 파싱 모두 미구현

## 완료된 작업 이력

### ✅ 2026-03-13: 룰북 파서 및 게임 시작 연결
1. `RuleForge.csproj` RuleBook 리소스 설정
2. 룰북 JSON 스키마 재설계 및 데이터 입력 (아이템 5, 적 3, 스킬 4, NPC 3, 퀘스트 2)
3. `RulebookParser` JSON 파서 구현 (`TrpgRule.cs`, `System.Text.Json`)
4. `GameStartPreprocess` 연결 및 PlayerSetup 씬 완성

**실행 흐름**:
```
룰북 파싱 → PlayerSetup (이름 입력) → 시작의 평원(Field) → 주변 탐색 → [시작 마을 / 초원 동굴]
```

### ✅ 2026-03-14: 게임플레이 시스템 확장
1. **소비 아이템 실제 효과** - `UseConsumable(index, player)`: HP/MP 실제 회복, 로그 반환
2. **전투 골드 보상** - `TrpgEnemy.GoldReward`, `TrpgBattle.GiveEnemyReward()`에서 골드 지급
3. **퀘스트 수락/관리 UI**
   - `Establishment.ShowQuestMenu()`: Quest NPC에서 퀘스트 수락/완료보고/보상 수령
   - `TrpgPlayer.AcceptedQuests`: 수락한 퀘스트 목록
   - `TrpgQuestRegistry`: 파서 단계에서 퀘스트 ID → 인스턴스 등록
4. **필드 야영(휴식) 시스템** - `Field.Rest()`: HP/MP 최대치의 50% 회복
5. **인벤토리 UI** - `TrpgGameLogic.OpenInventory()`: 필드/마을에서 소비 아이템 사용 가능
6. **저장/로드 시스템** (`TrpgGameData.cs`)
   - `GameSaveManager.Save()`: 플레이어/던전/퀘스트 상태 JSON 직렬화 (`save.json`)
   - `GameSaveManager.Load()`: 역직렬화 후 게임 상태 복원
   - 필드/마을에서 "저장하기" 선택지 추가
   - `InitializeGame()`에 "이어하기" 선택지 추가 (세이브 파일 존재 시)

## 다음 할일 (우선순위)

### 🔴 높음
1. **경험치/레벨업 시스템** - 전투 승리 시 EXP 획득 및 레벨업
2. **장비 스탯 반영** - Equipment의 EquipmentStatuses를 전투 데미지 계산에 반영

### 🟡 중간
3. **상점 NPC 연동** - Trader 타입 NPC가 `TrpgShop`을 통해 상점 진입 (현재 NPC 진입까지만)
4. **필드 채집(Gathering)** - `Field.Gathering()`에서 `GatherableItems`를 실제로 지급
5. **TR_0_Overview.md 챕터/스토리 파싱** - LLM 연동 준비용 Markdown 파싱

### 🔵 LLM 로컬모델 연동 시 구현
- **NPC 대화 시스템** - LLM 기반 동적 대화 생성, NPC 성격/태도 반영
- **퀘스트 수락/관리 UI 고도화** - NPC 대화 연동
- **TR_0_Overview.md 스토리 → LLM 프롬프트 주입**
