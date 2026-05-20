# EditMode/RunStateMachineTests.cs

```csharp
using CoreRacer.Gameplay.Run;
using NUnit.Framework;

namespace CoreRacer.Tests.EditMode
{
    public sealed class RunStateMachineTests
    {
        [Test]
        public void Can_Start_Run_From_Menu()
        {
            var sm = new RunStateMachine();
            Assert.True(sm.TrySetState(RunState.Running));
            Assert.AreEqual(RunState.Running, sm.State);
        }

        [Test]
        public void Cannot_Return_Directly_From_Running_To_Menu()
        {
            var sm = new RunStateMachine();
            sm.TrySetState(RunState.Running);
            Assert.False(sm.TrySetState(RunState.MainMenu));
        }
    }
}

```
