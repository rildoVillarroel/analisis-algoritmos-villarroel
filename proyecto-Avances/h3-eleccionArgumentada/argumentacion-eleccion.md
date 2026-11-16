# Elección del algoritmo para MI caso:
# proyecto : ALGORITMO DE ASIGNACION OPTIMA PERSONAL DE EMPRESA ELECTRICA

## 1. Mi proceso, en una línea

Asignar personal técnico a las vacantes de cinco proyectos eléctricos simultáneos (H1), minimizando el costo total de asignación y respetando las restricciones obligatorias (competencia mínima, experiencia mínima, certificación y horas libres).

## 2. Mis datos

- **Hoy (n):** 20 trabajadores y 20 puestos (5 proyectos × 4 puestos) → 400 pares trabajador-puesto. En mi muestra sintética, solo 29 de esos 400 pares son elegibles (~7 %).
- **Peor mes (hipótesis de trabajo):** 100 trabajadores y 100 puestos (25 proyectos simultáneos) → 10.000 pares. Es una hipótesis de escala, no una medición.
- **Forma de los datos:** vienen sin ningún orden; cada par tiene un costo real entre 0 y 100 (`C = 100 − S`); muchos pares están prohibidos por restricciones obligatorias; la carga horaria de cada persona cambia de una semana a otra, así que el problema se resuelve de nuevo cada período. Un trabajador ocupa como máximo un puesto.

## 3. La operación que más pesa

**Elegir la combinación de menor costo entre todas las asignaciones posibles** (comparar emparejamientos). Construir la matriz de costos solo cuesta O(n²) cálculos baratos (400 hoy); lo que explota es decidir cuál de las n! asignaciones es la mejor.

## 4. Candidatos

| Técnica | Cómo funciona (una línea) | Costo en el peor caso (O) | Qué necesita |
|---|---|---|---|
| A: Fuerza bruta | Prueba las n! permutaciones y se queda con la de menor costo | O(n · n!) | Poca memoria; inviable para n grande |
| B: Voraz (greedy) | Ordena todos los pares por costo y toma siempre el más barato con trabajador y puesto libres | O(n² log n) | Lista de m ≤ n² pares; no garantiza el óptimo ni cubrir todos los puestos |
| C: Húngaro | Ajusta potenciales fila/columna y va aumentando emparejamientos hasta cubrir todo | O(n³) | Matriz n×n; garantiza el costo mínimo si existe asignación factible |

Recurrencias (según la clase 6):

- **A:** con m trabajadores libres, `L(m) = m·L(m−1) + 1` → llamadas = Σ n!/k! ≈ e·n!; como cada llamada recorre n trabajadores, el trabajo es O(n·n!). **El teorema maestro no aplica**: el problema se achica restando 1, no dividiendo; se resuelve desplegando.
- **B:** el ordenamiento de los m = n² pares tiene la forma de MergeSort, `T(m) = 2·T(m/2) + m` → a = 2, b = 2, d = 1 → `a = b^d` → todos los pisos pesan igual y hay log m pisos → **m log m** = n² log(n²) = O(n² log n).
- **C:** es iterativo (no se llama a sí mismo): n filas × hasta n pasos de aumento × n columnas por paso = O(n³). No hay recurrencia que resolver.

## 5. Mi elección y por qué

**Elijo C: el algoritmo húngaro.**

Con MIS datos (n = 20, 20! = 2.432.902.008.176.640.000):

| | n = 20 (hoy) | n = 100 (peor mes) |
|---|---|---|
| A fuerza bruta (llamadas ≈ e·n!) | ≈ 6,6 × 10¹⁸ (≈ 771 años a 100 millones por segundo) | ≈ 2,5 × 10¹⁵⁸ |
| B voraz (m·log₂ m con m = n²) | ≈ 3.458 | ≈ 132.877 |
| C húngaro (n³ como máximo) | ≤ 8.000 | ≤ 1.000.000 |

- **A queda descartada**: ya con n = 20 es imposible. Solo sirve como referencia para comprobar que los otros son correctos con n ≤ 10 (con n = 10 hace 9.864.101 llamadas).
- **B es más barato que C, pero no es confiable**: no garantiza el óptimo ni siquiera que todos los puestos queden cubiertos. Ejemplo mínimo `[[1,2],[2,100]]`: el voraz toma el 1 y queda obligado a pagar 100 → total 101, mientras que el óptimo es 2 + 2 = 4. Con n = 20 el húngaro cuesta a lo sumo 8.000 pasos: la diferencia de tiempo con el voraz es despreciable, y la diferencia de calidad no.
- Evidencia medida (Parte D del programa, 200 instancias de n = 20): *voraz no cubrió todos los puestos en ___ de 200; no fue óptimo en ___ de las completas; sobrecosto medio ___ %*. **(completar con la salida de `-- calidad`)**

