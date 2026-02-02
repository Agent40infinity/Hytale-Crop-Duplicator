using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using SimpleFileBrowser;

public class EntityDuplicationUI : MonoBehaviour
{
    public TMP_InputField entityRef;
    public TMP_InputField entityOutput;

    public GameObject successText;

    public string modPrefix = "More_Chillies";

    private string entityName;
    private string entityReference;
    private string outputPath;
    private bool generateModels = true;
    private int cropType = 0;
    private int cropStage = 6;

    private string serverDir => outputPath + "/Server";
    private string languageDir => serverDir + "/Languages";
    private string enDir => languageDir + "/en-US";
    private string serverLang => languageDir + "/en-US/server.lang";
    private string itemDir => serverDir + "/Item";
    private string itemsDir => itemDir + "/Items";
    private string plantsDir => itemsDir + "/Plants";
    private string seedsDir => itemsDir + "/Seeds";
    private string dropsDir => serverDir + "/Drops";
    private string newDropDir => dropsDir + "/" + entityName.Replace("More_Chillies_", "").Replace("_", "");
    private string commonDir => outputPath + "/Common";
    private string resourcesDir => commonDir + "/Resources";
    private string newResourceDir => resourcesDir + "/" + entityName.Replace("More_Chillies_", "").Replace("_", "");
    private string seedbagDir => resourcesDir + "/SeedBag_Textures";
    private string newSeedbagDir => seedbagDir + "/" + entityName.Replace("More_Chillies_", "").Replace("_", "");

    private string modelFile => "{\n  'lod': 'auto',\n  'nodes': []\n}";

public void Start()
    {
        successText.SetActive(false);
    }

    public void SetEntityName(string name) { entityName = name; }

    public void ToggleModels(bool toggle) { generateModels = toggle; }

    public void CropType(int type) { cropType = type; }

    public void CropStage(int stage) { cropStage = stage; }

    public void GetReferenceObj(string _)
    {
        if (FileBrowser.IsOpen)
        {
            return;
        }

        SimpleFileBrowser.FileBrowser.ShowLoadDialog(OnSuccessGetFile, OnFailed, FileBrowser.PickMode.Files);
    }

    public void OnSuccessGetFile(string[] filePath)
    {
        entityReference = filePath[0];
        entityRef.text = entityReference;
    }

    public void GetOutputPath(string _)
    {
        if (FileBrowser.IsOpen)
        {
            return;
        }

        SimpleFileBrowser.FileBrowser.ShowLoadDialog(OnSuccessGetFolder, OnFailed, FileBrowser.PickMode.Folders);
    }

    public void OnSuccessGetFolder(string[] filePath)
    {
        outputPath = filePath[0];
        entityOutput.text = outputPath;
    }

    public void OnFailed() { return; }

    public void SubmitAndCreate()
    {
        if (string.IsNullOrEmpty(entityName) || string.IsNullOrEmpty(entityReference) || string.IsNullOrEmpty(outputPath))
        {
            return;
        }

        StartCoroutine(CreateFiles());
    }

    public IEnumerator CreateFiles()
    {
        if (!Directory.Exists(serverDir))
        {
            Directory.CreateDirectory(serverDir);
        }

        if (!Directory.Exists(languageDir))
        {
            Directory.CreateDirectory(languageDir);
        }

        if (!Directory.Exists(enDir))
        {
            Directory.CreateDirectory(enDir);
        }

        if (!File.Exists(serverLang))
        {
            File.Create(serverLang);
        }

        using (FileStream fs = new FileStream(serverLang, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    var text = sr.ReadToEnd();

                    if (!text.Contains("items." + entityName + ".name"))
                    {
                        sw.WriteLine("items." + entityName + ".name = '");
                    }

                    if (!text.Contains("items." + entityName + ".description"))
                    {
                        sw.WriteLine("items." + entityName + ".description = '");
                    }
                }
            }
        }

        if (!Directory.Exists(commonDir))
        {
            Directory.CreateDirectory(commonDir);
        }

        if (cropType == 0 || cropType == 1) // Normal Crop
        { 
            
        }

        if (cropType == 0 || cropType == 2) // Eternal Crop
        { 
            
        }

        successText.SetActive(true);
        yield return new WaitForSeconds(2);
        successText.SetActive(false);
    }
}