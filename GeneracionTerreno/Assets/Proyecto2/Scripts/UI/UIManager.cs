using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Singleton sencillo ("simpleton")
    public static UIManager Instance { get; private set; }

    public GridManager gridManager;
    public Button generateBtn, evolveBtn, refineBtn, assingBtn;

    private EvolutionStrategy es;
    private SimulatedAnnealing sa;

    private LevelRepresentation currentLevel;

    [Header("Grid Parameters")]
    public int levelWidth;
    public int levelHeight;
    public int numBoxes;

    [Header("Evolution Strategy Parameters")]
    public int esMu;
    public int esLambda;
    public int esGenerations;

    [Header("Simulated Annealing Parameters")]
    public float saInitialTemp;
    public float saCoolingRate;
    public int saIterations;
    public float saDesiredWallDensity;

    [Header("UI Text")]
    public TMPro.TMP_InputField widthHeight;
    public TMPro.TMP_InputField boxes;
    public TMPro.TMP_InputField mu;
    public TMPro.TMP_InputField lambda;
    public TMPro.TMP_InputField generations;
    public TMPro.TMP_InputField initialTemp;
    public TMPro.TMP_InputField coolingRate;
    public TMPro.TMP_InputField iterations;
    public TMPro.TMP_InputField desiredWallDensity;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Inicializar las estrategias/algoritmos aquí
        es = new EvolutionStrategy();
        sa = new SimulatedAnnealing();
    }

    void Start()
    {
        // Seguridad: asegurar instancias si por alguna razón no se crearon en Awake
        if (es == null) es = new EvolutionStrategy();
        if (sa == null) sa = new SimulatedAnnealing();

        if (generateBtn != null) generateBtn.onClick.AddListener(OnGenerate);
        if (evolveBtn != null) evolveBtn.onClick.AddListener(OnEvolve);
        if (refineBtn != null) refineBtn.onClick.AddListener(OnRefine);
        if (assingBtn!= null) assingBtn.onClick.AddListener(OnAssignParameters);


        // Actualizar los textos iniciales
        if (widthHeight != null) widthHeight.text = levelHeight.ToString();
        if (boxes != null) boxes.text = numBoxes.ToString();
        if (mu != null) mu.text = esMu.ToString();
        if (lambda != null) lambda.text = esLambda.ToString();
        if (generations != null) generations.text = esGenerations.ToString();
        if (initialTemp != null) initialTemp.text = saInitialTemp.ToString("F1");
        if (coolingRate != null) coolingRate.text = saCoolingRate.ToString("F2");
        if (iterations != null) iterations.text = saIterations.ToString();
        if (desiredWallDensity != null) desiredWallDensity.text = saDesiredWallDensity.ToString("F2");
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void OnGenerate()
    {
        currentLevel = BackwardsGenerator.Generate(levelWidth, levelHeight, numBoxes);
        if (gridManager != null && currentLevel != null)
            gridManager.GenerateLevel(currentLevel.grid);
    }

    void OnEvolve()
    {
        es.InitializePopulation(levelWidth, levelHeight, numBoxes);
        currentLevel = es.Run();
        if (gridManager != null && currentLevel != null)
            gridManager.GenerateLevel(currentLevel.grid);
    }

    void OnRefine()
    {
        if (currentLevel == null) return;
        currentLevel = sa.Run(currentLevel);
        if (gridManager != null && currentLevel != null)
            gridManager.GenerateLevel(currentLevel.grid);
    }

    void OnAssignParameters()
    {
        levelHeight = levelWidth = int.Parse(widthHeight.text);
        numBoxes = int.Parse(boxes.text);
        esMu = int.Parse(mu.text);
        esLambda = int.Parse(lambda.text);
        esGenerations = int.Parse(generations.text);
        saInitialTemp = float.Parse(initialTemp.text);
        saCoolingRate = float.Parse(coolingRate.text);
        saIterations = int.Parse(iterations.text);
        saDesiredWallDensity = float.Parse(desiredWallDensity.text);
    }
}
