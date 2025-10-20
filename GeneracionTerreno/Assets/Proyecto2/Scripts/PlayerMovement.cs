using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Velocidades")]
    [SerializeField] private float moveSpeed = 5f;        // X/Z
    [SerializeField] private float verticalSpeed = 3f;    // Y
    [SerializeField] private float wSpeed = 2f;           // 4ª dimensión

    [Header("Proyección W -> 3D")]
    [SerializeField] private float wProjectionScale = 1f; // cuánto afecta w a Z o escala
    [SerializeField] private bool usePerspectiveW = true; // true: w desplaza Z; false: w modifica escala

    // Posición interna en 4D: (x, y, z, w)
    private Vector4 position4;

    // Guardar escala original para la proyección no-perspectiva
    private Vector3 originalScale;

    void Start()
    {
        // Inicializar posición4 desde la posición 3D actual; w = 0 por defecto
        Vector3 p = transform.position;
        position4 = new Vector4(p.x, p.y, p.z, 0f);
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Leer entradas horizontales (X,Z)
        float hx = Input.GetAxis("Horizontal"); // A/D, Left/Right
        float hz = Input.GetAxis("Vertical");   // W/S, Up/Down

        // Y: Space para subir, LeftControl para bajar
        float hy = 0f;
        if (Input.GetKey(KeyCode.Space)) hy += 1f;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) hy -= 1f;

        // W dimensión: Q para decrementar, E para incrementar
        float hw = 0f;
        if (Input.GetKey(KeyCode.E)) hw += 1f;
        if (Input.GetKey(KeyCode.Q)) hw -= 1f;

        // Construir delta 4D con velocidades y scaledelta
        Vector4 delta4 = new Vector4(hx, hy, hz, hw);
        delta4.x *= moveSpeed;
        delta4.z *= moveSpeed;
        delta4.y *= verticalSpeed;
        delta4.w *= wSpeed;

        // Aplicar tiempo
        delta4 *= Time.deltaTime;

        // Actualizar posición 4D
        position4 += delta4;

        // Proyectar a 3D y aplicar al transform
        ApplyProjectionToTransform();
    }

    private void ApplyProjectionToTransform()
    {
        Vector3 proj;
        if (usePerspectiveW)
        {
            // Perspectiva simple: w desplaza el eje Z
            float projectedZ = position4.z + position4.w * wProjectionScale;
            proj = new Vector3(position4.x, position4.y, projectedZ);

            // Restaurar escala original
            transform.localScale = originalScale;
        }
        else
        {
            // Ortográfica: w no cambia Z, pero afecta la escala del objeto (efecto de "profundidad")
            proj = new Vector3(position4.x, position4.y, position4.z);

            float scaleFactor = 1f + position4.w * wProjectionScale;
            // Evitar escala negativa o cero
            scaleFactor = Mathf.Max(0.01f, scaleFactor);
            transform.localScale = originalScale * scaleFactor;
        }

        transform.position = proj;
    }

    // Permite ajustar la coordenada w desde otros scripts
    public void SetW(float newW)
    {
        position4.w = newW;
    }

    // Visualización en el editor para ayudar a depurar
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 pos3;
        if (Application.isPlaying)
        {
            pos3 = usePerspectiveW
                ? new Vector3(position4.x, position4.y, position4.z + position4.w * wProjectionScale)
                : new Vector3(position4.x, position4.y, position4.z);
        }
        else
        {
            pos3 = transform.position;
        }

        Gizmos.DrawWireSphere(pos3, 0.25f);
        Gizmos.DrawLine(transform.position, pos3);
    }
}
