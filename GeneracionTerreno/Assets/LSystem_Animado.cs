using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;


public class LSystem_Animado : MonoBehaviour
{
    [Header("Definición del L-System")]
    public string axiom = "X";      // Punto de partida
    public int iterations = 2;      // Número de pasos
    public float angle = 25f;       // Ángulo para ramas
    public float length = 0.5f;     // Longitud de cada segmento

    [Header("Animación")]
    [SerializeField] private bool animateByIterations = false; // false: anima solo el resultado final; true: muestra iteración por iteración
    [SerializeField] private float stepDelay = 0.02f;          // pausa entre símbolos (F, [, ])
    [SerializeField] private float iterationPause = 0.5f;      // pausa entre iteraciones (si animateByIterations = true)

    [Header("Refs")]
    [SerializeField] private SplineContainer container;

    [System.Serializable]
    public class Rule { public char symbol; public string replacement; }
    public List<Rule> rules = new List<Rule>();

    private void Start()
    {
        StartCoroutine(RunAnimated());
    }

    private IEnumerator RunAnimated()
    {
        // 1) Precalcular secuencias por iteración (para poder animar por iteraciones si se desea)
        var sequences = new List<string>();
        string s = axiom;
        sequences.Add(s);
        for (int i = 0; i < iterations; i++)
        {
            s = Generate(s);
            sequences.Add(s);
        }

        // 2) Animar
        if (animateByIterations)
        {
            for (int i = 1; i < sequences.Count; i++)
            {
                ClearContainer();
                yield return StartCoroutine(DrawAnimated(sequences[i]));
                if (iterationPause > 0f) yield return new WaitForSeconds(iterationPause);
            }
        }
        else
        {
            ClearContainer();
            yield return StartCoroutine(DrawAnimated(sequences[sequences.Count - 1]));
        }
    }

    private string Generate(string input)
    {
        var result = new System.Text.StringBuilder(input.Length * 2);
        foreach (char c in input)
        {
            bool replaced = false;
            foreach (var rule in rules)
            {
                if (c == rule.symbol)
                {
                    result.Append(rule.replacement);
                    replaced = true;
                    break;
                }
            }
            if (!replaced) result.Append(c);
        }
        return result.ToString();
    }

    private IEnumerator DrawAnimated(string sequence)
    {
        // Pila para guardar/recuperar estado
        Stack<(Vector3 pos, Quaternion rot, Spline spline)> stack =
            new Stack<(Vector3, Quaternion, Spline)>();

        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        Vector3 forward = Vector3.up; // crecimiento vertical

        // Spline inicial
        Spline currentSpline = new Spline();
        container.AddSpline(currentSpline);

        // Knot inicial con rotación definida para evitar “plano”
        BezierKnot startKnot = new BezierKnot(position);
        startKnot.Rotation = Quaternion.LookRotation(forward, Vector3.right);
        currentSpline.Add(startKnot);

        foreach (char c in sequence)
        {
            switch (c)
            {
                case 'F':
                    {
                        // Dirección actual
                        Vector3 dir = rotation * forward;

                        // Avanzar
                        Vector3 newPos = position + dir * length;

                        // Crear knot con orientación estable (evita plano)
                        BezierKnot knot = new BezierKnot(newPos);
                        Vector3 normal = Vector3.Cross(dir, Vector3.right);
                        if (normal.sqrMagnitude < 0.001f) normal = Vector3.Cross(dir, Vector3.forward);
                        knot.Rotation = Quaternion.LookRotation(dir, normal);

                        currentSpline.Add(knot);
                        position = newPos;

                        if (stepDelay > 0f) yield return new WaitForSeconds(stepDelay);
                        else yield return null;
                        break;
                    }

                case '+': // Yaw +
                    rotation *= Quaternion.AngleAxis(angle, rotation * Vector3.forward);
                    break;
                case '-': // Yaw -
                    rotation *= Quaternion.AngleAxis(-angle, rotation * Vector3.forward);
                    break;
                case '&': // Pitch +
                    rotation *= Quaternion.AngleAxis(angle, rotation * Vector3.right);
                    break;
                case '^': // Pitch -
                    rotation *= Quaternion.AngleAxis(-angle, rotation * Vector3.right);
                    break;
                case '\\': // Roll +
                    rotation *= Quaternion.AngleAxis(angle, rotation * Vector3.up);
                    break;
                case '/': // Roll -
                    rotation *= Quaternion.AngleAxis(-angle, rotation * Vector3.up);
                    break;

                case '[':
                    {
                        // Guardar estado y crear nuevo spline para la rama
                        stack.Push((position, rotation, currentSpline));
                        currentSpline = new Spline();
                        container.AddSpline(currentSpline);

                        // Knot inicial de la rama con orientación definida
                        Vector3 dir = rotation * forward;
                        BezierKnot branchStart = new BezierKnot(position);
                        Vector3 normal = Vector3.Cross(dir, Vector3.right);
                        if (normal.sqrMagnitude < 0.001f) normal = Vector3.Cross(dir, Vector3.forward);
                        branchStart.Rotation = Quaternion.LookRotation(dir, normal);
                        currentSpline.Add(branchStart);

                        if (stepDelay > 0f) yield return new WaitForSeconds(stepDelay);
                        else yield return null;
                        break;
                    }

                case ']':
                    {
                        if (stack.Count > 0)
                        {
                            var prev = stack.Pop();
                            position = prev.pos;
                            rotation = prev.rot;
                            currentSpline = prev.spline;
                        }
                        if (stepDelay > 0f) yield return new WaitForSeconds(stepDelay);
                        else yield return null;
                        break;
                    }

                case 'X':
                    // No dibuja, marcador para expansión
                    break;
            }
        }
    }

    private void ClearContainer()
    {
        if (container == null) return;
        // Elimina todos los splines anteriores
        var toRemove = new List<Spline>(container.Splines);
        foreach (var s in toRemove)
            container.RemoveSpline(s);
    }
}
