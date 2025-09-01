using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class LSystem : MonoBehaviour
{
    [Header("Definición del L-System")]
    public string axiom;          // Punto de partida
    public int iterations;          // Número de pasos
    public float angle;           // Ángulo para ramas
    public float length;           // Longitud inicial

    public List<Rule> rules = new List<Rule>();

    private string currentString;

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
        Stack<(Vector3 pos, Quaternion rot)> transformStack = new Stack<(Vector3, Quaternion)>();
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        foreach (char c in sequence)
        {

            switch(c)
            {
                case 'F':
                    Vector3 newPos = position + rotation * Vector3.up * length;
                    Debug.DrawLine(position, newPos, Color.green, 100f);
                    position = newPos;
                    break;
                case '+':
                    rotation *= Quaternion.Euler(0, 0, angle);
                    break;
                case '-':
                    rotation *= Quaternion.Euler(0, 0, -angle);
                    break;
                case '[':
                    transformStack.Push((position, rotation));
                    break;
                case ']':
                     (position, rotation) = transformStack.Pop();
                    break;
                case 'X':
                    // 'X' no dibuja nada, solo es una variable para las reglas
                    break;
            }
        }
    }
}

