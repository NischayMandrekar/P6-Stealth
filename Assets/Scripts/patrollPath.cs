using UnityEditor;
using UnityEngine;

public class patrollPath : MonoBehaviour
{
    const float wayPointGizmosRadius = .3f;
    void OnDrawGizmos()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            int j = GetNextIndex(i);
            Gizmos.DrawSphere(GetPosition(i), wayPointGizmosRadius);
            Gizmos.DrawLine(GetPosition(i), GetPosition(j));
        }
    }
    
    public int GetNextIndex(int curIndex)
    {
        int nextIndex=transform.childCount==curIndex+1? 0:curIndex + 1;
        return nextIndex;
    }

    public Vector3 GetPosition(int i)
    {
        return transform.GetChild(i).position;
    }
}
