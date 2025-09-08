using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Splines; 

public class LSystem : MonoBehaviour
{
    [Header("Definici�n del L-System")]
    public string axiom;          // Punto de partida
    public int iterations;          // N�mero de pasos
    public float angle;           // �ngulo para ramas
    public float length;           // Longitud inicial

    public List<Rule> rules = new List<Rule>();

    private string currentString;
    [SerializeField]private SplineContainer container;
    // Las reglas para el sistema L se definen como objetos de la clase Rule y se agregan a la lista 'rules'.
    // Ejemplo de definici�n de reglas en el Inspector de Unity o mediante c�digo:

    // Ejemplo 1: Definici�n en el Inspector de Unity
    // En el Editor de Unity, puedes agregar elementos a la lista 'rules' y configurar los valores de 'symbol' y 'replacement'.

    // Ejemplo 2: Definici�n mediante c�digo
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
        Stack<(Vector3 pos, Quaternion rotation, Spline spline)> transformStack =
        new Stack<(Vector3, Quaternion, Spline)>();

        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        Vector3 forward = Vector3.up; // Crecimiento vertical inicial

        Spline currentSpline = new Spline();
        container.AddSpline(currentSpline);


        BezierKnot startKnot = new BezierKnot(position);
        startKnot.Rotation = Quaternion.LookRotation(forward, Vector3.right);
        currentSpline.Add(startKnot);

        foreach (char c in sequence)
        {
            switch (c)
            {
                case 'F':
                    // Calcular direcci�n actual
                    Vector3 dir = rotation * forward;

                    // Avanzar en la direcci�n
                    Vector3 newPos = position + dir * length;

                    // Crear nodo con orientaci�n para evitar que quede plano
                    BezierKnot knot = new BezierKnot(newPos);

                    // Calcular un "up" estable para el spline
                    Vector3 normal = Vector3.Cross(dir, Vector3.right);
                    if (normal.sqrMagnitude < 0.001f) // si era paralelo al eje X
                        normal = Vector3.Cross(dir, Vector3.forward);

                    knot.Rotation = Quaternion.LookRotation(dir, normal);

                    currentSpline.Add(knot);

                    position = newPos;
                    break;

                case '+': // Yaw positivo (rotaci�n alrededor de Z)
                    rotation *= Quaternion.AngleAxis(angle, rotation * Vector3.forward);
                    break;
                case '-': // Yaw negativo
                    rotation *= Quaternion.AngleAxis(-angle, rotation * Vector3.forward);
                    break;
                case '&': // Pitch positivo (rotaci�n alrededor de X)
                    rotation *= Quaternion.AngleAxis(angle, rotation * Vector3.right);
                    break;
                case '^': // Pitch negativo
                    rotation *= Quaternion.AngleAxis(-angle, rotation * Vector3.right);
                    break;
                case '\\': // Roll positivo (rotaci�n alrededor de Y)
                    rotation *= Quaternion.AngleAxis(angle, rotation * Vector3.up);
                    break;
                case '/': // Roll negativo
                    rotation *= Quaternion.AngleAxis(-angle, rotation * Vector3.up);
                    break;

                case '[':
                    transformStack.Push((position, rotation, currentSpline));
                    currentSpline = new Spline();
                    container.AddSpline(currentSpline);
                    currentSpline.Add(new BezierKnot(position));
                    break;

                case ']':
                    if (transformStack.Count > 0)
                    {
                        var previousState = transformStack.Pop();
                        position = previousState.pos;
                        rotation = previousState.rotation;
                        currentSpline = previousState.spline;
                    }
                    break;

                case 'X':
                    break;
            }
        }
    }
}


