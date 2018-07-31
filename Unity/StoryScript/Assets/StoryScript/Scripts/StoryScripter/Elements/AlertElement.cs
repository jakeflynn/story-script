using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlertElement : StoryElement
{
    [SerializeField] private Button ConfirmButton;

    protected override void Awake()
    {
        base.Awake();
        if (!ConfirmButton) ConfirmButton = GetComponentInChildren<Button>();
        if (ConfirmButton)
        {
            ConfirmButton.onClick.AddListener(delegate { ClickConfirm(); });
        }
        else Debug.LogError("AlertElement: Unable to find a valid button object on element.");
    }

    public void ClickConfirm()
    {
        if (MetaData.ContainsKey("branch") && MetaData.ContainsKey("link"))
        {
            //get branch and link values
            string bname;
            string link;
            if (MetaData.TryGetValue("branch", out bname) && MetaData.TryGetValue("link", out link))
            {
                if (link == "" || link == "exit")
                {
                    Terminate();
                }
                ExecuteLink(bname, link);
            }
        }
        else Terminate();
    }
}
