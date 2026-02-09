using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using SimpleFileBrowser;
using System.Text.RegularExpressions;
using System.Linq;

public class EntityDuplicationUI : MonoBehaviour
{
    public TMP_InputField modNameInput;
    public TMP_InputField entityNameInput;
    public TMP_InputField entityRefInput;
    public TMP_InputField entityOutputInput;

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
    private int cropStage = 4;

    private string entityReferenceFolder => entityReference.Split("\\").Last().Replace("Plant_Crop_", "").Replace("_Item.json", "");
    private string assetsRefDir => entityReference.Split("\\Server\\")[0];
    private string refPlantDir => string.Format("{0}{1}{2}{3}{4}{5}", assetsRefDir, "/Server/Item/Items/Plant/Crop/", entityReferenceFolder, "/Plant_Crop_", entityReferenceFolder, "_Block");
    private string refSeedDir => string.Format("{0}{1}{2}{3}{4}", assetsRefDir, "/Server/Item/Items/Plant/Crop/", entityReferenceFolder, "/Plant_Seeds_", entityReferenceFolder);
    private string refDropsDir => string.Format("{0}{1}{2}{3}{4}", assetsRefDir, "/Server/Drops/Crop/", entityReferenceFolder, "/Drops_Plant_Crop_", entityReferenceFolder);
    private string refResourceDir => assetsRefDir + "/Common/Resources/";
    private string refIngrediantsDir => refResourceDir + "Ingredients/";
    private string refModelDir => refIngrediantsDir + entityReferenceFolder;
    private string refSeedbagTextureDir => string.Format("{0}{1}{2}", refResourceDir, "Plants/SeedBag_Textures/", entityReferenceFolder);

    private string serverDir => outputPath + "/Server";
    private string languageDir => serverDir + "/Languages";
    private string enDir => languageDir + "/en-US";
    private string serverLang => languageDir + "/en-US/server.lang";
    private string itemDir => serverDir + "/Item";
    private string itemsDir => itemDir + "/Items";
    private string plantsDir => itemsDir + "/Plants";
    private string seedsDir => itemsDir + "/Seeds"; 
    private string dropsDir => serverDir + "/Drops";
    private string newDropDir => dropsDir + "/" + entityNameFolder;
    private string commonDir => outputPath + "/Common";
    private string resourcesDir => commonDir + "/Resources";
    private string newResourceDir => resourcesDir + "/" + entityNameFolder;
    private string newModelDir => newResourceDir + "/" + entityName;
    private string seedbagTextureDir => newResourceDir + "/" + entityName + "_Seedbag.png";
    private string eternalSeedbagTextureDir => newResourceDir + "/" + entityName + "_Eternal_Seedbag.png";

    const string eternalString = "_Eternal.json";
    const string modelExtension = ".blockymodel";

    public void Start()
    {
        successText.SetActive(false);
    }

    public void Awake()
    {
        modNameInput.text = modName = PlayerPrefs.GetString("ModName");
        entityNameInput.text = entityName = PlayerPrefs.GetString("EntityName");
        entityRefInput.text = entityReference = PlayerPrefs.GetString("Reference");
        entityOutputInput.text = outputPath = PlayerPrefs.GetString("OutputPath");
    }

    public void OnDestroy()
    {
        PlayerPrefs.SetString("ModName", modName);
        PlayerPrefs.SetString("EntityName", entityName);
        PlayerPrefs.SetString("Reference", entityReference);
        PlayerPrefs.SetString("OutputPath", outputPath);
    }

    public void SetModName(string name) { modName = name; }

    public void SetEntityName(string name) { entityName = name; }

    public void ToggleModels(bool toggle) { generateModels = toggle; }

    public void CropType(int type) { cropType = type; }

    public void CropStage(int stage) { cropStage = stage + 1; }

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
        entityRefInput.text = entityReference;
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
        entityOutputInput.text = outputPath;
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

        using (FileStream fs = new FileStream(serverLang, FileMode.OpenOrCreate, FileAccess.ReadWrite))
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

                    if (!text.Contains("items." + uniqueID + "_Plant.name"))
                    {
                        sw.WriteLine("items." + uniqueID + "_Plant.name = '");
                    }

                    if (!text.Contains("items." + uniqueID + "_Plant.description"))
                    {
                        sw.WriteLine("items." + uniqueID + "_Plant.description = '");
                    }

                    if (!text.Contains("items." + uniqueID + "_Seeds.name"))
                    {
                        sw.WriteLine("items." + uniqueID + "_Seeds.name = '");
                    }

                    if (!text.Contains("items." + uniqueID + "_Seeds.description"))
                    {
                        sw.WriteLine("items." + uniqueID + "_Seeds.description = '");
                    }

                    if (!text.Contains("items." + uniqueID + "_Seeds_Eternal.name"))
                    {
                        sw.WriteLine("items." + uniqueID + "_Seeds_Eternal.name = '");
                    }

