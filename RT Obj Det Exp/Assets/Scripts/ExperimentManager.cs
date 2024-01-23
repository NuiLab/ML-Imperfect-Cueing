using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Security.Cryptography;

public class ExperimentManager : MonoBehaviour
{
    public GameObject HoldScreenPanel;
    public Transform controllerTransform;
    public SpriteRenderer SearchTargetIcon;
    public Text CountdownText;
    public GameObject selected_object = null;
    public HighlightManager selected_HM = null;
    public Outline selected_HM_Outline = null;
    public StateManager stateManager;
    public CueManager cueManager;
    public GuiManager guiManager;
    public HelperFunctions helperFunctions;
    public DataLogger dataLogger;

    public bool resetMap = false;
    public int userID;
    public int ObjectNumber = 1;
    public int DisplayTime = 30; // seconds to hold while seeing icon of target
    public string[] PracticeObjectNames = new string[]{"ObjectA", "ObjectB", "ObjectC"};
    public string[] ObjectNames = new string[40];
    public (string, string) trialOdering;
    public Sprite[] ObjectImages;
    public GameObject[] Objects;
    public GameObject[] PracticeObjects;
    public GameObject[] CurrObjects;
    public GameObject[] CurrPracticeObjects;

    public int trialIndex = 0;
    int[] allTrials = new int[86];
    public string cueAidType;
    int[] unshuffledList = new int[40];
    int[] cueObjectOrdering = new int[40];
    int[] controlObjectOrdering = new int[40];
    string[] cueAidTypes = new string[]{"WOZ", "ML"}; // Alternates every participant, participant will only see one type
    (string, string)[] trialOderings = new (string, string)[]{("CONTROL", "CUE"), ("CUE", "CONTROL"), ("CUE", "CONTROL")};
    private System.Random _random = new System.Random();


    void Awake()
    {
      for (int i = 0; i < 40; i++){
        //9/20/2022 - Dan Rehberg - Fewer objects for a demo
        // Reduced for demo
        /*int val = (i + 1) % 5;
            ObjectNames[i] = string.Format("Object{0}", i + 1);// val); //Requires unique string names
        unshuffledList[i] = val + 1; //Do not use zero
        */
        // Original
        
        ObjectNames[i] = string.Format("Object{0}", i + 1);
        unshuffledList[i] = i + 1;
        
      }
      cueObjectOrdering = Shuffle(_random, (int[]) unshuffledList.Clone());
      controlObjectOrdering = Shuffle(_random, (int[]) unshuffledList.Clone());
      allTrials[0] = -3;
      allTrials[1] = -2;
      allTrials[2] = -1;
      allTrials[43] = -3;
      allTrials[44] = -2;
      allTrials[45] = -1;
      if (resetMap) {
        PlayerPrefs.SetInt("saved_map", 0);
        PlayerPrefs.Save();
      }
    }

    int[] Shuffle(System.Random rand, int[] array)
    {
        int p = array.Length;
        for (int n = p - 1; n > 0; n--) {
            int r = rand.Next(0, n);
            int t = array[r];
            array[r] = array[n];
            array[n] = t;
        }
        return array;
    }

