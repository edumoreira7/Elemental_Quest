using UnityEngine;

public static class QuestStageLayout
{
    // x=left, y=right, z=walkable top. Main path first, optional ledge last.
    static readonly Vector3[][] floors={
        new[]{new Vector3(0,12,-2),new Vector3(18,30,-.6f),new Vector3(40,52,-2),new Vector3(55,69,-2),new Vector3(77,89,-.7f),new Vector3(98,112,-2),new Vector3(56,60,1)},
        new[]{new Vector3(0,12,-2),new Vector3(21,34,-2),new Vector3(38,50,-.8f),new Vector3(52,70,-2),new Vector3(76,89,-1),new Vector3(92,112,-2),new Vector3(55,59,1.5f)},
        new[]{new Vector3(0,14,-2),new Vector3(18,34,-1),new Vector3(38,50,-2),new Vector3(52,70,-2),new Vector3(73,89,-.6f),new Vector3(93,112,-2),new Vector3(55,59,1)}
    };
    public static Vector3[] Floors(int region){return floors[region-2];}
    public static float Ground(int region,float x)
    {
        var route=Floors(region);
        for(int i=0;i<route.Length-1;i++)if(x>=route[i].x&&x<=route[i].y)return route[i].z;
        return -2;
    }
}
