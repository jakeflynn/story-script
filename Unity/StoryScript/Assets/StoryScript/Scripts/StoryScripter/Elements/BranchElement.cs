using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchElement : StoryElement {

    public override void Initialize(StoryEngine storyMan, string text, Dictionary<string, string> metaData)
    {
        storyEng = storyMan;
        string bname;
        if (metaData.TryGetValue("name", out bname))
        {
            string nodeName;
            if (metaData.TryGetValue("enter", out nodeName))
            {
                storyEng.ExecuteBranch(bname, nodeName);
                Destroy(gameObject);
            }
        }
    }
}
