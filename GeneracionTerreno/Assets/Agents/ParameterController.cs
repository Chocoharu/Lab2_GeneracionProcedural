using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParameterController : MonoBehaviour
{
    public WorldGenerator worldGenerator;
    public GameObject panel;

    //Coast Parameters
    [SerializeField] private TMPro.TMP_InputField coastWidthDepth;
    [SerializeField] private TMPro.TMP_InputField coastTokenLimit;
    [SerializeField] private TMPro.TMP_InputField coastInitialTokens;
    [SerializeField] private TMPro.TMP_InputField coastPerlinScale;
    [SerializeField] private TMPro.TMP_InputField coastPerlinWeight;
    [SerializeField] private TMPro.TMP_InputField coastCenterBiasWeight;

    //Smooth Parameters
    [SerializeField] private TMPro.TMP_InputField smoothTokens;
    [SerializeField] private  TMPro.TMP_InputField globalSmoothIterations;

    //Beach Parameters
    [SerializeField] private TMPro.TMP_InputField beachTokens;
    [SerializeField] private TMPro.TMP_InputField beachSmoothRadius;
    [SerializeField] private TMPro.TMP_InputField beachMaxHeight;
    [SerializeField] private TMPro.TMP_InputField beachMinHeight;

    //Mountain Parameters
    [SerializeField] private TMPro.TMP_InputField mountainTokens;
    [SerializeField] private TMPro.TMP_InputField mountainHeightIncrement;
    [SerializeField] private TMPro.TMP_InputField mountainSmoothRadius;
    [SerializeField] private TMPro.TMP_InputField mountainDirectionChangeInterval;
    [SerializeField] private TMPro.TMP_InputField mountainRadius;

    //Hill Parameters
    [SerializeField] private TMPro.TMP_InputField hillsCount;
    [SerializeField] private TMPro.TMP_InputField hillTokens;
    [SerializeField] private TMPro.TMP_InputField hillHeightIncrement;
    [SerializeField] private TMPro.TMP_InputField hillSmoothRadius;
    [SerializeField] private TMPro.TMP_InputField hillMinDistance;
    [SerializeField] private TMPro.TMP_InputField hillMaxDistance;
    [SerializeField] private TMPro.TMP_InputField hillRPerp;
    [SerializeField] private TMPro.TMP_InputField hillRAlong;

    //River Parameters
    [SerializeField] private TMPro.TMP_InputField minDist;
    [SerializeField] private TMPro.TMP_InputField maxDist;
    [SerializeField] private TMPro.TMP_InputField riverDepthFactor;
    [SerializeField] private TMPro.TMP_InputField riverSmoothRadius;

    private void Start()
    {
        // Inicializa los campos de entrada con los valores actuales del WorldGenerator
        coastWidthDepth.text = worldGenerator.width.ToString();
        coastTokenLimit.text = worldGenerator.tokenLimit.ToString();
        coastInitialTokens.text = worldGenerator.initialTokens.ToString();
        coastPerlinScale.text = worldGenerator.coastPerlinScale.ToString();
        coastPerlinWeight.text = worldGenerator.coastPerlinWeight.ToString();
        coastCenterBiasWeight.text = worldGenerator.coastCenterBiasWeight.ToString();

        smoothTokens.text = worldGenerator.smoothTokens.ToString();
        globalSmoothIterations.text = worldGenerator.globalSmoothIterations.ToString();

        beachTokens.text = worldGenerator.beachTokens.ToString();
        beachSmoothRadius.text = worldGenerator.beachSmoothRadius.ToString();
        beachMaxHeight.text = worldGenerator.beachMaxHeight.ToString();
        beachMinHeight.text = worldGenerator.beachMinHeight.ToString();

        mountainTokens.text = worldGenerator.mountainTokens.ToString();
        mountainHeightIncrement.text = worldGenerator.mountainHeightIncrement.ToString();
        mountainSmoothRadius.text = worldGenerator.mountainSmoothRadius.ToString();
        mountainDirectionChangeInterval.text = worldGenerator.mountainDirectionChangeInterval.ToString();
        mountainRadius.text = worldGenerator.mountainRadius.ToString();

        hillsCount.text = worldGenerator.hillsCount.ToString();
        hillTokens.text = worldGenerator.hillTokens.ToString();
        hillHeightIncrement.text = worldGenerator.hillHeightIncrement.ToString();
        hillSmoothRadius.text = worldGenerator.hillSmoothRadius.ToString();
        hillMinDistance.text = worldGenerator.hillMinDistance.ToString();
        hillMaxDistance.text = worldGenerator.hillMaxDistance.ToString();
        hillRPerp.text = worldGenerator.hillRPerp.ToString();
        hillRAlong.text = worldGenerator.hillRAlong.ToString();

        minDist.text = worldGenerator.minDist.ToString();
        maxDist.text = worldGenerator.maxDist.ToString();
        riverDepthFactor.text = worldGenerator.riverDepthFactor.ToString();
        riverSmoothRadius.text = worldGenerator.riverSmoothRadius.ToString();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePanel();
        }
    }
    private void AssignNewValues()
    {
        // Actualiza los valores del WorldGenerator con los valores de los campos de entrada
        worldGenerator.width = int.Parse(coastWidthDepth.text);
        worldGenerator.depth = int.Parse(coastWidthDepth.text);
        worldGenerator.tokenLimit = int.Parse(coastTokenLimit.text);
        worldGenerator.initialTokens = int.Parse(coastInitialTokens.text);
        worldGenerator.coastPerlinScale = float.Parse(coastPerlinScale.text);
        worldGenerator.coastPerlinWeight = float.Parse(coastPerlinWeight.text);
        worldGenerator.coastCenterBiasWeight = float.Parse(coastCenterBiasWeight.text);
        worldGenerator.smoothTokens = int.Parse(smoothTokens.text);
        worldGenerator.globalSmoothIterations = int.Parse(globalSmoothIterations.text);
        worldGenerator.beachTokens = int.Parse(beachTokens.text);
        worldGenerator.beachSmoothRadius = int.Parse(beachSmoothRadius.text);
        worldGenerator.beachMaxHeight = float.Parse(beachMaxHeight.text);
        worldGenerator.beachMinHeight = float.Parse(beachMinHeight.text);
        worldGenerator.mountainTokens = int.Parse(mountainTokens.text);
        worldGenerator.mountainHeightIncrement = float.Parse(mountainHeightIncrement.text);
        worldGenerator.mountainSmoothRadius = int.Parse(mountainSmoothRadius.text);
        worldGenerator.mountainDirectionChangeInterval = int.Parse(mountainDirectionChangeInterval.text);
        worldGenerator.mountainRadius = int.Parse(mountainRadius.text);
        worldGenerator.hillsCount = int.Parse(hillsCount.text);
        worldGenerator.hillTokens = int.Parse(hillTokens.text);
        worldGenerator.hillHeightIncrement = float.Parse(hillHeightIncrement.text);
        worldGenerator.hillSmoothRadius = int.Parse(hillSmoothRadius.text);
        worldGenerator.hillMinDistance = int.Parse(hillMinDistance.text);
        worldGenerator.hillMaxDistance = int.Parse(hillMaxDistance.text);
        worldGenerator.hillRPerp = int.Parse(hillRPerp.text);
        worldGenerator.hillRAlong = int.Parse(hillRAlong.text);
        worldGenerator.minDist = int.Parse(minDist.text);
        worldGenerator.maxDist = int.Parse(maxDist.text);
        worldGenerator.riverDepthFactor = float.Parse(riverDepthFactor.text);
        worldGenerator.riverSmoothRadius = int.Parse(riverSmoothRadius.text);
    }

    public void GenerateWorld()
    {
        AssignNewValues();
        worldGenerator.CreateWorld();
    }

    public void TogglePanel()
    {
        panel.SetActive(!panel.activeSelf);
    }
}
