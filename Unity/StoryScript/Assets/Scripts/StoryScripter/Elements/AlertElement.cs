using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertElement : StoryElement
{
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
