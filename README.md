🎮 ToyParty
📱 APK 다운로드
🧩 구현 내용
🔹 블럭 시스템

직선으로 3개 이상의 블럭이 모이면 블럭 파괴

파괴 후 위에서 블럭이 떨어져 빈 공간을 채움

힌트 기능 : 3초간 아무 행동이 없으면 가능한 매칭 위치 표시 (On/Off 가능)

특수 블럭 : 4개 이상 매칭 시 생성

방향은 마지막으로 이동한 블럭의 이동 방향으로 결정

해당 방향의 한 줄 전체를 파괴

🎭 장애물 (Jack in the Box)

주변 블럭이 매칭될 때마다 광대가 등장해 Goal로 이동

발동 시 Jack-in-the-Box 애니메이션이 재생

광대가 모두 수집되면 클리어 조건 달성

🖥️ UI 시스템

일시정지 메뉴 : 계속하기 / 다시하기 / 나가기

클리어 UI : 점수 + 트로피 표시, 재시작 버튼

실패 UI : 점수 + 검은 트로피 표시, 재시작 버튼

이동횟수 +5 버튼 : 우측 하단에서 이동 횟수 증가

💥 인게임 연출

블럭 파괴 시 카메라 쉐이크

블럭 파괴 시 기기 진동 (Haptics)

AI 음성 합성으로 시작 / 클리어 시 아이 목소리 연출

🧱 개발 철학

SOLID 원칙 기반 설계

낮은 결합도(Loose Coupling)

높은 응집도(High Cohesion)

유지보수 및 기능 확장을 고려한 모듈형 구조

📂 폴더 트리
Assets/
└─ Scripts/
   ├─ Board/
   │  ├─ TileBoardManager.cs          # 보드 상태 및 매칭 로직 총괄
   │  ├─ HexDirections.cs             # 육각형 방향 정의 유틸
   │  ├─ TileMatchFinder.cs           # 매칭 감지 및 검사
   │  └─ BoardAbstractions.cs         # 보드 추상화 및 인터페이스

   ├─ Gems/
   │  ├─ Gem.cs                       # 개별 블럭 로직
   │  ├─ GemFactory.cs                # Gem 생성 / 풀링 관리
   │  ├─ GemType.cs                   # 블럭 타입 정의 (Enum)
   │  └─ GemCrush.cs                  # 블럭 파괴 시 효과 및 이벤트

   ├─ Obstacles/
   │  ├─ Jack.cs                      # 잭인더박스 동작 로직
   │  ├─ JackFactory.cs               # 잭 생성 및 재활용 관리
   │  ├─ JackScoreService.cs          # 점수/수집 연동 로직
   │  ├─ JackVisuals.cs               # 잭 애니메이션 및 비주얼 제어
   │  └─ JackPenaltyFlyVFX.cs         # 광대 이동 연출 (비주얼 이펙트)

   ├─ Input/
   │  ├─ CountRule.cs                 # 입력 시 이동 횟수 처리 규칙
   │  └─ InputHandler.cs              # 터치 / 클릭 입력 처리

   ├─ UI/
   │  ├─ HudUI.cs                     # HUD 갱신 (점수, 이동횟수 등)
   │  ├─ PauseUIController.cs         # 일시정지 UI 제어
   │  ├─ AddMovesButton.cs            # 이동횟수 +5 버튼 기능
   │  ├─ CameraShaker.cs              # 카메라 쉐이크 연출
   │  ├─ HapticsAndShakeListener.cs   # 파괴 시 진동/쉐이크 트리거
   │  ├─ PointsFromDestructionRule.cs # 파괴 점수 계산 규칙
   │  └─ ScaleTweener.cs              # UI 애니메이션 트윈 처리

   ├─ Services/
   │  ├─ MoveCountService.cs          # 이동 횟수 관리
   │  ├─ PauseService.cs              # 일시정지 상태 관리
   │  └─ PointsService.cs             # 점수 계산 및 누적 관리

   ├─ Audio/
   │  ├─ SoundManager.cs              # 사운드 재생 제어
   │  ├─ AudioLibrary_SO.cs           # ScriptableObject 기반 사운드 데이터
   │  └─ Audio.cs                     # 개별 오디오 컴포넌트

   ├─ Hint/
   │  └─ HintSystem.cs                # 힌트 표시 시스템

   └─ Utility/
      ├─ GameBootstrapper.cs          # 초기화 및 씬 셋업
      ├─ GameOutcomeCoordinator.cs    # 클리어 / 실패 판정 및 흐름 제어
      ├─ HintSystem.cs.cs.cs          # (중복 제거 필요)
      └─ VibrationManager.cs.cs       # 기기 진동 제어