    public void SetUserID(int ID)
    {
      userID = ID;

      // OLD METHOD (first 10 are ML)
      //
      // if (ID < 11) {
      //   cueAidType = "ML";
      // } else if (ID >= 11 && ID < 21){
      //   cueAidType = "WOZ";
      // } else {
      //   cueAidType = cueAidTypes[ID % 2];
      // }

      // NEW METHOD (even odd switch off)
      //
      cueAidType = cueAidTypes[ID % 2];

      // FUTURE METHOD (ALL WOZ -- NO ML) just uncomment below and comment out the "new method" above
      //
      // cueAidType = "WOZ";

      dataLogger.CUE_ASSIST_TYPE = cueAidType.ToString();
      trialOdering = trialOderings[ID % 3];
      dataLogger.FIRST_CUE = trialOdering.Item1;
      for (int j = 3; j < 43; j++){
        allTrials[j] = trialOdering.Item1 == "CUE" ? cueObjectOrdering[j-3] : controlObjectOrdering[j-3];
      }
      for (int j = 46; j < 86; j++){
        allTrials[j] = trialOdering.Item1 == "CUE" ? controlObjectOrdering[j-46] : cueObjectOrdering[j-46];
      }
      var tmpStr = "";
      tmpStr += "User ID: "+ID.ToString()+"\n";
      tmpStr += "Cue Aid: "+cueAidType.ToString()+"\n";
      tmpStr += "Condition Odering: "+trialOdering.ToString()+"\n";
      tmpStr += "Object Ordering: [";
      foreach (int ObjNum in allTrials) {
        tmpStr += ObjNum.ToString()+", ";
      }
      tmpStr += "]\n";
      dataLogger.INFO = tmpStr;
    }

    public void LoadBlocks()
    {
      void helperFunction(string name, GameObject go, ref GameObject obj) {
        var pos = helperFunctions.StringToVector3(PlayerPrefs.GetString("pos_"+name, "ERROR"));
        var rot = helperFunctions.StringToQuaternion(PlayerPrefs.GetString("rot_"+name, "ERROR"));
        obj = Instantiate(go, stateManager.environment.transform);
        obj.SetActive(true);
        obj.transform.localPosition = pos;
        obj.transform.localRotation = rot;
        obj.transform.Find("Highlight Sphere").GetComponent<Outline>().enabled = false;
        obj.transform.Find("Highlight Sphere").GetComponent<Outline>().OutlineColor = Color.white;
        obj.transform.Find("Highlight Sphere").gameObject.AddComponent<SphereCollider>();
        obj.name = name;
      }

      foreach (Transform child in stateManager.environment.transform) {
        GameObject.Destroy(child.gameObject);
      }
      for (int i = 0; i < 3; i++) {
        helperFunction(PracticeObjectNames[i], PracticeObjects[i], ref CurrPracticeObjects[i]);
      }
      for (int i = 0; i < 40; i++) {
        helperFunction(ObjectNames[i], Objects[i], ref CurrObjects[i]);
      }
    }

    void Update()
    {
      if (ObjectNumber > 0) {
        dataLogger.TRIAL_NUMBER = trialIndex.ToString();
        dataLogger.TIME_S += Time.deltaTime;
        dataLogger.TIME_MS += Time.deltaTime * 1000;
        dataLogger.CURR_TIME_S += Time.deltaTime;
        dataLogger.CURR_TIME_MS += Time.deltaTime * 1000;
        dataLogger.LOC_X = Camera.main.transform.position.x.ToString();
        dataLogger.LOC_Y = Camera.main.transform.position.y.ToString();
        dataLogger.LOC_Z = Camera.main.transform.position.z.ToString();
        dataLogger.ROT_X = Camera.main.transform.rotation.x.ToString();
        dataLogger.ROT_Y = Camera.main.transform.rotation.y.ToString();
        dataLogger.ROT_Z = Camera.main.transform.rotation.z.ToString();
        dataLogger.OBJ_X = CurrObjects[ObjectNumber - 1].transform.position.x.ToString();
        dataLogger.OBJ_Y = CurrObjects[ObjectNumber - 1].transform.position.y.ToString();
        dataLogger.OBJ_Z = CurrObjects[ObjectNumber - 1].transform.position.z.ToString();
        dataLogger.UpdateTransform();
        if (dataLogger.SELECT == "true") dataLogger.SELECT = "false";
      }
    }

    private IEnumerator Countdown(int duration = 5)
    {
      while (duration > 0) {
        CountdownText.text = string.Format("Continuing in {0}...", duration);
        yield return new WaitForSeconds (1);
        duration--;
      }
      if (duration <= 0) {
        CountdownText.text = "Press the trigger to continue...";
        guiManager.ShowControllerArrow(true);
        guiManager.SetControllerArrowText("CONTINUE");
        stateManager.step = "RELEASE";
      }
    }

