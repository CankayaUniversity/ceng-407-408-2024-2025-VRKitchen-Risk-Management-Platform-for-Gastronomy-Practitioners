using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInputRemapEntry : MonoBehaviour
{
    public TextMeshProUGUI InputName;
    public TextMeshProUGUI PrimaryInputPath;
    public Image PrimaryInputIcon;
    public TextMeshProUGUI SecondaryInputPath;
    public Image SecondaryInputIcon;

    public UIRemappingButton PrimaryInputButton;
    public UIRemappingButton SecondaryInputButton;
}
