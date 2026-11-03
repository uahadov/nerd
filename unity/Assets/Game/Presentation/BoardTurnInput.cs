using Nard.Interaction;
using System.Linq;
using Nard.Match;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nard.Presentation
{
    [DisallowMultipleComponent,RequireComponent(typeof(BoardView))]
    public sealed class BoardTurnInput : MonoBehaviour
    {
        [SerializeField] private bool advancedGestures;
        [SerializeField] private GameMode initialMode=GameMode.Long;
        private BoardView view;
        private long lastPress=-1;
        private Vector2 pressPosition;private bool tracking;private int pressUsed;
        private SpriteRenderer draggedRenderer;
        private GameObject ghost;private int quickTarget=-1;
        private int Used()=>Loop.Tokens.Count(t=>t.State==TokenState.Used);
        public LocalTurnLoop Loop { get; private set; }
        private void Start()=>Bind(new LocalTurnLoop(MatchSession.Create("local-hotseat",initialMode,RuleContract.VersionFor(initialMode)),new LocalDiceSource()));
        public void Bind(LocalTurnLoop loop)
        {CancelGesture();Loop=loop??throw new System.ArgumentNullException(nameof(loop));view=GetComponent<BoardView>();Render();}
        private void Update()
        {
            if(Loop==null||Camera.main==null)return;
            var touch=Touchscreen.current;
            int fingers=0;if(touch!=null)foreach(var finger in touch.touches)if(finger.press.isPressed)fingers++;
            if(advancedGestures&&fingers>1){CancelGesture();lastPress=Time.frameCount;Render("Bir barmaqla toxunun");return;}
            if(advancedGestures&&touch!=null&&touch.primaryTouch.phase.ReadValue()==UnityEngine.InputSystem.TouchPhase.Canceled){CancelGesture();return;}
            bool released=touch!=null&&touch.primaryTouch.press.wasReleasedThisFrame;
            if(!released&&Mouse.current!=null)released=Mouse.current.leftButton.wasReleasedThisFrame;
            if(advancedGestures&&released&&tracking)
            {
                var at=touch!=null&&touch.primaryTouch.press.wasReleasedThisFrame?touch.primaryTouch.position.ReadValue():Mouse.current.position.ReadValue();
                tracking=false;ClearGhost();
                if(Vector2.Distance(at,pressPosition)>12&&Used()==pressUsed&&Loop.Selected!=TapSelection.None)Drop(at);
                else if(quickTarget>=0&&Used()==pressUsed){Loop.Tap(Loop.Session.Player,quickTarget);Render();}
                quickTarget=-1;
            }

            if(advancedGestures&&tracking&&!released)
            {
                var at=touch!=null&&touch.primaryTouch.press.isPressed?touch.primaryTouch.position.ReadValue():Mouse.current!=null?Mouse.current.position.ReadValue():pressPosition;
                if(Vector2.Distance(at,pressPosition)>12&&Used()==pressUsed&&Loop.Selected!=TapSelection.None)
                {
                    if(ghost==null)
                    {
                        var checker=view.Checkers.LastOrDefault(c=>c.Point==Loop.Selected&&c.Player==Loop.Session.Player);
                        if(checker!=null){draggedRenderer=checker.Renderer;draggedRenderer.enabled=false;ghost=new GameObject("DragPreview",typeof(SpriteRenderer));var sprite=ghost.GetComponent<SpriteRenderer>();sprite.sprite=checker.Renderer.sprite;sprite.color=checker.Renderer.color;sprite.sortingOrder=100;ghost.transform.localScale=checker.Renderer.transform.lossyScale;}
                    }
                    if(ghost!=null){var world=Camera.main.ScreenToWorldPoint(new Vector3(at.x,at.y,-Camera.main.transform.position.z));world.z=0;ghost.transform.position=world;}
                }
            }
            if(touch!=null&&touch.primaryTouch.press.wasPressedThisFrame)
            {
                int active=0;foreach(var finger in touch.touches)if(finger.press.isPressed)active++;
                if(active>1){tracking=false;quickTarget=-1;ClearGhost();lastPress=Time.frameCount;Render("Bir barmaqla toxunun");return;}
                BeginDrag(touch.primaryTouch.position.ReadValue());
            }
            else if(Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)BeginDrag(Mouse.current.position.ReadValue());
        }
        private void BeginDrag(Vector2 p)
        {pressPosition=p;tracking=true;pressUsed=Used();quickTarget=-1;
         var world=Camera.main.ScreenToWorldPoint(new Vector3(p.x,p.y,-Camera.main.transform.position.z));int point=BoardHitMap.PointAt(view.LocalToBoard(view.transform.InverseTransformPoint(world)),Loop.Session.Player);
         if(advancedGestures&&point==Loop.Selected&&Loop.Targets.Count==1)quickTarget=Loop.Targets[0];else ProcessPress(p,Time.frameCount);}
        private void Drop(Vector2 screen)
        {
            // A release is a board destination, never a roll/undo button press.
            if(screen.x<0||screen.y<0||screen.x>=Screen.width||screen.y>=Screen.height){Render();return;}
            var world=Camera.main.ScreenToWorldPoint(new Vector3(screen.x,screen.y,-Camera.main.transform.position.z));
            int point=BoardHitMap.PointAt(view.LocalToBoard(view.transform.InverseTransformPoint(world)),Loop.Session.Player);
            if(Loop.Targets.Contains(point))Loop.Tap(Loop.Session.Player,point);
            Render();
        }
        private void ClearGhost(){if(draggedRenderer!=null)draggedRenderer.enabled=true;draggedRenderer=null;if(ghost!=null){ghost.SetActive(false);Destroy(ghost);ghost=null;}}
        private void CancelGesture(){tracking=false;quickTarget=-1;ClearGhost();}
        private void OnApplicationFocus(bool focused){if(!focused)CancelGesture();}
        private void OnDisable()=>CancelGesture();
        public bool ProcessPress(Vector2 screen,long pressId)
        {
            if(Loop==null||pressId<=lastPress)return false;
            lastPress=pressId;var camera=Camera.main;if(camera==null)return false;
            var world=camera.ScreenToWorldPoint(new Vector3(screen.x,screen.y,-camera.transform.position.z));
            var local=view.LocalToBoard(view.transform.InverseTransformPoint(world));
            int point=BoardHitMap.PointAt(local,Loop.Session.Player);
            bool accepted=BoardHitMap.IsUndoButton(local)?Loop.TryUndo(Loop.Session.Player):BoardHitMap.IsRollButton(local)?Loop.TryRoll(Loop.Session.Player):
                Loop.Tap(Loop.Session.Player,point)!=TapOutcome.Rejected;
            Render();return accepted;
        }
        public void Render()=>Render(Message(Loop.Reason));
        private void Render(string message)
        {
            view.Display(Loop.Session.Mode==GameMode.Long?BoardSnapshot.From(Loop.LongPosition):BoardSnapshot.From(Loop.ShortPosition));
            view.SetTurnOverlay(Loop,message);
        }
        private string Message(string reason)
        {
            switch(reason)
            {
                case "UNDO_COMPLETE":return "Gediş geri alındı";
                case "UNDO_NOT_ALLOWED":return "Bu növbəni geri almaq olmaz";
                case "ROLL_TO_START":return "İlk atış: hər oyunçu üçün bir zər";
                case "OPENING_TIE":return "İlkin atış bərabərdir · yenidən atın";
                case "OPENING_WINNER_NEEDS_FRESH_ROLL":return "P"+Loop.Session.Player+" başlayır · təzə iki zər atın";
                case "SELECT_CHECKER":return "Öz daşınıza, sonra bal rəngli hədəfə toxunun";
                case "CHOOSE_MARKED_TARGET":return "Bal rəngli hədəfə toxunun";
                case "MOVE_ACCEPTED":return "Gediş edildi · qalan hüquqları oynayın";
                case "TURN_COMPLETE":return "Növbə tamamlandı · P"+Loop.Session.Player+" zər atsın";
                case "NO_LEGAL_MOVES_TURN_PASSED":return "Qanuni gediş yoxdur · növbə P"+Loop.Session.Player;
                case "GAME_FINISHED":return "Partiya bitdi · qalib P"+Loop.Session.Result?.Winner;
                case "ROLL_FIRST":return "Əvvəlcə zər atın";
                case "WRONG_PLAYER":return "Hazırda sizin növbəniz deyil";
                case "ROLL_NOT_ALLOWED":return "Hazırda zər atmaq olmaz";
                case "INVALID_DICE":case "DICE_SOURCE_FAILED":return "Zər alınmadı · yenidən cəhd edin";
                default:return "Bu seçim və ya gediş mümkün deyil";
            }
        }
    }
}
