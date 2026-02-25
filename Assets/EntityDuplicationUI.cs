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

    public TMP_Dropdown cropStagesDropdown;

    public TMP_Dropdown rarityDropdown;
    private List<string> qualitiesList = new List<string>();

    public GameObject successText;

    private string modName;
    private string entityName;

    private string entityReference;
    private string outputPath;
    private bool copyRarity = true;
    private int cropType = 0;
    private int cropStage = 4;
    private int qualityIndex = 0;

    private string uniqueID => string.Format("{0}_{1}", modName, entityName);
    private string seedID => string.Format("{0}_{1}", modName, seedName);
    private string plantID => string.Format("{0}_{1}", modName, plantName);
    private string seedName => string.Format("{0}_{1}", entityName, "Seeds");
    private string plantName => string.Format("{0}_{1}", entityName, "Plant");
    private string entityNameFolder => entityName.Replace("_", "");
    private string modNameCategory => modName.Replace("_", "");

    private string entityReferenceFolder => entityReference.Split("\\").Last().Replace("Plant_Crop_", "").Replace("_Item.json", "");
    private string assetsRefDir => entityReference.Split("\\Server\\")[0];
    private string refRaritiesDir => assetsRefDir + "\\Server\\Item\\Qualities\\";
    private string refPlantDir => string.Format("{0}{1}{2}{3}{4}{5}", assetsRefDir, "/Server/Item/Items/Plant/Crop/", entityReferenceFolder, "/Plant_Crop_", entityReferenceFolder, "_Block");
    private string refSeedDir => string.Format("{0}{1}{2}{3}{4}", assetsRefDir, "/Server/Item/Items/Plant/Crop/", entityReferenceFolder, "/Plant_Seeds_", entityReferenceFolder);
    private string refDropsDir => string.Format("{0}{1}{2}{3}{4}", assetsRefDir, "/Server/Drops/Crop/", entityReferenceFolder, "/Drops_Plant_Crop_", entityReferenceFolder);
    private string refResourceDir => assetsRefDir + "/Common/Resources/";
    private string refIngrediantsDir => refResourceDir + "Ingredients/";
    private string refModelDir => refIngrediantsDir + entityReferenceFolder;
    private string refSeedbagTextureDir => string.Format("{0}{1}{2}", refResourceDir, "Plants/SeedBag_Textures/", entityReferenceFolder);

    private string refTemplateDir => assetsRefDir + "/Server/Item/Items/Plant/Crop/_Template/";
    private string refTemplateItem => string.Format("{0}{1}{2}", refTemplateDir, "Template_Crop_Item", assetExtension);
    private string refTemplateSeeds => string.Format("{0}{1}{2}", refTemplateDir, "Template_Seeds", assetExtension);
    private string refTemplatePlant => string.Format("{0}{1}{2}", refTemplateDir, "Template_Crop_Block", assetExtension);

    private string serverDir => outputPath + "/Server";
    private string languageDir => serverDir + "/Languages";
    private string enDir => languageDir + "/en-US";
    private string serverLang => enDir + "/server.lang";
    private string itemDir => serverDir + "/Item";
    private string categoryDir => itemDir + "/Category/CreativeLibrary/";
    private string itemsDir => itemDir + "/Items";
    private string plantsDir => itemsDir + "/Plants";
    private string seedsDir => itemsDir + "/Seeds";
    private string templatesDir => itemsDir + "/_Template";
    private string dropsDir => serverDir + "/Drops";
    private string newDropDir => dropsDir + "/" + entityNameFolder;
    private string commonDir => outputPath + "/Common";
    private string resourcesDir => commonDir + "/Resources";
    private string newResourceDir => resourcesDir + "/" + entityNameFolder;
    private string newModelDir => newResourceDir + "/" + entityName;
    private string seedbagTextureDir => newResourceDir + "/" + entityName + "_Seedbag.png";
    private string eternalSeedbagTextureDir => newResourceDir + "/" + entityName + "_Eternal_Seedbag.png";

    private string langItemName => "items." + uniqueID + langName;
    private string langItemDesc => "items." + uniqueID + langDescription;
    private string langPlantName => "items." + plantID + langName;
    private string langPlantDesc => "items." + plantID + langDescription;
    private string langSeedsName => "items." + seedID + langName;
    private string langSeedsDesc => "items." + seedID + langDescription;
    private string langSeedsEternalName => "items." + seedID + eternalString + langName;
    private string langSeedsEternalDesc => "items." + seedID + eternalString + langDescription;

    const string langName = ".name";
    const string langDescription = ".description";
    const string modelExtension = ".blockymodel";
    const string assetExtension = ".json";
    const string eternalString = "_Eternal";
    const string eternalExtension = eternalString + assetExtension;

    public void Start()
    {
        successText.SetActive(false);
    }

    public void Awake()
    {
        modNameInput.text = modName = PlayerPrefs.GetString("ModName");
        entityNameInput.text = entityName = PlayerPrefs.GetString("EntityName");
        OnSuccessGetFile(new string[] { PlayerPrefs.GetString("Reference") });
        OnSuccessGetFolder(new string[] { PlayerPrefs.GetString("OutputPath") });
        cropStagesDropdown.value = cropStage = PlayerPrefs.GetInt("cropStages");
    }

    public void OnDestroy()
    {
        PlayerPrefs.SetString("ModName", modName);
        PlayerPrefs.SetString("EntityName", entityName);
        PlayerPrefs.SetString("Reference", entityReference);
        PlayerPrefs.SetString("OutputPath", outputPath);
        PlayerPrefs.SetInt("cropStages", cropStage);
    }

    public void SetModName(string name) { modName = name; }

    public void SetEntityName(string name) { entityName = name; }

    public void CropType(int type) { cropType = type; }

    public void CropStage(int stage) { cropStage = stage; }

    public void SetQuality(int index) { qualityIndex = index; }

    public void ToggleCopyRarity(bool toggle) { copyRarity = toggle; }

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

        qualitiesList.Clear();
        rarityDropdown.ClearOptions();

        var files = Directory.GetFiles(refRaritiesDir, "*.json", SearchOption.TopDirectoryOnly).ToList();

        foreach (var file in files)
        {
            qualitiesList.Add(file.Replace(refRaritiesDir, "").Replace(assetExtension, ""));
        }

        rarityDropdown.AddOptions(qualitiesList);

        rarityDropdown.gameObject.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(entityReference));
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

                    if (!text.Contains(modName + ".name"))
                    {
                        sw.WriteLine(modName + ".name = " + modName.Replace("_", " "));
                    }

                    if (!text.Contains(Strings.LangUIFood(modName)))
                    {
                        sw.WriteLine(Strings.LangUIFood(modName) + " = Food");
                    }

                    if (!text.Contains(Strings.LangUISeeds(modName)))
                    {
                        sw.WriteLine(Strings.LangUISeeds(modName) + " = Seeds");
                    }

                    if (!text.Contains(Strings.LangUIPlants(modName)))
                    {
                        sw.WriteLine(Strings.LangUIPlants(modName) + " = Plants");
                    }

                    if (!text.Contains(langItemName))
                    {
                        sw.WriteLine(langItemName + " = " + entityName.Replace("_", " "));
                    }

                    if (!text.Contains(langItemDesc))
                    {
                        sw.WriteLine(langItemDesc + " = .");
                    }

                    if (!text.Contains(langPlantName))
                    {
                        sw.WriteLine(langPlantName + " = " + plantName.Replace("_", " "));
                    }

                    if (!text.Contains(langPlantDesc))
                    {
                        sw.WriteLine(langPlantDesc + " = .");
                    }

                    if (!text.Contains(langSeedsName))
                    {
                        sw.WriteLine(langSeedsName + " = " + seedName.Replace("_", " "));
                    }

                    if (!text.Contains(langSeedsDesc))
                    {
                        sw.WriteLine(langSeedsDesc + " = .");
                    }

                    if (!text.Contains(langSeedsEternalName))
                    {
                        sw.WriteLine(langSeedsEternalName + " = " + seedName.Replace("_", " ") + string.Format(" ({0})", eternalString.Replace("_", "")));
                    }

                    if (!text.Contains(langSeedsEternalDesc))
                    {
                        sw.WriteLine(langSeedsEternalDesc + " = .");
                    }
                }
            }
        }

        if (!Directory.Exists(categoryDir))
        {
            Directory.CreateDirectory(categoryDir);
        }

        if (!File.Exists(categoryDir + modName + assetExtension))
        {
            using (FileStream fs = new FileStream(categoryDir + modName + assetExtension, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(Strings.CategoryTemplate(modName));
                }
            }
        }

        if (!Directory.Exists(templatesDir))
        {
            Directory.CreateDirectory(templatesDir);
        }

        var templateFood = templatesDir + "/" + modName + "_Template_Food" + assetExtension;
        var templateSeeds = templatesDir + "/" + modName + "_Template_Seeds" + assetExtension;
        var templatePlant = templatesDir + "/" + modName + "_Template_Plant" + assetExtension;

        if (!File.Exists(templateFood))
        {
            File.Copy(refTemplateItem, templateFood, false);
        }

        using (FileStream fs = new FileStream(templateFood, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                var text = sr.ReadToEnd();
                fs.Position = 0;

                using (StreamWriter sw = new StreamWriter(fs))
                {
                    text = InsertBetween(text, "\"Categories\": [\n    ", "\n  ],", "\"" + modName + ".Food\"");

                    sw.Write(text);
                }
            }
        }

        if (!File.Exists(templateSeeds))
        {
            File.Copy(refTemplateSeeds, templateSeeds, false);
        }

        using (FileStream fs = new FileStream(templateSeeds, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                var text = sr.ReadToEnd();
                fs.Position = 0;

                using (StreamWriter sw = new StreamWriter(fs))
                {
                    text = InsertBetween(text, "\"Categories\": [\n    ", "\n  ],", "\"" + modName + ".Seeds\"");

                    sw.Write(text);
                }
            }
        }

        if (!File.Exists(templatePlant))
        {
            File.Copy(refTemplatePlant, templatePlant, false);
        }

        using (FileStream fs = new FileStream(templatePlant, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                var text = sr.ReadToEnd();
                fs.Position = 0;

                using (StreamWriter sw = new StreamWriter(fs))
                {
                    text = InsertBetween(text, "\"Categories\": [\n    ", "\n  ],", "\"" + modName + ".Plants\"");

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

        if (!Directory.Exists(newDropDir))
        {
            Directory.CreateDirectory(newDropDir);
        }

        if (!Directory.Exists(newResourceDir))
        {
            Directory.CreateDirectory(newResourceDir);
        }

        var itemPath = Path.Combine(itemsDir, uniqueID) + assetExtension;
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
                fs.Position = 0;

                using (StreamWriter sw = new StreamWriter(fs))
                {
                    text = InsertBetween(text, "\"Name\": \"server.", "\",", langItemName);
                    text = InsertBetween(text, "\"Description\": \"server.", "\"", langItemDesc);

                    text = InsertBetween(text, "\"Parent\": \"", "\",", modName + "_Template_Food");
                    text = InsertBetween(text, "\"Quality\": \"", "\",", qualitiesList[qualityIndex]);

                    text = InsertBetween(text, "\"Texture\": \"Resources/", "_Texture.png\",", entityNameFolder + "/"  + entityName);
                    text = InsertBetween(text, "Model\": \"Resources/", ".blockymodel\",", entityNameFolder + "/" + entityName);

                    sw.Write(text);
                }
            }
        }

        var createdPlantDir = plantsDir + "/" + uniqueID + "_Plant";
        var createdSeedsDir = seedsDir + "/" + uniqueID + "_Seeds";
        var createdDropDir = newDropDir + "/" + modName + "_Drops_" + entityName;

        if (cropType == 0 || cropType == 1) // Normal Crop
        {
            if (!File.Exists(createdPlantDir + assetExtension))
            {
                File.Copy(refPlantDir + assetExtension, createdPlantDir + assetExtension, false);
            }

            using (FileStream fs = new FileStream(createdPlantDir + assetExtension, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    var text = sr.ReadToEnd();
                    text = text.Replace("\r", "");
                    fs.Position = 0;

                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        text = InsertBetween(text, "\"Name\": \"server.", "\"", langPlantName);
                        text = InsertBetween(text, "\"Parent\": \"", "\",", modName + "_Template_Plant");

                        sw.Write(text);
                    }
                }
            }

            if (!File.Exists(createdSeedsDir + assetExtension))
            {
                File.Copy(refSeedDir + assetExtension, createdSeedsDir + assetExtension, false);
            }

            using (FileStream fs = new FileStream(createdSeedsDir + assetExtension, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    var text = sr.ReadToEnd();
                    text = text.Replace("\r", "");
                    fs.Position = 0;

                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        text = InsertBetween(text, "\"Name\": \"server.", "\",", langSeedsName);
                        text = InsertBetween(text, "\"Description\": \"server.", "\"", langSeedsDesc);

                        text = InsertBetween(text, "\"Parent\": \"", "\",", modName + "_Template_Seeds");
                        text = InsertBetween(text, "\"Quality\": \"", "\",", qualitiesList[qualityIndex]);

                        sw.Write(text);
                    }
                }
            }

            for (int i = 0; i <= cropStage + 1; i++)
            {
                var dropStageDir = createdDropDir + (i == 0 ? "_Plant" : "_Stage" + (i == cropStage + 1 ? "Final" : i)) + assetExtension;

                if (!File.Exists(dropStageDir))
                {
                    File.Copy(refDropsDir + (i == 0 ? "_Block" : "_Stage" + (i == cropStage + 1 ? "Final" : i)) + assetExtension, dropStageDir, false);
                }
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
            if (!File.Exists(createdPlantDir + eternalExtension))
            {
                File.Copy(refPlantDir + eternalExtension, createdPlantDir + eternalExtension, false);
            }

            using (FileStream fs = new FileStream(createdPlantDir + eternalExtension, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    var text = sr.ReadToEnd();
                    text = text.Replace("\r", "");
                    fs.Position = 0;

                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        text = InsertBetween(text, "\"Name\": \"server.", "\"", langPlantName);
                        text = InsertBetween(text, "\"Parent\": \"", "\",", uniqueID + "_Plant");

                        sw.Write(text);
                    }
                }
            }

            if (!File.Exists(createdSeedsDir + eternalExtension))
            {
                File.Copy(refSeedDir + eternalExtension, createdSeedsDir + eternalExtension, false);
            }

            using (FileStream fs = new FileStream(createdSeedsDir + eternalExtension, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    var text = sr.ReadToEnd();
                    text = text.Replace("\r", "");
                    fs.Position = 0;

                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        text = InsertBetween(text, "\"Name\": \"server.", "\",", langSeedsEternalName);
                        text = InsertBetween(text, "\"Description\": \"server.", "\"", langSeedsEternalDesc);

                        text = InsertBetween(text, "\"Quality\": \"", "\",", qualitiesList[qualityIndex]);
                        text = InsertBetween(text, "\"Categories\": [\n    ", "\n  ],", "\"" + modName + ".Seeds\"");

                        sw.Write(text);
                    }
                }
            }

            for (int i = 0; i <= cropStage + 1; i++)
            {
                var dropStageDir = createdDropDir + eternalString + (i == 0 ? "_Plant" : "_Stage" + (i == cropStage + 1 ? "Final" : i)) + assetExtension;

                if (!File.Exists(dropStageDir))
                {
                    var originDir = refDropsDir + eternalString + (i == 0 ? "_Block" : "_Stage" + (i == cropStage + 1 ? "Final" : i)) + assetExtension;
                    File.Copy(originDir, dropStageDir, false);
                }

                using (FileStream fs = new FileStream(dropStageDir, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        var text = sr.ReadToEnd();
                        text = text.Replace("\r", "");
                        fs.Position = 0;

                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            text = InsertBetween(text, "\"Name\": \"server.", "\",", langSeedsEternalName);
                            text = InsertBetween(text, "\"Description\": \"server.", "\"", langSeedsEternalDesc);

                            text = InsertBetween(text, "\"Quality\": \"", "\",", qualitiesList[qualityIndex]);
                            text = InsertBetween(text, "\"Categories\": [\n    ", "\n  ],", "\"" + modName + ".Seeds\"");

                            sw.Write(text);
                        }
                    }
                }
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

public class Strings
{
    public static string LangUIFood(string modName) => "ui." + modName + ".Food";
    public static string LangUISeeds(string modName) => "ui." + modName + ".Seeds";
    public static string LangUIPlants(string modName) => "ui." + modName + ".Plants";
    public static string CategoryTemplate(string modName) => "{\n  \"Icon\": \"Icons/ItemCategories/Natural-Vegetal.png\",\n  \"Order\": 2,\n  \"Children\": [\n    {\n      \"Id\": \"Food\",\n      \"Name\": \"server." + LangUIFood(modName) + "\",\n      \"Icon\": \"Icons/ItemCategories/Items-Ingredients.png\"\n    },\n    {\n      \"Id\": \"Seeds\",\n      \"Name\": \"server." + LangUISeeds(modName) + "\",\n      \"Icon\": \"Icons/ItemCategories/Items-Potion.png\"\n    },\n    {\n      \"Id\": \"Plants\",\n      \"Name\": \"server." + LangUIPlants(modName) + "\",\n      \"Icon\": \"Icons/ItemCategories/Natural-Vegetal.png\"\n    }\n  ]\n}";
}