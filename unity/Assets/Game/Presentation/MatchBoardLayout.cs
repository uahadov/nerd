using System;
using UnityEngine;
namespace Nard.Presentation
{
    /// <summary>Presentation-only geometry; canonical point IDs never change with orientation.</summary>
    public sealed class MatchBoardLayout
    {
        public int Width {get;} public int Height {get;}
        public bool Portrait {get;}
        public bool CompactPortrait=>Portrait&&Safe.height/Safe.width<1.85f;
        public Rect Safe {get;} public Rect Board {get;} public Rect Rail {get;} public Rect Header {get;}
        public Rect Undo {get;} public Rect Roll {get;} public Rect Status {get;} public Rect Dice {get;}
        public float Unit {get;} public float BoardScale=>Board.width/(Portrait?10f:16f);
        public float CheckerDiameter=>BoardScale*(Portrait?1.24f:.92f);
        public MatchBoardLayout(int width,int height,Rect safe)
        {
            if(width<=0||height<=0)throw new ArgumentOutOfRangeException(nameof(width));
            Width=width;Height=height;Portrait=height>width;
            float x=Mathf.Clamp(safe.xMin,0,width),y=Mathf.Clamp(safe.yMin,0,height);
            float right=Mathf.Clamp(safe.xMax,x,width),top=Mathf.Clamp(safe.yMax,y,height);
            Safe=right>x&&top>y?Rect.MinMaxRect(x,y,right,top):new Rect(0,0,width,height);
            if(Portrait)
            {
                float ratio=CompactPortrait?1.15f:1.43f,reference=CompactPortrait?360:390;
                float header=CompactPortrait?62:70,footer=CompactPortrait?128:152;
                float w=Mathf.Min(Safe.width*.97f,Safe.height/(ratio+(header+footer+8)/reference));
                Unit=w/reference;float h=w*ratio;
                float bottom=Safe.center.y-(h+(header+footer)*Unit)*.5f;
                Rail=new Rect(Safe.center.x-w/2,bottom,w,footer*Unit);
                Board=new Rect(Rail.xMin,Rail.yMax,w,h);
                Header=new Rect(Board.xMin,Board.yMax,w,header*Unit);Status=Header;
                float button=(w-36*Unit)/2;
                Undo=new Rect(Rail.xMin+12*Unit,Rail.yMin+12*Unit,button,48*Unit);
                Roll=new Rect(Rail.xMax-12*Unit-button,Rail.yMin+12*Unit,button,48*Unit);
                Dice=new Rect(Rail.xMin+28*Unit,Rail.yMax-50*Unit,92*Unit,40*Unit);
            }
            else
            {
                Unit=Mathf.Min(Safe.height/390,Safe.width/760);float m=7*Unit;
                float h=Mathf.Min(Safe.height-2*m,(Safe.width-3*m-150*Unit)*.75f);
                Board=new Rect(Safe.xMin+m,Safe.center.y-h/2,h/.75f,h);
                Rail=new Rect(Board.xMax+5*Unit,Board.yMin,Safe.xMax-m-Board.xMax-5*Unit,h);
                Header=Rail;Status=new Rect(Rail.xMin+8*Unit,Rail.yMax-182*Unit,Rail.width-16*Unit,166*Unit);
                Undo=new Rect(Rail.xMin+10*Unit,Rail.yMin+16*Unit,Rail.width-20*Unit,44*Unit);
                Roll=new Rect(Undo.xMin,Undo.yMax+10*Unit,Undo.width,44*Unit);
                Dice=Centered(WorldToPixel(FromBoard(new Vector2(4,0))),1.9f*BoardScale,.9f*BoardScale);
            }
        }
        private static Rect Centered(Vector2 p,float w,float h)=>new Rect(p.x-w/2,p.y-h/2,w,h);
        public Vector2 PixelToWorld(Vector2 p)=>(p-new Vector2(Width,Height)*.5f)/100;
        public Vector2 WorldToPixel(Vector2 p)=>p*100+new Vector2(Width,Height)*.5f;
        public Vector2 FromBoard(Vector2 p)=>PixelToWorld(Board.center+p*BoardScale);
        public Vector2 IntoBoard(Vector2 p)=>(WorldToPixel(p)-Board.center)/BoardScale;
        // Upper tray = original left half; lower tray = original right half. All points stay upright.
        public Rect Tray(int half)
        {float h=(Board.height-Board.width*.05f)/2;return new Rect(Board.xMin,half==0?Board.yMax-h:Board.yMin,Board.width,h);}
        public Rect BarPocket(int player)
        {
            if(!Portrait)return Centered(WorldToPixel(FromBoard(new Vector2(0,player==0?-3.3f:3.3f))),.72f*BoardScale,4*BoardScale);
            return Centered(Board.center+Vector2.right*(player==0?-.32f:.32f)*Board.width,Board.width*.20f,Board.width*.11f);
        }
        public Rect OffPocket(int player)
        {if(Portrait){var tray=Tray(player==0?1:0);return Centered(new Vector2(tray.xMax-Board.width*.035f,tray.center.y),Board.width*.052f,Board.width*.13f);}return Centered(WorldToPixel(FromBoard(new Vector2(7.27f,player==0?-3.7f:3.7f))),.86f*BoardScale,.86f*BoardScale);}
        public Rect PointTouchRect(int point)
        {
            if(point<0||point>23)throw new ArgumentOutOfRangeException(nameof(point));
            if(Portrait)
            {
                bool left=point>=6&&point<18,top=point>=12;var tray=Tray(left?0:1);
                int col=point<12?(11-point)%6:(point-12)%6;
                float lane=tray.width*.86f/6,edge=tray.width*.07f;
                float inset=(left&&!top)||(!left&&top)?.04f:.08f,row=CompactPortrait?.20f:.22f;
                return new Rect(tray.xMin+edge+col*lane+.01f,top?tray.yMax-tray.width*(inset+row):tray.yMin+tray.width*inset,lane-.02f,tray.width*row);
            }
            var p=ReferenceBoardSkin.ToLocal(BoardGeometry.PointBase(point));
            return Centered(WorldToPixel(FromBoard(new Vector2(p.x,point<12?-3.1f:3.1f))),.98f*BoardScale,4f*BoardScale);
        }
        public Vector2 CheckerPixel(int point,int player,int ordinal,int count)
        {
            if(point==24)return OffPocket(player).center;
            var cell=point==-1?BarPocket(player):PointTouchRect(point);
            if(point==-1&&Portrait)return cell.center+Vector2.right*(ordinal-(count-1)*.5f)*Mathf.Min(CheckerDiameter*.36f,cell.width/Mathf.Max(1,count));
            // On a dense portrait head, show five spaced layers plus the truthful count badge.
            // Every renderer still belongs to its original point; input selects the whole lane.
            if(Portrait&&point>=0&&count>5){ordinal=Mathf.Max(0,ordinal-(count-5));count=5;}
            bool top=point==-1?player==1:point>=12;
            float d=CheckerDiameter,spacing=Mathf.Min(d*.88f,Mathf.Max(0,cell.height-d*.9f)/Mathf.Max(1,count-1));
            return new Vector2(cell.center.x,(top?cell.yMax-d*.5f:cell.yMin+d*.5f)+(top?-1:1)*spacing*ordinal);
        }
        public Vector2 TargetPixel(int point,int player)
        {if(point==24)return OffPocket(player).center;var cell=PointTouchRect(point);return new Vector2(cell.center.x,point<12?cell.yMax-CheckerDiameter*.24f:cell.yMin+CheckerDiameter*.24f);}
        public Vector2 CanonicalToWorld(Vector2 p)
        {
            if(p.y>9.1f)return PixelToWorld(Undo.center);
            if(p.y< -9.05f)return PixelToWorld(Roll.center);
            if(p.x>=4.22f&&p.x<=4.84f)return PixelToWorld(OffPocket(p.y<0?0:1).center);
            if(!Portrait)return FromBoard(ReferenceBoardSkin.ToLocal(p));
            if(Mathf.Abs(p.x+.25f)<.3f)return PixelToWorld(BarPocket(p.y<0?0:1).center);
            int index=BoardHitMap.PointAt(p,0);if(index<0)return new Vector2(100,100);
            return PixelToWorld(PointTouchRect(index).center);
        }
        public Vector2 WorldToCanonical(Vector2 world)
        {
            var pixel=WorldToPixel(world);
            if(Undo.Contains(pixel))return new Vector2(0,9.45f);
            if(Roll.Contains(pixel)||Dice.Contains(pixel))return new Vector2(0,-9.35f);
            for(int player=0;player<2;player++)
            {if(OffPocket(player).Contains(pixel))return new Vector2(4.53f,player==0?-4:4);if(BarPocket(player).Contains(pixel))return new Vector2(-.25f,player==0?-4:4);}
            for(int i=0;i<24;i++)if(PointTouchRect(i).Contains(pixel)){var p=BoardGeometry.PointBase(i);p.y=i<12?-4:4;return p;}
            return new Vector2(100,100);
        }
    }
}