**Cuándo cambiaría de opinión:**

1. Si un trabajador puede tomar **varios puestos** según sus horas libres, el húngaro clásico (uno a uno) deja de aplicar: habría que usar asignación generalizada o flujo de costo mínimo.
2. Si n creciera a miles y se necesitara una respuesta inmediata aproximada (n³ ya serían ~10⁹ pasos), usaría el voraz como solución inicial.
3. Con n ≤ 8–10 usaría la fuerza bruta solo para verificar que el húngaro es correcto.

## 6. Cómo lo voy a medir

- **Tamaños:** fuerza bruta n = 4, 5, 6, 7, 8, 9, 10 (más allá no entra); voraz y húngaro n = 20, 50, 100, 200, 500.
- **Mismos datos para todos:** instancia sintética con semilla fija (2026); la generación de datos y la construcción de la matriz **no** entran en el tiempo.
- **Protocolo (el de la clase):** 3 calentamientos por algoritmo, 3 mediciones por algoritmo y tamaño con `Stopwatch`, promedio = suma de tiempos / 3, y verificación de cada asignación (sin puestos repetidos, solo pares elegibles, costo recalculado) fuera del tiempo medido.
- **Qué comparo:** conteos exactos (llamadas, comparaciones, operaciones — iguales en cualquier computadora), tiempo en ms, costo total, cobertura de puestos y calidad en 200 instancias (semillas 1 a 200).
- **Archivos CSV generados:** `resultados.csv` (cada corrida), `resumen.csv` (promedios), `conteos.csv` (operaciones medidas contra teóricas) y `calidad.csv` (200 instancias, húngaro contra voraz).
- **Cómo se ejecuta:** `dotnet run -c Release -- eleccion` (o `-- datos | duelo | escala | calidad`).

### Evidencia (conteos exactos de la fuerza bruta, verificados contra la fórmula e·n!)

| n | 4 | 5 | 6 | 7 | 8 | 9 | 10 |
|---|---|---|---|---|---|---|---|
| llamadas | 65 | 326 | 1.957 | 13.700 | 109.601 | 986.410 | 9.864.101 |

**(pegar aquí las tablas de tiempos de tu salida de `-- duelo` y `-- escala`)**

---

### Resumen de una línea

*Con n = 20 (400 pares) y n = 100 en el peor mes, el trabajo es elegir la mejor de n! asignaciones; la fuerza bruta es imposible, el voraz es rápido pero puede fallar, y el húngaro garantiza el mínimo con a lo sumo n³ pasos. Evidencia: conteos exactos y tiempos medidos en el programa.*

##  RESULTADO SALIDA EN CONSOLA

$ dotnet run -c Release -- eleccion
PARTE A · MIS DATOS Y CUÁNTO CUESTA CADA CANDIDATO (todo exacto: son conteos)
Proceso: asignar n trabajadores a n puestos de proyectos eléctricos (datos SINTÉTICOS).
Hoy: n = 20 (5 proyectos, 20 puestos).   Peor mes (hipótesis de trabajo): n = 100 (25 proyectos).
Operación que más pesa: elegir la combinación de menor costo entre las n! posibles.

    n   pares n²   A fuerza bruta e·n!   B voraz m·log2 m   C húngaro n³
   10        100             9,864,101                664          1,000
   20        400              6.61E+18              3,458          8,000
   50      2,500              8.27E+64             28,219        125,000
  100     10,000             2.54E+158            132,877      1,000,000

20! = 2,432,902,008,176,640,000 permutaciones. A 100 millones de permutaciones por segundo (hipótesis generosa):
  tardaría ~771 años. El húngaro hace 8.000 pasos de orden n³.

--------------------------------------------------------
 TORNEO DE ASIGNACIÓN: FUERZA BRUTA · VORAZ · HÚNGARO
--------------------------------------------------------
Tamaños: 4, 5, 6, 7, 8, 9, 10, 20, 50, 100, 200, 500   (semilla 2026, datos sintéticos)

FASE 1: CALENTAMIENTO
FuerzaBruta - Calentamiento 1 completado.
FuerzaBruta - Calentamiento 2 completado.
FuerzaBruta - Calentamiento 3 completado.
Voraz - Calentamiento 1 completado.
Voraz - Calentamiento 2 completado.
Voraz - Calentamiento 3 completado.
Hungaro - Calentamiento 1 completado.
Hungaro - Calentamiento 2 completado.
Hungaro - Calentamiento 3 completado.

