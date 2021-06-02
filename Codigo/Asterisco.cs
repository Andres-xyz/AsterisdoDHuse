using UnityEngine;
// Maquina de estado
using Diccionario = System.Collections.Generic.Dictionary<Definicion,System.Collections.Generic.Dictionary<byte, Definicion>>;
using Mensaje = System.Collections.Generic.Dictionary<byte, Definicion>;

delegate void Definicion();

public class Asterisco : MonoBehaviour
{
    // Maquina de estados
    private Definicion estadoActual;    // Variable de la maquina de estado para el estado actual
    private Diccionario maquina;        // Variable de la maquina de estado para apuntar a las funciones y asociarlo con el mensaje
    private Mensaje mensaje;            // Variable de la maquina de estado para el mensaje (el mensaje sera un numero del tipo int)

    // Variables de la maya
    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Vector3[] vertices;
    private Vector3[] normales;
    private Vector2[] uv;
    private int[] triangulos;

    // Variable del material
    private Material materialLejos;
    private Material materialCerca;

    // Variable de la camara
    private Transform posCam;

    // Variables de la rotacion del asterisco
    public float rot = 5f;   // <------------------------- Velocidad de rotacion
    private float rotacion;                             // Valor de la rotacion
    private float SinAng;                               // Seno del angulo de la rotacion
    private float CosAng;                               // Coseno del angulo de la rotacion
    private Quaternion q = Quaternion.identity;         // Cuaternion para hacer rotar el asterisco

    // Otras variables
    private Vector3 distancia;
    //private Vector3 posAsterisco;     // <----------------------------------------------------------------------------------- (En caso de que los asteriscos sean fijos a la escena habilitar)
    private byte condicionador = 0;     // Variable que servira para condicionar el estado del asterisco cerca o lejos haciendolo más eficiente
    private byte contacondicion = 3;    // Variable que cuenta y condiciona la cantidad de veces que se ejecuta la maquina de estado
    
    // Carga modelo minimo del asterisco para ser usado a lo lejos
    void Awake()
    {
        // Busca transform de la camara
        posCam = GameObject.Find("Main Camera").GetComponent<Transform>();
        // Asignamos la posicion del asterisco (Asterisco fijo en la escena) <---------------------------------------------
        //posAsterisco = transform.position;

        // Componentes del mesh
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        //Condiciones de la MeshRenderer
        meshRenderer.lightProbeUsage = 0;       //Lices probes off
        meshRenderer.reflectionProbeUsage = 0;  //Refleccion off
        meshRenderer.receiveShadows = false;    //Recibe sombra Falso

        // Se carga el material con su direccion donde esta guardado
        materialLejos = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Objeto3D/Asterisco/Mater_Asterisco/Lejos/Mat_AsteriscoLejos.mat", typeof(Material));
        materialCerca = (Material)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Objeto3D/Asterisco/Mater_Asterisco/Cerca/Mat_AsteriscoCerca.mat", typeof(Material));

        // Asignacion de la maya
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        // Inicio de maquina de estado
        MaquinaDeEstado();
        // inicio del estrado
        estadoActual = DondeEsta;   // Averigua en que estado inicia
        mensaje = maquina[estadoActual];

        //Iniciamos corrutina para controlar el estado del asterisco
        StartCoroutine(EstadoAsterisco());
    }

    // Subrutina para el estado del asterisco cada 0.05 segundos (20 veces por segundo)
    System.Collections.IEnumerator EstadoAsterisco()
    {
        while (true)
        {
            // Contador que condiciona la cantidad de veces que se accede a la maquina de estados (1 vez cada 4) seria cada 0.2 segundos o sea 5 veces por segundo
            if (contacondicion > 2)
            {
                // Se envia mensaje del estado de la maya
                EnviarMensaje(Distancia());

                // Llamamos a la maquina de estado
                estadoActual();

                // reinicia contacondicion
                contacondicion = 0;
            }

            // Suma 1 al contador para que cada 3 pasadas 1 habilite a la maquina de estado.
            contacondicion = (byte)(contacondicion + 1);

            // Llamda a rotas el asterisco
            RotacionAsterisco();

            yield return new WaitForSeconds(0.05f); //subrutina interrumpida cada 0.05 segundos o sea 20 veces por segundos
        }
    }

    // Calculo de la distancia entre el asterisco y la camara
    // Resultados posibles 0, 1, 2, 3. y se determina si el asterisco esta cerca, lejos o no hay cambios
    byte Distancia()
    {
        // Calculamos el vector de distancia entre la camara y el asterisco
        //distancia = posCam.position - posAsterisco;     // <--------------------------------------------------------------------- (En caso que los asteriscos sean fijos a la escena)
        distancia = posCam.position - transform.position;   // Borrar esta linea cuando se habilita la anterior <-------------------

        // Determinamos si el asterisco esta cerca o lejos de la camara
        distancia.z = ((distancia.x * distancia.x) + (distancia.y * distancia.y) + (distancia.z * distancia.z));    // En .z se almacena la distancia al cuadrado
        distancia.y = Mathf.Floor(distancia.z - 25);    // 25 es por que la distancia es 5 unidades (5*5)           // En .y se almacena el resulrtado en numero entero de la diferencia entre la distancia cercana de la camara 
        distancia.x = Mathf.Clamp(distancia.y, 0, 1);   // Resultado 0 si esta cerca 1 si esta lejos                // En .x se pasa el resultado 0 o 1

        //Retornamos el resultado de la distancia 0 Cerca 1 Lejos
        return System.Convert.ToByte(distancia.x + condicionador);  //Pasamos de float a Byte (Se suma condicionador que puede valer 2 o 0 esto hace que no se recargue la maya si no cambia de estado)
    }

