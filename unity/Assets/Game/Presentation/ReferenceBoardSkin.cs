using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Nard.Presentation
{
    /// <summary>Render-only skin authorized in D031. One static board; no rules or responsive-layout system.</summary>
    public sealed class ReferenceBoardSkin : IDisposable
    {
        private readonly Sprite board,ivory,walnut,offWood;
        public ReferenceBoardSkin()
        {
            var table=Resources.Load<Texture2D>("NardBoard");var pieces=Resources.Load<Texture2D>("NardCheckers");
            if(table==null||pieces==null)throw new InvalidOperationException("Reference board textures missing");
            board=Sprite.Create(table,new Rect(0,0,table.width,table.height),new Vector2(.5f,.5f),100);
            offWood=Sprite.Create(table,new Rect(table.width*.944f,table.height*.09f,table.width*.047f,table.height*.82f),new Vector2(.5f,.5f),100);
            float half=pieces.width/2f;
            ivory=Sprite.Create(pieces,new Rect(0,0,half,pieces.height),new Vector2(.5f,.5f),100);
            walnut=Sprite.Create(pieces,new Rect(half,0,half,pieces.height),new Vector2(.5f,.5f),100);
        }
        // Inverse mapping keeps the accepted canonical hit map and every move rule unchanged.
        public static Vector2 ToLocal(Vector2 p)
        {
            if(p.y>9.1f)return new Vector2(-4+p.x,-7.45f+(p.y-9.45f));
            if(p.y< -9.05f)return new Vector2(4+p.x,-7.45f+(p.y+9.35f));
            return new Vector2(p.x>=4.22f&&p.x<=4.84f?8.55f+p.x-4.53f:(p.x+.25f)*1.516f,p.y*.71f);
        }
        public static Vector2 ToBoard(Vector2 p)
        {
            if(p.y>=-7.73f&&p.y<=-7.17f)
            {
                if(Mathf.Abs(p.x+4)<2.1f)return new Vector2(p.x+4,9.45f+p.y+7.45f);
                if(Mathf.Abs(p.x-4)<2.1f)return new Vector2(p.x-4,-9.35f+p.y+7.45f);
            }
            return new Vector2(p.x>=8.24f&&p.x<=8.86f?4.53f+p.x-8.55f:p.x/1.516f-.25f,p.y/.71f);
        }
        private static void Size(SpriteRenderer r,float width,float height)
        {r.transform.localScale=new Vector3(width/r.sprite.bounds.size.x,height/r.sprite.bounds.size.y,1);}
        public void Apply(BoardView view,GameObject content,int selected,int player,IReadOnlyList<int> targets)
        {
            var art=new GameObject("ReferenceBoard",typeof(SpriteRenderer));art.transform.SetParent(content.transform,false);
            var renderer=art.GetComponent<SpriteRenderer>();renderer.sprite=board;renderer.sortingOrder=0;Size(renderer,16,12);
            foreach(var r in content.GetComponentsInChildren<SpriteRenderer>())
            {
                string n=r.name;
                if(n=="ReferenceBoard")continue;
                if(n=="Off tray"){r.enabled=view.Snapshot.Off(0)+view.Snapshot.Off(1)>0||targets.Contains(24);r.sprite=offWood;r.color=Color.white;r.transform.localPosition=new Vector3(8.55f,0,0);Size(r,.9f,10.5f);continue;}
                if(n=="Frame"||n=="Field"||n=="Bar"||n.StartsWith("Point_")||n.StartsWith("Rim_")){r.enabled=false;continue;}
                r.transform.localPosition=ToLocal(r.transform.localPosition);
                if(n.StartsWith("Target")){Size(r,n.StartsWith("TargetFace")?.3f:.4f,n.StartsWith("TargetFace")?.3f:.4f);}
                if(n=="UndoButton"||n=="RollButton")Size(r,3.2f,.6f);
            }
            var counts=new Dictionary<int,int>();
            foreach(var c in view.Checkers)
            {
                int key=c.Point*2+c.Player;int ordinal=counts.TryGetValue(key,out int value)?value:0;counts[key]=ordinal+1;
                int count=c.Point==-1?view.Snapshot.Bar(c.Player):c.Point==24?view.Snapshot.Off(c.Player):Math.Abs(view.Snapshot.Point(c.Point));
                float d=.82f;float spacing=Mathf.Min(d,3.7f/Mathf.Max(1,count-1));bool top=c.Point<0||c.Point==24?c.Player==1:c.Point>=12;
                float x=c.Point==-1?0:c.Point==24?8.55f:ToLocal(BoardGeometry.PointBase(c.Point)).x;
                var r=c.Renderer;r.sprite=c.Player==0?ivory:walnut;r.color=Color.white;r.transform.localPosition=new Vector3(x,(top?1:-1)*(4.95f-d*.5f-spacing*ordinal),0);Size(r,d/ .84f,d/ .84f);
            }
            var selectedChecker=view.Checkers.LastOrDefault(c=>c.Point==selected&&c.Player==player);
            var selection=content.GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(r=>r.name.StartsWith("Selection_"));
            if(selection!=null&&selectedChecker!=null){selection.transform.localPosition=selectedChecker.Renderer.transform.localPosition;float d=selectedChecker.Renderer.bounds.size.x*1.08f;Size(selection,d,d);}
            foreach(var mesh in content.GetComponentsInChildren<TextMesh>())
            {
                string n=mesh.name;
                if(n.StartsWith("Count_"))
                {int index=int.Parse(n.Substring(6));int count=Math.Abs(view.Snapshot.Point(index));mesh.gameObject.SetActive(count>5);mesh.transform.localPosition=new Vector2(ToLocal(BoardGeometry.PointBase(index)).x,index<12?-.15f:.15f);continue;}
                if(n.StartsWith("Index_")||n=="BarLabel"||n=="OffLabel"||n=="Temporary"){mesh.gameObject.SetActive(false);continue;}
                Vector2 p=mesh.transform.localPosition;
                if(n=="Title")p=new Vector2(-4.8f,6.8f);
                else if(n=="Legend")p=new Vector2(2.7f,6.8f);
                else if(n=="Unavailable")p=new Vector2(0,-6.45f);
                else if(n=="TokenSummary")p=new Vector2(0,-7.95f);
                else p=ToLocal(p);
                mesh.transform.localPosition=p;
                if(n.StartsWith("BarCount")||n.StartsWith("OffCount"))mesh.gameObject.SetActive(mesh.text!="0");
            }
            // Keep move rights outside the ornamented playing surface.
            foreach(var tr in content.GetComponentsInChildren<Transform>())
            {
                string n=tr.name;int slot;
                if(n.StartsWith("TokenValue_")||n.StartsWith("TokenFace_")||n.StartsWith("TokenStrike_")||n.StartsWith("TokenUnavailable_")||n.StartsWith("Token_"))
                {if(int.TryParse(n.Substring(n.LastIndexOf('_')+1),out slot))tr.localPosition=new Vector3(-1.2f+slot*.8f,-7.45f,0);}
            }
        }
        public void Dispose(){Destroy(board);Destroy(ivory);Destroy(walnut);Destroy(offWood);}
        private static void Destroy(UnityEngine.Object o){if(Application.isPlaying)UnityEngine.Object.Destroy(o);else UnityEngine.Object.DestroyImmediate(o);}
    }
}
