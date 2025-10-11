using System.Collections.Generic;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    private List<float> fitnessResults = new();

    public void LogResult(float fitness)
    {
        fitnessResults.Add(fitness);
    }

    public void PrintSummary()
    {
        float avg = 0;
        foreach (float f in fitnessResults) avg += f;
        avg /= fitnessResults.Count;
        Debug.Log($"Promedio fitness: {avg}");
    }
}
