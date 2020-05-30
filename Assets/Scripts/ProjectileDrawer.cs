using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDrawer : MonoBehaviour
{
    public Material drawMat;
    public float length;
    public float delay;
    private void OnPostRender()
    {

    }

    private void OnRenderObject()
    {
        GL.PushMatrix();
        drawMat.SetPass(0);
        GL.Begin(GL.LINES);
        foreach (Projectile p in ProjectileManager.current.ActiveProj)
        {
            if (p.isActive && p.frames > 0)
            {
                GL.Vertex(p.pos);
                GL.Vertex(p.pos - p.dir * p.speed * 0.02f);
            }
        }
        GL.End();
        GL.PopMatrix();
    }
}