FASE 2: VERIFICACIÓN
4 trabajadores: asignaciones válidas (fuerza bruta = húngaro).
5 trabajadores: asignaciones válidas (fuerza bruta = húngaro).
6 trabajadores: asignaciones válidas (fuerza bruta = húngaro).
7 trabajadores: asignaciones válidas (fuerza bruta = húngaro).
8 trabajadores: asignaciones válidas (fuerza bruta = húngaro).
9 trabajadores: asignaciones válidas (fuerza bruta = húngaro).
10 trabajadores: asignaciones válidas (fuerza bruta = húngaro).
20 trabajadores: asignaciones válidas (húngaro cubre todos los puestos).
50 trabajadores: asignaciones válidas (húngaro cubre todos los puestos).
100 trabajadores: asignaciones válidas (húngaro cubre todos los puestos).
200 trabajadores: asignaciones válidas (húngaro cubre todos los puestos).
500 trabajadores: asignaciones válidas (húngaro cubre todos los puestos).

FASE 3: MEDICIONES

TAMANO: 4 trabajadores y 4 puestos (pares elegibles: 8)
FuerzaBruta - Ejecucion 1: 0.0183 ms
FuerzaBruta - Ejecucion 2: 0.0031 ms
FuerzaBruta - Ejecucion 3: 0.0025 ms
PROMEDIO FuerzaBruta: 0.0080 ms
Voraz - Ejecucion 1: 0.0100 ms
Voraz - Ejecucion 2: 0.0048 ms
Voraz - Ejecucion 3: 0.0037 ms
PROMEDIO Voraz: 0.0062 ms
Hungaro - Ejecucion 1: 0.0041 ms
Hungaro - Ejecucion 2: 0.0036 ms
Hungaro - Ejecucion 3: 0.0031 ms
PROMEDIO Hungaro: 0.0036 ms

TAMANO: 5 trabajadores y 5 puestos (pares elegibles: 10)
FuerzaBruta - Ejecucion 1: 0.0061 ms
FuerzaBruta - Ejecucion 2: 0.0070 ms
FuerzaBruta - Ejecucion 3: 0.0136 ms
PROMEDIO FuerzaBruta: 0.0089 ms
Voraz - Ejecucion 1: 0.0044 ms
Voraz - Ejecucion 2: 0.0045 ms
Voraz - Ejecucion 3: 0.0045 ms
PROMEDIO Voraz: 0.0045 ms
Hungaro - Ejecucion 1: 0.0047 ms
Hungaro - Ejecucion 2: 0.0136 ms
Hungaro - Ejecucion 3: 0.0119 ms
PROMEDIO Hungaro: 0.0101 ms

TAMANO: 6 trabajadores y 6 puestos (pares elegibles: 14)
FuerzaBruta - Ejecucion 1: 0.0269 ms
FuerzaBruta - Ejecucion 2: 0.0313 ms
FuerzaBruta - Ejecucion 3: 0.0697 ms
PROMEDIO FuerzaBruta: 0.0426 ms
Voraz - Ejecucion 1: 0.0054 ms
Voraz - Ejecucion 2: 0.0830 ms
Voraz - Ejecucion 3: 0.0106 ms
PROMEDIO Voraz: 0.0330 ms
Hungaro - Ejecucion 1: 0.0090 ms
Hungaro - Ejecucion 2: 0.0185 ms
Hungaro - Ejecucion 3: 0.0150 ms
PROMEDIO Hungaro: 0.0142 ms

TAMANO: 7 trabajadores y 7 puestos (pares elegibles: 26)
FuerzaBruta - Ejecucion 1: 0.1749 ms
FuerzaBruta - Ejecucion 2: 0.1599 ms
FuerzaBruta - Ejecucion 3: 0.1684 ms
PROMEDIO FuerzaBruta: 0.1677 ms
Voraz - Ejecucion 1: 0.0073 ms
Voraz - Ejecucion 2: 0.0063 ms
Voraz - Ejecucion 3: 0.0078 ms
PROMEDIO Voraz: 0.0071 ms
Hungaro - Ejecucion 1: 0.0082 ms
Hungaro - Ejecucion 2: 0.0207 ms
Hungaro - Ejecucion 3: 0.0079 ms
PROMEDIO Hungaro: 0.0123 ms

