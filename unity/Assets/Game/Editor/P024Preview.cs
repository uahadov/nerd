using System;
using System.IO;
using System.Linq;
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
    // Explicit, local Editor evidence run. Synthetic Input System mouse; no physical-device claim.
    public static class P024Preview
    {
        private const string Request="Nard.P024.Preview";
        private static double beginAfter,deadline;private static int firstFrame=-1,step,nextFrame=20;
        private static Mouse mouse;private static InputSettings previous;private static string dir;
        private sealed class Dice:IDiceSource{public int NextDie()=>3;}
        [MenuItem("NARD/P02.4/Open undo long match")] public static void OpenLong()=>EditorSceneManager.OpenScene(P024Build.ScenePath);
        [MenuItem("NARD/P02.4/Open undo short match")] public static void OpenShort()
        {OpenLong();var input=UnityEngine.Object.FindAnyObjectByType<BoardTurnInput>();var obj=new SerializedObject(input);obj.FindProperty("initialMode").enumValueIndex=(int)GameMode.Short;obj.ApplyModifiedPropertiesWithoutUndo();}
        public static void Capture(){OpenLong();SessionState.SetBool(Request,true);beginAfter=EditorApplication.timeSinceStartup+3;EditorApplication.update+=Begin;}
        private static void Begin(){if(EditorApplication.timeSinceStartup<beginAfter||EditorApplication.isCompiling||EditorApplication.isUpdating)return;EditorApplication.update-=Begin;EditorApplication.isPlaying=true;}
        [InitializeOnLoadMethod] private static void Resume(){if(!SessionState.GetBool(Request,false))return;dir=Environment.GetEnvironmentVariable("NARD_GUI_CAPTURE_DIR");deadline=EditorApplication.timeSinceStartup+120;EditorApplication.update+=Observe;}
        private static void Mouse(float x,float y,bool down){var p=(Vector2)Camera.main.WorldToScreenPoint(UnityEngine.Object.FindAnyObjectByType<BoardView>().BoardToLocal(new Vector2(x,y)));InputSystem.QueueStateEvent(mouse,new MouseState{position=p,buttons=(ushort)(down?1:0)});}
        private static void Shot(string name)=>ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png"));
        private static bool Check(BoardTurnInput input,int used,string stage)
        {
            var loop=input.Loop;bool ok=loop.LongPosition.Point(0)==15-used&&loop.Tokens.Count(t=>t.State==TokenState.Used)==used&&loop.Session.Player==0;
            File.AppendAllText(Path.Combine(dir,"input-states.txt"),stage+" head="+loop.LongPosition.Point(0)+" used="+loop.Tokens.Count(t=>t.State==TokenState.Used)+" ok="+ok+Environment.NewLine);
            if(!ok){Debug.LogError("NARD_P024_GUI_STATE_FAILED_"+stage);SessionState.SetBool(Request,false);EditorApplication.Exit(1);}
            return ok;
        }
        private static void Observe()
        {
            if(EditorApplication.timeSinceStartup>deadline){Debug.LogError("NARD_P024_GUI_TIMEOUT");SessionState.SetBool(Request,false);EditorApplication.Exit(1);return;}
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
            var input=UnityEngine.Object.FindAnyObjectByType<BoardTurnInput>();
            if(firstFrame<0)
            {
                if(input?.Loop==null)return;Application.runInBackground=true;EditorApplication.ExecuteMenuItem("Window/General/Game");Directory.CreateDirectory(dir);
                previous=UnityEngine.Object.Instantiate(InputSystem.settings);previous.hideFlags=HideFlags.HideAndDontSave;var settings=UnityEngine.Object.Instantiate(previous);settings.hideFlags=HideFlags.HideAndDontSave;
                settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings=settings;
                var root=MatchSession.Create("gui-undo",GameMode.Long,RuleContract.VersionFor(GameMode.Long)).Begin().ResolveOpening(6,1);var loop=new LocalTurnLoop(root,new Dice());loop.TryRoll(0);input.Bind(loop);mouse=InputSystem.AddDevice<Mouse>();firstFrame=Time.frameCount;
            }
            int frame=Time.frameCount-firstFrame;if(frame<nextFrame)return;nextFrame=frame+5;
            switch(step++)
            {
                case 0:if(!Check(input,0,"start"))return;Shot("editor-undo-start");break;
                case 1:Mouse(3.75f,-4,true);break;
                case 2:Mouse(1.8f,-3,true);break;
                case 3:if(!Check(input,0,"drag"))return;Shot("editor-undo-drag");break;
                case 4:Mouse(1.8f,-3,false);break;
                case 5:if(!Check(input,1,"moved"))return;Shot("editor-undo-moved");break;
                case 6:Mouse(0,9.45f,true);break;
                case 7:Mouse(0,9.45f,false);break;
                case 8:if(!Check(input,0,"restored"))return;Shot("editor-undo-restored");break;
                case 9:Mouse(3.75f,-4,true);break;
                case 10:Mouse(20,20,true);break;
                case 11:Mouse(20,20,false);break;
                case 12:if(!Check(input,0,"invalid"))return;Shot("editor-undo-invalid");break;
                case 13:Mouse(3.75f,-4,true);break;
                case 14:Mouse(3.75f,-4,false);break;
                case 15:if(!Check(input,1,"quick"))return;Shot("editor-undo-quick");break;
                case 16:Mouse(0,9.45f,true);break;
                case 17:Mouse(0,9.45f,false);break;
                default:
                    bool ok=input.Loop.LongPosition.Point(0)==15&&input.Loop.LongPosition.UsedMask==0&&input.Loop.Session.Player==0&&!input.Loop.CanUndo&&File.Exists(Path.Combine(dir,"editor-undo-quick.png"));
                    InputSystem.RemoveDevice(mouse);var settings=InputSystem.settings;InputSystem.settings=previous;if(settings!=null)UnityEngine.Object.Destroy(settings);
                    Debug.Log(ok?"NARD_P024_GUI_DRAG_UNDO_INVALID_QUICK_VERIFIED":"NARD_P024_GUI_STATE_FAILED");SessionState.SetBool(Request,false);EditorApplication.update-=Observe;EditorApplication.Exit(ok?0:1);break;
            }
        }
    }
}
