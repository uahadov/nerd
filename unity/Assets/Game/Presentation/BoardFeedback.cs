using System;
using System.Collections.Generic;
using System.Linq;
using Nard.Interaction;
using UnityEngine;
namespace Nard.Presentation
{
    /// <summary>Presentation only: observes accepted state. Never generates dice or changes a move.</summary>
    [DisallowMultipleComponent]
    public sealed class BoardFeedback : MonoBehaviour
    {
        public const float MoveSeconds=.16f;
        private sealed class Motion {public Transform Target;public Vector3 Start,End,Scale;public Quaternion Rotation;public float Time;public bool Pulse,Die;}
        private readonly List<Motion> motions=new List<Motion>();
        private Dictionary<int,List<Vector3>> previous;
        private LocalTurnLoop observed;private int selection,player,roll;
        private Vector2Int viewport;private Rect safe;
        private AudioSource source;private AudioClip tap,wood,dice;
        public bool SoundEnabled {get;private set;}=true;
        public string LastEvent {get;private set;}="NONE";
        public int EventCount {get;private set;}
        public int ActiveMotions=>motions.Count;
        public AudioClip WoodClip=>wood;
        public AudioClip DiceClip=>dice;
        private void Awake()
        {
            source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.volume=.32f;
            tap=MakeClip("Nard ivory contact",.055f,0);wood=MakeClip("Nard walnut landing",.09f,1);dice=MakeClip("Nard dice on wood",.19f,2);
        }
        public void ToggleSound(){SoundEnabled=!SoundEnabled;source.mute=!SoundEnabled;if(!SoundEnabled)source.Stop();}
        public void Finish()
        {
            foreach(var m in motions)if(m.Target!=null){m.Target.localPosition=m.End;m.Target.localScale=m.Scale;m.Target.localRotation=m.Rotation;}
            motions.Clear();
        }
        private void Emit(string kind,AudioClip clip)
        {LastEvent=kind;EventCount++;if(SoundEnabled&&Application.isFocused&&source!=null)source.PlayOneShot(clip);}
        public void Observe(BoardView view,LocalTurnLoop loop)
        {
            Finish();var layout=view.CurrentLayout;if(layout==null)return;
            var now=new Dictionary<int,List<Vector3>>();
            foreach(var c in view.Checkers){int key=c.Player*26+c.Point+1;if(!now.ContainsKey(key))now[key]=new List<Vector3>();now[key].Add(c.Renderer.transform.localPosition);}
            int diceHash=loop.CopyDisplayedRoll().Aggregate(0,(a,b)=>a*7+b);
            var input=view.GetComponent<BoardTurnInput>();
            bool reset=observed!=loop||previous==null||viewport!=new Vector2Int(layout.Width,layout.Height)||safe!=layout.Safe||(input!=null&&!input.enabled);
            if(!reset)
            {
                bool moved=false;
                for(int owner=0;owner<2;owner++)
                {
                    int removed=-1,added=-1;
                    for(int p=-1;p<=24;p++)
                    {int key=owner*26+p+1;int before=previous.TryGetValue(key,out var old)?old.Count:0,after=now.TryGetValue(key,out var current)?current.Count:0;if(before==after+1)removed=p;if(after==before+1)added=p;}
                    if(removed>=-1&&added>=-1&&removed!=added)
                    {
                        int key=owner*26+removed+1;
                        if(previous.TryGetValue(key,out var poses)&&poses.Count>0)
                        {
                            var c=view.Checkers.LastOrDefault(x=>x.Player==owner&&x.Point==added);
                            if(c!=null){var t=c.Renderer.transform;motions.Add(new Motion{Target=t,Start=poses.Last(),End=t.localPosition,Scale=t.localScale,Rotation=t.localRotation});moved=true;}
                        }
                    }
                }
                if(moved)Emit(loop.Reason=="UNDO_COMPLETE"?"UNDO":"MOVE",wood);
                else if(diceHash!=roll)
                {
                    foreach(var t in GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("DieGroup_")))motions.Add(new Motion{Target=t,Start=t.localPosition,End=t.localPosition,Scale=t.localScale,Rotation=t.localRotation,Die=true});
                    Emit("ROLL",dice);
                }
                else if(loop.Selected!=selection&&loop.Selected!=TapSelection.None)
                {
                    var c=view.Checkers.LastOrDefault(x=>x.Point==loop.Selected&&x.Player==loop.Session.Player);
                    if(c!=null){var t=c.Renderer.transform;motions.Add(new Motion{Target=t,Start=t.localPosition,End=t.localPosition,Scale=t.localScale,Rotation=t.localRotation,Pulse=true});}
                    Emit("SELECT",tap);
                }
                else if(player!=loop.Session.Player)Emit("TURN",tap);
            }
            observed=loop;previous=now;viewport=new Vector2Int(layout.Width,layout.Height);safe=layout.Safe;
            selection=loop.Selected;player=loop.Session.Player;roll=diceHash;
            UpdateMotions(0);
        }
        private void Update()=>UpdateMotions(Time.unscaledDeltaTime);
        private void UpdateMotions(float dt)
        {
            for(int i=motions.Count-1;i>=0;i--)
            {
                var m=motions[i];if(m.Target==null){motions.RemoveAt(i);continue;}m.Time+=dt;float t=Mathf.Clamp01(m.Time/MoveSeconds),ease=1-Mathf.Pow(1-t,3),arc=Mathf.Sin(t*Mathf.PI);
                if(m.Die){m.Target.localPosition=m.End+Vector3.up*arc*.018f;m.Target.localRotation=m.Rotation*Quaternion.Euler(0,0,Mathf.Sin(t*Mathf.PI*4)*5*(1-t));}
                else if(m.Pulse)m.Target.localScale=m.Scale*(1+.045f*arc);
                else m.Target.localPosition=Vector3.LerpUnclamped(m.Start,m.End,ease)+Vector3.up*arc*.035f;
                if(t>=1){m.Target.localPosition=m.End;m.Target.localScale=m.Scale;m.Target.localRotation=m.Rotation;motions.RemoveAt(i);}
            }
        }
        public static AudioClip MakeClip(string name,float seconds,int material)
        {
            const int rate=22050;var values=new float[Mathf.CeilToInt(seconds*rate)];uint noise=3719;
            for(int i=0;i<values.Length;i++)
            {
                noise^=noise<<13;noise^=noise>>17;noise^=noise<<5;float n=(noise&65535)/32767.5f-1,t=i/(float)rate;
                float envelope=Mathf.Exp(-t*(material==2?30:65));
                if(material==2){envelope+=.6f*Mathf.Exp(-Mathf.Max(0,t-.045f)*70)*(t>=.045f?1:0);envelope+=.4f*Mathf.Exp(-Mathf.Max(0,t-.10f)*80)*(t>=.10f?1:0);}
                values[i]=Mathf.Clamp((.06f*n+.14f*Mathf.Sin(t*(material==0?3100:material==1?1900:2500)))*envelope,-.28f,.28f);
            }
            var clip=AudioClip.Create(name,values.Length,1,rate,false);clip.SetData(values,0);return clip;
        }
        private void OnApplicationFocus(bool focused){if(!focused){Finish();source?.Stop();}}
        private void OnDisable()=>Finish();
        private void OnDestroy(){Finish();foreach(var clip in new[]{tap,wood,dice})if(clip!=null)Destroy(clip);}
    }
}
