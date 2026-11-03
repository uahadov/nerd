using System;
using System.Collections.Generic;
using Nard.Match;
using Nard.Interaction;
using System.Linq;
using UnityEngine;

namespace Nard.Presentation
{
    [DisallowMultipleComponent]
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private bool referenceStyle;
        private ReferenceBoardSkin skin;
        public bool IsReferenceStyle=>referenceStyle;
        public Vector2 BoardToLocal(Vector2 p)=>referenceStyle?ReferenceBoardSkin.ToLocal(p):p;
        public Vector2 LocalToBoard(Vector2 p)=>referenceStyle?ReferenceBoardSkin.ToBoard(p):p;
        [SerializeField] private GameMode initialMode = GameMode.Long;
        private GameObject content;
        private GameObject tapOverlay;
        private Sprite square, circle, triangle, rounded;
        private readonly List<UnityEngine.Object> generated = new List<UnityEngine.Object>();
        private readonly List<CheckerVisual> checkers = new List<CheckerVisual>();
        private readonly List<SpriteRenderer> points = new List<SpriteRenderer>();
        public BoardSnapshot Snapshot { get; private set; }
        public IReadOnlyList<CheckerVisual> Checkers => checkers.AsReadOnly();
        public IReadOnlyList<SpriteRenderer> Points => points.AsReadOnly();
        public const string TemporaryNotice = "Müvəqqəti qrafika · P02.1";
        private static readonly Color Cream = new Color32(246,235,213,255);
        private static readonly Color Walnut = new Color32(121,86,61,255);
        private static readonly Color Terracotta = new Color32(201,111,76,255);
        private static readonly Color Turquoise = new Color32(77,153,149,255);
        public sealed class CheckerVisual
        {
            public int Player { get; internal set; }
            public int Point { get; internal set; } // -1 bar, 24 off
            public SpriteRenderer Renderer { get; internal set; }
        }
        private void Awake()
        {
            CreateSprites();
            Display(BoardSnapshot.From(MatchSession.Create("technical-board", initialMode, RuleContract.VersionFor(initialMode))));
        }
        private void LateUpdate()
        {
            var camera = Camera.main;
            if (camera != null && camera.orthographic)
                camera.orthographicSize = referenceStyle?Mathf.Max(8.4f,9.2f/camera.aspect):Mathf.Max(9.8f, 5.3f / camera.aspect);
        }
        public void Display(BoardSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (square == null) CreateSprites();
            // Disable first so even two snapshots within one frame cannot render stale pieces.
            if (content != null) { content.SetActive(false); Release(content); }
            checkers.Clear(); points.Clear(); Snapshot = snapshot;
            content = new GameObject("BoardContent"); content.transform.SetParent(transform, false);
            Shape("Frame", square, Vector2.zero, new Vector2(9.9f,15.7f), Walnut, 0);
            Shape("Field", square, Vector2.zero, new Vector2(9.5f,15.3f), Cream, 1);
            Shape("Bar", square, new Vector2(-.25f,0), new Vector2(.6f,14.3f), Walnut, 2);
            Shape("Off tray", square, new Vector2(4.53f,0), new Vector2(.62f,14.3f), Walnut, 2);
            for (int index = 0; index < 24; index++)
            {
                var edge = BoardGeometry.PointBase(index); bool top = index >= 12;
                var point = Shape("Point_"+index, triangle, new Vector2(edge.x, top?4.25f:-4.25f),
                    new Vector2(.61f,5.3f), index%2==0?Terracotta:Turquoise, 3);
                if (top) point.transform.localRotation=Quaternion.Euler(0,0,180);
                points.Add(point);
                Label("Index_"+index,index.ToString(),new Vector2(edge.x,top?7.33f:-7.33f),.27f,Walnut);
                int signed = snapshot.Point(index), count = Math.Abs(signed);
                if (count>0)
                {
                    Stack(signed>0?0:1,index,edge,top,count);
                    Label("Count_"+index,count.ToString(),new Vector2(edge.x,top?1.28f:-1.28f),.25f,Walnut);
                }
            }
            for(int p=0;p<2;p++)
            {
                bool top=p==1;
                Stack(p,-1,new Vector2(-.25f,top?6.9f:-6.9f),top,snapshot.Bar(p));
                Stack(p,24,new Vector2(4.53f,top?6.9f:-6.9f),top,snapshot.Off(p));
                Label("BarCount_"+p,snapshot.Bar(p).ToString(),new Vector2(-.25f,top?.9f:-.9f),.25f,Cream);
                Label("OffCount_"+p,snapshot.Off(p).ToString(),new Vector2(4.53f,top?.9f:-.9f),.25f,Cream);
            }
            Label("BarLabel","BAR",new Vector2(-.25f,0),.24f,Cream);
            Label("OffLabel","ÇIX",new Vector2(4.53f,0),.24f,Cream);
            Label("Title",snapshot.Mode==GameMode.Long?"UZUN NƏRD":"QISA NƏRD",new Vector2(0,8.9f),.48f,Walnut);
            Label("Temporary",TemporaryNotice,new Vector2(0,8.2f),.28f,Walnut);
            Label("Legend","0–23 xana indeksləri · Açıq P0 / Qoz P1",new Vector2(0,-8.2f),.27f,Walnut);
            Label("Unavailable","Daş seçimi və zər atışı hələ hazır deyil",new Vector2(0,-8.75f),.27f,Walnut);
            for(int i=0;i<2;i++)
            {
                Shape("DicePlace_"+i,referenceStyle?rounded:square,new Vector2(1.35f+i*.95f,0),new Vector2(.73f,.73f),Walnut,4);
                Shape("DiceInset_"+i,referenceStyle?rounded:square,new Vector2(1.35f+i*.95f,0),new Vector2(.66f,.66f),Cream,5);
                Label("DiceEmpty_"+i,"—",new Vector2(1.35f+i*.95f,0),.32f,Walnut);
            }
            Label("DiceLabel","Zər yerləri",new Vector2(1.83f,-.68f),.24f,Walnut);
        }
        // Render-only interaction hints. Targets and selection are provided by the application layer.
        public void SetTapOverlay(int selected,IReadOnlyList<int> targets,string feedback,int player,string inputNotice="Texniki nümunə · Zər atışı və yeni növbə yoxdur")
        {
            DrawTapHints(selected,targets,player);
            SetLabel("Temporary","Müvəqqəti qrafika · P02.2");
            SetLabel("Legend",inputNotice);
            SetLabel("Unavailable",feedback);
            var reset=Shape("ExampleReset",square,new Vector2(0,-9.35f),new Vector2(4.2f,.53f),Walnut,2);
            reset.transform.SetParent(tapOverlay.transform,false);
            Label("ExampleResetLabel","Texniki nümunəni sıfırla",new Vector2(0,-9.35f),.25f,Cream);
            content.transform.Find("ExampleResetLabel").SetParent(tapOverlay.transform,false);
        }
        private void DrawTapHints(int selected,IReadOnlyList<int> targets,int player)
        {
            if(tapOverlay!=null){tapOverlay.SetActive(false);Release(tapOverlay);}
            tapOverlay=new GameObject("TapOverlay");tapOverlay.transform.SetParent(content.transform,false);
            var honey=new Color32(232,184,93,255);
            for(int i=checkers.Count-1;i>=0;i--)
            {
                var c=checkers[i];
                if(c.Point!=selected||c.Player!=player)continue;
                Overlay("Selection_"+selected,c.Renderer.transform.localPosition,c.Renderer.bounds.size.x*1.38f,circle,honey,9);
                break;
            }
            foreach(int target in targets)
            {
                Vector2 at=target==24?new Vector2(4.53f,player==0?-1.42f:1.42f):
                    new Vector2(BoardGeometry.PointBase(target).x,target<12?-1.65f:1.65f);
                Overlay("Target_"+target,at,.36f,circle,Walnut,30);
                Overlay("TargetFace_"+target,at,.27f,circle,honey,31);
            }
        }
        public void SetTurnOverlay(LocalTurnLoop loop,string feedback)
        {
            DrawTapHints(loop.Selected,loop.Targets,loop.Session.Player);
            SetLabel("Temporary","Müvəqqəti qrafika · P02.4");
            SetLabel("Legend",loop.Session.Player<0?"Lokal iki oyunçu · ilkin atış":"Növbə "+loop.Session.TurnNumber+" · P"+loop.Session.Player+(loop.Session.Player==0?" (açıq)":" (qoz)"));
            SetLabel("Unavailable",feedback);
            var values=loop.CopyDisplayedRoll();var tokens=loop.Tokens;
            SetLabel("DiceLabel",loop.IsOpeningDisplay?"İlkin atış · P0 / P1":tokens.Count==4?"Qoşa · 4 hüquq":"İki zər");
            for(int die=0;die<2;die++)
            {
                SetLabel("DiceEmpty_"+die,values.Length==0?"—":"");
                if(values.Length==0)continue;
                bool faded=tokens.Count==2?tokens[die].State!=TokenState.Available:tokens.Count>0&&tokens.All(t=>t.State!=TokenState.Available);
                DrawPips(die,values[die],faded?new Color32(154,137,116,255):Walnut);
            }
            for(int slot=0;slot<tokens.Count;slot++)
            {
                var token=tokens[slot];var at=new Vector2(-3.8f+slot*.8f,0);
                var edge=Shape("Token_"+slot,square,at,new Vector2(.59f,.59f),Walnut,4);edge.transform.SetParent(tapOverlay.transform,false);
                var face=Shape("TokenFace_"+slot,square,at,new Vector2(.52f,.52f),token.State==TokenState.Available?Cream:new Color32(197,184,159,255),5);face.transform.SetParent(tapOverlay.transform,false);
                Label("TokenValue_"+slot,token.Value.ToString(),at,.32f,Walnut);content.transform.Find("TokenValue_"+slot).SetParent(tapOverlay.transform,false);
                if(token.State!=TokenState.Available)
                {
                    var line=Shape("TokenStrike_"+slot,square,at,new Vector2(.62f,.035f),Walnut,25);line.transform.SetParent(tapOverlay.transform,false);line.transform.localRotation=Quaternion.Euler(0,0,45);
                    if(token.State==TokenState.Unavailable)
                    {var cross=Shape("TokenUnavailable_"+slot,square,at,new Vector2(.62f,.035f),Walnut,25);cross.transform.SetParent(tapOverlay.transform,false);cross.transform.localRotation=Quaternion.Euler(0,0,-45);}
                }
            }
            if(tokens.Count>0)
            {
                string counts="İşləndi "+tokens.Count(t=>t.State==TokenState.Used)+" · qalıq "+tokens.Count(t=>t.State==TokenState.Available);
                Label("TokenSummary",counts,new Vector2(-2.6f,-.68f),.22f,Walnut);content.transform.Find("TokenSummary").SetParent(tapOverlay.transform,false);
            }
            var undo=Shape("UndoButton",square,new Vector2(0,9.45f),new Vector2(4.2f,.45f),loop.CanUndo?Walnut:new Color32(172,155,131,255),2);undo.transform.SetParent(tapOverlay.transform,false);
            Label("UndoLabel","Geri al",new Vector2(0,9.45f),.24f,Cream);content.transform.Find("UndoLabel").SetParent(tapOverlay.transform,false);
            var button=Shape("RollButton",square,new Vector2(0,-9.35f),new Vector2(4.2f,.53f),loop.CanRoll?Walnut:new Color32(172,155,131,255),2);button.transform.SetParent(tapOverlay.transform,false);
            string buttonText=loop.CanRoll?(loop.Session.Player<0?"İlk atışı et":"Zər at · P"+loop.Session.Player):loop.Session.Phase==MatchPhase.Finished?"Partiya bitdi":"Gedişi tamamlayın";
            Label("RollButtonLabel",buttonText,new Vector2(0,-9.35f),.25f,Cream);content.transform.Find("RollButtonLabel").SetParent(tapOverlay.transform,false);
            if(referenceStyle){if(skin==null)skin=new ReferenceBoardSkin();skin.Apply(this,content,loop.Selected,loop.Session.Player,loop.Targets);}
        }
        private void DrawPips(int die,int value,Color color)
        {
            var center=new Vector2(1.35f+die*.95f,0);
            var spots=new List<Vector2>();
            if(value%2==1)spots.Add(Vector2.zero);
            if(value>=2){spots.Add(new Vector2(-.18f,.18f));spots.Add(new Vector2(.18f,-.18f));}
            if(value>=4){spots.Add(new Vector2(.18f,.18f));spots.Add(new Vector2(-.18f,-.18f));}
            if(value==6){spots.Add(new Vector2(-.18f,0));spots.Add(new Vector2(.18f,0));}
            for(int i=0;i<spots.Count;i++)Overlay("Pip_"+die+"_"+i,center+spots[i],.095f,circle,color,21);
        }
        private void SetLabel(string name,string value)
        {
            var child=content.transform.Find(name);if(child!=null)child.GetComponent<TextMesh>().text=value;
        }
        private void Overlay(string name,Vector2 at,float size,Sprite sprite,Color color,int order)
        {
            var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(tapOverlay.transform,false);
            go.transform.localPosition=at;go.transform.localScale=new Vector3(size,size,1);
            var renderer=go.GetComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.color=color;renderer.sortingOrder=order;
        }
        private void Stack(int player,int point,Vector2 edge,bool top,int count)
        {
            for(int i=0;i<count;i++)
            {
                var at=BoardGeometry.CheckerCenter(edge,top,i,count); float d=BoardGeometry.Diameter(count)*.94f;
                Shape("Rim_"+player+"_"+point+"_"+i,circle,at,new Vector2(d,d),player==0?Walnut:Cream,10);
                var face=Shape("Checker_"+player+"_"+point+"_"+i,circle,at,new Vector2(d*.84f,d*.84f),
                    player==0?new Color32(255,249,231,255):new Color32(85,53,35,255),11);
                checkers.Add(new CheckerVisual{Player=player,Point=point,Renderer=face});
            }
        }
        private SpriteRenderer Shape(string name,Sprite sprite,Vector2 at,Vector2 size,Color color,int order)
        {
            var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(content.transform,false);
            go.transform.localPosition=at;go.transform.localScale=new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1);
            var renderer=go.GetComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.color=color;renderer.sortingOrder=order;
            return renderer;
        }
        private void Label(string name,string text,Vector2 at,float height,Color color)
        {
            var go=new GameObject(name,typeof(TextMesh));go.transform.SetParent(content.transform,false);go.transform.localPosition=at;
            var mesh=go.GetComponent<TextMesh>();mesh.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            mesh.fontSize=64;mesh.characterSize=height/6.4f;mesh.anchor=TextAnchor.MiddleCenter;
            mesh.alignment=TextAlignment.Center;mesh.text=text;
            // Legacy TextMesh vertex colours are not converted like SpriteRenderer colours in linear projects.
            mesh.color=QualitySettings.activeColorSpace==ColorSpace.Linear?color.linear:color;
            var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=mesh.font.material;renderer.sortingOrder=20;
        }
        private void CreateSprites()
        {
            square=MakeSprite(8,8,(x,y)=>true);
            if(referenceStyle)rounded=MakeSprite(128,128,(x,y)=>{float a=Mathf.Max(Mathf.Abs(x-.5f)-.36f,0);float b=Mathf.Max(Mathf.Abs(y-.5f)-.36f,0);return a*a+b*b<=.14f*.14f;});
            circle=MakeSprite(96,96,(x,y)=>(x-.5f)*(x-.5f)+(y-.5f)*(y-.5f)<=.25f);
            triangle=MakeSprite(96,384,(x,y)=>Mathf.Abs(x-.5f)<=(1-y)*.5f);
        }
        private Sprite MakeSprite(int width,int height,Func<float,float,bool> inside)
        {
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Bilinear;
            var colors=new Color[width*height];
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)colors[y*width+x]=inside((x+.5f)/width,(y+.5f)/height)?Color.white:Color.clear;
            texture.SetPixels(colors);texture.Apply();generated.Add(texture);
            var sprite=Sprite.Create(texture,new Rect(0,0,width,height),new Vector2(.5f,.5f),width);
            generated.Add(sprite);return sprite;
        }
        private static void Release(UnityEngine.Object value)
        { if(Application.isPlaying)Destroy(value);else DestroyImmediate(value); }
        private void OnDestroy() { skin?.Dispose(); foreach(var value in generated)if(value!=null)Release(value); }
    }
}
