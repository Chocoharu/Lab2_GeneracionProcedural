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
        gridManager.GenerateLevel(currentLevel.grid);
    }

    void OnEvolve()
    {
        es.InitializePopulation(8, 8, 3);
        currentLevel = es.Run();
        gridManager.GenerateLevel(currentLevel.grid);
    }

    void OnRefine()
    {
        currentLevel = sa.Run(currentLevel);
        gridManager.GenerateLevel(currentLevel.grid);
    }
}