    // Rotacion del asterisco
    private void RotacionAsterisco()
    {
        // Se calcula la rotacion segun el tiempo que va transcurrioendo para que rote uniformemente
        rotacion = rotacion + (rot * Time.deltaTime);
        
        // Se calcula el seno y coseno del angulo que va rotando
        SinAng = Mathf.Sin(rotacion);
        CosAng = Mathf.Cos(rotacion);

        // Se asigna el angulo de rotacion al cuaternion q
        q.Set(0, SinAng, 0, CosAng);

        // Se aplica el cuaternion calculado al transform del asterisco
        transform.rotation = q;
    }







    /*
    // Update is called once per frame
    void Update()
    {
        // Se envia mensaje del estado de la maya
        EnviarMensaje(Distancia());

        // Llamamos a la maquina de estado
        estadoActual();
    }
    */






    /*
                                    Maquina de estado
        !-----------------------------------------------------------------------!
        ! ↓ Estados ↓   !     0     !     1         !     2         !     3     ! ← Valores byte
        !---------------!-----------!---------------!---------------!-----------!
        ! DondeEsta     ! MayaCerca ! MayaLejos     !               !           !
        !---------------!-----------!---------------!---------------!-----------!
        ! MayaLejos     ! MayaCerca ! NoHayCambios  !               !           !
        !---------------!-----------!---------------!---------------!-----------!
        ! MayaCerca     !           !               ! NoHayCambios  ! MayaLejos !
        !---------------!-----------!---------------!---------------!-----------!
        ! NoHayCambios  ! MayaCerca ! NoHayCambios  ! NoHayCambios  ! MayaLejos !
        !-----------------------------------------------------------------------!

    */

    // Funcion de maquina de estado
    private void MaquinaDeEstado()
    {
        // Estados de la maquina
        maquina = new Diccionario()
        {
            // Averigua en que estado esta el asterisco
            {
                DondeEsta, new Mensaje()
                {
                    {0, MayaCerca},     // Cuando recibe el mensaje 0, conduce al estado del asterisco cerca
                    {1, MayaLejos}      // Cuando recibe el mensaje 1, conduce al estado del asterisco lejos
                }
            },

            // Estado del asterisco al estar lejos
            {
                MayaLejos, new Mensaje()
                {
                    {0, MayaCerca},     // Cuando recibe el mensaje 0, conduce al estado del asterisco cerca
                    {1, NoHayCambios}   // Cuando recibe el mensaje 1, conduce al estado No hay cambios para no tener que cargar la misma maya
                }
            },
            
            // Estado del asterisco al estar cerca
            {
                MayaCerca, new Mensaje()
                {
                    {2, NoHayCambios},  // Cuando recibe el mensaje 2, conduce al estado No hay cambios para no terner que cargar la misma maya
                    {3, MayaLejos}      // Cuando recibe el mensaje 3, conduce al estado del asterisco lejos
                }
            },

            // Estado en el que en el estado No hay cambios
            {
                NoHayCambios, new Mensaje()
                {
                    {0, MayaCerca},     // Cuando recibe el mensaje 0, conduce al estado del asterisco cerca
                    {1, NoHayCambios},  // Cuando recibe el mensaje 1, conduce al estado No hay cambios para no terner que cargar la misma maya
                    {2, NoHayCambios},  // Cuando recibe el mensaje 2, conduce al estado No hay cambios para no terner que cargar la misma maya
                    {3, MayaLejos}      // Cuando recibe el mensaje 3, conduce al estado del asterisco lejos
                }
            }
        };
    }

    // Envio de mensajes de la maquina de estado
    private void EnviarMensaje(byte msj)
    {
        // Actualiza el estado de la maquina
        estadoActual = mensaje[msj];
        // Refresca la lista de mensajes
        mensaje = maquina[estadoActual];
    }

    // Funciones apuntadas en la maquina de estado

    // Funcion del estado
    // Busca en que estado se encuentra el asterisco cerca o lejos
    private void DondeEsta()
    {
        EnviarMensaje(Distancia());
    }

