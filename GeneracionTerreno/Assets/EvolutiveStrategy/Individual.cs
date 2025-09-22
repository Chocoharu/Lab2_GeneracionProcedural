using UnityEngine;

public class Individual
{
    public float[] genes;
    public float fitness;

    public Individual(int geneLength)
    {
        genes = new float[geneLength];
        for (int i = 0; i < geneLength; i++)
        {
            genes[i] = Random.Range(-1f, 1f);
        }
    }
}
