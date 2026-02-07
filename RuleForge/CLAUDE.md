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
   - 씬 전환 메서드 및 선택지 관리 제공
   - 확장 가능한 상태 저장을 위한 CustomData 딕셔너리 사용

3. **TrpgGameLogic** ([TrpgGameLogic.cs:122](TrpgGameLogic.cs#L122)) - 비즈니스 로직 (싱글톤)
   - 챕터, 퀘스트, 활동 관리
   - 플레이어 생성 및 관리 처리
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

1. **TrpgPlayer** ([TrpgPlayer.cs:140](TrpgPlayer.cs#L140)) - TrpgActor를 확장
   - PlayerProfile: 나이, 성별, 성격, 직업, 배경 스토리, 레벨
   - PlayerClass: 캐릭터 클래스 시스템
   - PlayerItemBag: 소비템, 장비, 키 아이템 인벤토리
   - PlayerEquipments: 장착된 아이템 관리

2. **TrpgActor** - 캐릭터(플레이어, NPC)의 기본 클래스
   - CommonAttributes: 동적 상태 속성 (HP, MP 등)

3. **아이템 시스템** - 세 가지 아이템 타입
   - Equipment (장비): 장착/해제 가능
   - Consumable (소비템): 일회용 또는 수량 기반
   - KeyItem (키 아이템): 퀘스트 관련 아이템

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

## 미구현 시스템 (우선순위별)

### 핵심 게임플레이 시스템

#### 1. 전투 시스템 🔴
**필요 작업**:
- Battle/Combat 클래스 생성 (전투 상태, 턴 관리)
- Enemy/Monster 클래스 생성 (적 캐릭터, 스탯, AI)
- 전투 액션 시스템 (Attack, Defend, Skill 사용, Guard 등)
- 턴제/실시간 전투 시스템 결정
- 전투 결과 처리 (승리/패배, 경험치, 전리품, 레벨업)
- 전투 UI/렌더링 시스템

**참고**: [TrpgInterface.cs:122-137](TrpgInterface.cs#L122-L137) HandleBattleInput에 TODO 주석 존재

#### 2. 스킬 시스템 🔴
**필요 작업**:
- Skill 클래스 생성 (이름, 설명, 효과, MP/리소스 소모량, 타겟 타입)
- Player에 스킬 목록 추가 (List<Skill> PlayerSkills)
- 스킬 사용 로직 (타겟 선택, 효과 적용, 리소스 소모)
- 직업별/레벨별 스킬 습득 시스템
- 스킬 업그레이드/강화 시스템

**참고**: [TrpgInterface.cs:338-353](TrpgInterface.cs#L338-L353) SelectSkill에 TODO 주석 존재

#### 3. 월드/위치 시스템 🔴
**필요 작업**:
- Location 클래스 생성 (이름, 설명, 접근 가능 여부, 위험도)
- World/Map 시스템 (위치 간 연결 관계, 이동 조건, 거리)
- 위치별 Activity 타입 매핑 (Dungeon → Combat, Village → Social)
- Chapter/Quest 시스템과 연동 (특정 위치 잠금/해금)
- 지역별 적 출현 테이블

**참고**: [TrpgInterface.cs:319-333](TrpgInterface.cs#L319-L333) SelectLocation에 TODO 주석 존재

### 경제 및 거래 시스템

#### 4. 상점 시스템 🟡
**필요 작업**:
- Shop 클래스 생성 (상점 이름, 판매 아이템 목록, 가격 데이터)
- Player에 Gold/Currency 시스템 추가
- 상점별 판매 아이템 목록 데이터 구조
- 구매/판매 가격 계산 로직 (할인, 흥정 등)
- 상점 인벤토리 관리 (재고 제한, 리필 시스템)

**참고**:
- [TrpgInterface.cs:358-372](TrpgInterface.cs#L358-L372) BuyItem에 TODO 주석
- [TrpgInterface.cs:377-428](TrpgInterface.cs#L377-L428) SellItem은 구현됨 (골드 추가 로직만 필요)

### 스토리 및 상호작용 시스템

#### 5. NPC 대화 시스템 🟡
**필요 작업**:
- NPC 클래스 확장 (TrpgActor 기반)
- 대화 트리 구조 (DialogueNode, 선택지 분기)
- LLM 통합 (동적 대화 생성)
- NPC별 성격/태도 시스템
- 호감도/평판 시스템

**참고**: [TrpgInterface.cs:217-220](TrpgInterface.cs#L217-L220) HandleSocialInput "Talk" 케이스

#### 6. 퀘스트 수락/관리 시스템 🟡
**필요 작업**:
- NPC와 퀘스트 연결 (NPC가 제공하는 퀘스트 목록)
- 퀘스트 수락 조건 확인 (레벨, 선행 퀘스트, 아이템 보유 등)
- 퀘스트 진행 상황 추적 UI
- 퀘스트 보상 시스템

**참고**:
- Quest 클래스는 [TrpgGameLogic.cs:59](TrpgGameLogic.cs#L59)에 이미 존재
- [TrpgInterface.cs:229-233](TrpgInterface.cs#L229-L233) "Accept Quest" 케이스

### 게임 시스템

#### 7. 저장/로드 시스템 🟢
**필요 작업**:
- 게임 상태 직렬화 (JSON/Binary)
- 파일 I/O (세이브 파일 읽기/쓰기)
- 세이브 슬롯 관리 (여러 세이브 파일 지원)
- 게임 상태 역직렬화
- 세이브 파일 검증 및 버전 호환성 처리
- 자동 저장 기능

**참고**:
- [TrpgInterface.cs:100-102](TrpgInterface.cs#L100-L102) Load Game
- [TrpgInterface.cs:189-192](TrpgInterface.cs#L189-L192) Save Game

#### 8. 휴식 시스템 🟢
**필요 작업**:
- HP/MP 회복 로직
- 시간 경과 시스템 (선택적)
- 안전 지역 확인
- 휴식 중 이벤트 (랜덤 인카운터 등)

**참고**: [TrpgInterface.cs:185-187](TrpgInterface.cs#L185-L187) Rest 케이스

#### 9. 설정 시스템 🟢
**필요 작업**:
- 옵션 관리 (음량, 난이도, 텍스트 속도 등)
- 키 바인딩 설정
- 그래픽/디스플레이 옵션
- 설정 저장/불러오기

**참고**: [TrpgInterface.cs:104-107](TrpgInterface.cs#L104-L107) Settings 메뉴

### 기타 미완성 영역

1. **Agent.cs** ([Agent.cs:9](Agent.cs#L9)) - 빈 클래스, 용도 불명확
2. **OllamaAPI.cs** - 대체 LLM API 통합 (현재 사용되지 않음)
3. **TrpgRule** ([TrpgRule.cs:22](TrpgRule.cs#L22)) - 룰 시스템 및 룰북 파서 미구현
4. **GameStartPreprocess** ([Program.cs:110](Program.cs#L110)) - LLM 모델 로딩 및 룰 설정 미구현
5. 멀티플레이어 클라이언트 모드 구현 누락
6. TCP 서버와 게임 상태 통합 미완료

## 개발 우선순위 가이드

### 🔴 높음 (핵심 게임플레이)
전투, 스킬, 월드/위치 시스템 - 게임의 기본 루프를 완성하는 데 필수

### 🟡 중간 (컨텐츠 확장)
상점, NPC 대화, 퀘스트 관리 - 게임 깊이와 재미를 추가

### 🟢 낮음 (편의 기능)
저장/로드, 휴식, 설정 - 사용자 경험 개선
