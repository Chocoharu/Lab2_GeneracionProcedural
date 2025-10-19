using System.Collections.Generic;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    private List<float> fitnessResults = new List<float>();

    public void LogResult(float fitness)
    {
        fitnessResults.Add(fitness);
    }

    public void PrintSummary()
    {
        if (fitnessResults.Count == 0)
        {
            Debug.Log("No results recorded.");
            return;
        }
        float sum = 0;
        float min = float.MaxValue, max = float.MinValue;
        foreach (var f in fitnessResults)
        {
            sum += f;
            min = Mathf.Min(min, f);
            max = Mathf.Max(max, f);
        }
        float avg = sum / fitnessResults.Count;
        Debug.Log($"Fitness - count: {fitnessResults.Count}, avg: {avg}, min: {min}, max: {max}");
    }
}