    public void ControllerSelect()
    {
      dataLogger.SELECT = "true";
      if (selected_object != null) {
        if (ObjectNumber > 0) {
          dataLogger.SELECTED_OBJ_NUM = selected_object.name;
          dataLogger.UpdateTiming();
        }
        selected_object.GetComponent<HighlightManager>().Disable();
        if (trialIndex + 1 == 86) {
          guiManager.ShowEndScreen();
          dataLogger.CloseFiles();
        }
        ObjectNumber = allTrials[++trialIndex];
        dataLogger.OBJ_NUM = ObjectNumber.ToString();
        dataLogger.CUE_TYPE = trialIndex < 43 ? trialOdering.Item1.ToString() : trialOdering.Item2.ToString();
        if (trialIndex == 43) {
          guiManager.ShowBreakScreen();
        }
        else StartHoldScreen(ObjectNumber);
        selected_object = null;
      }
    }

    public void StartHoldScreen(int ObjectNumber)
    {
      SearchTargetIcon.sprite = ObjectImages[ObjectNumber + (ObjectNumber < 0 ? 3 : 2)];
      HoldScreenPanel.SetActive(true);
      guiManager.ShowHoldScreen();
      guiManager.ShowControllerArrow(false);
      stateManager.step = "HOLD";
      cueManager.HideCursor();
      cueManager.HideArrow();
      StartCoroutine(Countdown(DisplayTime));
    }

    public void StopHoldScreen()
    {
      dataLogger.CURR_TIME_S = 0.0f;
      dataLogger.CURR_TIME_MS = 0.0f;
      HoldScreenPanel.SetActive(false);
      guiManager.ShowControllerArrow(false);
      stateManager.step = "SEARCH";
      if (trialIndex < 43 && trialOdering.Item1 == "CUE" || trialIndex >= 43 && trialOdering.Item1 == "CONTROL") { //CUE
        cueManager.ShowArrow();
      }
      else if (trialIndex >= 43 && trialOdering.Item1 == "CUE" || trialIndex < 43 && trialOdering.Item1 == "CONTROL") { //CONTROL
        cueManager.ShowCursor();
      }

    }
  }

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Security.Cryptography;

public class ExperimentManager : MonoBehaviour
{
    public GameObject HoldScreenPanel;
    public Transform controllerTransform;
    public SpriteRenderer SearchTargetIcon;
    public Text CountdownText;
    public GameObject selected_object = null;
    public HighlightManager selected_HM = null;
    public Outline selected_HM_Outline = null;
    public StateManager stateManager;
    public CueManager cueManager;
    public GuiManager guiManager;
    public HelperFunctions helperFunctions;
    public DataLogger dataLogger;

    public bool resetMap = false;
    public int userID;
    public int ObjectNumber = 1;
    public int DisplayTime = 30; // seconds to hold while seeing icon of target
    public string[] PracticeObjectNames = new string[]{"ObjectA", "ObjectB", "ObjectC"};
    public string[] ObjectNames = new string[40];
    public (string, string) trialOdering;
    public Sprite[] ObjectImages;
    public GameObject[] Objects;
    public GameObject[] PracticeObjects;
    public GameObject[] CurrObjects;
    public GameObject[] CurrPracticeObjects;

    public int trialIndex = 0;
    int[] allTrials = new int[86];
    public string cueAidType;
    int[] unshuffledList = new int[40];
    int[] cueObjectOrdering = new int[40];
    int[] controlObjectOrdering = new int[40];
    string[] cueAidTypes = new string[]{"WOZ", "ML"}; // Alternates every participant, participant will only see one type
    (string, string)[] trialOderings = new (string, string)[]{("CONTROL", "CUE"), ("CUE", "CONTROL"), ("CUE", "CONTROL")};
    private System.Random _random = new System.Random();


