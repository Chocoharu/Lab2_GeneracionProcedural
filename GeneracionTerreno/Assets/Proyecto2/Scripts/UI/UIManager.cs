using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GridManager gridManager;
    public Button generateBtn, evolveBtn, refineBtn;

    private EvolutionStrategy es;
    private SimulatedAnnealing sa;

    private LevelRepresentation currentLevel;

    void Start()
    {
        es = new EvolutionStrategy();
        sa = new SimulatedAnnealing();

        generateBtn.onClick.AddListener(OnGenerate);
        evolveBtn.onClick.AddListener(OnEvolve);
        refineBtn.onClick.AddListener(OnRefine);
    }

    void OnGenerate()
    {
        currentLevel = BackwardsGenerator.Generate(8, 8, 3);
        gridManager.GenerateLevel(currentLevel);
    }

    void OnEvolve()
    {
        es.width = currentLevel.width;
        es.height = currentLevel.height;
        es.boxes = currentLevel.FindBoxes().Count;
        es.InitializePopulation();
        currentLevel = es.Run();
        gridManager.GenerateLevel(currentLevel);
    }

    void OnRefine()
    {
        currentLevel = sa.Run(currentLevel);
        gridManager.GenerateLevel(currentLevel);
    }
}
