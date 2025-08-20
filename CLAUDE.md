# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

GameKit은 Unity 기반의 모듈형 게임 개발 프레임워크로, 일반적인 게임 기능을 제공하는 여러 패키지로 구성되어 있습니다. 프로젝트는 기능 도메인별로 어셈블리가 구성된 클린 아키텍처 패턴을 따릅니다.

**현재 버전**: 0.1.0  
**최소 Unity 버전**: 6000.0 (Unity 6)

## 프로젝트 구조

### 패키지 아키텍처
GameKit은 모듈별 개별 패키지와 메타 패키지를 제공하는 하이브리드 구조를 사용합니다:

- **개별 패키지**: 필요한 기능만 선택적으로 설치 가능
- **메타 패키지**: 전체 GameKit을 한 번에 설치 가능
- **의존성 자동 해결**: Unity Package Manager가 패키지 간 의존성을 자동으로 관리

### 핵심 모듈
- **GameKit.Common** - Result 패턴, 에러 처리 등 공유 유틸리티
  - 의존성: UniTask
- **GameKit.Assets** - Addressable 에셋 시스템 확장  
  - 의존성: Unity Addressables, UniTask
- **GameKit.Navigation** - 씬과 화면 네비게이션 시스템, 페이지 관리
  - 의존성: GameKit.Common, GameKit.Assets, Unity Addressables, UniTask, VitalRouter
- **GameKit.SceneLauncher** - 씬 로딩 및 라이프사이클 관리
  - 의존성: UniTask
- **GameKit.GameSessions** - 게임 세션 상태 관리 (메타 패키지)
  - 의존성: GameKit.GameSessions.Core, GameKit.GameSessions.VContainer
- **GameKit.GameSessions.Core** - 게임 세션 핵심 비즈니스 로직
  - 의존성: GameKit.Common, VitalMediator.Abstractions, UniTask

### VContainer 통합 모듈
- **GameKit.Navigation.VContainer** - 네비게이션용 VContainer DI 통합
  - 의존성: GameKit.Navigation, GameKit.Assets, VContainer, UniTask
- **GameKit.SceneLauncher.VContainer** - 씬 런처용 VContainer DI 통합
  - 의존성: GameKit.SceneLauncher, VContainer, UniTask
- **GameKit.GameSessions.VContainer** - 게임 세션용 VContainer DI 통합
  - 의존성: GameKit.GameSessions.Core, GameKit.Common, VContainer, VitalMediator, UniTask

### 테스트 모듈
- **GameKit.Navigation.Tests** - 네비게이션 시스템 단위 테스트
- **GameKit.SceneLauncher.Tests** - 씬 런처 단위 테스트

### 메타 패키지
- **GameKit** - 모든 핵심 모듈과 VContainer 통합을 포함하는 완전한 패키지

## Unity 개발 명령어

이 프로젝트는 Unity 프로젝트이므로 모든 빌드, 테스트, 실행은 Unity Editor를 통해 수행됩니다:

### 테스트
- Unity Editor를 열고 Test Runner 윈도우 사용 (Window > General > Test Runner)
- 런타임 기능은 Play Mode 테스트 실행
- 에디터 전용 기능은 Edit Mode 테스트 실행
- 개별 테스트 어셈블리는 모듈별로 분리되어 있음

### 빌드
- Unity의 Build Settings (File > Build Settings)을 사용하여 타겟 플랫폼용 빌드
- 어셈블리는 Unity 빌드 시스템에 의해 자동으로 컴파일됨
- Addressables Groups 윈도우를 사용하여 어드레서블 에셋 관리

## 주요 외부 의존성

### 핵심 라이브러리
- **UniTask** - Unity용 비동기 프로그래밍 라이브러리
- **VContainer** - Unity용 의존성 주입 컨테이너
- **VitalRouter** - 메시지 라우팅 및 커맨드 패턴 구현
- **VitalMediator** - 커스텀 미디에이터 패턴 구현
- **LitMotion** - 고성능 애니메이션 라이브러리
- **Unity Addressables** - 에셋 관리 시스템

## 주요 아키텍처 패턴

### Result 패턴
코드베이스는 에러 처리를 위해 Result 패턴을 광범위하게 사용합니다:
- `GameKit.Common.Results`의 `Result<T>` 및 `FastResult<T>` 타입
- 예상되는 에러 케이스에 대해 예외 사용을 피함
- 코드와 설명이 포함된 구조화된 에러 정보 제공

### VitalRouter를 사용한 Command 패턴
네비게이션과 게임 세션 작업은 커맨드 기반 아키텍처를 사용합니다:
- VitalRouter 메시지 시스템을 통해 커맨드가 라우팅됨
- 예: 페이지 네비게이션용 `ToPageCommand`, `BackPageCommand`
- 핸들러는 UniTask를 사용하여 커맨드를 비동기적으로 처리

