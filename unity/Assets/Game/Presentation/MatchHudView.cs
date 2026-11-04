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
        private readonly Sprite creamField,topFrame,bottomFrame,sideFrame,hinge;
        private readonly Sprite[] topPoints=new Sprite[6],bottomPoints=new Sprite[6];
        private Transform root;private MatchBoardLayout layout;
        private static readonly Color Cream=new Color32(246,235,213,255),Wood=new Color32(103,64,40,255),Honey=new Color32(224,176,100,255),Ink=new Color32(78,48,31,255);
        public MatchHudView(ReferenceBoardSkin skin)
        {
            this.skin=skin;
            var texture=skin.BoardSprite.texture;
            creamField=Sprite(128,128,(x,y)=>{float v=.003f*Mathf.Sin(x*347+y*113);return new Color(.963f+v,.865f+v,.698f+v,1);});
            topFrame=Slice(texture,new Rect(0,1008,720,78));bottomFrame=Slice(texture,new Rect(0,0,720,90));sideFrame=Slice(texture,new Rect(0,90,128,918));
            hinge=Slice(texture,new Rect(701,843,49,96));
            for(int i=0;i<6;i++){topPoints[i]=PointSprite(texture,new Rect(130+i*90,694,90,318),true);bottomPoints[i]=PointSprite(texture,new Rect(130+i*90,83,90,345),false);}
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

            if(layout.Portrait)PortraitBoard();
            else Draw("ResponsiveBoard",skin.BoardSprite,layout.Board,Color.white,0);
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
                float d=layout.Portrait?layout.PortraitLaneWidth*.24f:layout.CheckerDiameter*.44f;Draw("LegalTarget_"+target,circle,Centered(p,d,d),Ink,40);Draw("LegalTargetFace_"+target,circle,Centered(p,d*.65f,d*.65f),new Color32(77,153,149,255),41);
            }
            Status(loop);
            Dice(loop);
            Button("Undo",layout.Undo,"↶  Geri al",loop.CanUndo);
            if(loop.CanRoll)Button("Roll",layout.Roll,"Zər at",true);

        }
        private void PortraitBoard()
        {
            Rect board=layout.Board;float w=board.width;
            Draw("PortraitBoardFrame",grain,board,Color.white,0);
            // One continuous case: only its outside top/bottom edges carry a full ornamental rail.
            Draw("PortraitOuterTop",topFrame,new Rect(board.xMin,board.yMax-w*.09f,w,w*.09f),Color.white,4);
            Draw("PortraitOuterBottom",bottomFrame,new Rect(board.xMin,board.yMin,w,w*.09f),Color.white,4);
            for(int half=0;half<2;half++)
            {
                Rect tray=layout.Tray(half);
                Draw("TrayLeft_"+half,sideFrame,new Rect(tray.xMin,tray.yMin,w*.07f,tray.height),Color.white,3);
                Draw("TrayRight_"+half,sideFrame,new Rect(tray.xMax-w*.07f,tray.yMin,w*.07f,tray.height),Color.white,3).flipX=true;
                var field=layout.PortraitField(half);
                Draw("CreamField_"+half,creamField,field,Color.white,1);
                for(int col=0;col<6;col++)
                {
                    Rect top=layout.PointVisualRect((half==0?12:18)+col),bottom=layout.PointVisualRect((half==0?11:5)-col);
                    Draw("TopPoint_"+half+"_"+col,topPoints[col],top,Color.white,2);
                    Draw("BottomPoint_"+half+"_"+col,bottomPoints[col],bottom,Color.white,2);
                }
            }
            Draw("PortraitCentralBar",grain,layout.PortraitBar,new Color(.85f,.81f,.76f,1),4);
            foreach(int side in new[]{-1,1})
            {
                var r=Draw("PortraitHinge_"+side,hinge,Centered(board.center+Vector2.right*side*layout.PortraitLaneWidth*3.18f,layout.PortraitLaneWidth*.22f,layout.PortraitLaneWidth*.44f),Color.white,6);
                r.transform.localRotation=Quaternion.Euler(0,0,90);
            }
            Draw("PortraitHudWood",grain,layout.Header,new Color(.86f,.83f,.78f,1),46);
            Draw("PortraitControlWood",grain,layout.Rail,new Color(.86f,.83f,.78f,1),46);
        }
        private void PortraitStatus(LocalTurnLoop loop)
        {
            Rect r=layout.Status;float u=layout.Unit;
            Label("ModeTitle",loop.Session.Mode==GameMode.Long?"UZUN NƏRD":"QISA NƏRD",new Vector2(r.center.x,r.yMax-9*u),12*u,Cream,50);
            for(int player=0;player<2;player++)
            {
                bool active=loop.Session.Player==player;
                var seat=Centered(new Vector2(r.xMin+(player==0?.25f:.75f)*r.width,r.yMin+17*u),r.width*.46f,32*u);
                Draw("PlayerSeat_"+player,grain,seat,active?Color.white:new Color(.87f,.85f,.80f,1),48);
                Draw("PlayerActiveEdge_"+player,rounded,new Rect(seat.xMin+8*u,seat.yMin,seat.width-16*u,3*u),active?new Color32(77,153,149,255):Wood,49);
                Draw("PlayerChecker_"+player,player==0?skin.IvorySprite:skin.WalnutSprite,Centered(new Vector2(seat.xMin+22*u,seat.center.y),24*u,24*u),Color.white,50);
                Label("PlayerName_"+player,player==0?"Açıq tərəf":"Qoz tərəf",seat.center+new Vector2(10*u,4*u),15*u,Cream,50);
                string turn=loop.Session.Phase==MatchPhase.Finished?(loop.Session.Result.Winner==player?"Qalib":"Partiya bitdi"):active?"Sizin növbəniz":"Gözləyir";
                Label("PlayerTurn_"+player,turn,seat.center+new Vector2(10*u,-9*u),9*u,active?Cream:new Color32(207,185,156,255),50);
            }
            string feedback=loop.Session.Phase==MatchPhase.Finished?(loop.Session.Result.Winner==0?"Açıq tərəf qalibdir":"Qoz tərəf qalibdir"):loop.Reason=="OPENING_TIE"?"Bərabərdir · yenidən atın":loop.Reason=="DICE_SOURCE_FAILED"||loop.Reason=="INVALID_DICE"?"Zər alınmadı · yenidən atın":loop.CanRoll?"Başlamaq üçün zər atın":loop.Reason=="UNDO_COMPLETE"?"Gediş geri alındı":loop.Selected==TapSelection.None?"Öz daşınızı seçin":"İşarəli xanaya oynayın";
            Label("MoveFeedback",feedback,new Vector2(layout.Rail.xMin+260*u,layout.Rail.yMin+64*u),13*u,Cream,50);
        }
        private void Status(LocalTurnLoop loop)
        {
            if(layout.Portrait){PortraitStatus(loop);return;}
            var r=layout.Status;float u=layout.Unit;
            Label("ModeTitle",loop.Session.Mode==GameMode.Long?"UZUN NƏRD":"QISA NƏRD",new Vector2(r.center.x,r.yMax-10*u),13*u,Cream,50);
            for(int player=0;player<2;player++)
            {
                bool current=loop.Session.Player==player;float x=layout.Portrait?r.xMin+(player==0?.25f:.75f)*r.width:r.center.x;
                float y=layout.Portrait?r.yMin+22*u:r.yMax-(player==0?66:122)*u;
                Rect plate=Centered(new Vector2(x,y),layout.Portrait?r.width*.46f:r.width,40*u);
                Draw("PlayerSeat_"+player,rounded,plate,current?new Color32(61,104,98,255):new Color32(98,65,43,255),48);
                Draw("PlayerChecker_"+player,player==0?skin.IvorySprite:skin.WalnutSprite,Centered(new Vector2(plate.xMin+22*u,y),30*u,30*u),Color.white,50);
                Label("PlayerName_"+player,player==0?"Açıq tərəf":"Qoz tərəf",new Vector2(x+12*u,y+6*u),12*u,Cream,50);
                Label("PlayerTurn_"+player,loop.Session.Phase==MatchPhase.Finished?(loop.Session.Result.Winner==player?"Qalib":"Partiya bitdi"):current?"Növbə sizdə":"Gözləyir",new Vector2(x+12*u,y-9*u),9*u,current?Cream:new Color32(198,173,141,255),50);
            }
            if(!layout.Portrait)Label("MatchFormat","Lokal · Tək partiya",new Vector2(r.center.x,r.yMin+2*u),10*u,Cream,50);
            string feedback=loop.Session.Phase==MatchPhase.Finished?(loop.Session.Result.Winner==0?"Açıq tərəf qalibdir":"Qoz tərəf qalibdir"):loop.Reason=="OPENING_TIE"?"Bərabərdir · yenidən atın":loop.Reason=="DICE_SOURCE_FAILED"||loop.Reason=="INVALID_DICE"?"Zər alınmadı · yenidən atın":loop.Session.Player<0?"Başlamaq üçün zər atın":loop.CanRoll?"Yeni növbə":loop.Reason=="UNDO_COMPLETE"?"Gediş geri alındı":"İşarəli xanaya oynayın";
            float fy=layout.Portrait?layout.Rail.yMin+3*u:layout.Rail.yMin+187*u;
            Label("MoveFeedback",feedback,new Vector2(layout.Rail.center.x,fy),9*u,Cream,50);
        }
        private void Dice(LocalTurnLoop loop)
        {
            float u=layout.Unit;var area=layout.Dice;var values=loop.CopyDisplayedRoll();
            for(int die=0;die<2;die++)
            {
                float side=area.height*.92f;Vector2 at=new Vector2(area.center.x+(die==0?-.28f:.28f)*area.width,area.center.y);
                Draw("DieRim_"+die,diceFace,Centered(at,side,side),Wood,50);Draw("DieFace_"+die,diceFace,Centered(at,side*.90f,side*.90f),Cream,51);
                if(values.Length==0){Label("DieEmpty_"+die,"·",at,side*.5f,Ink,52);continue;}
                int value=values[die];var spots=new List<Vector2>();if(value%2==1)spots.Add(Vector2.zero);
                if(value>=2){spots.Add(new Vector2(-1,1));spots.Add(new Vector2(1,-1));}if(value>=4){spots.Add(new Vector2(1,1));spots.Add(new Vector2(-1,-1));}if(value==6){spots.Add(new Vector2(-1,0));spots.Add(new Vector2(1,0));}
                for(int i=0;i<spots.Count;i++)Draw("MatchPip_"+die+"_"+i,circle,Centered(at+spots[i]*side*.23f,side*.10f,side*.10f),Ink,52);
            }
            var tokens=loop.Tokens;int available=tokens.Count(t=>t.State==TokenState.Available);
            Vector2 center=layout.Portrait?new Vector2(layout.Rail.xMin+135*u,layout.Rail.yMax-20*u):new Vector2(layout.Rail.center.x,layout.Rail.yMin+135*u);
            if(layout.Portrait&&(loop.CanRoll||loop.Session.Phase==MatchPhase.Finished))
                Label("RightsRemaining",loop.Session.Phase==MatchPhase.Finished?"Partiya bitdi":"Atış gözlənilir",new Vector2(layout.Rail.xMin+260*u,layout.Rail.yMax-14*u),14*u,Cream,52);
            else if(tokens.Count>0)
            {
                Label("RightsRemaining",available+" gediş qalıb",layout.Portrait?new Vector2(layout.Rail.xMin+260*u,layout.Rail.yMax-14*u):center+Vector2.up*15*u,(layout.Portrait?14:11)*u,Cream,52);
                for(int i=0;i<tokens.Count;i++)
                {
                    Vector2 at=center+Vector2.right*(i-(tokens.Count-1)*.5f)*(layout.Portrait?12:18)*u;
                    Draw("MoveRight_"+i,circle,Centered(at,(layout.Portrait?8:12)*u,(layout.Portrait?8:12)*u),tokens[i].State==TokenState.Available?Honey:new Color32(141,113,80,255),52);
                    if(tokens[i].State==TokenState.Used)Draw("SpentRight_"+i,rounded,Centered(at,10*u,1.5f*u),Cream,53);
                    if(tokens[i].State==TokenState.Unavailable)Label("BlockedRight_"+i,"×",at,12*u,Cream,53);
                }
            }
        }
        private void Button(string name,Rect rect,string label,bool active)
        {
            Draw(name+"Frame",rounded,rect,active?new Color32(161,123,77,255):new Color32(99,76,55,255),49);
            var inner=rect;inner.xMin+=2*layout.Unit;inner.xMax-=2*layout.Unit;inner.yMin+=2*layout.Unit;inner.yMax-=2*layout.Unit;
            Draw(name+"Wood",grain,inner,active?Color.white:new Color(.80f,.77f,.70f,1),50);Label(name+"Label",label,rect.center,(layout.Portrait?15:13)*layout.Unit,active?Cream:new Color32(182,160,133,255),51);
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
        private Sprite PointSprite(Texture2D texture,Rect rect,bool top)
        {
            var sprite=Slice(texture,rect);
            // OverrideGeometry takes sprite-rect pixels, unlike the world-unit vertices getter.
            float w=rect.width,h=rect.height;
            sprite.OverrideGeometry(new[]{new Vector2(0,top?h:0),new Vector2(w,top?h:0),new Vector2(w/2,top?0:h)},new ushort[]{0,1,2});return sprite;
        }
        private Sprite Slice(Texture2D texture,Rect rect,Vector4 border=default)
        {var sprite=UnityEngine.Sprite.Create(texture,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,border);owned.Add(sprite);return sprite;}
        private Sprite Sprite(int width,int height,Func<float,float,Color> color)
        {
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Bilinear;var pixels=new Color[width*height];
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)pixels[y*width+x]=color((x+.5f)/width,(y+.5f)/height);texture.SetPixels(pixels);texture.Apply();owned.Add(texture);var sprite=UnityEngine.Sprite.Create(texture,new Rect(0,0,width,height),new Vector2(.5f,.5f),width,0,SpriteMeshType.FullRect,new Vector4(12,12,12,12));owned.Add(sprite);return sprite;
        }
        public void Dispose(){foreach(var item in owned){if(Application.isPlaying)UnityEngine.Object.Destroy(item);else UnityEngine.Object.DestroyImmediate(item);}}
    }
}
