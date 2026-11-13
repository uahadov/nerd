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
            grain=Sprite(256,256,(x,y)=>{float thread=.004f*Mathf.Sin(x*1608)+.004f*Mathf.Sin(y*1608);float light=.018f*(1-y)-.012f*((x-.5f)*(x-.5f)+(y-.5f)*(y-.5f));return new Color(.19f+thread+light,.33f+thread+light,.29f+thread+light);});
        }
        public void Apply(BoardView view,GameObject content,LocalTurnLoop loop,MatchBoardLayout geometry)
        {
            layout=geometry;
            foreach(var r in content.GetComponentsInChildren<Renderer>())r.enabled=false;
            var go=new GameObject("MatchHud");go.transform.SetParent(content.transform,false);root=go.transform;
            Rect full=new Rect(0,0,layout.Width,layout.Height);
            Draw("TableSurface",grain,full,Color.white,-3);
            if(layout.Portrait)
            {
                Draw("HeaderPlank",rounded,layout.Header,new Color32(74,47,30,225),-2);
                Draw("HeaderTrim",rounded,new Rect(layout.Header.xMin,layout.Header.yMin,layout.Header.width,2*layout.Unit),Honey,-1);
                Draw("FooterPlank",rounded,layout.Rail,new Color32(74,47,30,225),-2);
                Draw("FooterTrim",rounded,new Rect(layout.Rail.xMin,layout.Rail.yMax-2*layout.Unit,layout.Rail.width,2*layout.Unit),Honey,-1);
            }

            var shadow=layout.Board;shadow.position+=new Vector2(0,-3*layout.Unit);shadow.xMin-=2*layout.Unit;shadow.xMax+=2*layout.Unit;
            Draw("BoardGroundShadow",rounded,shadow,new Color(0,0,0,.18f),-1);
            var board=Draw("ResponsiveBoard",skin.BoardSprite,layout.Board,Color.white,0);
            if(layout.Portrait){board.transform.localRotation=Quaternion.Euler(0,0,-90);board.transform.localScale=new Vector3(layout.Board.height/100/skin.BoardSprite.bounds.size.x,layout.Board.width/100/skin.BoardSprite.bounds.size.y,1);}
            var counts=new Dictionary<int,int>();
            foreach(var c in view.Checkers)
            {
                int key=c.Point*2+c.Player,ordinal=counts.TryGetValue(key,out int n)?n:0;counts[key]=ordinal+1;
                int count=c.Point==-1?view.Snapshot.Bar(c.Player):c.Point==24?view.Snapshot.Off(c.Player):Math.Abs(view.Snapshot.Point(c.Point));
                float diameter=c.Point==24?layout.CheckerDiameter*.54f:c.Point==-1&&layout.Portrait?layout.CheckerDiameter*.68f:layout.CheckerDiameter;
                Vector2 position=layout.PixelToWorld(layout.CheckerPixel(c.Point,c.Player,ordinal,count));
                var r=c.Renderer;r.enabled=true;r.sprite=c.Player==0?skin.IvorySprite:skin.WalnutSprite;r.color=Color.white;
                r.transform.localPosition=position;float size=(layout.Portrait?layout.CheckerFootprint(c.Point):diameter/.84f)/100;r.transform.localScale=new Vector3(size/r.sprite.bounds.size.x,size/r.sprite.bounds.size.y,1);r.sortingOrder=10+ordinal;
            }
            foreach(int point in Enumerable.Range(0,24))
            {
                int count=Math.Abs(view.Snapshot.Point(point));if(count<=5)continue;
                var p=layout.Portrait?layout.CheckerPixel(point,0,count-1,count):layout.TargetPixel(point,0);
                Draw("StackCountBadge_"+point,circle,Centered(p,19*layout.Unit,19*layout.Unit),Cream,44);
                Label("StackCount_"+point,count.ToString(),p,11*layout.Unit,Ink,45);
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
                float d=layout.Portrait?layout.CheckerFootprint(selected.Point)*1.08f:layout.CheckerDiameter*1.12f;
                Draw("SelectedChecker",circle,Centered(layout.WorldToPixel(selected.Renderer.transform.localPosition),d,d),Honey,9);
            }
            foreach(int target in loop.Targets)
            {
                var p=layout.TargetPixel(target,loop.Session.Player);
                if(target<24)
                {
                    float ghostSize=layout.Portrait?layout.CheckerFootprint(target):layout.CheckerDiameter/.84f;
                    var ghostSprite=loop.Session.Player==0?skin.IvorySprite:skin.WalnutSprite;
                    Draw("LegalGhost_"+target,ghostSprite,Centered(p,ghostSize,ghostSize),new Color(1,1,1,.45f),38);
                }
                float d=layout.Portrait?layout.PortraitLaneWidth*.46f:layout.CheckerDiameter*.62f;
                Draw("LegalTarget_"+target,circle,Centered(p,d,d),Honey,40);
                Draw("LegalTargetFace_"+target,circle,Centered(p,d*.68f,d*.68f),new Color32(77,153,149,255),41);
            }
            Status(loop);
            Dice(loop);
            Button("Undo",layout.Undo,"↶  Geri al",loop.CanUndo);
            if(loop.CanRoll)Button("Roll",layout.Roll,"Zər at",true);
            bool audible=view.GetComponent<BoardFeedback>()?.SoundEnabled??true;
            Draw("SoundControl",circle,Centered(layout.Sound.center,28*layout.Unit,28*layout.Unit),Wood,49);
            Label("SoundLabel",audible?"♪":"×",layout.Sound.center,16*layout.Unit,Cream,51);

        }
        private void Seat(LocalTurnLoop loop,int player,Rect rect)
        {
            float u=layout.Unit;bool active=loop.Session.Player==player;
            Vector2 puck=new Vector2(rect.xMin+18*u,rect.center.y);
            Draw("SeatRim_"+player,circle,Centered(puck,33*u,33*u),active?new Color32(77,153,149,255):Wood,48);
            Draw("PlayerChecker_"+player,player==0?skin.IvorySprite:skin.WalnutSprite,Centered(puck,30*u,30*u),Color.white,50);
            Label("PlayerName_"+player,player==0?"Açıq tərəf":"Qoz tərəf",new Vector2(rect.center.x+12*u,rect.center.y+6*u),13*u,Cream,50);
            string text=loop.Session.Phase==MatchPhase.Finished?(loop.Session.Result.Winner==player?"Qalib":"Partiya bitdi"):active?"Növbə sizdə":"Gözləyir";
            Label("PlayerTurn_"+player,text,new Vector2(rect.center.x+12*u,rect.center.y-10*u),10*u,active?Honey:new Color32(187,201,185,255),50);
        }
        private void Status(LocalTurnLoop loop)
        {
            float u=layout.Unit;string mode=loop.Session.Mode==GameMode.Long?"UZUN NƏRD":"QISA NƏRD";
            if(layout.Portrait)
            {
                Label("ModeTitle",mode,new Vector2(layout.Header.xMin+54*u,layout.Header.center.y),13*u,Honey,50);
                Seat(loop,1,new Rect(layout.Header.center.x-60*u,layout.Header.yMin+4*u,160*u,38*u));
                Seat(loop,0,new Rect(layout.Rail.xMin+10*u,layout.Rail.yMax-40*u,165*u,34*u));
            }
            else
            {
                Label("ModeTitle",mode,new Vector2(layout.Rail.center.x,layout.Rail.yMax-48*u),14*u,Honey,50);
                Seat(loop,1,new Rect(layout.Rail.xMin+8*u,layout.Rail.yMax-110*u,layout.Rail.width-16*u,36*u));
                Seat(loop,0,new Rect(layout.Rail.xMin+8*u,layout.Rail.yMax-168*u,layout.Rail.width-16*u,36*u));
                Label("MatchFormat","Lokal · Tək partiya",new Vector2(layout.Rail.center.x,layout.Rail.yMax-190*u),10*u,Cream,50);
            }
            string feedback=loop.Session.Phase==MatchPhase.Finished?(loop.Session.Result.Winner==0?"Açıq tərəf qalibdir":"Qoz tərəf qalibdir"):loop.Reason=="OPENING_TIE"?"Bərabərdir · yenidən atın":loop.Reason=="DICE_SOURCE_FAILED"||loop.Reason=="INVALID_DICE"?"Zər alınmadı · yenidən atın":loop.CanRoll?"Zərə toxunun":loop.Reason=="UNDO_COMPLETE"?"Gediş geri alındı":loop.Selected==TapSelection.None?"Daşınızı seçin":"İşarəli xanaya oynayın";
            Label("MoveFeedback",feedback,layout.Portrait?new Vector2(layout.Board.center.x,layout.Rail.yMax+10*u):new Vector2(layout.Rail.center.x,layout.Rail.yMin+187*u),13*u,Cream,50);
        }
        private void Dice(LocalTurnLoop loop)
        {
            float u=layout.Unit;var area=layout.Dice;var values=loop.CopyDisplayedRoll();
            var tray=area;tray.xMin-=5*u;tray.xMax+=5*u;tray.yMin-=3*u;tray.yMax+=3*u;
            var rim=tray;rim.xMin-=1.5f*u;rim.xMax+=1.5f*u;rim.yMin-=1.5f*u;rim.yMax+=1.5f*u;
            Draw("DiceTrayShadow",rounded,tray,new Color(0,0,0,.20f),46);
            Draw("DiceTrayRim",rounded,rim,Honey,47);
            Draw("DiceTray",rounded,tray,new Color32(105,66,40,255),48);
            for(int die=0;die<2;die++)
            {
                float side=area.height*.92f;Vector2 at=new Vector2(area.center.x+(die==0?-.28f:.28f)*area.width,area.center.y);
                var group=new GameObject("DieGroup_"+die);group.transform.SetParent(root,false);group.transform.localPosition=Vector3.zero;var before=root;root=group.transform;
                Draw("DieRim_"+die,diceFace,Centered(at,side,side),Wood,50);Draw("DieFace_"+die,diceFace,Centered(at,side*.90f,side*.90f),Cream,51);
                if(values.Length==0){Label("DieEmpty_"+die,"·",at,side*.5f,Ink,52);AnchorDie(group.transform,at);root=before;continue;}
                int value=values[die];var spots=new List<Vector2>();if(value%2==1)spots.Add(Vector2.zero);
                if(value>=2){spots.Add(new Vector2(-1,1));spots.Add(new Vector2(1,-1));}if(value>=4){spots.Add(new Vector2(1,1));spots.Add(new Vector2(-1,-1));}if(value==6){spots.Add(new Vector2(-1,0));spots.Add(new Vector2(1,0));}
                for(int i=0;i<spots.Count;i++)Draw("MatchPip_"+die+"_"+i,circle,Centered(at+spots[i]*side*.23f,side*.10f,side*.10f),Ink,52);
                AnchorDie(group.transform,at);root=before;
            }
            var tokens=loop.Tokens;int available=tokens.Count(t=>t.State==TokenState.Available);
            Vector2 center=layout.Portrait?new Vector2(layout.Rail.xMax-92*u,layout.Rail.yMax-28*u):new Vector2(layout.Rail.center.x,layout.Rail.yMin+135*u);
            if(layout.Portrait&&(loop.CanRoll||loop.Session.Phase==MatchPhase.Finished))
                Label("RightsRemaining",loop.Session.Phase==MatchPhase.Finished?"Partiya bitdi":"Atış gözlənilir",new Vector2(layout.Rail.xMax-92*u,layout.Rail.yMax-10*u),14*u,Cream,52);
            else if(tokens.Count>0)
            {
                Label("RightsRemaining",available+" gediş qalıb",layout.Portrait?new Vector2(layout.Rail.xMax-92*u,layout.Rail.yMax-10*u):center+Vector2.up*15*u,(layout.Portrait?14:11)*u,Cream,52);
                for(int i=0;i<tokens.Count;i++)
                {
                    Vector2 at=center+Vector2.right*(i-(tokens.Count-1)*.5f)*(layout.Portrait?12:18)*u;
                    Draw("MoveRight_"+i,circle,Centered(at,(layout.Portrait?8:12)*u,(layout.Portrait?8:12)*u),tokens[i].State==TokenState.Available?Honey:new Color32(141,113,80,255),52);
                    if(tokens[i].State==TokenState.Used)Draw("SpentRight_"+i,rounded,Centered(at,10*u,1.5f*u),Cream,53);
                    if(tokens[i].State==TokenState.Unavailable)Label("BlockedRight_"+i,"×",at,12*u,Cream,53);
                }
            }
        }
        private void AnchorDie(Transform group,Vector2 pixel)
        {Vector3 at=layout.PixelToWorld(pixel);foreach(Transform child in group)child.localPosition-=at;group.localPosition=at;}
        private void Button(string name,Rect rect,string label,bool active)
        {
            float u=layout.Unit;var shadow=rect;shadow.y-=2*u;
            Draw(name+"Shadow",rounded,shadow,new Color(0,0,0,.22f),48);
            Draw(name+"Frame",rounded,rect,new Color32(104,65,39,255),49);
            var inner=rect;inner.xMin+=1.5f*u;inner.xMax-=1.5f*u;inner.yMin+=1.5f*u;inner.yMax-=1.5f*u;
            Color face=active?(name=="Roll"?new Color32(177,83,54,255):Cream):new Color32(75,93,75,255);
            Draw(name+"Face",rounded,inner,face,50);
            Label(name+"Label",label,rect.center,13*u,active&&name=="Undo"?Ink:active?Cream:new Color32(169,181,161,255),51);
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