### VContainer를 사용한 의존성 주입
VContainer가 모듈 전반에 걸쳐 의존성 주입에 사용됩니다:
- `.VContainer` 어셈블리의 확장 메서드가 등록 헬퍼 제공
- 적절한 객체 라이프사이클 관리를 위한 씬 기반 라이프타임 스코프
- Unity의 씬 로딩 시스템과 통합

### 어셈블리 분리
각 기능은 고유한 어셈블리 정의에서 격리됩니다:
- 핵심 로직 어셈블리는 최소한의 의존성을 가짐
- VContainer 통합 어셈블리는 핵심과 VContainer 모두에 의존
- 테스트 어셈블리는 해당하는 핵심 어셈블리를 참조

## 네비게이션 시스템 아키텍처

네비게이션 시스템은 씬과 화면(페이지) 네비게이션을 모두 지원합니다:

### 씬 네비게이션
- `SceneNavigator`가 Addressables를 통한 씬 로딩 처리
- `LoadingStatus`로 로딩 진행률 추적 지원
- 씬 경로는 유연한 구성을 위해 별칭 사용 가능

### 화면/페이지 네비게이션  
- `PageStack`을 사용한 스택 기반 페이지 관리
- 페이지는 `IPage` 인터페이스 구현
- 네비게이션 커맨드: `ToPageCommand`, `PushPageCommand`, `BackPageCommand`, `ReplacePageCommand`
- `PageNavigator`가 페이지 라이프사이클(표시/숨김) 처리
- `PageRegistry`가 페이지 인스턴스와 생성 관리

## 게임 세션 아키텍처

게임 세션은 게임플레이 상태 관리를 제공합니다:

### 모듈 구조
- **GameKit.GameSessions.Core**: 핵심 비즈니스 로직
  - `IGameState` 인터페이스가 게임 상태 구조 정의
  - `GameSession<T>`가 세션 ID로 상태를 래핑
  - `GameSessionManager<T>`가 세션 생성 및 관리
  - 커맨드: `CreateGameSessionCommand<T>`, `GetGameStateCommand<T>`
- **GameKit.GameSessions.VContainer**: VContainer DI 통합
  - 의존성 주입을 위한 ContainerBuilder 확장 메서드
  - 핸들러 등록 및 라이프사이클 관리
- **GameKit.GameSessions**: 메타 패키지 (Core + VContainer 포함)

## 개발 가이드라인

### 네임스페이스 규칙
모든 네임스페이스는 `GameKit.{Module}.{SubModule}` 패턴을 따릅니다:
- 핵심 모듈: `GameKit.Navigation.Screens.Page`
- 내부 구현: `GameKit.Navigation.Screens.Page.Internal`
- VContainer 통합: `GameKit.Navigation.VContainer`

### 비동기 프로그래밍
- Unity 호환성을 위해 표준 Task 대신 UniTask 사용
- 모든 비동기 작업은 CancellationToken 지원해야 함
- 커맨드 핸들러는 비동기 처리를 위해 UniTask 반환

### 에러 처리
- 예상되는 에러에 대해 예외 던지기 대신 Result 패턴 사용
- 의미 있는 에러 코드와 설명 제공
- Microsoft.Extensions.Logging 추상화를 사용하여 적절하게 에러 로깅

## 샘플 프로젝트

`Assets/Samples` 디렉토리에는 GameKit 모듈 사용법을 보여주는 예제들이 포함되어 있습니다:

### Navigation 샘플
- **PageQuickStart** - 페이지 네비게이션 시스템의 기본 사용법 데모
  - `HomePage`, `LoginPage`, `SettingsPage` 등 다양한 페이지 구현 예제
  - VContainer DI 통합 및 페이지 라이프사이클 관리
  - UI Toolkit(UXML/USS) 사용 예제
  
- **SceneOverview** - 씬 네비게이션 시스템 데모
  - Addressables를 통한 원격 씬 로딩
  - 다운로드 진행률 및 로딩 상태 추적
  - 로컬/원격 씬 구분 및 관리
  - 씬 전환 효과 및 페이드 처리

### 샘플 실행 방법
1. Unity Editor에서 해당 샘플 씬 열기
2. Play 모드로 실행하여 기능 테스트
3. 소스 코드 참조하여 구현 패턴 학습

## 패키지 설치 가이드

### 개별 패키지 설치
필요한 기능만 선택적으로 설치할 수 있습니다:

```json
{
  "dependencies": {
    "com.aaron.game-kit.navigation": "0.1.0",
    "com.aaron.game-kit.navigation.vcontainer": "0.1.0"
  }
}
```

### 전체 패키지 설치
모든 GameKit 기능을 한 번에 설치:

```json
{
  "dependencies": {
    "com.aaron.game-kit": "0.1.0"
  }
}
```

### 패키지 의존성 해결
Unity Package Manager가 다음을 자동으로 해결합니다:
- GameKit 모듈 간 의존성
- 외부 라이브러리 의존성 (UniTask, VContainer 등)
- 버전 호환성 관리