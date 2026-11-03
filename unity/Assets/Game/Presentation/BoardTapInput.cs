using Nard.Interaction;
using Nard.Match;
using Nard.Rules.Long;
using Nard.Rules.Short;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nard.Presentation
{
    [DisallowMultipleComponent,RequireComponent(typeof(BoardView))]
    public sealed class BoardTapInput : MonoBehaviour
    {
        private BoardView view;
        private long lastPress=-1;
        public TapSelection Selection { get; private set; }
        private void Start() { view=GetComponent<BoardView>();RestartExample(); }
        public void RestartExample()
        {
            // Fixed test input only: no random roll or automatic turn loop in P02.2.
            var position=LongNardeRules.SupplyOpeningRoll(LongNardeRules.ResolveOpening(LongNardeRules.Initial(),6,1),3,3);
            Bind(new TapSelection(position,0));
        }
        public void Bind(TapSelection selection)
        {
            if(selection==null)throw new System.ArgumentNullException(nameof(selection));
            view=GetComponent<BoardView>();Selection=selection;Render();
        }
        private void Update()
        {
            var touch=Touchscreen.current;
            if(touch!=null&&touch.primaryTouch.press.wasPressedThisFrame)
            {
                int active=0;foreach(var finger in touch.touches)if(finger.press.isPressed)active++;
                if(active>1){view.SetTapOverlay(Selection.Selected,Selection.Targets,"Bir barmaqla toxunun",Selection.LocalPlayer);lastPress=Time.frameCount;return;}
                ProcessPress(touch.primaryTouch.position.ReadValue(),Time.frameCount);
            }
            else if(Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)
                ProcessPress(Mouse.current.position.ReadValue(),Time.frameCount);
        }
        public bool ProcessPress(Vector2 screen,long pressId)
        {
            if(Selection==null||pressId<=lastPress)return false;
            lastPress=pressId;
            var camera=Camera.main;if(camera==null)return false;
            var world=camera.ScreenToWorldPoint(new Vector3(screen.x,screen.y,-camera.transform.position.z));
            var local=(Vector2)view.transform.InverseTransformPoint(world);
            if(BoardHitMap.IsExampleReset(local)){RestartExample();return true;}
            int point=BoardHitMap.PointAt(local,Selection.LocalPlayer);
            var outcome=Selection.Tap(point);Render();return outcome!=TapOutcome.Rejected;
        }
        private void Render()
        {
            view.Display(Selection.Mode==GameMode.Long?BoardSnapshot.From(Selection.LongPosition):BoardSnapshot.From(Selection.ShortPosition));
            var roll=Selection.Mode==GameMode.Long?Selection.LongPosition.CopyRoll():Selection.ShortPosition.CopyRoll();
            string notice=roll.Length==2?"Sabit "+roll[0]+"–"+roll[1]+" nümunəsi · P"+Selection.LocalPlayer+" · Zər atışı yoxdur":"Texniki nümunə · Zər atışı yoxdur";
            view.SetTapOverlay(Selection.Selected,Selection.Targets,Message(Selection.Reason),Selection.LocalPlayer,notice);
        }
        private static string Message(string reason)
        {
            switch(reason)
            {
                case "SELECT_CHECKER": return "Öz daşınıza, sonra işarəli xanaya toxunun";
                case "CHOOSE_MARKED_TARGET":return "Bal rəngli hədəfə toxunun";
                case "MOVE_ACCEPTED":return "Gediş edildi · başqa daş seçin";
                case "EXAMPLE_COMPLETE":return "Nümunə bitdi · yeni növbə hələ yoxdur";
                case "NOT_YOUR_MOVING_TURN":return "Hazırda sizin gediş növbəniz deyil";
                default:return "Bu seçim və ya gediş mümkün deyil";
            }
        }
    }
}
