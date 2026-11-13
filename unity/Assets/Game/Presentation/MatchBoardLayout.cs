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
        public Rect Sound {get;}
        public float Unit {get;} public float BoardScale=>(Portrait?Board.height:Board.width)/16;
        // All portrait lengths are multiples of one six-column pitch; input remains a whole lane.
        public float PortraitLaneWidth=>.9854f*BoardScale;
        public float CheckerDiameter=>Portrait?PortraitLaneWidth*.84f:BoardScale*.92f;
        public float CheckerFootprint(int point)=>Portrait?PortraitLaneWidth*(point==-1?.64f:point==24?.52f:.84f):CheckerDiameter*(point==24?.54f:1)/.84f;
        public Rect PortraitBar=>Centered(Board.center,Board.width*.87f,BoardScale*.80f);
        public MatchBoardLayout(int width,int height,Rect safe)
        {
            if(width<=0||height<=0)throw new ArgumentOutOfRangeException(nameof(width));
            Width=width;Height=height;Portrait=height>width;
            float x=Mathf.Clamp(safe.xMin,0,width),y=Mathf.Clamp(safe.yMin,0,height);
            float right=Mathf.Clamp(safe.xMax,x,width),top=Mathf.Clamp(safe.yMax,y,height);
            Safe=right>x&&top>y?Rect.MinMaxRect(x,y,right,top):new Rect(0,0,width,height);
            if(Portrait)
            {
                const float reference=390,header=46,footer=104;
                float w=Mathf.Min(Safe.width*.97f,Safe.height/(4f/3+(header+footer+20)/reference));
                Unit=w/reference;float h=w*4/3;
                Rail=new Rect(Safe.center.x-w/2,Safe.yMin+6*Unit,w,footer*Unit);
                Header=new Rect(Rail.xMin,Safe.yMax-header*Unit-6*Unit,w,header*Unit);Status=Header;
                float boardCenterY=(Header.yMin+Rail.yMax)*.5f;
                Board=new Rect(Rail.xMin,boardCenterY-h*.5f,w,h);
                float button=(w-32*Unit)/2;
                Undo=new Rect(Rail.xMin+10*Unit,Rail.yMin+8*Unit,button,46*Unit);
                Roll=new Rect(Rail.xMax-10*Unit-button,Rail.yMin+8*Unit,button,46*Unit);
                Dice=Centered(Board.center,BoardScale*2.2f,BoardScale*.90f);
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
            Sound=Portrait?new Rect(Header.xMax-44*Unit,Header.center.y-18*Unit,40*Unit,36*Unit):new Rect(Rail.xMax-46*Unit,Rail.yMax-40*Unit,40*Unit,36*Unit);
        }
        private static Rect Centered(Vector2 p,float w,float h)=>new Rect(p.x-w/2,p.y-h/2,w,h);
        public Vector2 PixelToWorld(Vector2 p)=>(p-new Vector2(Width,Height)*.5f)/100;
        public Vector2 WorldToPixel(Vector2 p)=>p*100+new Vector2(Width,Height)*.5f;
        public Vector2 FromBoard(Vector2 p)=>PixelToWorld(Board.center+(Portrait?new Vector2(p.y,-p.x):p)*BoardScale);
        public Vector2 IntoBoard(Vector2 p){var q=(WorldToPixel(p)-Board.center)/BoardScale;return Portrait?new Vector2(-q.y,q.x):q;}
        // Upper tray = original left half; lower tray = original right half. All points stay upright.
        public Rect Tray(int half)
        {float h=(Board.height-(Portrait?PortraitBar.height:Board.width*.05f))/2;return new Rect(Board.xMin,half==0?Board.yMax-h:Board.yMin,Board.width,h);}
        public Rect BarPocket(int player)
        {
            if(!Portrait)return Centered(WorldToPixel(FromBoard(new Vector2(0,player==0?-3.3f:3.3f))),.72f*BoardScale,4*BoardScale);
            return Centered(WorldToPixel(FromBoard(new Vector2(0,player==0?-3.3f:3.3f))),2.6f*BoardScale,.78f*BoardScale);
        }
        public Rect OffPocket(int player)
        {return Centered(WorldToPixel(FromBoard(new Vector2(7.27f,player==0?-3.7f:3.7f))),.86f*BoardScale,.86f*BoardScale);}
        public Rect PortraitField(int half)
        {var tray=Tray(half);return new Rect(tray.xMin+BoardScale*.85f,tray.yMin+BoardScale*.72f,tray.width-BoardScale*1.7f,tray.height-BoardScale*1.44f);}
        public Rect PointVisualRect(int point)
        {
            if(point<0||point>23)throw new ArgumentOutOfRangeException(nameof(point));
            if(!Portrait)return PointTouchRect(point);
            float x=ReferenceBoardSkin.ToLocal(BoardGeometry.PointBase(point)).x,pitch=PortraitLaneWidth/BoardScale;
            var a=WorldToPixel(FromBoard(new Vector2(x-pitch*.47f,point>=12?.98f:-5.15f)));
            var b=WorldToPixel(FromBoard(new Vector2(x+pitch*.47f,point>=12?5.15f:-.98f)));
            return Rect.MinMaxRect(Mathf.Min(a.x,b.x),Mathf.Min(a.y,b.y),Mathf.Max(a.x,b.x),Mathf.Max(a.y,b.y));
        }
        public Rect PointTouchRect(int point)
        {
            if(point<0||point>23)throw new ArgumentOutOfRangeException(nameof(point));
            if(Portrait)
            {
                var visual=PointVisualRect(point);float lane=PortraitLaneWidth;
                return new Rect(visual.xMin-(point>=12?.17f*lane:0),visual.center.y-lane*.5f+.01f,visual.width+.17f*lane,lane-.02f);
            }
            var p=ReferenceBoardSkin.ToLocal(BoardGeometry.PointBase(point));
            return Centered(WorldToPixel(FromBoard(new Vector2(p.x,point<12?-3.1f:3.1f))),.98f*BoardScale,4f*BoardScale);
        }
        public Vector2 CheckerPixel(int point,int player,int ordinal,int count)
        {
            if(Portrait)
            {
                Rect cell=point==-1?BarPocket(player):point==24?OffPocket(player):PointVisualRect(point);
                float lane=PortraitLaneWidth,footprint=CheckerFootprint(point),margin=lane*.05f;
                int rowCount=point>=0&&point<24&&count>5?5:count;
                int layer=point>=0&&point<24&&count>5?ordinal/5:0;
                int inLayer=point>=0&&point<24&&count>5?ordinal%5:ordinal;
                float spacing=Mathf.Min(footprint*.88f+lane*.05f,(cell.width-2*margin-footprint)/Mathf.Max(1,rowCount-1));
                bool right=point==-1?player==1:point==24?player==1:point>=12;
                float maxShift=Mathf.Max(0,(cell.height-footprint)*.42f);
                float yOffset=point>=0&&point<24&&count>5?(layer-1)*Mathf.Min(1.6f*Unit,maxShift):0;
                return new Vector2((right?cell.xMax-footprint/2-margin:cell.xMin+footprint/2+margin)+(right?-1:1)*spacing*inLayer,cell.center.y+yOffset);
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
            if(Portrait)return new Vector2(point<12?cell.xMax-PortraitLaneWidth*.10f:cell.xMin+PortraitLaneWidth*.10f,cell.center.y);
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
