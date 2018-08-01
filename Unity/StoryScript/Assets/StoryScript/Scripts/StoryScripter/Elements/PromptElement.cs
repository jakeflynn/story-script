using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PromptElement : DialogueElement {
    public float ButtonSpacing = 60.0f;
    public List<PromptChoice> Choices = new List<PromptChoice>();
    [SerializeField]
    private RectTransform ContentArea;

    protected override void Awake()
    {
        base.Awake();
        if (!ContentArea) ContentArea = GetComponentInChildren<RectTransform>();
    }

    public virtual void PopulateChoices()
    {
        if (Choices.Count < 4) gameObject.GetComponentInChildren<ScrollRect>().vertical = false;
        for (int i = 0; i < Choices.Count; ++i)
        {
            if (!storyEng) storyEng = FindObjectOfType<StoryEngine>();
            Button button = Instantiate(storyEng.ChoiceButtonTemplate, new Vector3(0.0f, -ButtonSpacing * i, 0.0f), Quaternion.identity, ContentArea).GetComponent<Button>();
            button.gameObject.transform.localPosition = new Vector3(0.0f, -ButtonSpacing * i, 0.0f);
            int ind = i;
            button.onClick.AddListener(delegate { ClickButton(ind); });
            if (button.gameObject.GetComponentInChildren<Text>()) button.gameObject.GetComponentInChildren<Text>().text = Choices[i].text;
            ContentArea.sizeDelta = new Vector2(ContentArea.sizeDelta.x, ContentArea.sizeDelta.y + ButtonSpacing);
        }
        
    }
    void Test() { Debug.Log("Clicked!"); }

    public virtual void ClickButton(int index)
    {
        if (Choices[index].link.ToUpper() == "EXIT") ClickConfirm();
        if (storyEng && storyEng.Branches.Count > 0)
        {
            string branchName;
            if (MetaData.TryGetValue("branch", out branchName))
            {
                SetResponses(index);
                ExecuteLink(branchName, Choices[index].link);
            }
        }
    }


    public override void ClickConfirm()
    {
        base.ClickConfirm();
        //storyEng.Next();
    }

    public virtual void SetResponses(int selectedIndex)
    {
        if (!storyEng)
        {
            Debug.LogError("Unable to set prompt responses for prompt " + name + " as there is no reference to the story engine.");
            return;
        }

        //Set All index not selected to false
        for (int i = 0; i < Choices.Count; ++i)
        {
            string branchName;
            if (MetaData.TryGetValue("branch", out branchName))
            {
                string k = branchName.Trim() + "-" + Choices[i].name.Trim();
                if (!storyEng.PromptResponses.ContainsKey(k))
                {
                    string val = i == selectedIndex ? "TRUE" : "FALSE";
                    storyEng.PromptResponses.Add(k, val);
                } 
            }
        }
    }


}

public struct PromptChoice
{
    public string text;
    public string link;
    public string name;

    public PromptChoice (string t, string l, string n)
    {
        text = t;
        link = l;
        name = n;
    }
}

[System.Serializable]
public class ButtonClickEvent : UnityEvent<int>
{

}
