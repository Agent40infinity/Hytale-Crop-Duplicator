using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using SimpleFileBrowser;
using System.Text.RegularExpressions;

public class EntityDuplicationUI : MonoBehaviour
{
    public TMP_InputField entityRef;
    public TMP_InputField entityOutput;

    public GameObject successText;

    private string modName;
    private string entityName;

    private string uniqueID => string.Format("{0}_{1}", modName, entityName);
    private string entityNameFolder => entityName.Replace("_", "");
    private string modNameCategory => modName.Replace("_", "");

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

    public void SetModName(string name) { modName = name; }

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

        using (FileStream fs = new FileStream(serverLang, FileMode.Open, FileAccess.ReadWrite))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    var text = sr.ReadToEnd();

                    if (!text.Contains("items." + uniqueID + ".name"))
                    {
                        sw.WriteLine("items." + uniqueID + ".name = '");
                    }

                    if (!text.Contains("items." + uniqueID + ".description"))
                    {
                        sw.WriteLine("items." + uniqueID + ".description = '");
                    }
                }
            }
        }

        if (!Directory.Exists(itemDir))
        {
            Directory.CreateDirectory(itemDir);
        }

        if (!Directory.Exists(itemsDir))
        {
            Directory.CreateDirectory(itemsDir);
        }

        var itemPath = Path.Combine(itemsDir, uniqueID) + ".json";
        var splitName = entityReference.Split("_");
        var itemNameNoTag = splitName[splitName.Length - 2];

        if (!File.Exists(itemPath))
        {
            File.Copy(entityReference, itemPath, false);
        }

        using (FileStream fs = new FileStream(itemPath, FileMode.Open, FileAccess.ReadWrite))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                var text = sr.ReadToEnd();

                using (StreamWriter sw = new StreamWriter(fs))
                {
                    text = InsertBetween(text, "\"Name\": \"server.items.", ".name\",", uniqueID);
                    text = InsertBetween(text, "\"Description\": \"server.items.", ".description\"", uniqueID);

                    text = InsertBetween(text, "\"Texture\": \"Resources/", "_Texture.png\",", entityNameFolder + "/"  + entityName);
                    text = InsertBetween(text, "Model\": \"Resources/", ".blockymodel\",", entityNameFolder + "/" + entityName);
                    sw.Write(text);
                }
            }
        }

        if (!Directory.Exists(plantsDir))
        {
            Directory.CreateDirectory(plantsDir);
        }

        if (!Directory.Exists(seedsDir))
        {
            Directory.CreateDirectory(seedsDir);
        }

        if (cropType == 0 || cropType == 1) // Normal Crop
        {

        }

        if (cropType == 0 || cropType == 2) // Eternal Crop
        {

        }

        if (!Directory.Exists(commonDir))
        {
            Directory.CreateDirectory(commonDir);
        }

        successText.SetActive(true);
        yield return new WaitForSeconds(2);
        successText.SetActive(false);
    }

    public static string InsertBetween(string sourceString, string startTag, string endTag, string insert)
    {
        Regex regex = new Regex(string.Format("{0}(.*?){1}", Regex.Escape(startTag), Regex.Escape(endTag)), RegexOptions.RightToLeft);
        return regex.Replace(sourceString, startTag + insert + endTag);
    }
}