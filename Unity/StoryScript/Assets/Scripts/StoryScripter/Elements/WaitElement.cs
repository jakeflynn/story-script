using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitElement : StoryElement {

    public override void Initialize(StoryEngine storyMan, string text, Dictionary<string, string> metaData)
    {
        storyEng = storyMan;
        storyEng.Wait = true;
        storyEng.Running = false;
        //storyEng.StartCoroutine(storyEng.Wait5());
        //Debug.Log("Created wait node");
    }

    public void RemoveNode()
    {
        Destroy(gameObject);
    }
}
