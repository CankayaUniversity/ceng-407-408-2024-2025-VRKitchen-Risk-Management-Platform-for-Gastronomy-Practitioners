using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Accessibility/Item")]
public class Item : ScriptableObject
{
    public Sprite Icon;
    public string Name;
    public bool Usable;
    public AudioClip UseSFX;
    public GameObject Prefab3D;
    [TextArea] public string Description;

    public AudioClip SFX;
    public bool SFXLoop = false;
}
