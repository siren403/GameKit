using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Navigation.Screens.Core;
using GameKit.Navigation.Screens.Core.Commands;
using GameKit.Navigation.Screens.Dialog.Commands;
using GameKit.Navigation.Screens.Dialog.Internal;
using NUnit.Framework;
using GameKit.Navigation.VContainer;
using UnityEngine.TestTools;
using VContainer;
using VitalRouter;

namespace GameKit.Navigation.Tests.Screens
{
    public class ShowDialogCommandTest
    {
        #region 기본 성공 시나리오

        [UnityTest]
        public IEnumerator ShowDialogCommand_새다이얼로그표시_성공() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var builder = new ContainerBuilder();
            var dialog = new MockDialog { Id = MockDialog.Confirm };
            builder.RegisterDialogs(dialogs => { dialogs.InMemory(dialog.Id, dialog); });
            var container = builder.Build();

            var router = container.Resolve<Router>();
            var navigator = container.Resolve<DialogNavigator>();

            // Act
            await router.PublishAsync(new ShowDialogCommand(dialog.Id));

            // Assert
            Assert.IsTrue(dialog.IsVisible);
            Assert.IsTrue(navigator.IsCurrentDialog(dialog.Id));
        });

        [UnityTest]
        public IEnumerator ShowDialogCommand_빈스택에서다이얼로그표시_성공() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var builder = new ContainerBuilder();
            var dialog = new MockDialog { Id = MockDialog.Confirm };
            builder.RegisterDialogs(dialogs => { dialogs.InMemory(dialog.Id, dialog); });
            var container = builder.Build();

            var router = container.Resolve<Router>();
            var navigator = container.Resolve<DialogNavigator>();
            var stack = container.Resolve<DialogStack>();

            // 스택이 비어있는지 확인
            Assert.IsFalse(stack.TryPeek(out _), "스택은 처음에 비어있어야 함");

            // Act
            await router.PublishAsync(new ShowDialogCommand(dialog.Id));

            // Assert
            Assert.IsTrue(dialog.IsVisible);
            Assert.IsTrue(navigator.IsCurrentDialog(dialog.Id));
            Assert.IsTrue(stack.TryPeek(out var topDialogId), "스택에 다이얼로그가 추가되어야 함");
            Assert.AreEqual(dialog.Id, topDialogId, "새 다이얼로그가 스택 최상단에 있어야 함");
        });

        [UnityTest]
        public IEnumerator ShowDialogCommand_여러다이얼로그스택_성공() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var builder = new ContainerBuilder();
            var dialog1 = new MockDialog { Id = MockDialog.Confirm };
            var dialog2 = new MockDialog { Id = MockDialog.Alert };
            var dialog3 = new MockDialog { Id = MockDialog.Loading };
            builder.RegisterDialogs(dialogs =>
            {
                dialogs.InMemory(dialog1.Id, dialog1);
                dialogs.InMemory(dialog2.Id, dialog2);
                dialogs.InMemory(dialog3.Id, dialog3);
            });
            var container = builder.Build();

            var router = container.Resolve<Router>();
            var navigator = container.Resolve<DialogNavigator>();
            var stack = container.Resolve<DialogStack>();

            // 첫 번째 다이얼로그 표시
            await router.PublishAsync(new ShowDialogCommand(dialog1.Id));
            Assert.IsTrue(dialog1.IsVisible, "dialog1이 먼저 표시됨");
            Assert.IsFalse(dialog2.IsVisible, "dialog2는 아직 표시되지 않음");

            // 두 번째 다이얼로그 추가 (스택에 쌓임)
            await router.PublishAsync(new ShowDialogCommand(dialog2.Id));
            Assert.IsTrue(dialog1.IsVisible, "dialog1은 계속 표시됨 (다이얼로그는 스택으로 쌓임)");
            Assert.IsTrue(dialog2.IsVisible, "dialog2도 표시됨");

            // Act - 세 번째 다이얼로그 추가
            await router.PublishAsync(new ShowDialogCommand(dialog3.Id));

            // Assert
            Assert.IsTrue(dialog1.IsVisible, "모든 다이얼로그 표시됨");
            Assert.IsTrue(dialog2.IsVisible);
            Assert.IsTrue(dialog3.IsVisible);
            Assert.IsTrue(navigator.IsCurrentDialog(dialog3.Id), "마지막 다이얼로그가 현재 다이얼로그");

            // 스택 상태 확인
            Assert.IsTrue(stack.TryPeek(out var topDialogId), "스택에 다이얼로그가 있어야 함");
            Assert.AreEqual(dialog3.Id, topDialogId, "마지막 다이얼로그가 스택 최상단에 있어야 함");
        });

        #endregion

        #region 중복 방지 로직

        [UnityTest]
        public IEnumerator ShowDialogCommand_동일다이얼로그중복표시_무시됨() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var builder = new ContainerBuilder();
            var dialog = new MockDialog { Id = MockDialog.Confirm };
            builder.RegisterDialogs(dialogs => { dialogs.InMemory(dialog.Id, dialog); });
            var container = builder.Build();

            var router = container.Resolve<Router>();
            var navigator = container.Resolve<DialogNavigator>();

            ScreenErrorCommand? receivedError = null;
            using var subscribe = router.Subscribe<ScreenErrorCommand>((error, ctx) => { receivedError = error; });

            // 첫 번째 표시
            await router.PublishAsync(new ShowDialogCommand(dialog.Id));
            Assert.IsTrue(dialog.IsVisible, "첫 번째 표시 후 다이얼로그가 보임");
            Assert.IsTrue(navigator.IsCurrentDialog(dialog.Id), "첫 번째 표시 후 현재 다이얼로그가 됨");

            // Act - 동일한 다이얼로그 다시 표시 시도
            await router.PublishAsync(new ShowDialogCommand(dialog.Id));
            await UniTask.Yield(); // 에러 이벤트 처리 대기

            // Assert
            Assert.IsTrue(dialog.IsVisible, "중복 표시 무시되어 다이얼로그는 계속 보임");
            Assert.IsTrue(navigator.IsCurrentDialog(dialog.Id), "여전히 현재 다이얼로그");

            // DialogErrorCommand 검증
            Assert.IsNotNull(receivedError);
            Assert.AreEqual(MockDialog.Confirm, receivedError.Value.ScreenId);
            Assert.AreEqual(ScreenOperation.None, receivedError.Value.Operation);
            Assert.AreEqual(ScreenErrorCodes.AlreadyCurrent, receivedError.Value.ErrorCode);
        });

        #endregion

        #region 에러 처리

        [UnityTest]
        public IEnumerator ShowDialogCommand_존재하지않는다이얼로그_DialogErrorCommand발행() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var builder = new ContainerBuilder();
            builder.RegisterDialogs(dialogs => { }); // 빈 다이얼로그 등록
            var container = builder.Build();

            var router = container.Resolve<Router>();
            var navigator = container.Resolve<DialogNavigator>();

            ScreenErrorCommand? receivedError = null;
            using var subscribe = router.Subscribe<ScreenErrorCommand>((error, ctx) => { receivedError = error; });

            // Act - 존재하지 않는 다이얼로그 표시 시도
            await router.PublishAsync(new ShowDialogCommand("NonExistentDialog"));
            await UniTask.Yield(); // 에러 이벤트 처리 대기

            // Assert
            Assert.IsFalse(navigator.IsCurrentDialog("NonExistentDialog"), "다이얼로그가 등록되지 않음");

            // DialogErrorCommand 검증
            Assert.IsNotNull(receivedError);
            Assert.AreEqual("NonExistentDialog", receivedError.Value.ScreenId);
            Assert.AreEqual(ScreenOperation.Get, receivedError.Value.Operation);
            Assert.AreEqual(ScreenErrorCodes.NotFound, receivedError.Value.ErrorCode);
            Assert.That(receivedError.Value.Message, Does.Contain("not found"));
        });

        #endregion

        #region 스택 상태 검증

        [UnityTest]
        public IEnumerator ShowDialogCommand_스택상태정확히관리됨() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var builder = new ContainerBuilder();
            var dialog1 = new MockDialog { Id = MockDialog.Confirm };
            var dialog2 = new MockDialog { Id = MockDialog.Alert };
            var dialog3 = new MockDialog { Id = MockDialog.Loading };
            builder.RegisterDialogs(dialogs =>
            {
                dialogs.InMemory(dialog1.Id, dialog1);
                dialogs.InMemory(dialog2.Id, dialog2);
                dialogs.InMemory(dialog3.Id, dialog3);
            });
            var container = builder.Build();

            var router = container.Resolve<Router>();
            var stack = container.Resolve<DialogStack>();

            // 초기 상태: 빈 스택
            Assert.IsFalse(stack.TryPeek(out _), "초기에는 빈 스택");

            // Act & Assert - 다이얼로그 1 표시
            await router.PublishAsync(new ShowDialogCommand(dialog1.Id));
            Assert.IsTrue(stack.TryPeek(out var current1), "다이얼로그 1이 스택에 추가됨");
            Assert.AreEqual(dialog1.Id, current1);

            // Act & Assert - 다이얼로그 2 표시 (스택에 2개)
            await router.PublishAsync(new ShowDialogCommand(dialog2.Id));
            Assert.IsTrue(stack.TryPeek(out var current2), "다이얼로그 2가 스택 최상단");
            Assert.AreEqual(dialog2.Id, current2);

            // Act & Assert - 다이얼로그 3 표시 (스택에 3개)
            await router.PublishAsync(new ShowDialogCommand(dialog3.Id));
            Assert.IsTrue(stack.TryPeek(out var current3), "다이얼로그 3이 스택 최상단");
            Assert.AreEqual(dialog3.Id, current3);

            // 스택 크기 확인 (내부 상태 검증)
            var stackSize = 0;
            while (stack.TryPop(out _)) stackSize++;
            Assert.AreEqual(3, stackSize, "스택에 3개 다이얼로그가 있어야 함");
        });

        #endregion

        #region 동시성 제어

        [UnityTest]
        public IEnumerator ShowDialogCommand_작업중취소_정상처리됨() => UniTask.ToCoroutine(async () =>
        {
            // Arrange
            var builder = new ContainerBuilder();
            var slowDialog = new MockDialog { Id = MockDialog.Confirm };
            builder.RegisterDialogs(dialogs => { dialogs.InMemory(slowDialog.Id, slowDialog); });
            var container = builder.Build();

            var router = container.Resolve<Router>();
            var navigator = container.Resolve<DialogNavigator>();
            var stack = container.Resolve<DialogStack>();

            using var cts = new CancellationTokenSource();

            // Act - 작업 시작 후 즉시 취소
            var command = new ShowDialogCommand(slowDialog.Id);
            var task = router.PublishAsync(command, cts.Token);
            await UniTask.DelayFrame(1, cancellationToken: cts.Token);
            cts.Cancel();

            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                // 취소 예외는 정상적인 동작
            }

            // Assert
            // 취소된 다이얼로그는 Navigator에서 현재 다이얼로그로 인식되지 않아야 함
            Assert.IsFalse(navigator.IsCurrentDialog(slowDialog.Id), "취소된 다이얼로그는 현재 다이얼로그가 아니어야 함");

            // 스택 상태 확인 - 취소된 다이얼로그는 스택에 추가되지 않아야 함
            Assert.IsFalse(stack.TryPeek(out _), "취소된 다이얼로그는 스택에 추가되지 않아야 함");

            // 시스템이 안정적이어야 함 (예외 없이 동작)
            Assert.DoesNotThrow(() => navigator.IsCurrentDialog(slowDialog.Id), "시스템이 안정적이어야 함");
        });

        #endregion
    }
}