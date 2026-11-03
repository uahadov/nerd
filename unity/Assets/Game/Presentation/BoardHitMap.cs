using UnityEngine;

namespace Nard.Presentation
{
    /// <summary>Technical portrait coordinates only. No legality or movement distance calculations.</summary>
    public static class BoardHitMap
    {
        public const int Outside = -2;
        public static int PointAt(Vector2 local,int player)
        {
            if(player<0||player>1)return Outside;
            for(int i=0;i<24;i++)
            {
                var at=BoardGeometry.PointBase(i);
                bool row=i<12?local.y>=-7.05f&&local.y<=-1.43f:local.y>=1.43f&&local.y<=7.05f;
                if(row&&Mathf.Abs(local.x-at.x)<.32f)return i;
            }
            bool ownZone=player==0?local.y>=-7.05f&&local.y<=-1.1f:local.y>=1.1f&&local.y<=7.05f;
            if(ownZone&&Mathf.Abs(local.x+.25f)<=.3f)return -1;
            if(ownZone&&Mathf.Abs(local.x-4.53f)<=.31f)return 24;
            return Outside;
        }
        public static bool IsUndoButton(Vector2 p)=>Mathf.Abs(p.x)<2.1f&&Mathf.Abs(p.y-9.45f)<.25f;
        public static bool IsRollButton(Vector2 local)=>Mathf.Abs(local.x)<2.1f&&Mathf.Abs(local.y+9.35f)<.28f;
        public static bool IsExampleReset(Vector2 local)=>Mathf.Abs(local.x)<2.1f&&Mathf.Abs(local.y+9.35f)<.28f;
    }
}
