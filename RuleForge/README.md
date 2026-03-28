# RuleForge

.NET 9.0 기반 TRPG 엔진. LLM 연동 내러티브 생성 지원.

## 실행 방법

```bash
# 빌드
dotnet build

# 실행
dotnet run

# 멀티플레이어 호스트 (포트 지정)
dotnet run -- 8080
```

## 패키지 설치

두 패키지는 반드시 동일 버전으로 설치해야 합니다.

```bash
dotnet add package LLamaSharp --version 0.26.0
dotnet add package LLamaSharp.Backend.Cpu --version 0.26.0
```

## LLM 모델 설정

`GameSetting/model/` 디렉토리에 GGUF 모델 파일을 넣고, `GameSetting/GameSetting.ini`의 `[Model] path=` 에 파일명을 입력합니다. 모델이 없으면 폴백 모드로 실행됩니다.
