using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryEngine : MonoBehaviour {


    public GameObject Canvas;
    public GameObject DialogueTemplate;
    public GameObject AlertTemplate;
    public GameObject PromptTemplate;
    public GameObject ChoiceButtonTemplate;
    public TextAsset StoryFile;
    [ReadOnly] public bool Running = false;
    [ReadOnly] public bool Wait = false;
    [ReadOnly] public int CursorPos = 0;
    [ReadOnly] public Queue<ElementQueueNode> ElementQueue = new Queue<ElementQueueNode>();
    [HideInInspector] public Dictionary<string, SSBranch> Branches;
    [HideInInspector] public Dictionary<string, string> PromptResponses = new Dictionary<string, string>();

    private string[] CurrentStory;
    [ReadOnly]public StoryElement ActiveElement;

    

    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        
        if (!Canvas)
        {
            Debug.LogError("No Canvas reference found in Story Engine. Will be unable to render story script.");
        }

        LoadStoryFromTextAsset(StoryFile);
        StartExecution();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0) && Running)
        {
            Next();
        }
        
    }

    public void LoadStoryFromTextAsset(TextAsset File)
    {
        if(File != StoryFile) StoryFile = File;
        StoryParser.ReadAsset(File.text, out ElementQueue, out Branches);
    }

    public void LoadStoryFromPath(string path)
    {
        string file = string.Empty;
        if (File.Exists(path))
        {
            StreamReader sr = new StreamReader(path);
            file = sr.ReadToEnd();
            StoryParser.ReadAsset(file, out ElementQueue, out Branches);
        }
        else Debug.Log("No file found at: " + path);
    }

    public void StartExecution()
    {
        if (ElementQueue.Count > 0) StartCoroutine(ExecuteScript());
    }

    //Create a single alert box outside of the story engine.
    public void Alert(string text)
    {
        var ActiveElement = MonoBehaviour.Instantiate(AlertTemplate, Canvas.transform).GetComponent<StoryElement>();
        if (ActiveElement)
        {
            ActiveElement.Initialize(this, text, new Dictionary<string, string>());
        }
    }

    public void ExecuteBranch(string BranchID, string node = "root")
    {
        SSBranch temp = null;
        if (Branches.TryGetValue(BranchID, out temp))
        {
            temp.Execute(node);
        }
    }

    //Next is called to end a wait node or move on from a dialogue
    public void Next()
    {
        if (ActiveElement)
        {
            if (ActiveElement is DialogueElement && !(ActiveElement is PromptElement)) //Prompt elements inherit from Dialogues but they have many more buttons!
            {
                (ActiveElement as DialogueElement).ClickConfirm();
            }
            else if (ActiveElement is WaitElement)
            {
                Running = true;
                Wait = false;
                Destroy(ActiveElement.gameObject);
            }
        }
    }

    public void ClearAll()
    {
        Branches.Clear();
        PromptResponses.Clear();
        ElementQueue.Clear();
    }

    public GameObject GetTemplateFromType(ElementType t)
    {
        GameObject go = null;
        switch (t)
        {
            case ElementType.alert:
                go = AlertTemplate;
                break;
            case ElementType.dialogue:
                go = DialogueTemplate;
                break;
            case ElementType.prompt:
                go = PromptTemplate;
                break;
            default:
                go = null;
                break;
        }
        return go;
    }

    public IEnumerator ExecuteScript()
    {
        Running = true;
        int c = ElementQueue.Count;
        for (int i = 0; i < c; i++)
        {
            ActiveElement = ElementQueue.Dequeue().CreateElement();
            if (ActiveElement && !(ActiveElement is WaitElement)) //Wait elements remain until Next() is called
            {
                yield return new WaitUntil(()=> ActiveElement.ReturnToQueue);
            }

            if (Wait) yield return new WaitUntil(() => !Wait);
        }
        Running = false;
    }
}
