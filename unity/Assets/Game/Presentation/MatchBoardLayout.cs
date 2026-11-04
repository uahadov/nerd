using System;
using UnityEngine;
namespace Nard.Presentation
{
    /// <summary>Pixel-space presentation geometry. No dice, turn or movement rules.</summary>
    public sealed class MatchBoardLayout
    {
        public int Width {get;} public int Height {get;}
        public bool Portrait {get;}
        public Rect Safe {get;} public Rect Board {get;} public Rect Rail {get;} public Rect Header {get;}
        public Rect Undo {get;} public Rect Roll {get;} public Rect Status {get;} public Rect Dice {get;}
        public float Unit {get;} public float BoardScale=> (Portrait?Board.height:Board.width)/16f;
        public MatchBoardLayout(int width,int height,Rect safe)
        {
            if(width<=0||height<=0)throw new ArgumentOutOfRangeException(nameof(width));
            Width=width;Height=height;Portrait=height>width;
            float x=Mathf.Clamp(safe.xMin,0,width),y=Mathf.Clamp(safe.yMin,0,height);
            float right=Mathf.Clamp(safe.xMax,x,width),top=Mathf.Clamp(safe.yMax,y,height);
            Safe=right>x&&top>y?Rect.MinMaxRect(x,y,right,top):new Rect(0,0,width,height);
            Unit=Portrait?Safe.width/390f:Mathf.Min(Safe.height/390f,Safe.width/700f);
            float m=8*Unit;
            if(Portrait)
            {
                float h=Mathf.Min((Safe.width-2*m)/.75f,(Safe.height-2*m)/(1+160*.75f/390f));
                h=Mathf.Max(h,Safe.height*.25f);float w=h*.75f;
                float free=Safe.height-h-2*m;
                float topH=Mathf.Min(90*Unit,free*.35f),bottomH=Mathf.Min(160*Unit,free-topH);
                float bottom=Safe.center.y-(h+topH+bottomH)*.5f;
                Board=new Rect(Safe.center.x-w*.5f,bottom+bottomH,w,h);
                Unit=Board.width/390f;
                Header=new Rect(Board.xMin,Board.yMax,Board.width,topH);
                Rail=new Rect(Board.xMin,bottom,Board.width,bottomH);
                Status=new Rect(Header.xMin+8*Unit,Header.yMin,Header.width-16*Unit,Header.height);
                Undo=new Rect(Rail.xMin+10*Unit,Rail.center.y-22*Unit,84*Unit,44*Unit);
                Roll=new Rect(Rail.xMax-94*Unit,Rail.center.y-22*Unit,84*Unit,44*Unit);
                Dice=new Rect(Rail.center.x-46*Unit,Rail.center.y-22*Unit,92*Unit,44*Unit);
            }
            else
            {
                float h=Mathf.Min(Safe.height-2*m,(Safe.width-3*m-120*Unit)*.75f);h=Mathf.Max(h,Safe.height*.25f);
                Board=new Rect(Safe.xMin+m,Safe.center.y-h*.5f,h/ .75f,h);
                Rail=new Rect(Board.xMax+3*Unit,Board.yMin,Safe.xMax-m-Board.xMax-3*Unit,h);
                Header=Rail;
                Status=new Rect(Rail.xMin+10*Unit,Rail.yMax-108*Unit,Rail.width-20*Unit,78*Unit);
                Dice=new Rect(Rail.center.x-46*Unit,Rail.center.y-12*Unit,92*Unit,44*Unit);
                Undo=new Rect(Rail.xMin+12*Unit,Rail.yMin+24*Unit,Rail.width-24*Unit,44*Unit);
                Roll=new Rect(Undo.xMin,Undo.yMax+12*Unit,Undo.width,44*Unit);
            }
            // Dice rest in the empty middle of the board, not in a separate app panel.
            var diceCenter=WorldToPixel(FromBoard(new Vector2(4,0)));
            Dice=new Rect(diceCenter.x-.9f*BoardScale,diceCenter.y-.425f*BoardScale,1.8f*BoardScale,.85f*BoardScale);
        }
        public Vector2 PixelToWorld(Vector2 p)=>(p-new Vector2(Width,Height)*.5f)/100f;
        public Vector2 WorldToPixel(Vector2 p)=>p*100f+new Vector2(Width,Height)*.5f;
        public Vector2 FromBoard(Vector2 p)
        {if(Portrait)p=new Vector2(-p.y,p.x);return PixelToWorld(Board.center+p*BoardScale);}
        public Vector2 IntoBoard(Vector2 world)
        {var p=(WorldToPixel(world)-Board.center)/BoardScale;return Portrait?new Vector2(p.y,-p.x):p;}
        public Rect OffPocket(int player)
        {var center=WorldToPixel(FromBoard(new Vector2(7.27f,player==0?-3.7f:3.7f)));float size=.86f*BoardScale;return new Rect(center.x-size*.5f,center.y-size*.5f,size,size);}
        public Vector2 CanonicalToWorld(Vector2 p)
        {
            if(p.y>9.1f)return PixelToWorld(Undo.center);
            if(p.y< -9.05f)return PixelToWorld(Roll.center);
            if(p.x>=4.22f&&p.x<=4.84f)return PixelToWorld(OffPocket(p.y<0?0:1).center);
            return FromBoard(ReferenceBoardSkin.ToLocal(p));
        }
        public Vector2 WorldToCanonical(Vector2 world)
        {
            var pixel=WorldToPixel(world);
            if(Undo.Contains(pixel))return new Vector2(0,9.45f);
            if(Roll.Contains(pixel)||Dice.Contains(pixel))return new Vector2(0,-9.35f);
            for(int player=0;player<2;player++)if(OffPocket(player).Contains(pixel))return new Vector2(4.53f,player==0?-4:4);
            if(!Board.Contains(pixel))return new Vector2(100,100);
            return ReferenceBoardSkin.ToBoard(IntoBoard(world));
        }
    }
}
