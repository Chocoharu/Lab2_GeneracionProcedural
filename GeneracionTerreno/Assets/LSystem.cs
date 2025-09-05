using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Splines; 

public class LSystem : MonoBehaviour
{
    [Header("Definición del L-System")]
    public string axiom;          // Punto de partida
    public int iterations;          // Número de pasos
    public float angle;           // Ángulo para ramas
    public float length;           // Longitud inicial

    public List<Rule> rules = new List<Rule>();

    private string currentString;
    [SerializeField]private SplineContainer container;
    // Las reglas para el sistema L se definen como objetos de la clase Rule y se agregan a la lista 'rules'.
    // Ejemplo de definición de reglas en el Inspector de Unity o mediante código:

    // Ejemplo 1: Definición en el Inspector de Unity
    // En el Editor de Unity, puedes agregar elementos a la lista 'rules' y configurar los valores de 'symbol' y 'replacement'.

    // Ejemplo 2: Definición mediante código
    void Start()
    {
        Debug.Log($"Resultado final del L-System: {currentString}");
        // Continuamos con el flujo normal del sistema L
        currentString = axiom;
        for (int i = 0; i < iterations; i++)
        {
            currentString = Generate(currentString);
        }
        Debug.Log($"Resultado final del L-System: {currentString}");
        Draw(currentString);
    }

    string Generate(string input)
    {
        string result = "";
        foreach (char c in input)
        {
            bool replaced = false;
            foreach (var rule in rules)
            {
                if (c == rule.symbol)
                {
                    result += rule.replacement;
                    replaced = true;
                    break;
                }
            }
            if (!replaced) result += c.ToString();
        }
        return result;
    }

    void Draw(string sequence)
    {
        Stack<(Vector3 pos, Vector3 H, Vector3 L, Vector3 U, Spline spline)> transformStack =
        new Stack<(Vector3, Vector3, Vector3, Vector3, Spline)>();

        // Estado inicial: tortuga en convención estándar
        Vector3 position = Vector3.zero;
        Vector3 H = Vector3.up;  // Y+
        Vector3 L = Vector3.left;   // Left -> X-
        Vector3 U = Vector3.forward;  // Up -> Z

        Spline spline = new Spline();
        container.AddSpline(spline);
        spline.Add(new BezierKnot(position));

        foreach (char c in sequence)
        {
            switch (c)
            {
                case 'F':
                    Vector3 newPos = position + H * length;
                    Debug.Log($"Posición: {position}");
                    Debug.DrawRay(position, H * length, Color.red, 10f);
                    spline.Add(new BezierKnot(newPos));
                    position = newPos;
                    break;

                case '+': // yaw +
                    Rotate(ref H, ref L, ref U, U, angle + Random.Range(-5f, 5f));
                    break;
                case '-': // yaw -
                    Rotate(ref H, ref L, ref U, U, -angle + Random.Range(-5f, 5f));
                    break;
                case '&': // pitch +
                    Rotate(ref H, ref L, ref U, L, angle + Random.Range(-5f, 5f));
                    break;
                case '^': // pitch -
                    Rotate(ref H, ref L, ref U, L, -angle + Random.Range(-5f, 5f));
                    break;
                case '\\': // roll +
                    Rotate(ref H, ref L, ref U, H, angle + Random.Range(-5f, 5f));
                    break;
                case '/': // roll -
                    Rotate(ref H, ref L, ref U, H, -angle + Random.Range(-5f, 5f));
                    break;

                case '[':
                    transformStack.Push((position, H, L, U, spline));
                    spline = new Spline();
                    container.AddSpline(spline);
                    spline.Add(new BezierKnot(position));
                    break;

                case ']':
                    (position, H, L, U, spline) = transformStack.Pop();
                    break;

                case 'X':
                    // no dibuja
                    break;
            }
        }
    }

    // Función de rotación: rota H,L,U alrededor de un eje
    void Rotate(ref Vector3 H, ref Vector3 L, ref Vector3 U, Vector3 axis, float angle)
    {
        Quaternion q = Quaternion.AngleAxis(angle, axis);
        H = q * H;
        L = q * L;
        U = q * U;
    }
}

