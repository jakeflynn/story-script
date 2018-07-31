using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryElement : MonoBehaviour {


    protected StoryEngine storyEng;
    protected Dictionary<string, string> MetaData = new Dictionary<string, string>();
    [SerializeField]
    private Text DialogueText;
    protected bool terminate = false;
    public bool ReturnToQueue
    {
       get { return terminate; }
    }

    protected virtual void Awake()
    {
        if (!DialogueText) DialogueText = gameObject.GetComponent<Text>();

    }

    protected virtual void Terminate()
    {
        terminate = true;
        Destroy(this.gameObject);
    }

    public virtual void Initialize(StoryEngine storyMan, string text, Dictionary<string, string> metaData)
    {
        MetaData = new Dictionary<string, string>(metaData);
        DialogueText.text = text;
        storyEng = storyMan;
    }

    public virtual void ExecuteLink(string bname, string link)
    {
        if (!storyEng) 
        {
            Debug.LogError("No story engine found!");
            return;
        }

        SSBranch branch;
        if (storyEng.Branches.TryGetValue(bname, out branch))
        {
            SSElementInfo info;
            if ( branch.Elements.TryGetValue(link, out info))
            {
                if (info.elementType == ElementType.prompt)
                {
                    PromptNode node = new PromptNode();
                    node.Info = info;
                    node.Choices = info.Choices;
                    node.storyEngine = this.storyEng;
                    var nextNode = node.CreateElement();
                    if (nextNode)
                    {
                        storyEng.ActiveElement = nextNode;
                        Destroy(gameObject);
                    }
                }
                else if (info.elementType == ElementType.alert || info.elementType == ElementType.dialogue)
                {
                    var node = new ElementQueueNode();
                    node.Info = info;
                    node.storyEngine = this.storyEng;
                    var nextNode = node.CreateElement();
                    if (nextNode)
                    {
                        storyEng.ActiveElement = nextNode;
                        Destroy(gameObject);
                    }
                }
            }
        }
    }

}



