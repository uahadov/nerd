using System;
using System.Collections.Generic;
using System.IO;
using Nard.Interaction;
using Nard.Match;
using Nard.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Nard.Build
{
    public static class P023Preview
    {
        private const string Request="Nard.P023.Preview";
        private static double beginAfter,deadline;
        private static int firstFrame=-1,nextFrame=20,action,phase;
        private static Mouse mouse;private static InputSettings previous;
        private static string dir;private static Dice dice;
        private sealed class Dice : IDiceSource
        {private readonly Queue<int> values=new Queue<int>(new[]{6,1,3,3,4,2});public int Calls;public int NextDie(){Calls++;return values.Dequeue();}}
        private static readonly Vector2[] actions={new Vector2(0,-9.35f),new Vector2(0,-9.35f),new Vector2(3.75f,-4),new Vector2(1.8f,-1.65f),new Vector2(1.8f,-4),new Vector2(-1,-1.65f),new Vector2(-1,-4),new Vector2(-2.95f,-1.65f),new Vector2(3.75f,-4),new Vector2(1.8f,-1.65f),new Vector2(0,-9.35f)};
        [MenuItem("NARD/P02.3/Open local long match")] public static void OpenLong()=>EditorSceneManager.OpenScene(P023Build.ScenePath);
        [MenuItem("NARD/P02.3/Open local short match")] public static void OpenShort()
        {
            OpenLong();var input=UnityEngine.Object.FindAnyObjectByType<BoardTurnInput>();
            var obj=new SerializedObject(input);obj.FindProperty("initialMode").enumValueIndex=(int)GameMode.Short;obj.ApplyModifiedPropertiesWithoutUndo();
            // Intentional unsaved Editor preview choice, not a second mode-selection product screen.
        }
        public static void Capture(){OpenLong();SessionState.SetBool(Request,true);beginAfter=EditorApplication.timeSinceStartup+3;EditorApplication.update+=Begin;}
        private static void Begin()
        {if(EditorApplication.timeSinceStartup<beginAfter||EditorApplication.isCompiling||EditorApplication.isUpdating)return;EditorApplication.update-=Begin;EditorApplication.isPlaying=true;}
        [InitializeOnLoadMethod] private static void Resume()
        {if(!SessionState.GetBool(Request,false))return;dir=Environment.GetEnvironmentVariable("NARD_GUI_CAPTURE_DIR");deadline=EditorApplication.timeSinceStartup+120;EditorApplication.update+=Observe;}
        private static void Press(Vector2 point,bool down)
        {var screen=(Vector2)Camera.main.WorldToScreenPoint(new Vector3(point.x,point.y,0));InputSystem.QueueStateEvent(mouse,new MouseState{position=screen,buttons=(ushort)(down?1:0)});}
        private static void Observe()
        {
            if(EditorApplication.timeSinceStartup>deadline){Debug.LogError("NARD_P023_PREVIEW_TIMEOUT");SessionState.SetBool(Request,false);EditorApplication.Exit(1);return;}
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
            if(firstFrame<0)
            {
                var input=UnityEngine.Object.FindAnyObjectByType<BoardTurnInput>();if(input?.Loop==null)return;
                Application.runInBackground=true;EditorApplication.ExecuteMenuItem("Window/General/Game");Directory.CreateDirectory(dir);
                previous=UnityEngine.Object.Instantiate(InputSystem.settings);previous.hideFlags=HideFlags.HideAndDontSave;
                var settings=UnityEngine.Object.Instantiate(previous);settings.hideFlags=HideFlags.HideAndDontSave;
                settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings=settings;
                dice=new Dice();input.Bind(new LocalTurnLoop(MatchSession.Create("gui-turn",GameMode.Long,RuleContract.VersionFor(GameMode.Long)),dice));mouse=InputSystem.AddDevice<Mouse>();firstFrame=Time.frameCount;
            }
            int frame=Time.frameCount-firstFrame;if(frame<nextFrame)return;
            if(action<actions.Length)
            {
                if(phase==0){Press(actions[action],true);phase=1;nextFrame=frame+2;}
                else if(phase==1){Press(actions[action],false);phase=2;nextFrame=frame+3;}
                else
                {
                    string name=action==0?"editor-opening":action==1?"editor-double":action==3?"editor-used":action==9?"editor-next-player":action==10?"editor-fresh-roll":null;
                    if(name!=null)ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png"));action++;phase=0;nextFrame=frame+3;
                }
            }
            else if(File.Exists(Path.Combine(dir,"editor-fresh-roll.png")))
            {
                var loop=UnityEngine.Object.FindAnyObjectByType<BoardTurnInput>().Loop;var roll=loop.CopyDisplayedRoll();
                if(loop.Session.Player!=1||loop.Session.Phase!=MatchPhase.Moving||roll[0]!=4||roll[1]!=2||dice.Calls!=6){Debug.LogError("NARD_P023_GUI_STATE_FAILED");EditorApplication.Exit(1);return;}
                InputSystem.RemoveDevice(mouse);var settings=InputSystem.settings;InputSystem.settings=previous;if(settings!=null)UnityEngine.Object.Destroy(settings);
                Debug.Log("NARD_P023_GUI_OPENING_ROLL_CONSUME_CHANGE_PLAYER_CAPTURED");SessionState.SetBool(Request,false);EditorApplication.update-=Observe;EditorApplication.Exit(0);
            }
        }
    }
}
