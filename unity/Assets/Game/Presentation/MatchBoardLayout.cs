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
        // All portrait lengths are multiples of one six-column pitch; input remains a whole lane.
        public float PortraitLaneWidth=>Board.width*.86f/6;
        public float CheckerDiameter=>Portrait?PortraitLaneWidth*.80f:BoardScale*.92f;
        public float CheckerFootprint(int point)=>Portrait?PortraitLaneWidth*(point==-1?.54f:point==24?.26f:.80f):CheckerDiameter*(point==24?.54f:1)/.84f;
        public Rect PortraitBar=>Centered(Board.center,Board.width,PortraitLaneWidth*.36f);
        public MatchBoardLayout(int width,int height,Rect safe)
        {
            if(width<=0||height<=0)throw new ArgumentOutOfRangeException(nameof(width));
            Width=width;Height=height;Portrait=height>width;
            float x=Mathf.Clamp(safe.xMin,0,width),y=Mathf.Clamp(safe.yMin,0,height);
            float right=Mathf.Clamp(safe.xMax,x,width),top=Mathf.Clamp(safe.yMax,y,height);
            Safe=right>x&&top>y?Rect.MinMaxRect(x,y,right,top):new Rect(0,0,width,height);
            if(Portrait)
            {
                const float reference=390,header=48,footer=96;
                // Minimum: bar + two fields, each with outer/inner clearance, two 1.65-pitch rows and a central gap.
                const float minBoardRatio=(.36f+2*(.70f+.16f+2*1.65f+.46f))*.86f/6;
                float w=Mathf.Min(Safe.width*.97f,Safe.height/(minBoardRatio+(header+footer+8)/reference));
                Unit=w/reference;float lane=w*.86f/6;
                float h=Mathf.Min(Safe.height-(header+footer+8)*Unit,11.6f*lane);
                float bottom=Safe.center.y-(h+(header+footer)*Unit)*.5f;
                Rail=new Rect(Safe.center.x-w/2,bottom,w,footer*Unit);
                Board=new Rect(Rail.xMin,Rail.yMax,w,h);
                Header=new Rect(Board.xMin,Board.yMax,w,header*Unit);Status=Header;
                float button=(w-36*Unit)/2;
                Undo=new Rect(Rail.xMin+12*Unit,Rail.yMin+6*Unit,button,44*Unit);
                Roll=new Rect(Rail.xMax-12*Unit-button,Rail.yMin+6*Unit,button,44*Unit);
                Dice=new Rect(Rail.xMin+22*Unit,Rail.yMax-36*Unit,70*Unit,30*Unit);
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
        {float h=(Board.height-(Portrait?PortraitBar.height:Board.width*.05f))/2;return new Rect(Board.xMin,half==0?Board.yMax-h:Board.yMin,Board.width,h);}
        public Rect BarPocket(int player)
        {
            if(!Portrait)return Centered(WorldToPixel(FromBoard(new Vector2(0,player==0?-3.3f:3.3f))),.72f*BoardScale,4*BoardScale);
            return Centered(Board.center+Vector2.right*(player==0?-2:2)*PortraitLaneWidth,PortraitLaneWidth*1.60f,PortraitLaneWidth*.64f);
        }
        public Rect OffPocket(int player)
        {if(Portrait){var tray=Tray(player==0?1:0);return Centered(new Vector2(tray.xMax-Board.width*.035f,tray.center.y),PortraitLaneWidth*.38f,PortraitLaneWidth*1.20f);}return Centered(WorldToPixel(FromBoard(new Vector2(7.27f,player==0?-3.7f:3.7f))),.86f*BoardScale,.86f*BoardScale);}
        public Rect PortraitField(int half)
        {
            var tray=Tray(half);float lane=PortraitLaneWidth;
            return Rect.MinMaxRect(Board.xMin+Board.width*.07f,tray.yMin+(half==0?.16f:.70f)*lane,Board.xMax-Board.width*.07f,tray.yMax-(half==0?.70f:.16f)*lane);
        }
        public Rect PointVisualRect(int point)
        {
            if(point<0||point>23)throw new ArgumentOutOfRangeException(nameof(point));
            if(!Portrait)return PointTouchRect(point);
            var field=PortraitField(point>=6&&point<18?0:1);float lane=PortraitLaneWidth,row=(field.height-.46f*lane)/2;
            int col=point<12?(11-point)%6:(point-12)%6;
            return new Rect(field.xMin+(col+.03f)*lane,point>=12?field.yMax-row:field.yMin,.94f*lane,row);
        }
        public Rect PointTouchRect(int point)
        {
            if(point<0||point>23)throw new ArgumentOutOfRangeException(nameof(point));
            if(Portrait)
            {
                var visual=PointVisualRect(point);float lane=PortraitLaneWidth;
                return new Rect(visual.center.x-lane*.5f+.01f,visual.yMin-(point>=12?.17f*lane:0),lane-.02f,visual.height+.17f*lane);
            }
            var p=ReferenceBoardSkin.ToLocal(BoardGeometry.PointBase(point));
            return Centered(WorldToPixel(FromBoard(new Vector2(p.x,point<12?-3.1f:3.1f))),.98f*BoardScale,4f*BoardScale);
        }
        public Vector2 CheckerPixel(int point,int player,int ordinal,int count)
        {
            if(Portrait)
            {
                Rect cell=point==-1?BarPocket(player):point==24?OffPocket(player):PointVisualRect(point);
                float lane=PortraitLaneWidth,footprint=CheckerFootprint(point),margin=lane*.06f;
                if(point==-1)
                {
                    float step=Mathf.Min(footprint,(cell.width-footprint-2*margin)/Mathf.Max(1,count-1));
                    return cell.center+Vector2.right*(ordinal-(count-1)*.5f)*step;
                }
                bool top=point==24?player==1:point>=12;
                float spacing=Mathf.Min(footprint*.84f+lane*.05f,(cell.height-2*margin-footprint)/Mathf.Max(1,count-1));
                return new Vector2(cell.center.x,(top?cell.yMax-footprint/2-margin:cell.yMin+footprint/2+margin)+(top?-1:1)*spacing*ordinal);
            }
            if(point==24)return OffPocket(player).center;
            var legacy=point==-1?BarPocket(player):PointTouchRect(point);bool upper=point==-1?player==1:point>=12;
            float d=CheckerDiameter,oldSpacing=Mathf.Min(d*.88f,Mathf.Max(0,legacy.height-d*.9f)/Mathf.Max(1,count-1));
            return new Vector2(legacy.center.x,(upper?legacy.yMax-d*.5f:legacy.yMin+d*.5f)+(upper?-1:1)*oldSpacing*ordinal);
        }
        public Vector2 TargetPixel(int point,int player)
        {
            if(point==24)return OffPocket(player).center;
            var cell=PointTouchRect(point);
            if(Portrait)return new Vector2(cell.center.x,point<12?cell.yMax-PortraitLaneWidth*.10f:cell.yMin+PortraitLaneWidth*.10f);
            return new Vector2(cell.center.x,point<12?cell.yMax-CheckerDiameter*.24f:cell.yMin+CheckerDiameter*.24f);
        }
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
