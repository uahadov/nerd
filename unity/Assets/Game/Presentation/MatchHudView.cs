using System;
using System.Collections.Generic;
using System.Linq;
using Nard.Interaction;
using Nard.Match;
using UnityEngine;
namespace Nard.Presentation
{
    /// <summary>Board-focused mobile HUD. All values and legal hints come from the existing application.</summary>
    public sealed class MatchHudView : IDisposable
    {
        private readonly ReferenceBoardSkin skin;
        private readonly Sprite rounded,circle,grain,diceFace;
        private readonly List<UnityEngine.Object> owned=new List<UnityEngine.Object>();
        private Transform root;private MatchBoardLayout layout;
        private static readonly Color Cream=new Color32(246,235,213,255),Wood=new Color32(103,64,40,255),Honey=new Color32(224,176,100,255),Ink=new Color32(78,48,31,255);
        public MatchHudView(ReferenceBoardSkin skin)
        {
            this.skin=skin;
            rounded=Sprite(96,96,(x,y)=>{float a=Mathf.Max(Mathf.Abs(x-.5f)-.37f,0),b=Mathf.Max(Mathf.Abs(y-.5f)-.37f,0);return a*a+b*b<=.13f*.13f?Color.white:Color.clear;});
            diceFace=Sprite(96,96,(x,y)=>{float a=Mathf.Max(Mathf.Abs(x-.5f)-.46f,0),b=Mathf.Max(Mathf.Abs(y-.5f)-.46f,0);return a*a+b*b<=.04f*.04f?Color.white:Color.clear;});
            circle=Sprite(96,96,(x,y)=>(x-.5f)*(x-.5f)+(y-.5f)*(y-.5f)<.25f?Color.white:Color.clear);
            grain=Sprite(128,64,(x,y)=>{float v=.006f*Mathf.Sin(y*190+x*6)+.004f*Mathf.Sin(y*57+x*11);return new Color(.48f+v,.31f+v,.20f+v);});
        }
        public void Apply(BoardView view,GameObject content,LocalTurnLoop loop,MatchBoardLayout geometry)
        {
            layout=geometry;
            foreach(var r in content.GetComponentsInChildren<Renderer>())r.enabled=false;
            var go=new GameObject("MatchHud");go.transform.SetParent(content.transform,false);root=go.transform;
            Rect full=new Rect(0,0,layout.Width,layout.Height);
            Draw("TableSurface",grain,full,Color.white,-3);

            var board=Draw("ResponsiveBoard",skin.BoardSprite,layout.Board,Color.white,0);
            if(layout.Portrait){board.transform.localRotation=Quaternion.Euler(0,0,90);board.transform.localScale=new Vector3(layout.Board.height/100/skin.BoardSprite.bounds.size.x,layout.Board.width/100/skin.BoardSprite.bounds.size.y,1);}
            // A continuous wooden table surrounds the board; no full-height cream sidebar.
            if(layout.Portrait)Draw("HeaderJoint",rounded,new Rect(layout.Header.xMin+12*layout.Unit,layout.Header.yMin+3*layout.Unit,layout.Header.width-24*layout.Unit,layout.Unit),Honey,2);
            var counts=new Dictionary<int,int>();
            foreach(var c in view.Checkers)
            {
                int key=c.Point*2+c.Player,ordinal=counts.TryGetValue(key,out int n)?n:0;counts[key]=ordinal+1;
                int count=c.Point==-1?view.Snapshot.Bar(c.Player):c.Point==24?view.Snapshot.Off(c.Player):Math.Abs(view.Snapshot.Point(c.Point));
                Vector2 position;
                float diameter=.82f;
                if(c.Point==24){position=layout.PixelToWorld(layout.OffPocket(c.Player).center);diameter=.62f;}
                else
                {
                    float x=c.Point==-1?0:ReferenceBoardSkin.ToLocal(BoardGeometry.PointBase(c.Point)).x;
                    bool top=c.Point==-1?c.Player==1:c.Point>=12;
                    float spacing=Mathf.Min(.82f,3.1f/Mathf.Max(1,count-1));
                    position=layout.FromBoard(new Vector2(x,(top?1:-1)*(4.54f-spacing*ordinal)));
                }
                var r=c.Renderer;r.enabled=true;r.sprite=c.Player==0?skin.IvorySprite:skin.WalnutSprite;r.color=Color.white;
                r.transform.localPosition=position;float size=diameter/ .84f*layout.BoardScale/100f;r.transform.localScale=new Vector3(size/r.sprite.bounds.size.x,size/r.sprite.bounds.size.y,1);r.sortingOrder=10+ordinal;
            }
            foreach(int point in Enumerable.Range(0,24))
            {
                int count=Math.Abs(view.Snapshot.Point(point));if(count<=5)continue;
                var p=layout.WorldToPixel(layout.FromBoard(new Vector2(ReferenceBoardSkin.ToLocal(BoardGeometry.PointBase(point)).x,point<12?-.15f:.15f)));
                Label("StackCount_"+point,count.ToString(),p,10*layout.Unit,Ink,45);
            }
            for(int player=0;player<2;player++)
            {
                var pocket=layout.OffPocket(player);int count=view.Snapshot.Off(player);
                if(count==0&&!loop.Targets.Contains(24))continue;
                Draw("OffPocket_"+player,rounded,pocket,Wood,5);
                Label("OffCount_"+player,count.ToString(),pocket.center,11*layout.Unit,Cream,45);
            }
            var selected=view.Checkers.LastOrDefault(c=>c.Point==loop.Selected&&c.Player==loop.Session.Player);
            if(selected!=null)
            {
                float d=.95f*layout.BoardScale;
                Draw("SelectedChecker",circle,Centered(layout.WorldToPixel(selected.Renderer.transform.localPosition),d,d),Honey,9);
            }
            foreach(int target in loop.Targets)
            {
                var p=target==24?layout.OffPocket(loop.Session.Player).center:layout.WorldToPixel(layout.FromBoard(new Vector2(ReferenceBoardSkin.ToLocal(BoardGeometry.PointBase(target)).x,target<12?-1.65f:1.65f)));
                float d=.38f*layout.BoardScale;Draw("LegalTarget_"+target,circle,Centered(p,d,d),Ink,40);Draw("LegalTargetFace_"+target,circle,Centered(p,d*.65f,d*.65f),Honey,41);
            }
            Status(loop);
            Dice(loop);
            Button("Undo",layout.Undo,"↶  Geri al",loop.CanUndo);
            if(loop.CanRoll)Button("Roll",layout.Roll,"Zər at",true);

        }
        private void Status(LocalTurnLoop loop)
        {
            var rect=layout.Status;float u=layout.Unit;
            string title=loop.Session.Mode==GameMode.Long?"UZUN NƏRD":"QISA NƏRD";
            string state=loop.Session.Player<0?"İlk atış":loop.Session.Phase==MatchPhase.Finished?"Partiya bitdi":loop.Session.Player==0?"Açıq daşlar":"Qoz daşlar";
            float y=rect.center.y;
            if(layout.Portrait)
            {Label("ModeTitle",title,new Vector2(rect.xMin+72*u,y+10*u),12*u,Cream,50);Label("PlayerStatus",state,new Vector2(rect.center.x+42*u,y+10*u),12*u,Cream,50);}
            else {Label("ModeTitle",title,new Vector2(rect.center.x,rect.yMax-10*u),14*u,Cream,50);Label("PlayerStatus",state,new Vector2(rect.center.x+10*u,rect.yMax-42*u),13*u,Cream,50);}
            string feedback=loop.Reason=="UNDO_COMPLETE"?"Gediş geri alındı":loop.Reason=="OPENING_TIE"?"Bərabərdir · yenidən atın":loop.Reason=="ILLEGAL_SELECTION_OR_TARGET"||loop.Reason=="OUTSIDE_BOARD"?"İşarəli xanaya oynayın":"";
            if(feedback.Length>0)Label("MoveFeedback",feedback,new Vector2(rect.center.x,layout.Portrait?y-14*u:rect.yMin+8*u),10*u,Cream,50);
            if(loop.Session.Player>=0)
            {
                Vector2 center=layout.Portrait?new Vector2(rect.center.x-16*u,y+10*u):new Vector2(rect.center.x-42*u,rect.yMax-42*u);
                Draw("PlayerChecker",loop.Session.Player==0?skin.IvorySprite:skin.WalnutSprite,Centered(center,23*u,23*u),Color.white,50);
            }
        }
        private void Dice(LocalTurnLoop loop)
        {
            float u=layout.Unit;var area=layout.Dice;var values=loop.CopyDisplayedRoll();
            for(int die=0;die<2;die++)
            {
                float side=.78f*layout.BoardScale;Vector2 at=new Vector2(area.center.x+(die==0?-.48f:.48f)*layout.BoardScale,area.center.y);
                Draw("DieRim_"+die,diceFace,Centered(at,side,side),Wood,50);Draw("DieFace_"+die,diceFace,Centered(at,side*.90f,side*.90f),Cream,51);
                if(values.Length==0){Label("DieEmpty_"+die,"·",at,side*.5f,Ink,52);continue;}
                int value=values[die];var spots=new List<Vector2>();if(value%2==1)spots.Add(Vector2.zero);
                if(value>=2){spots.Add(new Vector2(-1,1));spots.Add(new Vector2(1,-1));}if(value>=4){spots.Add(new Vector2(1,1));spots.Add(new Vector2(-1,-1));}if(value==6){spots.Add(new Vector2(-1,0));spots.Add(new Vector2(1,0));}
                for(int i=0;i<spots.Count;i++)Draw("MatchPip_"+die+"_"+i,circle,Centered(at+spots[i]*side*.23f,side*.10f,side*.10f),Ink,52);
            }
            var tokens=loop.Tokens;for(int i=0;i<tokens.Count;i++)
            {Vector2 at=loop.CanRoll?new Vector2(area.center.x+(i-(tokens.Count-1)*.5f)*13*u,area.yMin-10*u):layout.Roll.center+Vector2.right*(i-(tokens.Count-1)*.5f)*16*u;Draw("MoveRight_"+i,circle,Centered(at,10*u,10*u),tokens[i].State==TokenState.Available?Honey:new Color32(141,113,80,255),52);if(tokens[i].State!=TokenState.Available)Draw("SpentRight_"+i,rounded,Centered(at,8*u,1.5f*u),Cream,53);}
        }
        private void Button(string name,Rect rect,string label,bool active)
        {
            Draw(name+"Frame",rounded,rect,active?Honey:new Color32(166,137,99,255),49);
            var inner=rect;inner.xMin+=2*layout.Unit;inner.xMax-=2*layout.Unit;inner.yMin+=2*layout.Unit;inner.yMax-=2*layout.Unit;
            Draw(name+"Wood",grain,inner,active?Color.white:new Color(.80f,.77f,.70f,1),50);Label(name+"Label",label,rect.center,12*layout.Unit,Cream,51);
        }
        private SpriteRenderer Draw(string name,Sprite sprite,Rect pixel,Color color,int order)
        {
            var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(root,false);go.transform.localPosition=layout.PixelToWorld(pixel.center);go.transform.localScale=new Vector3(pixel.width/100/sprite.bounds.size.x,pixel.height/100/sprite.bounds.size.y,1);
            var renderer=go.GetComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.color=color;renderer.sortingOrder=order;if(sprite==rounded||sprite==grain||sprite==diceFace){renderer.drawMode=SpriteDrawMode.Sliced;renderer.size=new Vector2(pixel.width/100,pixel.height/100);go.transform.localScale=Vector3.one;}return renderer;
        }
        private void Label(string name,string value,Vector2 pixel,float height,Color color,int order)
        {
            var go=new GameObject(name,typeof(TextMesh));go.transform.SetParent(root,false);go.transform.localPosition=layout.PixelToWorld(pixel);
            var text=go.GetComponent<TextMesh>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=64;text.characterSize=height/100/6.4f;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.text=value;text.color=QualitySettings.activeColorSpace==ColorSpace.Linear?color.linear:color;
            var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=text.font.material;renderer.sortingOrder=order;
        }
        private static Rect Centered(Vector2 p,float w,float h)=>new Rect(p.x-w*.5f,p.y-h*.5f,w,h);
        private Sprite Sprite(int width,int height,Func<float,float,Color> color)
        {
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Bilinear;var pixels=new Color[width*height];
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)pixels[y*width+x]=color((x+.5f)/width,(y+.5f)/height);texture.SetPixels(pixels);texture.Apply();owned.Add(texture);var sprite=UnityEngine.Sprite.Create(texture,new Rect(0,0,width,height),new Vector2(.5f,.5f),width,0,SpriteMeshType.FullRect,new Vector4(12,12,12,12));owned.Add(sprite);return sprite;
        }
        public void Dispose(){foreach(var item in owned){if(Application.isPlaying)UnityEngine.Object.Destroy(item);else UnityEngine.Object.DestroyImmediate(item);}}
    }
}
