Shader "ShaderCG/Textura Con Tinte Luz Propia (Sin if)"
{
	Properties			//Propiedades
	{
		_Tinte ("Tinte", Color) = (1,1,1,1)							//Color del tinte
		_Lambert ("Lambert", Range (0,1)) = 0.8						//Lambert para disimular los saltos del color
		_Brillo ("Brillo", Range(0,5)) = 0.8						//Brillo para regular y controlar la luz
		_Contraste ("Contraste", Range(0,10)) = 0.2					//Contraste para regular y controlar la luz
		_Alpha ("Alfa", Range (0,2)) = 1.45							//Propiedad del alfa
		_TestTex("My texture", 2D) = "white"						//Textura una mascara para aparentar el objeto 3d de color blanco para poder cambiar su color con el tinte
		_LuzDir("Direccion de luz", Vector) = (-0.1, 0.435, -0.924)	//Luz direccional la unica luz con la que interactua para reducir los recursos en el prosesado
	}

	SubShader			//Inicio de Shader
	{
		Tags {"Queue"="Transparent"}

		Blend SrcAlpha OneMinusSrcAlpha							//Trabaja con la transparencia del alfa de la textura
		
		LOD 100													// 100 para que no interactuen con otras luces
		
		ZWrite Off												//Realiza un mapa de profundidad

		Pass
		{
			
			CGPROGRAM
			#pragma vertex vert									//Vert
			#pragma fragment frag								//Frag

			// Declaracion de variables del las propiedades
			uniform fixed4		_Tinte;
			uniform sampler2D	_TestTex;
			uniform fixed3		_LuzDir;
			uniform fixed		_Lambert;
			uniform fixed		_Brillo;
			uniform fixed		_Contraste;
			uniform fixed		_Alpha;
			
			struct appdata										//Entradas Vert
			{
				fixed4 pos		: POSITION;						//Posicion de los vertices
				fixed2 uv		: TEXCOORD0;					//Coordenadas de la textura en los vertices
				fixed3 normal	: NORMAL;						//Normales de los vertices
			};

			struct v2f											//Salidas Vert
			{
				fixed4 pos		: SV_POSITION;					//Vertices procesados
				fixed2 uv		: TEXCOORD0;					//Coordenada del vertice
				fixed4 color	: COLOR;						//Color del vertice
			};
			
			v2f vert (appdata v)								//Programa del Vert
			{
				v2f o;																	//Variable para la salida
				o.pos = UnityObjectToClipPos(v.pos);									//multiplicamos por la matrix MVP
				
				fixed dotNormalLuz = max (0.0, dot (v.normal, normalize(_LuzDir)));		//Producto punto entre la normal y la luz direccionela (vector de la luz normalizado)
				fixed hlambert = dotNormalLuz * _Lambert + (1 - _Lambert);				//Se calcula half lambert al producto punto

				o.color = pow (_Tinte * hlambert, _Contraste) * _Brillo;				//Color de salida con tinte, lamber, brillo y contraste
				o.uv = v.uv;															//Coordenadas de textura
				
				return o;																//Salida
			}
			
			fixed4 frag (v2f i) : COLOR							//Programa del Frag
			{
				fixed4 c;										//Variable para la salida
				c = tex2D(_TestTex, i.uv);						//Asignacion de la textura
				
				c = c * i.color;								//A lo que se dibuja se le agrega el color ya calulado
				
				c.a *= _Alpha;									//Se multiplica el alfa por su transparencia

				return c;
			}
			ENDCG
		}
	}
}
