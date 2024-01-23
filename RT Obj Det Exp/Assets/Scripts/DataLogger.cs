using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System;
using System.Text;

public class DataLogger : MonoBehaviour
{
  public ExperimentManager experimentManager;
  public string USER_ID;
  public string TRIAL_NUMBER;
  public string CUE_ASSIST_TYPE;
  public string CUE_TYPE;
  public string FIRST_CUE;
  public float TIME_S, TIME_MS;
  public float CURR_TIME_S, CURR_TIME_MS;
  public string OBJ_NUM;
  public string SELECT;
  public string SELECTED_OBJ_NUM;
  public string LOC_X, LOC_Y, LOC_Z;
  public string ROT_X, ROT_Y, ROT_Z;
  public string OBJ_X, OBJ_Y, OBJ_Z;
  public string CUE_X, CUE_Y, CUE_Z;
  public string CUE_CONF, CUE_DIST;
  public string INFO;

  string TransformDataHeader = "USER_ID, TRIAL_NUMBER, CUE_ASSIST_TYPE, CUE_TYPE, FIRST_CUE, TIME_S, TIME_MS, OBJ_NUM, SELECT, SELECTED_OBJ_NUM, LOC_X, LOC_Y, LOC_Z, ROT_X, ROT_Y, ROT_Z, OBJ_X, OBJ_Y, OBJ_Z, CUE_X, CUE_Y, CUE_Z, CUE_CONF, CUE_DIST";
  string TrialTimingHeader = "USER_ID, TRIAL_NUMBER, CUE_ASSIST_TYPE, CUE_TYPE, FIRST_CUE, OBJ_NUM, SELECTED_OBJ_NUM, CORRECT, TIME_S, TIME_MS";
  string TransformDataName, TrialTimingName, ExperimentInfoName;
  string[] TransformDataCache = new string[23];
  string[] TrialTimingCache = new string[9];
  FileStream fs_transform, fs_timing;
  StreamWriter sw_transform, sw_timing;
  StringBuilder sb_transform, sb_timing;

  public void SetUp(int ID)
  {
    USER_ID = ID.ToString();
    SELECT = "false";
    var time = System.DateTime.Now.ToString("hh_mm_ss_tt");
    var date = System.DateTime.Now.ToString("MM_dd_yyyy");
    var desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
    TransformDataName = desktop+"\\ExperimentData\\Subject"+ID.ToString()+"\\TransformData_"+time+"_"+date+".csv";
    TrialTimingName = desktop+"\\ExperimentData\\Subject"+ID.ToString()+"\\TrialTiming_"+time+"_"+date+".csv";
    ExperimentInfoName = desktop+"\\ExperimentData\\Subject"+ID.ToString()+"\\ExperimentInfo_"+time+"_"+date+".txt";
    fs_transform = new FileStream(TransformDataName, FileMode.Append, FileAccess.Write);
    fs_timing = new FileStream(TrialTimingName, FileMode.Append, FileAccess.Write);
    sw_transform = new StreamWriter(fs_transform);
    sw_timing = new StreamWriter(fs_timing);
    sb_transform = new StringBuilder();
    sb_timing = new StringBuilder();
    WriteHeaders();
  }

  public void WriteHeaders() {
    sw_transform.WriteLine(TransformDataHeader);
    sw_timing.WriteLine(TrialTimingHeader);
    using (FileStream fs_info = new FileStream(ExperimentInfoName, FileMode.Append, FileAccess.Write)) {
        using (StreamWriter sw_info = new StreamWriter(fs_info)) {
            sw_info.WriteLine(INFO);
            sw_info.Close();
        }
    }
  }

  public void LogCuePos(Vector3 pos, float conf)
  {
    CUE_X = pos.x.ToString();
    CUE_Y = pos.y.ToString();
    CUE_Z = pos.z.ToString();
    CUE_CONF = conf.ToString();
  }

  public void CloseFiles()
  {
    sw_transform.Close();
    sw_timing.Close();
  }

  public void UpdateTransform() {
    sb_transform.Length = 0;
    sb_transform.Append(USER_ID).Append(",").Append(TRIAL_NUMBER).Append(",");
    sb_transform.Append(CUE_ASSIST_TYPE).Append(",").Append(CUE_TYPE).Append(",");
    sb_transform.Append(FIRST_CUE).Append(",").Append(TIME_S.ToString()).Append(",");
    sb_transform.Append(TIME_MS.ToString()).Append(",").Append(OBJ_NUM).Append(",");
    sb_transform.Append(SELECT).Append(",").Append(SELECTED_OBJ_NUM).Append(",");
    sb_transform.Append(LOC_X).Append(",").Append(LOC_Y).Append(",").Append(LOC_Z);
    sb_transform.Append(",").Append(ROT_X).Append(",").Append(ROT_Y).Append(",").Append(ROT_Z);
    sb_transform.Append(",").Append(OBJ_X).Append(",").Append(OBJ_Y).Append(",").Append(OBJ_Z);
    sb_transform.Append(",").Append(CUE_X).Append(",").Append(CUE_Y).Append(",").Append(CUE_Z).Append(",").Append(CUE_CONF).Append(",").Append(CUE_DIST);
    sw_transform.WriteLine(sb_transform.ToString());
  }

  public void UpdateTiming() {
    sb_timing.Length = 0;
    sb_timing.Append(USER_ID).Append(",").Append(TRIAL_NUMBER).Append(",");
    sb_timing.Append(CUE_ASSIST_TYPE).Append(",").Append(CUE_TYPE).Append(",");
    sb_timing.Append(FIRST_CUE).Append(",").Append(OBJ_NUM).Append(",");
    sb_timing.Append(SELECTED_OBJ_NUM).Append(",").Append(SELECTED_OBJ_NUM == "Object"+OBJ_NUM ? "true" : "false");
    sb_timing.Append(",").Append(CURR_TIME_S).Append(",").Append(CURR_TIME_MS);
    sw_timing.WriteLine(sb_timing.ToString());
  }


}
