using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Until
{
    public static TextMesh CreatTextMesh(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40, Color color = default, TextAnchor textAnchor = TextAnchor.MiddleCenter, TextAlignment textAlignment = TextAlignment.Center, int sortOrder = 500) {
         
        if (color == null) color = Color.white;
        return CreatTextMesh(parent, text, localPosition, fontSize, color, textAnchor, textAlignment, sortOrder);
    }
    public static TextMesh CreatTextMesh(Transform parent,string text,Vector3 localPosition,int fontSize,Color color,TextAnchor textAnchor,TextAlignment textAlignment,int sortOrder) {
        GameObject textGB = new GameObject("Word_Text",typeof(TextMesh));
        if(parent) textGB.transform.parent.SetParent(parent);
        textGB.transform.localPosition = localPosition;
        TextMesh mesh = textGB.GetComponent<TextMesh>();
        mesh.text = text;
        mesh.fontSize = fontSize;
        mesh.alignment = textAlignment;
        mesh.anchor = textAnchor;
        mesh.color = color;
        mesh.GetComponent<MeshRenderer>().sortingOrder = sortOrder;
        //textGB.AddComponent<BoxCollider>();
        return mesh;
    }
}