    void Awake()
    {
      for (int i = 0; i < 40; i++){
        ObjectNames[i] = string.Format("Object{0}", i + 1);
        unshuffledList[i] = i + 1;
      }
      cueObjectOrdering = Shuffle(_random, (int[]) unshuffledList.Clone());
      controlObjectOrdering = Shuffle(_random, (int[]) unshuffledList.Clone());
      allTrials[0] = -3;
      allTrials[1] = -2;
      allTrials[2] = -1;
      allTrials[43] = -3;
      allTrials[44] = -2;
      allTrials[45] = -1;
      if (resetMap) {
        PlayerPrefs.SetInt("saved_map", 0);
        PlayerPrefs.Save();
      }
    }

    int[] Shuffle(System.Random rand, int[] array)
    {
        int p = array.Length;
        for (int n = p - 1; n > 0; n--) {
            int r = rand.Next(0, n);
            int t = array[r];
            array[r] = array[n];
            array[n] = t;
        }
        return array;
    }

    public void SetUserID(int ID)
    {
      userID = ID;
      if (ID < 11) {
        cueAidType = "ML";
      } else if (ID >= 11 && ID < 21){
        cueAidType = "WOZ";
      } else {
        cueAidType = cueAidTypes[ID % 2];
      }
      dataLogger.CUE_ASSIST_TYPE = cueAidType.ToString();
      trialOdering = trialOderings[ID % 3];
      dataLogger.FIRST_CUE = trialOdering.Item1;
      for (int j = 3; j < 43; j++){
        allTrials[j] = trialOdering.Item1 == "CUE" ? cueObjectOrdering[j-3] : controlObjectOrdering[j-3];
      }
      for (int j = 46; j < 86; j++){
        allTrials[j] = trialOdering.Item1 == "CUE" ? controlObjectOrdering[j-46] : cueObjectOrdering[j-46];
      }
      var tmpStr = "";
      tmpStr += "User ID: "+ID.ToString()+"\n";
      tmpStr += "Cue Aid: "+cueAidType.ToString()+"\n";
      tmpStr += "Condition Odering: "+trialOdering.ToString()+"\n";
      tmpStr += "Object Ordering: [";
      foreach (int ObjNum in allTrials) {
        tmpStr += ObjNum.ToString()+", ";
      }
      tmpStr += "]\n";
      dataLogger.INFO = tmpStr;
    }

    public void LoadBlocks()
    {
      void helperFunction(string name, GameObject go, ref GameObject obj) {
        var pos = helperFunctions.StringToVector3(PlayerPrefs.GetString("pos_"+name, "ERROR"));
        var rot = helperFunctions.StringToQuaternion(PlayerPrefs.GetString("rot_"+name, "ERROR"));
        obj = Instantiate(go, stateManager.environment.transform);
        obj.SetActive(true);
        obj.transform.localPosition = pos;
        obj.transform.localRotation = rot;
        obj.transform.Find("Highlight Sphere").GetComponent<Outline>().enabled = false;
        obj.transform.Find("Highlight Sphere").GetComponent<Outline>().OutlineColor = Color.white;
        obj.transform.Find("Highlight Sphere").gameObject.AddComponent<SphereCollider>();
        obj.name = name;
      }

      foreach (Transform child in stateManager.environment.transform) {
        GameObject.Destroy(child.gameObject);
      }
      for (int i = 0; i < 3; i++) {
        helperFunction(PracticeObjectNames[i], PracticeObjects[i], ref CurrPracticeObjects[i]);
      }
      for (int i = 0; i < 40; i++) {
        helperFunction(ObjectNames[i], Objects[i], ref CurrObjects[i]);
      }
    }

