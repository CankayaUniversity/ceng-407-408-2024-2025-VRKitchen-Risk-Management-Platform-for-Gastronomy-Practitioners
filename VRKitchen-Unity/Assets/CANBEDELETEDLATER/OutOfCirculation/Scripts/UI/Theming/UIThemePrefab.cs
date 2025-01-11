using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add that to any prefab part of the theme data. The function Apply will be called for each UI element to which the
/// prefab try to be applied. This class will be subclassed for each type so it can apply the right value (see
/// UIThemePrefabButton or UIThemePrefabSlider)
/// </summary>
public abstract class UIThemePrefab : MonoBehaviour
{
    public abstract void Apply(Object uiElement);
    
    //will be called before Apply is called. Allow to cache some common stuff once, pratical when the prefab will be
    //applied to a lot of Element in a row.
    public virtual void Init() { }

    //should return the element needed by Apply. Used by dynamic special rule that can't know what their element are.
    public abstract Object GetElement(GameObject root);
}