    // Maya Asterisco lejos
    void MayaLejos()
    {
        // Asignacion de la maya
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        // Material para la maya
        meshRenderer.material = materialLejos;

        // Vertices del Asterisco lejos
        vertices = new[]
        {
            new Vector3(-0.3581f,   -0.1164f,   0f),
            new Vector3(1.0104f,    -0.0827f,   0f),
            new Vector3(0.7688f,    0.6607f,    0f),
            new Vector3(-0.3581f,   -0.1164f,   0f),
            new Vector3(0.7688f,    0.6607f,    0f),
            new Vector3(1.0104f,    -0.0827f,   0f),
            new Vector3(0f,         0f,         0.1706f),
            new Vector3(0f,         0f,         -0.1706f),
            new Vector3(1.4367f,    0.4278f,    0f),
            new Vector3(0f,         0f,         0.1706f),
            new Vector3(1.4367f,    0.4278f,    0f),
            new Vector3(0f,         0f,         -0.1706f),
            new Vector3(0f,         0f,         0.1706f),
            new Vector3(0f,         0f,         -0.1706f),
            new Vector3(-0.9108f,   -1.1906f,   0f),
            new Vector3(0f,         0f,         0.1706f),
            new Vector3(-0.9108f,   -1.1906f,   0f),
            new Vector3(0f,         0f,         -0.1706f),
            new Vector3(0.2213f,    0.3046f,    0f),
            new Vector3(-0.866f,    -0.527f,    0f),
            new Vector3(-0.2337f,   -0.9864f,   0f),
            new Vector3(0.2213f,    0.3046f,    0f),
            new Vector3(-0.2337f,   -0.9864f,   0f),
            new Vector3(-0.866f,    -0.527f,    0f),
            new Vector3(0.0001f,    -0.3765f,   0f),
            new Vector3(0.3909f,    0.9354f,    0f),
            new Vector3(-0.3907f,   0.9354f,    0f),
            new Vector3(0.0001f,    -0.3765f,   0f),
            new Vector3(-0.3907f,   0.9354f,    0f),
            new Vector3(0.3909f,    0.9354f,    0f),
            new Vector3(0.3581f,    -0.1163f,   0f),
            new Vector3(-0.7688f,   0.6608f,    0f),
            new Vector3(-1.0103f,   -0.0826f,   0f),
            new Vector3(0.3581f,    -0.1163f,   0f),
            new Vector3(-1.0103f,   -0.0826f,   0f),
            new Vector3(-0.7688f,   0.6608f,    0f),
            new Vector3(-0.2214f,   0.3046f,    0f),
            new Vector3(0.2336f,    -0.9865f,   0f),
            new Vector3(0.8659f,    -0.5271f,   0f),
            new Vector3(-0.2214f,   0.3046f,    0f),
            new Vector3(0.8659f,    -0.5271f,   0f),
            new Vector3(0.2336f,    -0.9865f,   0f),
            new Vector3(0f,         0f,         0.1706f),
            new Vector3(0f,         0f,         -0.1706f),
            new Vector3(0.8508f,    -1.2341f,   0f),
            new Vector3(0f,         0f,         0.1706f),
            new Vector3(0.8508f,    -1.2341f,   0f),
            new Vector3(0f,         0f,         -0.1706f),
            new Vector3(0f,         0f,         0.1706f),
            new Vector3(0f,         0f,         -0.1706f),
            new Vector3(0.0371f,    1.4985f,    0f),
            new Vector3(0f,         0f,         0.1706f),
            new Vector3(0.0371f,    1.4985f,    0f),
            new Vector3(0f,         0f,         -0.1706f),
            new Vector3(0f,         0f,         0.1706f),
            new Vector3(0f,         0f,         -0.1706f),
            new Vector3(-1.4137f,   0.4983f,    0f),
            new Vector3(0f,         0f,         0.1706f),
            new Vector3(-1.4137f,   0.4983f,    0f),
            new Vector3(0f,         0f,         -0.1706f)
        };
        mesh.vertices = vertices;

        // Coordenadas de textura Asterisco lejos
        uv = new[]
        {
            new Vector2(0.5f,       0f),
            new Vector2(0f,         1f),
            new Vector2(1f,         1f),
            new Vector2(0.5f,       0f),
            new Vector2(0f,         1f),
            new Vector2(1f,         1f),
            new Vector2(1.1897f,    0.4104f),
            new Vector2(0.8114f,    0.4104f),
            new Vector2(1f,         0.959f),
            new Vector2(1.1897f,    0.4104f),
            new Vector2(1f,         0.959f),
            new Vector2(0.8114f,    0.4104f),
            new Vector2(1.1897f,    0.4104f),
            new Vector2(0.8114f,    0.4104f),
            new Vector2(1f,         0.959f),
            new Vector2(1.1897f,    0.4104f),
            new Vector2(1f,         0.959f),
            new Vector2(0.8114f,    0.4104f),
            new Vector2(0.5f,       0f),
            new Vector2(0f,         1f),
            new Vector2(1f,         1f),
            new Vector2(0.5f,       0f),
            new Vector2(0f,         1f),
            new Vector2(1f,         1f),
            new Vector2(0.5f,       0f),
            new Vector2(0f,         1f),
            new Vector2(1f,         1f),
            new Vector2(0.5f,       0f),
            new Vector2(0f,         1f),
            new Vector2(1f,         1f),
            new Vector2(0.5f,       0f),
            new Vector2(0f,         1f),
            new Vector2(1f,         1f),
            new Vector2(0.5f,       0f),
            new Vector2(0f,         1f),
            new Vector2(1f,         1f),
            new Vector2(0.5f,       0f),
            new Vector2(0f,         1f),
            new Vector2(1f,         1f),
            new Vector2(0.5f,       0f),
            new Vector2(0f,         1f),
            new Vector2(1f,         1f),
            new Vector2(1.1897f,    0.4104f),
            new Vector2(0.8114f,    0.4104f),
            new Vector2(1f,         0.959f),
            new Vector2(1.1897f,    0.4104f),
            new Vector2(1f,         0.959f),
            new Vector2(0.8114f,    0.4104f),
            new Vector2(1.1897f,    0.4104f),
            new Vector2(0.8114f,    0.4104f),
            new Vector2(1f,         0.959f),
            new Vector2(1.1897f,    0.4104f),
            new Vector2(1f,         0.959f),
            new Vector2(0.8114f,    0.4104f),
            new Vector2(1.1897f,    0.4104f),
            new Vector2(0.8114f,    0.4104f),
            new Vector2(1f,         0.959f),
            new Vector2(1.1897f,    0.4104f),
            new Vector2(1f,         0.959f),
            new Vector2(0.8114f,    0.4104f)
        };
        mesh.uv = uv;

        // Normales del Asterisco lejos
        normales = new[]
        {
            new Vector3(0f,             0f,             1f),
            new Vector3(0.9653399f,     -0.03370139f,   0.2588107f),
            new Vector3(0.761173f,      0.5946788f,     0.2587908f),
            new Vector3(0f,             0f,             -1f),
            new Vector3(0.761173f,      0.5946788f,     -0.2587908f),
            new Vector3(0.9653399f,     -0.03370139f,   -0.2588107f),
            new Vector3(0.06480099f,    -0.2510038f,    0.9658147f),
            new Vector3(0.06480099f,    -0.2510038f,    -0.9658147f),
            new Vector3(0.9923577f,     0.1233947f,     0f),
            new Vector3(-0.06480099f,   0.2510038f,     0.9658147f),
            new Vector3(0.8902896f,     0.4553947f,     0f),
            new Vector3(-0.06480099f,   0.2510038f,     -0.9658147f),
            new Vector3(-0.2000005f,    0.1650005f,     0.9658026f),
            new Vector3(-0.2000005f,    0.1650005f,     -0.9658026f),
            new Vector3(-0.7303132f,    -0.6831124f,    0f),
            new Vector3(0.2000005f,     -0.1650005f,    0.9658026f),
            new Vector3(-0.4526055f,    -0.8917108f,    0f),
            new Vector3(0.2000005f,     -0.1650005f,    -0.9658026f),
            new Vector3(0f,             0f,             1f),
            new Vector3(-0.8008136f,    -0.5401092f,    0.2588044f),
            new Vector3(-0.2662064f,    -0.9285222f,    0.2588062f),
            new Vector3(0f,             0f,             -1f),
            new Vector3(-0.2662064f,    -0.9285222f,    -0.2588062f),
            new Vector3(-0.8008136f,    -0.5401092f,    -0.2588044f),
            new Vector3(0f,             0f,             1f),
            new Vector3(0.3303899f,     0.9076725f,     0.2587922f),
            new Vector3(-0.3303899f,    0.9076725f,     0.2587922f),
            new Vector3(0f,             0f,             -1f),
            new Vector3(-0.3303899f,    0.9076725f,     -0.2587922f),
            new Vector3(0.3303899f,     0.9076725f,     -0.2587922f),
            new Vector3(0f,             0f,             1f),
            new Vector3(-0.761173f,     0.5946788f,     0.2587908f),
            new Vector3(-0.9653399f,    -0.03370139f,   0.2588107f),
            new Vector3(0f,             0f,             -1f),
            new Vector3(-0.9653399f,    -0.03370139f,   -0.2588107f),
            new Vector3(-0.761173f,     0.5946788f,     -0.2587908f),
            new Vector3(0f,             0f,             1f),
            new Vector3(0.2662064f,     -0.9285222f,    0.2588062f),
            new Vector3(0.8008136f,     -0.5401092f,    0.2588044f),
            new Vector3(0f,             0f,             -1f),
            new Vector3(0.8008136f,     -0.5401092f,    -0.2588044f),
            new Vector3(0.2662064f,     -0.9285222f,    -0.2588062f),
            new Vector3(-0.2187026f,    -0.1392017f,    0.9658116f),
            new Vector3(-0.2187026f,    -0.1392017f,    -0.9658116f),
            new Vector3(0.4239855f,     -0.905669f,     0f),
            new Vector3(0.2187026f,     0.1392017f,     0.9658116f),
            new Vector3(0.7082059f,     -0.7060059f,    0f),
            new Vector3(0.2187026f,     0.1392017f,     -0.9658116f),
            new Vector3(0.2588f,        -0.0159f,       0.9658f),
            new Vector3(0.2588f,        -0.0159f,       -0.9658f),
            new Vector3(0.1893036f,     0.9819186f,     0f),
            new Vector3(-0.2588f,       0.0159f,        0.9658f),
            new Vector3(-0.1579086f,    0.9874537f,     0f),
            new Vector3(-0.2588f,       0.0159f,        -0.9658f),
            new Vector3(0.09510043f,    0.2412011f,     0.9658043f),
            new Vector3(0.09510043f,    0.2412011f,     -0.9658043f),
            new Vector3(-0.8753573f,    0.4834764f,     0f),
            new Vector3(-0.09510043f,   -0.2412011f,    0.9658043f),
            new Vector3(-0.9879295f,    0.1549046f,     0f),
            new Vector3(-0.09510043f,   -0.2412011f,    -0.9658043f)
        };
        mesh.normals = normales;

        // Triangulos de la maya Asterisco lejos
        triangulos = new[]
        {
            0,1,2,
            3,4,5,
            6,7,8,
            9,10,11,
            12,13,14,
            15,16,17,
            18,19,20,
            21,22,23,
            24,25,26,
            27,28,29,
            30,31,32,
            33,34,35,
            36,37,38,
            39,40,41,
            42,43,44,
            45,46,47,
            48,49,50,
            51,52,53,
            54,55,56,
            57,58,59
        };
        mesh.triangles = triangulos;

        // El condicionador vale 0 para que no se repita este estado hasta que cambie a cerca
        condicionador = 0;
    }

