using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearElement : StoryElement {
    public override void Initialize(StoryEngine storyMan, string text, Dictionary<string, string> metaData)
    {
        storyEng = storyMan;
        storyEng.ClearAll();
        Destroy(gameObject);
    }
}