    void Update()
    {
      if (ObjectNumber > 0) {
        dataLogger.TRIAL_NUMBER = trialIndex.ToString();
        dataLogger.TIME_S += Time.deltaTime;
        dataLogger.TIME_MS += Time.deltaTime * 1000;
        dataLogger.CURR_TIME_S += Time.deltaTime;
        dataLogger.CURR_TIME_MS += Time.deltaTime * 1000;
        dataLogger.LOC_X = Camera.main.transform.position.x.ToString();
        dataLogger.LOC_Y = Camera.main.transform.position.y.ToString();
        dataLogger.LOC_Z = Camera.main.transform.position.z.ToString();
        dataLogger.ROT_X = Camera.main.transform.rotation.x.ToString();
        dataLogger.ROT_Y = Camera.main.transform.rotation.y.ToString();
        dataLogger.ROT_Z = Camera.main.transform.rotation.z.ToString();
        dataLogger.OBJ_X = CurrObjects[ObjectNumber - 1].transform.position.x.ToString();
        dataLogger.OBJ_Y = CurrObjects[ObjectNumber - 1].transform.position.y.ToString();
        dataLogger.OBJ_Z = CurrObjects[ObjectNumber - 1].transform.position.z.ToString();
        dataLogger.UpdateTransform();
        if (dataLogger.SELECT == "true") dataLogger.SELECT = "false";
      }
    }

    private IEnumerator Countdown(int duration = 5)
    {
      while (duration > 0) {
        CountdownText.text = string.Format("Continuing in {0}...", duration);
        yield return new WaitForSeconds (1);
        duration--;
      }
      if (duration <= 0) {
        CountdownText.text = "Press the trigger to continue...";
        guiManager.ShowControllerArrow(true);
        guiManager.SetControllerArrowText("CONTINUE");
        stateManager.step = "RELEASE";
      }
    }

    public void ControllerSelect()
    {
      dataLogger.SELECT = "true";
      if (selected_object != null) {
        if (ObjectNumber > 0) {
          dataLogger.SELECTED_OBJ_NUM = selected_object.name;
          dataLogger.UpdateTiming();
        }
        selected_object.GetComponent<HighlightManager>().Disable();
        if (trialIndex + 1 == 86) {
          guiManager.ShowEndScreen();
          dataLogger.CloseFiles();
        }
        ObjectNumber = allTrials[++trialIndex];
        dataLogger.OBJ_NUM = ObjectNumber.ToString();
        dataLogger.CUE_TYPE = trialIndex < 43 ? trialOdering.Item1.ToString() : trialOdering.Item2.ToString();
        if (trialIndex == 43) {
          guiManager.ShowBreakScreen();
        }
        else StartHoldScreen(ObjectNumber);
        selected_object = null;
      }
    }

    public void StartHoldScreen(int ObjectNumber)
    {
      SearchTargetIcon.sprite = ObjectImages[ObjectNumber + (ObjectNumber < 0 ? 3 : 2)];
      HoldScreenPanel.SetActive(true);
      guiManager.ShowHoldScreen();
      guiManager.ShowControllerArrow(false);
      stateManager.step = "HOLD";
      cueManager.HideCursor();
      cueManager.HideArrow();
      StartCoroutine(Countdown(DisplayTime));
    }

    public void StopHoldScreen()
    {
      dataLogger.CURR_TIME_S = 0.0f;
      dataLogger.CURR_TIME_MS = 0.0f;
      HoldScreenPanel.SetActive(false);
      guiManager.ShowControllerArrow(false);
      stateManager.step = "SEARCH";
      if (trialIndex < 43 && trialOdering.Item1 == "CUE" || trialIndex >= 43 && trialOdering.Item1 == "CONTROL") { //CUE
        cueManager.ShowArrow();
      }
      else if (trialIndex >= 43 && trialOdering.Item1 == "CUE" || trialIndex < 43 && trialOdering.Item1 == "CONTROL") { //CONTROL
        cueManager.ShowCursor();
      }

    }
  }
*/
