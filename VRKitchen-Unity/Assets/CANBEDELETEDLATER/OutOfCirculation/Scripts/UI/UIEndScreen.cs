using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEndScreen : MonoBehaviour
{
    public string CoursePath;

    public void GoToCourse()
    {
        Application.OpenURL(CoursePath);
    }
}
