using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (FieldOfView))]

public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfView fow = (FieldOfView)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.viewRadius);
        Vector3 viewAngleA = fow.DirFromAngel(-fow.viewAngle / 2, false);
        Vector3 viewAngleB = fow.DirFromAngel(fow.viewAngle / 2, false);

        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleA * fow.viewRadius);
        Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleB * fow.viewRadius);

        Handles.color = Color.red;
        foreach(Transform visivleTarget in fow.visibleTargets)
        {
            Handles.DrawLine(fow.transform.position, visivleTarget.position);
        }
    }
}
