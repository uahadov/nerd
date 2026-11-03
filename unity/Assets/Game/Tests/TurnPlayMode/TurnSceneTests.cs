using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nard.Interaction;
using Nard.Match;
using Nard.Presentation;
using Nard.Rules.Long;
using Nard.Rules.Short;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Nard.Tests.Turn
{
    public sealed class TurnSceneTests
    {
        private sealed class Dice : IDiceSource
        {private readonly Queue<int> q;public int Calls;public Dice(params int[] values){q=new Queue<int>(values);}public int NextDie(){Calls++;return q.Dequeue();}}
        private sealed class SeedDice : IDiceSource
        {private readonly System.Random r;public SeedDice(int seed){r=new System.Random(seed);}public int NextDie()=>r.Next(1,7);}
        private InputSettings previous;
        private IEnumerator Load()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Game/Scenes/TurnBoard.unity",new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("TurnBoard",LoadSceneMode.Single);
#endif
            yield return null;yield return null;
            previous=UnityEngine.Object.Instantiate(InputSystem.settings);previous.hideFlags=HideFlags.HideAndDontSave;
            var settings=UnityEngine.Object.Instantiate(previous);settings.hideFlags=HideFlags.HideAndDontSave;
            settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings=settings;
        }
        [TearDown] public void Restore(){var current=InputSystem.settings;InputSystem.settings=previous;if(current!=null)UnityEngine.Object.Destroy(current);}
        private static MatchSession Create(GameMode mode)=>MatchSession.Create("turn-scene",mode,RuleContract.VersionFor(mode));
        private static BoardTurnInput Input()=>UnityEngine.Object.FindAnyObjectByType<BoardTurnInput>();
        private static Vector2 Screen(float x,float y)=>Camera.main.WorldToScreenPoint(new Vector3(x,y,0));
        private static IEnumerator Click(Mouse mouse,Vector2 p)
        {
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p,buttons=1});yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p,buttons=0});yield return null;
        }
        private static TurnMove[] LegalPath(LocalTurnLoop loop,int index=0)=>loop.Session.Mode==GameMode.Long?
            LongNardeRules.LegalTurns(loop.LongPosition)[index].Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray():
            ShortNardeRules.LegalTurns(loop.ShortPosition)[index].Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray();
        private static MatchSession Ready(GameMode mode)
        {
            var root=Create(mode).Begin().ResolveOpening(6,1);
            if(mode==GameMode.Long)return root;
            var moves=ShortNardeRules.LegalTurns(root.ShortPosition)[0].Select(m=>new TurnMove(m.Source,m.Target,m.Slot)).ToArray();
            Assert.IsTrue(root.TryCommitTurn(0,root.RuleSetVersion,moves,out root,out _));return root.AdvanceTurn();
        }
        [UnityTest] public IEnumerator MouseOpeningDoubleConsumptionAndNextPlayerStayInSync()
        {
            yield return Load();var input=Input();var dice=new Dice(6,1,3,3,4,2);input.Bind(new LocalTurnLoop(Create(GameMode.Long),dice));var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                yield return Click(mouse,Screen(0,-9.35f));Assert.AreEqual(0,input.Loop.Session.Player);Assert.IsTrue(input.Loop.CanRoll);Assert.IsTrue(input.Loop.IsOpeningDisplay);Capture("turn-opening");
                yield return Click(mouse,Screen(0,-9.35f));Assert.AreEqual(4,input.Loop.Tokens.Count);Assert.AreEqual(4,dice.Calls);AssertPips(3,3);Capture("turn-double");
                yield return Click(mouse,Screen(0,-9.35f));Assert.AreEqual(4,dice.Calls);Assert.AreEqual(MatchPhase.Moving,input.Loop.Session.Phase);
                var path=LegalPath(input.Loop);int n=0;
                foreach(var move in path)
                {
                    var from=BoardGeometry.PointBase(move.Source);var to=BoardGeometry.PointBase(move.Target);
                    yield return Click(mouse,Screen(from.x,move.Source<12?-4:4));yield return Click(mouse,Screen(to.x,move.Target<12?-1.65f:1.65f));
                    Assert.AreEqual(++n,input.Loop.Tokens.Count(t=>t.State==TokenState.Used));Assert.AreEqual(4,dice.Calls);
                    if(n==1)Capture("turn-used");
                }
                Assert.AreEqual(1,input.Loop.Session.Player);Assert.IsTrue(input.Loop.CanRoll);Assert.AreEqual(4,input.Loop.Tokens.Count(t=>t.State==TokenState.Used));Capture("turn-next-player");
                yield return Click(mouse,Screen(0,-9.35f));AssertPips(4,2);Assert.AreEqual(2,input.Loop.Tokens.Count);Assert.AreEqual(6,dice.Calls);
                CheckBoard(input);Assert.AreEqual(0,input.Loop.Tokens.Count(t=>t.State==TokenState.Used));
            }finally{InputSystem.RemoveDevice(mouse);}
        }
        [UnityTest] public IEnumerator EveryFaceAndDoubleTokenCountRendersWithoutCallingTheDiceSourceAgain()
        {
            yield return Load();var input=Input();
            foreach(GameMode mode in new[]{GameMode.Long,GameMode.Short})for(int a=1;a<=6;a++)for(int b=1;b<=6;b++)
            {
                var dice=new Dice(a,b);var loop=new LocalTurnLoop(Ready(mode),dice);input.Bind(loop);Assert.IsTrue(loop.TryRoll(loop.Session.Player));input.Render();yield return null;
                AssertPips(a,b);Assert.AreEqual(a==b?4:2,loop.Tokens.Count);Assert.AreEqual(2,dice.Calls);
                var session=loop.Session;var roll=loop.CopyDisplayedRoll();for(int i=0;i<3;i++)input.Render();
                Assert.AreSame(session,loop.Session);CollectionAssert.AreEqual(roll,loop.CopyDisplayedRoll());Assert.AreEqual(2,dice.Calls);CheckBoard(input);
            }
        }
        [UnityTest] public IEnumerator ShortOpeningUsesTheComparedDiceAndBlocksAnExtraRoll()
        {
            yield return Load();var input=Input();var dice=new Dice(6,1,2,2);input.Bind(new LocalTurnLoop(Create(GameMode.Short),dice));var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                yield return Click(mouse,Screen(0,-9.35f));Assert.AreEqual(MatchPhase.Moving,input.Loop.Session.Phase);AssertPips(6,1);Assert.AreEqual(2,dice.Calls);Capture("turn-short");
                yield return Click(mouse,Screen(0,-9.35f));Assert.AreEqual(2,dice.Calls);Assert.AreEqual(0,input.Loop.Tokens.Count(t=>t.State==TokenState.Used));
            }finally{InputSystem.RemoveDevice(mouse);}
        }
        [UnityTest] public IEnumerator TouchAndDuplicatePressCannotRerollOrDragASelectedChecker()
        {
            yield return Load();var input=Input();var dice=new Dice(3,3);input.Bind(new LocalTurnLoop(Ready(GameMode.Long),dice));var touch=InputSystem.AddDevice<Touchscreen>();
            try
            {
                var roll=Screen(0,-9.35f);var source=Screen(3.75f,-6.6f);var target=Screen(1.8f,-1.65f);
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Began,position=roll});yield return null;
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=roll});yield return null;Assert.AreEqual(2,dice.Calls);
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=2,phase=UnityEngine.InputSystem.TouchPhase.Began,position=source});yield return null;
                Assert.AreEqual(0,input.Loop.Selected);
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=2,phase=UnityEngine.InputSystem.TouchPhase.Moved,position=target});yield return null;
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=2,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=target});yield return null;Assert.AreEqual(0,input.Loop.Tokens.Count(t=>t.State==TokenState.Used));
                long id=Time.frameCount+100;Assert.IsFalse(input.ProcessPress(roll,id));Assert.IsFalse(input.ProcessPress(target,id));Assert.IsTrue(input.ProcessPress(target,id+1));Assert.IsFalse(input.ProcessPress(target,id));
                Assert.AreEqual(1,input.Loop.Tokens.Count(t=>t.State==TokenState.Used));Assert.AreEqual(2,dice.Calls);
                var blockedDice=new Dice(3,3);input.Bind(new LocalTurnLoop(Ready(GameMode.Long),blockedDice));
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=3,phase=UnityEngine.InputSystem.TouchPhase.Began,position=roll});
                InputSystem.QueueStateEvent(touch,new TouchState{touchId=4,phase=UnityEngine.InputSystem.TouchPhase.Began,position=source});yield return null;
                Assert.AreEqual(0,blockedDice.Calls);
            }finally{InputSystem.RemoveDevice(touch);}
        }
        [UnityTest] public IEnumerator ReachableBlockedAndPartialTurnsRenderUnavailableTokensSeparately()
        {
            yield return Load();var input=Input();
            foreach(bool blocked in new[]{true,false})
            {
                LocalTurnLoop found=null;
                for(int game=0;game<4&&found==null;game++)
                {
                    var loop=new LocalTurnLoop(Create(GameMode.Short),new SeedDice(2310+game));var random=new System.Random(2323+game);int safety=0;
                    while(loop.Session.Phase!=MatchPhase.Finished&&safety++<2000)
                    {
                        if(loop.CanRoll)loop.TryRoll(loop.Session.Player);
                        if(blocked?loop.Reason=="NO_LEGAL_MOVES_TURN_PASSED":loop.Tokens.Any(t=>t.State==TokenState.Used)&&loop.Tokens.Any(t=>t.State==TokenState.Unavailable)){found=loop;break;}
                        if(loop.Session.Phase==MatchPhase.Moving)
                        {
                            var paths=ShortNardeRules.LegalTurns(loop.ShortPosition);int actor=loop.Session.Player;
                            foreach(var move in LegalPath(loop,random.Next(paths.Count))){loop.Tap(actor,move.Source);loop.Tap(actor,move.Target);}
                        }
                        if(!blocked&&loop.Tokens.Any(t=>t.State==TokenState.Used)&&loop.Tokens.Any(t=>t.State==TokenState.Unavailable)){found=loop;break;}
                    }
                }
                Assert.NotNull(found);input.Bind(found);yield return null;CheckBoard(input);
                int unavailable=found.Tokens.Count(t=>t.State==TokenState.Unavailable);Assert.Greater(unavailable,0);
                Assert.AreEqual(unavailable,input.GetComponentsInChildren<SpriteRenderer>().Count(r=>r.name.StartsWith("TokenUnavailable_")));
                if(blocked)Assert.AreEqual(0,found.Tokens.Count(t=>t.State==TokenState.Used));
                Capture(blocked?"turn-blocked":"turn-partial");
            }
        }
        private static void AssertPips(int a,int b)
        {
            var sprites=Input().GetComponentsInChildren<SpriteRenderer>();Assert.AreEqual(a,sprites.Count(s=>s.name.StartsWith("Pip_0_")));Assert.AreEqual(b,sprites.Count(s=>s.name.StartsWith("Pip_1_")));
        }
        private static void CheckBoard(BoardTurnInput input)
        {
            var view=input.GetComponent<BoardView>();Assert.AreEqual(30,view.Checkers.Count);
            var state=input.Loop.Session.Mode==GameMode.Long?BoardSnapshot.From(input.Loop.LongPosition):BoardSnapshot.From(input.Loop.ShortPosition);
            for(int i=0;i<24;i++)Assert.AreEqual(Math.Abs(state.Point(i)),view.Checkers.Count(c=>c.Point==i));
            for(int p=0;p<2;p++){Assert.AreEqual(state.Bar(p),view.Checkers.Count(c=>c.Player==p&&c.Point==-1));Assert.AreEqual(state.Off(p),view.Checkers.Count(c=>c.Player==p&&c.Point==24));}
        }
        private static void Capture(string name)
        {
            string dir=Environment.GetEnvironmentVariable("NARD_BOARD_CAPTURE_DIR");if(string.IsNullOrEmpty(dir))return;
            var camera=Camera.main;var rt=new RenderTexture(540,960,24);var image=new Texture2D(540,960,TextureFormat.RGB24,false);var active=RenderTexture.active;var target=camera.targetTexture;
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,540,960),0,0);image.Apply();Directory.CreateDirectory(dir);File.WriteAllBytes(Path.Combine(dir,name+".png"),image.EncodeToPNG());}
            finally{camera.targetTexture=target;RenderTexture.active=active;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(image);}
        }
    }
}
