using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillsUI : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<Image>().color = ContentResize.skillColor;
    }

}