                    if (!text.Contains("items." + uniqueID + "_Seeds_Eternal.description"))
                    {
                        sw.WriteLine("items." + uniqueID + "_Seeds_Eternal.description = '");
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

        using (FileStream fs = new FileStream(itemPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
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

        if (!Directory.Exists(dropsDir))
        {
            Directory.CreateDirectory(dropsDir);
        }

        if (!Directory.Exists(newDropDir))
        {
            Directory.CreateDirectory(newDropDir);
        }

        if (!Directory.Exists(commonDir))
        {
            Directory.CreateDirectory(commonDir);
        }

        if (!Directory.Exists(resourcesDir))
        {
            Directory.CreateDirectory(resourcesDir);
        }

        if (!Directory.Exists(newResourceDir))
        {
            Directory.CreateDirectory(newResourceDir);
        }

        var createdPlantDir = plantsDir + "/" + uniqueID + "_Plant";
        var createdSeedsDir = seedsDir + "/" + uniqueID + "_Seeds";
        var createdDropDir = newDropDir + "/" + modName + "_Drops_" + entityName;

        if (cropType == 0 || cropType == 1) // Normal Crop
        {
            if (!File.Exists(createdPlantDir + ".json"))
            {
                File.Copy(refPlantDir + ".json", createdPlantDir + ".json", false);
            }

            if (!File.Exists(createdSeedsDir + ".json"))
            {
                File.Copy(refSeedDir + ".json", createdSeedsDir + ".json", false);
            }

            if (!File.Exists(createdDropDir + "_Block.json"))
            {
                File.Copy(refDropsDir + "_Block.json", createdDropDir + "_Block.json", false);
            }

            for (int i = 1; i <= cropStage; i++)
            {
                if (!File.Exists(createdDropDir + "_Stage" + i + ".json"))
                {
                    File.Copy(refDropsDir + "_Stage" + i + ".json", createdDropDir + "_Stage" + i + ".json", false);
                }
            }

            if (!File.Exists(createdDropDir + "_StageFinal.json"))
            {
                File.Copy(refDropsDir + "_StageFinal.json", createdDropDir + "_StageFinal.json", false);
            }

            if (!File.Exists(createdDropDir + "_StageFinal_Harvest.json"))
            {
                File.Copy(refDropsDir + "_StageFinal_Harvest.json", createdDropDir + "_StageFinal_Harvest.json", false);
            }

            if (!File.Exists(seedbagTextureDir))
            {
                File.Copy(refSeedbagTextureDir + ".png", seedbagTextureDir, false);
            }
        }

        if (cropType == 0 || cropType == 2) // Eternal Crop
        {
            if (!File.Exists(createdPlantDir + eternalString))
            {
                File.Copy(refPlantDir + eternalString, createdPlantDir + eternalString, false);
            }

            if (!File.Exists(createdSeedsDir + eternalString))
            {
                File.Copy(refSeedDir + eternalString, createdSeedsDir + eternalString, false);
            }

            if (!File.Exists(createdDropDir + "_Eternal_Block.json"))
            {
                File.Copy(refDropsDir + "_Eternal_Block.json", createdDropDir + "_Eternal_Block.json", false);
            }

            for (int i = 1; i <= cropStage; i++)
            {
                if (!File.Exists(createdDropDir + "_Eternal_Stage" + i + ".json"))
                {
                    File.Copy(refDropsDir + "_Eternal_Stage" + i + ".json", createdDropDir + "_Eternal_Stage" + i + ".json", false);
                }
            }

            if (!File.Exists(createdDropDir + "_Eternal_StageFinal.json"))
            {
                File.Copy(refDropsDir + "_Eternal_StageFinal.json", createdDropDir + "_Eternal_StageFinal.json", false);
            }

            if (!File.Exists(createdDropDir + "_Eternal_StageFinal_Harvest.json"))
            {
                File.Copy(refDropsDir + "_Eternal_StageFinal_Harvest.json", createdDropDir + "_Eternal_StageFinal_Harvest.json", false);
            }

            if (!File.Exists(eternalSeedbagTextureDir))
            {
                File.Copy(refSeedbagTextureDir + "_Eternal.png", eternalSeedbagTextureDir, false);
            }
        }

        if (!File.Exists(newModelDir + "_Texture.png"))
        {
            File.Copy(refModelDir + "_Texture.png", newModelDir + "_Texture.png", false);
        }

        if (generateModels)
        {
            if (!File.Exists(newModelDir + modelExtension))
            {
                File.Copy(refModelDir + modelExtension, newModelDir + modelExtension, false);
            }

            for (int i = 0; i <= cropStage; i++)
            {
                var stageIteration = string.Format("_0{0}", i + 1);
                var modelStage = newModelDir + stageIteration + modelExtension;

                if (!File.Exists(modelStage))
                {
                    File.Copy(refModelDir + stageIteration + modelExtension, modelStage, false);
                }
            }
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