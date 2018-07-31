using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryScripterData : MonoBehaviour {

}

[System.Serializable]
public class SSTuple<T1, T2>
{
    public T1 first;
    public T2 second;

    private static readonly IEqualityComparer Item1Comparer = EqualityComparer<T1>.Default;
    private static readonly IEqualityComparer Item2Comparer = EqualityComparer<T2>.Default;

    public SSTuple(T1 first, T2 second)
    {
        this.first = first;
        this.second = second;
    }

    public override string ToString()
    {
        return string.Format("<{0}, {1}>", first, second);
    }

    public static bool operator ==(SSTuple<T1, T2> a, SSTuple<T1, T2> b)
    {
        if (SSTuple<T1, T2>.IsNull(a) && !SSTuple<T1, T2>.IsNull(b))
            return false;

        if (!SSTuple<T1, T2>.IsNull(a) && SSTuple<T1, T2>.IsNull(b))
            return false;

        if (SSTuple<T1, T2>.IsNull(a) && SSTuple<T1, T2>.IsNull(b))
            return true;

        return
            a.first.Equals(b.first) &&
            a.second.Equals(b.second);
    }

    public static bool operator !=(SSTuple<T1, T2> a, SSTuple<T1, T2> b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + first.GetHashCode();
        hash = hash * 23 + second.GetHashCode();
        return hash;
    }

    public override bool Equals(object obj)
    {
        var other = obj as SSTuple<T1, T2>;
        if (object.ReferenceEquals(other, null))
            return false;
        else
            return Item1Comparer.Equals(first, other.first) &&
                   Item2Comparer.Equals(second, other.second);
    }

    private static bool IsNull(object obj)
    {
        return object.ReferenceEquals(obj, null);
    }
}

[System.Serializable]
public enum ElementType: int
{
    alert = 0,
    dialogue = 1,
    prompt = 2,
    none = 3
}

[System.Serializable]
public struct SSElementInfo
{
    public Dictionary<string, string> metaData;
    public List<PromptChoice> Choices;
    public string text;
    public ElementType elementType;
}

[System.Serializable]
public class SSBranch
{
    public Dictionary<string, SSElementInfo> Elements = new Dictionary<string, SSElementInfo>();

    public void Execute(string key)
    {
        SSElementInfo info = new SSElementInfo();
        if (Elements.TryGetValue(key, out info))
        {
            if (info.elementType == ElementType.prompt)
            {
                PromptNode node = new PromptNode();
                node.Info = info;
                node.Choices = info.Choices;
                node.storyEngine = GameObject.FindObjectOfType<StoryEngine>();
                var nextNode = node.CreateElement();
                node.storyEngine.ActiveElement = nextNode;
            }
            else if (info.elementType == ElementType.alert || info.elementType == ElementType.dialogue)
            {
                var node = new ElementQueueNode();
                node.Info = info;
                node.storyEngine = GameObject.FindObjectOfType<StoryEngine>();
                var nextNode = node.CreateElement();
                node.storyEngine.ActiveElement = nextNode;

            }
        }
        else Debug.LogError("Branch element '" + key + "' not found");

    }
}

[System.Serializable]
public class ElementQueueNode
{
    public SSElementInfo Info;
    public StoryEngine storyEngine;
    public virtual StoryElement CreateElement()
    {
        var ActiveElement = MonoBehaviour.Instantiate(storyEngine.GetTemplateFromType(Info.elementType), storyEngine.Canvas.transform).GetComponent<StoryElement>();
        if (ActiveElement)
        {
            ActiveElement.Initialize(storyEngine, Info.text, Info.metaData);
        }
        return ActiveElement;
    }
}

[System.Serializable]
public class WaitNode : ElementQueueNode
{
    public override StoryElement CreateElement()
    {
        GameObject go = new GameObject();
        go.name = "WaitObject";
        var waitElement = go.AddComponent<WaitElement>();
        waitElement.Initialize(storyEngine, Info.text, Info.metaData);
        return waitElement;
    }
}

public class BranchNode : ElementQueueNode
{
    public override StoryElement CreateElement()
    {
        GameObject go = new GameObject();
        go.name = "BranchObject";
        var branchElement = go.AddComponent<BranchElement>();
        branchElement.Initialize(storyEngine, Info.text, Info.metaData);
        return branchElement;
    }
}

public class ClearNode : ElementQueueNode
{
    public override StoryElement CreateElement()
    {
        GameObject go = new GameObject();
        go.name = "ClearObject";
        var clearElement = go.AddComponent<ClearElement>();
        clearElement.Initialize(storyEngine, Info.text, Info.metaData);
        return clearElement;
    }
}

public class PromptNode : ElementQueueNode
{
    public List<PromptChoice> Choices = new List<PromptChoice>();

    public override StoryElement CreateElement()
    {
        var element = base.CreateElement();
        if (element is PromptElement)
        {
            (element as PromptElement).Choices = new List<PromptChoice>(Choices);
            (element as PromptElement).PopulateChoices();
        }
        return element;
    }
}