TAMANO: 8 trabajadores y 8 puestos (pares elegibles: 27)
FuerzaBruta - Ejecucion 1: 1.3589 ms
FuerzaBruta - Ejecucion 2: 1.1514 ms
FuerzaBruta - Ejecucion 3: 1.5316 ms
PROMEDIO FuerzaBruta: 1.3473 ms
Voraz - Ejecucion 1: 0.0311 ms
Voraz - Ejecucion 2: 0.0050 ms
Voraz - Ejecucion 3: 0.0162 ms
PROMEDIO Voraz: 0.0174 ms
Hungaro - Ejecucion 1: 0.0096 ms
Hungaro - Ejecucion 2: 0.0097 ms
Hungaro - Ejecucion 3: 0.0141 ms
PROMEDIO Hungaro: 0.0111 ms

TAMANO: 9 trabajadores y 9 puestos (pares elegibles: 37)
FuerzaBruta - Ejecucion 1: 10.5160 ms
FuerzaBruta - Ejecucion 2: 9.8706 ms
FuerzaBruta - Ejecucion 3: 10.1174 ms
PROMEDIO FuerzaBruta: 10.1680 ms
Voraz - Ejecucion 1: 0.0259 ms
Voraz - Ejecucion 2: 0.0077 ms
Voraz - Ejecucion 3: 0.0076 ms
PROMEDIO Voraz: 0.0137 ms
Hungaro - Ejecucion 1: 0.0132 ms
Hungaro - Ejecucion 2: 0.0134 ms
Hungaro - Ejecucion 3: 0.0134 ms
PROMEDIO Hungaro: 0.0133 ms

TAMANO: 10 trabajadores y 10 puestos (pares elegibles: 38)
FuerzaBruta - Ejecucion 1: 107.9779 ms
FuerzaBruta - Ejecucion 2: 106.2643 ms
FuerzaBruta - Ejecucion 3: 105.0584 ms
PROMEDIO FuerzaBruta: 106.4335 ms
Voraz - Ejecucion 1: 0.0200 ms
Voraz - Ejecucion 2: 0.0256 ms
Voraz - Ejecucion 3: 0.0062 ms
PROMEDIO Voraz: 0.0173 ms
Hungaro - Ejecucion 1: 0.0271 ms
Hungaro - Ejecucion 2: 0.0319 ms
Hungaro - Ejecucion 3: 0.0275 ms
PROMEDIO Hungaro: 0.0288 ms

TAMANO: 20 trabajadores y 20 puestos (pares elegibles: 155)
Voraz - Ejecucion 1: 0.0408 ms
Voraz - Ejecucion 2: 0.0453 ms
Voraz - Ejecucion 3: 0.0295 ms
PROMEDIO Voraz: 0.0385 ms
Hungaro - Ejecucion 1: 0.0640 ms
Hungaro - Ejecucion 2: 0.0503 ms
Hungaro - Ejecucion 3: 0.0526 ms
PROMEDIO Hungaro: 0.0556 ms

TAMANO: 50 trabajadores y 50 puestos (pares elegibles: 901)
Voraz - Ejecucion 1: 0.1409 ms
Voraz - Ejecucion 2: 0.1385 ms
Voraz - Ejecucion 3: 0.1323 ms
PROMEDIO Voraz: 0.1372 ms
Hungaro - Ejecucion 1: 0.1811 ms
Hungaro - Ejecucion 2: 0.1747 ms
Hungaro - Ejecucion 3: 0.1737 ms
PROMEDIO Hungaro: 0.1765 ms

TAMANO: 100 trabajadores y 100 puestos (pares elegibles: 3,583)
Voraz - Ejecucion 1: 0.5077 ms
Voraz - Ejecucion 2: 0.5525 ms
Voraz - Ejecucion 3: 0.5684 ms
PROMEDIO Voraz: 0.5429 ms
Hungaro - Ejecucion 1: 1.2371 ms
Hungaro - Ejecucion 2: 1.1709 ms
Hungaro - Ejecucion 3: 1.1611 ms
PROMEDIO Hungaro: 1.1897 ms

TAMANO: 200 trabajadores y 200 puestos (pares elegibles: 14,981)
Voraz - Ejecucion 1: 2.4984 ms
Voraz - Ejecucion 2: 2.4714 ms
Voraz - Ejecucion 3: 2.2344 ms
PROMEDIO Voraz: 2.4014 ms
Hungaro - Ejecucion 1: 14.6425 ms
Hungaro - Ejecucion 2: 15.3693 ms
Hungaro - Ejecucion 3: 16.6185 ms
PROMEDIO Hungaro: 15.5434 ms

TAMANO: 500 trabajadores y 500 puestos (pares elegibles: 92,208)
Voraz - Ejecucion 1: 18.7729 ms
Voraz - Ejecucion 2: 15.9761 ms
Voraz - Ejecucion 3: 17.6678 ms
PROMEDIO Voraz: 17.4723 ms
Hungaro - Ejecucion 1: 230.3221 ms
Hungaro - Ejecucion 2: 231.3162 ms
Hungaro - Ejecucion 3: 223.5904 ms
PROMEDIO Hungaro: 228.4096 ms

