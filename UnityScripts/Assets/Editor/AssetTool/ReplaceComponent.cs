using System;
using UnityEditor;
using UnityEngine;
namespace AssetTool
{
public class ReplaceComponent
{
    private static void Repalce()
    {
        UnityEngine.GameObject[] selections = Selection.gameObjects;

        UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath("Assets/test.mat");
        Material mat = obj as Material;

        foreach (var o in selections)
        {
            SetObjRecursively(o,mat);
        }

        AssetDatabase.SaveAssets();
    }

    static void SetObjRecursively(GameObject rootObj,Material mat)
    {
        if (null != rootObj.renderer)
        {
            Debug.Log(rootObj.renderer.name);
        }
        if (null != rootObj.particleSystem)
        {
            Debug.Log(rootObj.particleSystem.name);
            if (null != rootObj.particleSystem.renderer)
            {
                if ("Default-Particle (Instance)" == rootObj.particleSystem.renderer.material.name)
                    rootObj.particleSystem.renderer.material = mat;
            }
        }
        System.Collections.Generic.IEnumerable<GameObject> subObj = rootObj.GetDirectChildren();
        System.Collections.Generic.IEnumerator<GameObject> e = subObj.GetEnumerator();
        if (null != rootObj)
        {
            while (e.MoveNext())
            {
                SetObjRecursively(e.Current,mat);
            }
        }
    }

    //[MenuItem("EctypeEditor/Replace Component")]
    static void ReplaceComponentSelect()
    {
        Repalce();
    }
}
}