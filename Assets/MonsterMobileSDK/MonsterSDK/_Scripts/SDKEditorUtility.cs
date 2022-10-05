using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
public class SDKEditorUtility
{
    public static Rect GetRectUnder(float deltaX, float deltaY, float deltaW, float deltaH, Rect parentRect)
    {
        Rect newRect = new Rect(parentRect.x + deltaX, parentRect.y + parentRect.height+ deltaY, parentRect.width + deltaW, parentRect.height+deltaH);
        return newRect;
    }
    public static Rect GetExactRectUnder(Rect parentRect)
    {
        return GetRectUnder(0, 0, 0, 0, parentRect);
    }
    public static Rect GetRectChildrenUnder(float deltaX, Rect parentRect)
    {
        
        return GetRectUnder(deltaX,0,-deltaX,0,parentRect);
    }
}
#endif