--------------------------------------------------------------------------------------
 TABLA DE PROMEDIOS
--------------------------------------------------------------------------------------
ALGORITMO        n  PROMEDIO (ms)      OPERACIONES  COCIENTE      COSTO     CUBRE
--------------------------------------------------------------------------------------
FuerzaBruta      4         0.0080               65      1.00      82.75       4/4
Voraz            4         0.0062               18      0.75      50.25       3/4
Hungaro          4         0.0036               21      0.33      82.75       4/4
FuerzaBruta      5         0.0089              326      1.00     119.25       5/5
Voraz            5         0.0045               28      0.84      79.50       4/5
Hungaro          5         0.0101               33      0.26     119.25       5/5
FuerzaBruta      6         0.0426            1,957      1.00      93.75       6/6
Voraz            6         0.0330               43      0.81      79.75       5/6
Hungaro          6         0.0142               70      0.32      93.75       6/6
FuerzaBruta      7         0.1677           13,700      1.00      89.75       7/7
Voraz            7         0.0071              106      0.87      68.25       6/7
Hungaro          7         0.0123               75      0.22      89.75       7/7
FuerzaBruta      8         1.3473          109,601      1.00      93.50       8/8
Voraz            8         0.0174              137      1.07      93.50       8/8
Hungaro          8         0.0111               91      0.18      93.50       8/8
FuerzaBruta      9        10.1680          986,410      1.00     131.75       9/9
Voraz            9         0.0137              196      1.02     105.50       8/9
Hungaro          9         0.0133              148      0.20     131.75       9/9
FuerzaBruta     10       106.4335        9,864,101      1.00     139.50     10/10
Voraz           10         0.0173              218      1.09      36.25      7/10
Hungaro         10         0.0288              195      0.20     139.50     10/10
Voraz           20         0.0385            1,160      1.03     186.75     19/20
Hungaro         20         0.0556            1,388      0.17     160.75     20/20
Voraz           50         0.1372            9,114      1.03     236.25     47/50
Hungaro         50         0.1765           12,495      0.10     203.00     50/50
Voraz          100         0.5429           44,417      1.05     414.00    97/100
Hungaro        100         1.1897          132,327      0.13     305.50   100/100
Voraz          200         2.4014          233,647      1.12     723.50   199/200
Hungaro        200        15.5434        2,197,506      0.27     599.75   200/200
Voraz          500        17.4723        1,681,271      1.11    1639.50   499/500
Hungaro        500       228.4096       40,288,122      0.32    1496.00   500/500

COCIENTE = operaciones medidas / operaciones teóricas (e·n! | m·log2 m | n³).
Si el cociente se queda en una constante cuando n crece, la forma O(·) predicha es la correcta.
CUBRE = puestos cubiertos / puestos requeridos (el voraz puede dejar puestos sin cubrir).

Fuerza bruta medida en ESTA máquina: ~92.7 millones de llamadas por segundo.
Para n = 20 harían falta ~6.61E+18 llamadas  ->  ~2,261 años.

 PRUEBA DE ELECCION FINALIZADA

Archivos generados:
1. analisis-algoritmos-villarroel\practicas-enClases\c6-argumentadaCoste\argumentadaCoste\resultados.csv
2. analisis-algoritmos-villarroel\practicas-enClases\c6-argumentadaCoste\argumentadaCoste\resumen.csv
3. analisis-algoritmos-villarroel\practicas-enClases\c6-argumentadaCoste\argumentadaCoste\conteos.csv

Todas las asignaciones fueron verificadas.

PARTE D · CALIDAD DE LA ASIGNACIÓN: rápido no es lo mismo que óptimo

Caso 2×2 [[1,2],[2,100]]      : fuerza bruta = 4 | voraz = 101 | húngaro = 4
Caso 3×3 del proyecto         : fuerza bruta = 41 | voraz = 41 | húngaro = 41

200 instancias sintéticas de n = 20 (semillas 1..200), húngaro contra voraz:
  voraz NO cubrió todos los puestos : 193 de 200
  voraz cubrió todo pero NO óptimo  : 7 de 7 instancias completas
  sobrecosto medio del voraz        : 21.24 %   (máximo 29.30 %)
El húngaro cubre los 20 puestos y da el mínimo en las 200.

Archivo generado: analisis-algoritmos-villarroel\practicas-enClases\c6-argumentadaCoste\argumentadaCoste\calidad.csv