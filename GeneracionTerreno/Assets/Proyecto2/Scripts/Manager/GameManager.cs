using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GridManager gridManager;
    public ExperimentManager experimentManager;

    // parameters
    public int width = 8, height = 8, boxes = 3;

    private EvolutionStrategy es;
    private SimulatedAnnealing sa;

    void Start()
    {
        // Simple demo: generate initial with backwards, visualize
        var level = BackwardsGenerator.Generate(width, height, boxes, steps: 200);
        EnsureInvariants(level);
        gridManager.GenerateLevel(level);
        Debug.Log("Generated initial level (backwards). Checking solvability...");

        var solver = new SolverSokoban();
        bool solvable = solver.IsSolvable(level);
        Debug.Log($"Initial level solvable? {solvable}. Solution length (if solvable): {solver.SolutionLength(level)}");

        // initialize algorithms
        es = new EvolutionStrategy() { mu = 12, lambda = 36, generations = 30, width = width, height = height, boxes = boxes };
        sa = new SimulatedAnnealing() { initialTemp = 1.0f, coolingRate = 0.995f, iterations = 1500 };

        // start a small pipeline (ES then SA) synchronous for demo
        es.InitializePopulation();
        var bestFromES = es.Run();
        Debug.Log($"ES best fitness: {bestFromES.fitness}");
        gridManager.GenerateLevel(bestFromES);

        var bestFromSA = sa.Run(bestFromES);
        Debug.Log($"SA improved fitness: {bestFromSA.fitness}");
        gridManager.GenerateLevel(bestFromSA);

        experimentManager.LogResult(bestFromES.fitness);
        experimentManager.LogResult(bestFromSA.fitness);
        experimentManager.PrintSummary();
    }

    private void EnsureInvariants(LevelRepresentation level)
    {
        // ensure single player & boxes==goals
        var p = level.FindPlayer();
        if (p.x < 0)
        {
            for (int x = 1; x < level.width; x++)
                for (int y = 1; y < level.height; y++)
                    if (level.grid[x, y] == 0) { level.grid[x, y] = 4; x = level.width; break; }
        }
        var boxesList = level.FindBoxes();
        var goals = level.FindGoals();
        if (goals.Count < boxesList.Count)
        {
            for (int x = 1; x < level.width && goals.Count < boxesList.Count; x++)
                for (int y = 1; y < level.height && goals.Count < boxesList.Count; y++)
                    if (level.grid[x, y] == 0) { level.grid[x, y] = 3; goals.Add((x, y)); }
        }
    }
}
