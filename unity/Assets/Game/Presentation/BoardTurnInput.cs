using Nard.Interaction;
using Nard.Match;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nard.Presentation
{
    [DisallowMultipleComponent,RequireComponent(typeof(BoardView))]
    public sealed class BoardTurnInput : MonoBehaviour
    {
        [SerializeField] private GameMode initialMode=GameMode.Long;
        private BoardView view;
        private long lastPress=-1;
        public LocalTurnLoop Loop { get; private set; }
        private void Start()=>Bind(new LocalTurnLoop(MatchSession.Create("local-hotseat",initialMode,RuleContract.VersionFor(initialMode)),new LocalDiceSource()));
        public void Bind(LocalTurnLoop loop)
        {Loop=loop??throw new System.ArgumentNullException(nameof(loop));view=GetComponent<BoardView>();Render();}
        private void Update()
        {
            var touch=Touchscreen.current;
            if(touch!=null&&touch.primaryTouch.press.wasPressedThisFrame)
            {
                int active=0;foreach(var finger in touch.touches)if(finger.press.isPressed)active++;
                if(active>1){lastPress=Time.frameCount;view.SetTurnOverlay(Loop,"Bir barmaqla toxunun");return;}
                ProcessPress(touch.primaryTouch.position.ReadValue(),Time.frameCount);
            }
            else if(Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)ProcessPress(Mouse.current.position.ReadValue(),Time.frameCount);
        }
        public bool ProcessPress(Vector2 screen,long pressId)
        {
            if(Loop==null||pressId<=lastPress)return false;
            lastPress=pressId;var camera=Camera.main;if(camera==null)return false;
            var world=camera.ScreenToWorldPoint(new Vector3(screen.x,screen.y,-camera.transform.position.z));
            var local=(Vector2)view.transform.InverseTransformPoint(world);
            bool accepted=BoardHitMap.IsRollButton(local)?Loop.TryRoll(Loop.Session.Player):
                Loop.Tap(Loop.Session.Player,BoardHitMap.PointAt(local,Loop.Session.Player))!=TapOutcome.Rejected;
            Render();return accepted;
        }
        public void Render()
        {
            view.Display(Loop.Session.Mode==GameMode.Long?BoardSnapshot.From(Loop.LongPosition):BoardSnapshot.From(Loop.ShortPosition));
            view.SetTurnOverlay(Loop,Message(Loop.Reason));
        }
        private string Message(string reason)
        {
            switch(reason)
            {
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
