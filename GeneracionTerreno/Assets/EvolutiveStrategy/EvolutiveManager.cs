using UnityEngine;

public class EvolutiveManager : MonoBehaviour
{
    public int mu; // Number of parents
    public int lambda; // Number of offspring
    public int geneLength; // Length of the gene array
    public float mutationRate; // Mutation rate

    private Individual[] population; // Current population
    private int generation = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        population = new Individual[mu];
        for (int i = 0; i < mu; i++)
        {
            population[i] = new Individual(geneLength);
        }
        generation = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            NextGeneration();
        }
    }

    float EvaluateFitness(Individual individual)
    {
        // Example fitness function: sum of genes
        float fitness = 0f;
        foreach (float gene in individual.genes)
        {
            fitness += gene;
        }
        return fitness;
    }

    Individual Mutate(Individual parent)
    {
        Individual offspring = new Individual(geneLength);
        for (int i = 0; i < geneLength; i++)
        {
            if (Random.value < mutationRate)
            {
                offspring.genes[i] = Random.Range(-1f, 1f); // Random mutation
            }
            else
            {
                offspring.genes[i] = parent.genes[i]; // No mutation
            }
        }
        return offspring;
    }

    void NextGeneration()
    {
        // Evaluar fitness de la población actual
        foreach (Individual individual in population)
        {
            individual.fitness = EvaluateFitness(individual);
        }
        // Ordenar población por fitness (descendente)
        System.Array.Sort(population, (a, b) => b.fitness.CompareTo(a.fitness));

        // Mostrar información de la generación actual por consola
        Debug.Log($"Generación: {generation}");
        for (int i = 0; i < population.Length; i++)
        {
            Debug.Log($"Individuo {i}: Fitness = {population[i].fitness}, Genes = [{string.Join(", ", population[i].genes)}]");
        }

        // Seleccionar los mejores mu individuos como padres
        Individual[] parents = new Individual[mu];
        System.Array.Copy(population, parents, mu);

        // Generar lambda descendientes mediante mutación
        Individual[] offspring = new Individual[lambda];
        for (int i = 0; i < lambda; i++)
        {
            int parentIndex = Random.Range(0, mu);
            offspring[i] = Mutate(parents[parentIndex]);
        }

        // Crear nueva población: combinar padres y descendientes
        population = new Individual[mu + lambda];
        System.Array.Copy(parents, population, mu);
        System.Array.Copy(offspring, 0, population, mu, lambda);
        generation++;
    }
}
