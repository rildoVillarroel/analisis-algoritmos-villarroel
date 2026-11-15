# INFORME EXPERIMENTAL: COMPARACIÓN DE ALGORITMOS DE ORDENAMIENTO

## Objetivo

Comparar experimentalmente el rendimiento de los algoritmos Bubble Sort y Merge Sort mediante diferentes cantidades de elementos, identificando sus diferencias de eficiencia y comportamiento temporal.

## Metodología

Se implementaron ambos algoritmos en C#. Se utilizaron arreglos de números enteros generados aleatoriamente, con tamaños de 10 000, 20 000, 40 000 y 80 000 elementos.

Antes de realizar las mediciones se efectuaron tres ejecuciones de calentamiento por algoritmo, cuyos tiempos fueron descartados. Posteriormente, cada algoritmo fue ejecutado tres veces para cada tamaño de entrada y se calculó el promedio de los tiempos obtenidos.

También se verificó que los arreglos estuvieran correctamente ordenados, realizando esta comprobación fuera del intervalo de medición.

El código cumple los requisitos siguientes:

- Un algoritmo cuadrático	Burbuja
- Un algoritmo \(n\log n\) Merge Sort
- Cuatro tamaños	10 000 a 80 000
- Tres calentamientos	3 por algoritmo
- Tres mediciones	3 por algoritmo y tamaño
- Promedio	Suma de tiempos / 3
- Verificación	EstaOrdenado()
- Resultados	Archivo CSV

Una decisión metodológica importante: ambos algoritmos reciben exactamente los mismos datos originales. Cada ejecución utiliza una copia independiente, por lo que ningún algoritmo recibe datos previamente ordenados por el otro.

Además, el cronómetro no incluye:
- Generación de números aleatorios.
- Copia del arreglo.
- Verificación del orden.
- Impresión en consola.
- Exportación del CSV.

## Resultados

Los resultados se presentan mediante una tabla comparativa de los tiempos promedio obtenidos para ambos algoritmos. La salida en la terminal git es:

$ dotnet run -c Release
--------------------------------------
 TORNEO DE ALGORITMOS DE ORDENAMIENTO
--------------------------------------

FASE 1: CALENTAMIENTO
Calentamiento 1 completado.
Calentamiento 2 completado.
Calentamiento 3 completado.

FASE 2: VERIFICACION
10000 elementos: ambos algoritmos correctos.
20000 elementos: ambos algoritmos correctos.
40000 elementos: ambos algoritmos correctos.
80000 elementos: ambos algoritmos correctos.

FASE 3: MEDICIONES

TAMANO: 10.000 elementos
BurbujaSort - Ejecucion 1: 121,8766 ms
BurbujaSort - Ejecucion 2: 118,0293 ms
BurbujaSort - Ejecucion 3: 123,0197 ms
PROMEDIO BurbujaSort: 120,9752 ms
MergeSort - Ejecucion 1: 0,7795 ms
MergeSort - Ejecucion 2: 0,7294 ms
MergeSort - Ejecucion 3: 0,7854 ms
PROMEDIO MergeSort: 0,7648 ms

TAMANO: 20.000 elementos
BurbujaSort - Ejecucion 1: 536,8133 ms
BurbujaSort - Ejecucion 2: 549,3358 ms
BurbujaSort - Ejecucion 3: 540,9319 ms
PROMEDIO BurbujaSort: 542,3603 ms
MergeSort - Ejecucion 1: 1,5783 ms
MergeSort - Ejecucion 2: 1,5733 ms
MergeSort - Ejecucion 3: 1,7205 ms
PROMEDIO MergeSort: 1,6240 ms

TAMANO: 40.000 elementos
BurbujaSort - Ejecucion 1: 2263,1232 ms
BurbujaSort - Ejecucion 2: 2265,2984 ms
BurbujaSort - Ejecucion 3: 2297,5002 ms
PROMEDIO BurbujaSort: 2275,3073 ms
MergeSort - Ejecucion 1: 3,5970 ms
MergeSort - Ejecucion 2: 3,6511 ms
MergeSort - Ejecucion 3: 4,6057 ms
PROMEDIO MergeSort: 3,9513 ms

TAMANO: 80.000 elementos
BurbujaSort - Ejecucion 1: 9425,1224 ms
BurbujaSort - Ejecucion 2: 9274,8171 ms
BurbujaSort - Ejecucion 3: 9263,9482 ms
PROMEDIO BurbujaSort: 9321,2959 ms
MergeSort - Ejecucion 1: 7,3195 ms
MergeSort - Ejecucion 2: 7,7495 ms
MergeSort - Ejecucion 3: 7,3987 ms
PROMEDIO MergeSort: 7,4892 ms

----------------------------------------
 TABLA DE PROMEDIOS
----------------------------------------
ALGORITMO       ELEMENTOS       PROMEDIO (ms)  
-----------------------------------------------
BurbujaSort     10000           120,9752       
MergeSort       10000           0,7648         
BurbujaSort     20000           542,3603       
MergeSort       20000           1,6240         
BurbujaSort     40000           2275,3073      
MergeSort       40000           3,9513         
BurbujaSort     80000           9321,2959      
MergeSort       80000           7,4892         

======================================
 EXPERIMENTO FINALIZADO
======================================

Archivos generados:
1. analisis-algoritmos-villarroel\practicas-enClases\c5-torneoAlgoritmos\torneoAlgoritmos\resultados.csv
2. analisis-algoritmos-villarroel\practicas-enClases\c5-torneoAlgoritmos\torneoAlgoritmos\resumen.csv

## Conclusiones

Se espera observar que Bubble Sort presenta un crecimiento considerable de su tiempo de ejecución al incrementar el tamaño de entrada, debido a su complejidad temporal cuadrática \(O(n^2)\).

Por otra parte, Merge Sort debería presentar un crecimiento más moderado, debido a su complejidad \(O(n\log n)\), lo que le permite manejar cantidades mayores de elementos de manera más eficiente.

El experimento permitirá comprobar que la complejidad algorítmica influye directamente en la escalabilidad de los programas y que, para grandes cantidades de datos, elegir un algoritmo eficiente resulta fundamental.