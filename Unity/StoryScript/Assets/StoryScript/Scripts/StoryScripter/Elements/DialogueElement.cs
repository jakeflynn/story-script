using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueElement : StoryElement
{
    [SerializeField]
    private Text UnitName;

    protected override void Awake()
    {
        base.Awake();
        
    }

    protected virtual void Start()
    {
        string s;
        if (MetaData.TryGetValue("name", out s))
        {
            UnitName.text = s;
        }
    }

    public virtual void ClickConfirm()
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
                    terminate = true;
                    Destroy(this.gameObject);
                }
                ExecuteLink(bname, link);
            }
        }
        else
        {
            terminate = true;
            Destroy(this.gameObject);
        }
        
    }
}
