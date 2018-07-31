using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryParser {

    //Tag Patterns
    private static Regex openingTag = new Regex("\\[.{2,}\\]");
    private static Regex closingTag = new Regex("\\[[/]\\]");
    private static Regex metaData = new Regex("\\{(.*)\\}");
    private static Regex choicePrompt = new Regex(@"\(choice (.*?)\:(.*?)\)");
    private static Regex textPrompt = new Regex(@"\(text\:(.*?)\)");
    private static Regex linkTag = new Regex(@"\@link:(.*)\@");

    private static Dictionary<string, SSBranch> Branches = new Dictionary<string, SSBranch>();

    public static void ReadAsset(string Story, out Queue<ElementQueueNode> nodes, out Dictionary<string, SSBranch> branches)
    {
        //Separate script into tags, anything not matching the regex will be ignored.
        var CurrentStory = Story.Split('\n');
        bool inOpenTag = false;
        int openTagIndex = 0;
        List<SSTuple<int, int>> TagIndices = new List<SSTuple<int, int>>(); //tuples allow us to store first and last indices as one object
        for (int i = 0; i < CurrentStory.Length; ++i)
        {
            if (openingTag.IsMatch(CurrentStory[i]))
            {
                if (!inOpenTag)
                {
                    openTagIndex = i;
                    inOpenTag = true;
                }   //right now, the system flags any two tags on the same line as "nested"
                else Debug.LogError("Story Manager: Line: " + (i + 1) + " Nested tags are not allowed.");
            }

            if (closingTag.IsMatch(CurrentStory[i]))
            {
                if (inOpenTag)
                {
                    inOpenTag = false;
                    TagIndices.Add(new SSTuple<int, int>(openTagIndex, i));
                }
            }
        }

        //Create the element queue
        var qNodes = new Queue<ElementQueueNode>();
        foreach (var tup in TagIndices)
        {
            ElementQueueNode n;
            if (tup.first == tup.second) //Handle sing-line tags
            {
                ProcessTag(CurrentStory[tup.first], out n);
            }
            else //multi-line tags
            {
                StringBuilder sb = new StringBuilder();
                for (int i = tup.first; i <= tup.second; ++i)
                {
                    sb.Append(CurrentStory[i].Trim() + " ");
                }
                ProcessTag(sb.ToString(), out n);
            }

            //Prompt elements are not queued by default, and others are only not queued if it contains the appropriate metadata
            string value = "";
            if (n != null && 
               ((n.Info.elementType != ElementType.prompt  && !n.Info.metaData.ContainsKey("queue"))
               || (n.Info.metaData.TryGetValue("queue", out value) && value.ToUpper() == "TRUE")))
            {
                qNodes.Enqueue(n);
            }

            //create branch or add node to branch if it exists, or if necessary
            if (n != null && n.Info.metaData.TryGetValue("branch", out value))
            {
                SSElementInfo elementInfo = n.Info;
                if (Branches.ContainsKey(value))
                {
                    Debug.Log("Branch exists. Adding node to that.");
                    SSBranch branch;
                    string id;
                    if (Branches.TryGetValue(value, out branch) && n.Info.metaData.TryGetValue("id", out id))
                    {
                        branch.Elements.Add(id, elementInfo);
                    }
                }
                else
                {
                    Debug.Log("Creating branch: " + value);
                    SSBranch branch = new SSBranch();
                    string id;
                    if (n.Info.metaData.TryGetValue("id", out id))
                    {
                        branch.Elements.Add(id, elementInfo);
                    }
                    Branches.Add(value, branch);
                }
            }
            
        }

        //set the out parameters
        nodes = qNodes;
        branches = Branches;
    }

    public static void ProcessTag(string RawTag, out ElementQueueNode node)
    {
        node = new ElementQueueNode();
        node.Info.elementType = (ElementType)3;
        node.storyEngine = MonoBehaviour.FindObjectOfType<StoryEngine>();
        if (RawTag.Contains("[comment]")) return;

        //Strip metadata and create the appropriate dictionary
        string tag = RawTag;
        var TagData = new Dictionary<string, string>();
        if (metaData.IsMatch(tag))
        {
            var m = metaData.Match(tag);
           TagData = GetMetaData(m.Groups[1].Value);
            tag = metaData.Replace(tag, "").Trim();
        }

        //Find the type of tag
        if (tag.StartsWith("[alert]"))
        {
            var s = tag.Replace("[alert]", "");
            s = s.Replace("[/]", "");
            node.Info.text = s.Trim();
            node.Info.elementType = ElementType.alert;
            node.Info.metaData = TagData;
        }
        else if (tag.StartsWith("[dialogue]"))
        {
            var s = tag.Replace("[dialogue]", "");
            s = s.Replace("[/]", "");
            node.Info.text = s.Trim();
            node.Info.elementType = ElementType.dialogue;
            node.Info.metaData = new Dictionary<string, string>(TagData);
        }
        else if (tag.StartsWith("[wait]"))
        {
            node = new WaitNode();
            node.storyEngine = MonoBehaviour.FindObjectOfType<StoryEngine>();
            node.Info.elementType = ElementType.dialogue;
            node.Info.text = "";
            node.Info.metaData = new Dictionary<string, string>(TagData);

        }
        else if (tag.StartsWith("[prompt]"))
        {
            node = new PromptNode();
            var s = choicePrompt.Replace(tag, "");
            s = s.Replace("[prompt]", "");
            s = s.Replace("[/]", "");
            node.Info.text = s.Trim();
            node.Info.elementType = ElementType.prompt;
            node.storyEngine = MonoBehaviour.FindObjectOfType<StoryEngine>();
            node.Info.metaData = new Dictionary<string, string>(TagData);


            if (choicePrompt.IsMatch(tag) || textPrompt.IsMatch(tag))
            {
                var matches = choicePrompt.Matches(tag);
                foreach (var match in matches)
                {
                    PromptChoice choice = new PromptChoice();
                    var c = match.ToString().Replace("(choice ", "");
                    c = c.Replace(")", "");
                    if (linkTag.IsMatch(c))
                    {
                        var m = linkTag.Match(c);
                        c = linkTag.Replace(c, "");
                        choice.link = m.Groups[1].ToString();
                    }else
                    {
                        choice.link = "exit";
                    }

                    var choiceData = c.Split(':');
                    if (choiceData.Length >= 2)
                    {
                        choice.name = choiceData[0].Trim();
                        choice.text = choiceData[1].Trim();
                    }
                    else
                    {
                        choice.name = "Data Missing";
                        choice.text = "Data Missing";
                    }

                    (node as PromptNode).Choices.Add(choice);
                }
                node.Info.Choices = new List<PromptChoice>((node as PromptNode).Choices);
            }
        }
        else if (tag.StartsWith("[branch]"))
        {
            node = new BranchNode();
            node.storyEngine = MonoBehaviour.FindObjectOfType<StoryEngine>();
            node.Info.elementType = ElementType.none;
            node.Info.text = "";
            node.Info.metaData = new Dictionary<string, string>(TagData);
        }
        else if (tag.StartsWith("[clear]"))
        {
            node = new ClearNode();
            node.storyEngine = MonoBehaviour.FindObjectOfType<StoryEngine>();
            node.Info.elementType = ElementType.none;
            node.Info.text = "";
            node.Info.metaData = new Dictionary<string, string>(TagData);
        }
        else
        {
            Debug.LogError("UNKNOWN COMMAND: " + tag);
        }
    }

    public static Dictionary<string, string> GetMetaData(string data)
    {
        Dictionary<string, string> d = new Dictionary<string, string>();
        var s = data.Trim(' ', '{', '}');
        var mdata = s.Split(',');
        foreach (var item in mdata)
        {
            var keypair = item.Split(':');
            if (keypair.Length == 2)
            {
                d.Add(keypair[0].Trim('"', ' '), keypair[1].Trim('"', ' '));
            }
            else Debug.LogError("Invalid syntax: " + item);
        }
        return d;

    }
}