    // Maya Asterisco cerca
    void MayaCerca()
    {
        // No es nesesario reacer el mesh ya que solo se cambian las variables interna pero si cuando se reducen los trianguos como ocurre en (MayaLejos).

        // Material para la maya
        meshRenderer.material = materialCerca;

        // Vertices del asterisco cerca
        vertices = new[]
        {
            new Vector3(0.0003f,    -0.0009f,   0.1702f),
            new Vector3(0.7124f,    0.22f,      0.0766f),
            new Vector3(0.1728f,    0.2365f,    0.0925f),
            new Vector3(0.2794f,    -0.0916f,   0.0925f),
            new Vector3(0.7972f,    0.0799f,    0.0594f),
            new Vector3(0.8749f,    0.2595f,    0f),
            new Vector3(0.7362f,    0.4022f,    0.052f),
            new Vector3(0.7435f,    0.4293f,    0f),
            new Vector3(0.215f,     0.2945f,    0f),
            new Vector3(-0.0097f,   0.7446f,    0.0766f),
            new Vector3(0.1563f,    0.8235f,    0.052f),
            new Vector3(-0.0223f,   0.9114f,    0f),
            new Vector3(-0.1691f,   0.782f,     0.0594f),
            new Vector3(-0.1722f,   0.2365f,    0.0925f),
            new Vector3(0.4465f,    -0.5982f,   0.0766f),
            new Vector3(0.5975f,    -0.5347f,   0.0594f),
            new Vector3(0.0003f,    -0.2944f,   0.0925f),
            new Vector3(0.5548f,    -0.7257f,   0f),
            new Vector3(0.3587f,    -0.7596f,   0.052f),
            new Vector3(0.636f,     -0.5243f,   0f),
            new Vector3(0.3487f,    -0.7858f,   0f),
            new Vector3(0.1798f,    0.8389f,    0f),
            new Vector3(-0.2064f,   0.7962f,    0f),
            new Vector3(0.0003f,    -0.3661f,   0f),
            new Vector3(-0.3481f,   -0.7858f,   0f),
            new Vector3(-0.3581f,   -0.7596f,   0.052f),
            new Vector3(-0.4459f,   -0.5982f,   0.0766f),
            new Vector3(-0.5542f,   -0.7257f,   0f),
            new Vector3(-0.5969f,   -0.5347f,   0.0594f),
            new Vector3(-0.2788f,   -0.0916f,   0.0925f),
            new Vector3(-0.347f,    -0.1138f,   0f),
            new Vector3(-0.6354f,   -0.5243f,   0f),
            new Vector3(-0.7056f,   0.2389f,    0.0766f),
            new Vector3(-0.832f,    0.1055f,    0.052f),
            new Vector3(-0.8603f,   0.3025f,    0f),
            new Vector3(-0.6919f,   0.4021f,    0.0594f),
            new Vector3(-0.2144f,   0.2945f,    0f),
            new Vector3(-0.6939f,   0.442f,     0f),
            new Vector3(-0.8539f,   0.0879f,    0f),
            new Vector3(0.1728f,    0.2365f,    -0.0925f),
            new Vector3(0.215f,     0.2945f,    0f),
            new Vector3(0.7435f,    0.4293f,    0f),
            new Vector3(0.7362f,    0.4022f,    -0.052f),
            new Vector3(0.8749f,    0.2595f,    0f),
            new Vector3(0.7124f,    0.22f,      -0.0766f),
            new Vector3(0.7972f,    0.0799f,    -0.0594f),
            new Vector3(0.2794f,    -0.0916f,   -0.0925f),
            new Vector3(0.0003f,    -0.0009f,   -0.1702f),
            new Vector3(-0.0097f,   0.7446f,    -0.0766f),
            new Vector3(-0.1722f,   0.2365f,    -0.0925f),
            new Vector3(0.4465f,    -0.5982f,   -0.0766f),
            new Vector3(0.0003f,    -0.2944f,   -0.0925f),
            new Vector3(-0.4459f,   -0.5982f,   -0.0766f),
            new Vector3(-0.2788f,   -0.0916f,   -0.0925f),
            new Vector3(-0.7056f,   0.2389f,    -0.0766f),
            new Vector3(-0.6919f,   0.4021f,    -0.0594f),
            new Vector3(-0.8603f,   0.3025f,    0f),
            new Vector3(-0.832f,    0.1055f,    -0.052f),
            new Vector3(-0.8539f,   0.0879f,    0f),
            new Vector3(-0.347f,    -0.1138f,   0f),
            new Vector3(-0.5969f,   -0.5347f,   -0.0594f),
            new Vector3(-0.5542f,   -0.7257f,   0f),
            new Vector3(-0.6354f,   -0.5243f,   0f),
            new Vector3(-0.3581f,   -0.7596f,   -0.052f),
            new Vector3(-0.3481f,   -0.7858f,   0f),
            new Vector3(0.0003f,    -0.3661f,   0f),
            new Vector3(0.3487f,    -0.7858f,   0f),
            new Vector3(0.3587f,    -0.7596f,   -0.052f),
            new Vector3(0.5548f,    -0.7257f,   0f),
            new Vector3(0.5975f,    -0.5347f,   -0.0594f),
            new Vector3(0.636f,     -0.5243f,   0f),
            new Vector3(0.3476f,    -0.1138f,   0f),
            new Vector3(0.8223f,    0.0488f,    0f),
            new Vector3(-0.2144f,   0.2945f,    0f),
            new Vector3(-0.2064f,   0.7962f,    0f),
            new Vector3(-0.1691f,   0.782f,     -0.0594f),
            new Vector3(-0.0223f,   0.9114f,    0f),
            new Vector3(0.1563f,    0.8235f,    -0.052f),
            new Vector3(0.1798f,    0.8389f,    0f),
            new Vector3(-0.6939f,   0.442f,     0f),
            new Vector3(0.8223f,    0.0488f,    0f),
            new Vector3(0.3476f,    -0.1138f,   0f)
        };
        mesh.vertices = vertices;

        // Coordenadas de textura Asterisco cerca
        uv = new[]
        {
            new Vector2(0.5053f,    0.5032f),
            new Vector2(0.4064f,    0.6846f),
            new Vector2(0.4116f,    0.5457f),
            new Vector2(0.5184f,    0.6041f),
            new Vector2(0.4408f,    0.7152f),
            new Vector2(0.3869f,    0.7266f),
            new Vector2(0.3546f,    0.6813f),
            new Vector2(0.3467f,    0.6818f),
            new Vector2(0.3886f,    0.5561f),
            new Vector2(0.2989f,    0.4603f),
            new Vector2(0.2682f,    0.5011f),
            new Vector2(0.2532f,    0.4478f),
            new Vector2(0.2969f,    0.4149f),
            new Vector2(0.4343f,    0.4286f),
            new Vector2(0.6476f,    0.6566f),
            new Vector2(0.622f,     0.6942f),
            new Vector2(0.6072f,    0.523f),
            new Vector2(0.6773f,    0.6929f),
            new Vector2(0.6971f,    0.6415f),
            new Vector2(0.6171f,    0.7041f),
            new Vector2(0.7049f,    0.6402f),
            new Vector2(0.2627f,    0.5066f),
            new Vector2(0.2949f,    0.404f),
            new Vector2(0.6321f,    0.5278f),
            new Vector2(0.7417f,    0.4509f),
            new Vector2(0.7349f,    0.4468f),
            new Vector2(0.6947f,    0.4142f),
            new Vector2(0.7358f,    0.3917f),
            new Vector2(0.6851f,    0.3698f),
            new Vector2(0.5552f,    0.4146f),
            new Vector2(0.5674f,    0.3929f),
            new Vector2(0.6842f,    0.3588f),
            new Vector2(0.476f,     0.2985f),
            new Vector2(0.5197f,    0.2714f),
            new Vector2(0.4665f,    0.2531f),
            new Vector2(0.43f,      0.2935f),
            new Vector2(0.417f,     0.4104f),
            new Vector2(0.419f,     0.2908f),
            new Vector2(0.5258f,    0.2664f),
            new Vector2(0.4116f,    0.5457f),
            new Vector2(0.3886f,    0.5561f),
            new Vector2(0.3467f,    0.6818f),
            new Vector2(0.3546f,    0.6813f),
            new Vector2(0.3869f,    0.7266f),
            new Vector2(0.4064f,    0.6846f),
            new Vector2(0.4408f,    0.7152f),
            new Vector2(0.5184f,    0.6041f),
            new Vector2(0.5053f,    0.5032f),
            new Vector2(0.2989f,    0.4603f),
            new Vector2(0.4343f,    0.4286f),
            new Vector2(0.6476f,    0.6566f),
            new Vector2(0.6072f,    0.523f),
            new Vector2(0.6947f,    0.4142f),
            new Vector2(0.5552f,    0.4146f),
            new Vector2(0.476f,     0.2985f),
            new Vector2(0.43f,      0.2935f),
            new Vector2(0.4665f,    0.2531f),
            new Vector2(0.5197f,    0.2714f),
            new Vector2(0.5258f,    0.2664f),
            new Vector2(0.5674f,    0.3929f),
            new Vector2(0.6851f,    0.3698f),
            new Vector2(0.7358f,    0.3917f),
            new Vector2(0.6842f,    0.3588f),
            new Vector2(0.7349f,    0.4468f),
            new Vector2(0.7417f,    0.4509f),
            new Vector2(0.6321f,    0.5278f),
            new Vector2(0.7049f,    0.6402f),
            new Vector2(0.6971f,    0.6415f),
            new Vector2(0.6773f,    0.6929f),
            new Vector2(0.622f,     0.6942f),
            new Vector2(0.6171f,    0.7041f),
            new Vector2(0.5216f,    0.6287f),
            new Vector2(0.4481f,    0.7237f),
            new Vector2(0.417f,     0.4104f),
            new Vector2(0.2949f,    0.404f),
            new Vector2(0.2969f,    0.4149f),
            new Vector2(0.2532f,    0.4478f),
            new Vector2(0.2682f,    0.5011f),
            new Vector2(0.2627f,    0.5066f),
            new Vector2(0.419f,     0.2908f),
            new Vector2(0.4481f,    0.7237f),
            new Vector2(0.5216f,    0.6287f)
        };
        mesh.uv = uv;

        // Normales del asterisco cerca
        normales = new[]
        {
            new Vector3(0.0001f,        -0.0002f,       1f),
            new Vector3(0.2026019f,     0.06180058f,    0.9773092f),
            new Vector3(0.1534016f,     0.2991031f,     0.9418097f),
            new Vector3(0.346902f,      -0.1127007f,    0.9311055f),
            new Vector3(0.4913823f,     -0.3690867f,    0.7888716f),
            new Vector3(0.9779149f,     0.2090032f,     0f),
            new Vector3(0.2550951f,     0.5996885f,     0.7584854f),
            new Vector3(0.3176974f,     0.9481922f,     0f),
            new Vector3(0.2573079f,     0.9663295f,     0f),
            new Vector3(0.09059726f,    0.2173934f,     0.9718705f),
            new Vector3(0.5458991f,     0.4233993f,     0.7229988f),
            new Vector3(-0.103503f,     0.9946292f,     0f),
            new Vector3(-0.5028767f,    0.3532836f,     0.7888635f),
            new Vector3(-0.2144002f,    0.2951002f,     0.9311007f),
            new Vector3(0.1275021f,     -0.1691028f,    0.9773164f),
            new Vector3(0.6144786f,     0.00979966f,    0.7888726f),
            new Vector3(0f,             -0.3809912f,    0.9245787f),
            new Vector3(0.668296f,      -0.7438955f,    0f),
            new Vector3(-0.1461965f,    -0.6350846f,    0.7584816f),
            new Vector3(0.990265f,      0.1391951f,     0f),
            new Vector3(-0.3002842f,    -0.9538498f,    0f),
            new Vector3(0.8043112f,     0.5942083f,     0f),
            new Vector3(-0.8829253f,    0.4695134f,     0f),
            new Vector3(0f,             -1f,            0f),
            new Vector3(0.3002842f,     -0.9538498f,    0f),
            new Vector3(0.1461965f,     -0.6350846f,    0.7584816f),
            new Vector3(-0.1275021f,    -0.1691028f,    0.9773164f),
            new Vector3(-0.668296f,     -0.7438955f,    0f),
            new Vector3(-0.6144786f,    0.00979966f,    0.7888726f),
            new Vector3(-0.3561011f,    -0.1104003f,    0.9279029f),
            new Vector3(-0.9545689f,    -0.2979903f,    0f),
            new Vector3(-0.990265f,     0.1391951f,     0f),
            new Vector3(-0.2002044f,    0.0690015f,     0.9773213f),
            new Vector3(-0.5587984f,    -0.335299f,     0.7584978f),
            new Vector3(-0.9139681f,    0.4057859f,     0f),
            new Vector3(-0.1805983f,    0.5873947f,     0.7888928f),
            new Vector3(-0.587803f,     0.8090041f,     0f),
            new Vector3(-0.1736028f,    0.9848158f,     0f),
            new Vector3(-0.8144019f,    -0.5803013f,    0f),
            new Vector3(0.1534016f,     0.2991031f,     -0.9418097f),
            new Vector3(0.2573079f,     0.9663295f,     0f),
            new Vector3(0.3176974f,     0.9481922f,     0f),
            new Vector3(0.2550951f,     0.5996885f,     -0.7584854f),
            new Vector3(0.9779149f,     0.2090032f,     0f),
            new Vector3(0.2026019f,     0.06180058f,    -0.9773092f),
            new Vector3(0.4913823f,     -0.3690867f,    -0.7888716f),
            new Vector3(0.346902f,      -0.1127007f,    -0.9311055f),
            new Vector3(0.0001f,        -0.0002f,       -1f),
            new Vector3(0.09059726f,    0.2173934f,     -0.9718705f),
            new Vector3(-0.2144002f,    0.2951002f,     -0.9311007f),
            new Vector3(0.1275021f,     -0.1691028f,    -0.9773164f),
            new Vector3(0f,             -0.3809912f,    -0.9245787f),
            new Vector3(-0.1275021f,    -0.1691028f,    -0.9773164f),
            new Vector3(-0.3561011f,    -0.1104003f,    -0.9279029f),
            new Vector3(-0.2002044f,    0.0690015f,     -0.9773213f),
            new Vector3(-0.1805983f,    0.5873947f,     -0.7888928f),
            new Vector3(-0.9139681f,    0.4057859f,     0f),
            new Vector3(-0.5587984f,    -0.335299f,     -0.7584978f),
            new Vector3(-0.8144019f,    -0.5803013f,    0f),
            new Vector3(-0.9545689f,    -0.2979903f,    0f),
            new Vector3(-0.6144786f,    0.00979966f,    -0.7888726f),
            new Vector3(-0.668296f,     -0.7438955f,    0f),
            new Vector3(-0.990265f,     0.1391951f,     0f),
            new Vector3(0.1461965f,     -0.6350846f,    -0.7584816f),
            new Vector3(0.3002842f,     -0.9538498f,    0f),
            new Vector3(0f,             -1f,            0f),
            new Vector3(-0.3002842f,    -0.9538498f,    0f),
            new Vector3(-0.1461965f,    -0.6350846f,    -0.7584816f),
            new Vector3(0.668296f,      -0.7438955f,    0f),
            new Vector3(0.6144786f,     0.00979966f,    -0.7888726f),
            new Vector3(0.990265f,      0.1391951f,     0f),
            new Vector3(0.9510657f,     -0.3089888f,    0f),
            new Vector3(0.7192998f,     -0.6946998f,    0f),
            new Vector3(-0.587803f,     0.8090041f,     0f),
            new Vector3(-0.8829253f,    0.4695134f,     0f),
            new Vector3(-0.5028767f,    0.3532836f,     -0.7888635f),
            new Vector3(-0.103503f,     0.9946292f,     0f),
            new Vector3(0.5458991f,     0.4233993f,     -0.7229988f),
            new Vector3(0.8043112f,     0.5942083f,     0f),
            new Vector3(-0.1736028f,    0.9848158f,     0f),
            new Vector3(0.7192998f,     -0.6946998f,    0f),
            new Vector3(0.9510657f,     -0.3089888f,    0f)
        };
        mesh.normals = normales;

        // Triangulos de la maya Asterisco cerca
        triangulos = new[]
        {
            0,1,2,
            0,3,1,
            4,1,3,
            1,4,5,
            1,5,6,
            6,2,1,
            7,2,6,
            6,5,7,
            7,8,2,
            0,2,9,
            9,2,8,
            9,8,10,
            9,10,11,
            9,11,12,
            9,12,13,
            0,9,13,
            0,14,3,
            3,14,15,
            0,16,14,
            14,17,15,
            14,18,17,
            14,16,18,
            15,17,19,
            17,18,20,
            11,10,21,
            10,8,21,
            12,11,22,
            18,16,23,
            18,23,20,
            16,24,23,
            16,25,24,
            16,26,25,
            0,26,16,
            26,27,25,
            25,27,24,
            26,28,27,
            0,29,26,
            29,28,26,
            28,29,30,
            28,30,31,
            27,28,31,
            0,32,29,
            29,32,33,
            32,34,33,
            32,35,34,
            0,13,32,
            13,35,32,
            35,13,36,
            35,36,37,
            34,35,37,
            33,34,38,
            29,33,38,
            29,38,30,
            36,13,12,
            36,12,22,
            39,40,41,
            39,41,42,
            41,43,42,
            42,43,44,
            39,42,44,
            45,44,43,
            46,44,45,
            47,44,46,
            47,39,44,
            47,48,39,
            48,40,39,
            47,49,48,
            47,46,50,
            47,50,51,
            47,51,52,
            47,52,53,
            47,54,49,
            47,53,54,
            49,54,55,
            55,54,56,
            57,56,54,
            53,57,54,
            53,58,57,
            58,56,57,
            53,59,58,
            60,59,53,
            60,53,52,
            60,52,61,
            60,62,59,
            60,61,62,
            63,61,52,
            51,63,52,
            51,64,63,
            64,61,63,
            51,65,64,
            66,65,51,
            66,51,67,
            66,67,68,
            68,67,50,
            69,68,50,
            69,70,68,
            69,71,70,
            69,46,71,
            46,69,50,
            67,51,50,
            71,46,45,
            71,45,72,
            45,43,72,
            73,74,75,
            73,75,49,
            75,74,76,
            75,76,48,
            49,75,48,
            76,77,48,
            78,77,76,
            78,40,77,
            48,77,40,
            73,49,55,
            73,55,79,
            55,56,79,
            5,4,80,
            4,81,80,
            4,3,81,
            81,3,15,
            81,15,19
        };
        mesh.triangles = triangulos;

        // El condicionador vale 2 para que no se repita este estado hasta que cambie a lejos
        condicionador = 2;
    }

    // No hay cambios de instruccion en el programa
    void NoHayCambios()
    {
        // No se hace nada
    }

}
