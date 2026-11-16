# Explicacion de la comparacion Lineal binario

Genera un arreglo ordenado y sin repetidos, con saltos aleatorios de 1 a 5 entre un elemento y el siguiente, y 1.000 claves tomadas del propio arreglo, así que todas existen.
Usa el tamaño pedido de 1.000.000. Agregué 100.000 y 500.000 solo para que se vea la tendencia; si el docente quiere únicamente 1.000.000, deja { 1000000 } en tamanos.
Hace 3 calentamientos por algoritmo y luego 3 mediciones por algoritmo y tamaño, con el promedio = suma / 3.
Verifica fuera del tiempo medido que el arreglo esté ordenado con EstaOrdenado(), que cada clave se encuentre en su posición exacta y que la suma de posiciones de cada corrida coincida con la esperada.

## Archivos CSV que genera

resultados.csv: cada corrida, con Algoritmo,Tamano,Repeticion,Tiempo_ms.
resumen.csv: los promedios.
conteos.csv: las comparaciones exactas por algoritmo y tamaño. No dependen de tu máquina, así que sirven de evidencia sólida.

## Conclusión

Lineal: en promedio recorre la mitad del arreglo por clave. Con n = 1.000.000 son unas 500.000 comparaciones por clave, o sea del orden de 5 × 10⁸ en total.
Binaria: hace como máximo unas 20 comparaciones por clave (log₂ 1.000.000 ≈ 20), o sea unas 20.000 en total.
Tiempo: el programa imprime cuántas veces más rápida resultó la binaria. En tu máquina será una diferencia de miles de veces; el valor exacto sale de tu corrida.
Tiempo de la binaria: será muy pequeño, del orden de centésimas de milisegundo. Por eso el programa también guarda las comparaciones, que son exactas.

# RESULTADO DE EJECUCION EN CONSOLA

$ dotnet run -c Release
----------------------------------------------
 BUSQUEDA LINEAL CONTRA BUSQUEDA BINARIA
----------------------------------------------

FASE 1: CALENTAMIENTO
BusquedaLineal - Calentamiento 1 completado.
BusquedaLineal - Calentamiento 2 completado.
BusquedaLineal - Calentamiento 3 completado.
BusquedaBinaria - Calentamiento 1 completado.
BusquedaBinaria - Calentamiento 2 completado.
BusquedaBinaria - Calentamiento 3 completado.

FASE 2: VERIFICACION
100,000 elementos: arreglo ordenado y las 1,000 claves se encuentran con ambos algoritmos.
500,000 elementos: arreglo ordenado y las 1,000 claves se encuentran con ambos algoritmos.
1,000,000 elementos: arreglo ordenado y las 1,000 claves se encuentran con ambos algoritmos.

FASE 3: MEDICIONES

TAMANO: 100,000 elementos, 1,000 claves
BusquedaLineal - Ejecucion 1: 15.5333 ms
BusquedaLineal - Ejecucion 2: 34.9838 ms
BusquedaLineal - Ejecucion 3: 18.9800 ms
PROMEDIO BusquedaLineal: 23.1657 ms
BusquedaBinaria - Ejecucion 1: 0.1947 ms
BusquedaBinaria - Ejecucion 2: 0.1107 ms
BusquedaBinaria - Ejecucion 3: 0.1522 ms
PROMEDIO BusquedaBinaria: 0.1525 ms

TAMANO: 500,000 elementos, 1,000 claves
BusquedaLineal - Ejecucion 1: 73.9688 ms
BusquedaLineal - Ejecucion 2: 73.1375 ms
BusquedaLineal - Ejecucion 3: 72.7175 ms
PROMEDIO BusquedaLineal: 73.2746 ms
BusquedaBinaria - Ejecucion 1: 0.2207 ms
BusquedaBinaria - Ejecucion 2: 0.1440 ms
BusquedaBinaria - Ejecucion 3: 0.1490 ms
PROMEDIO BusquedaBinaria: 0.1712 ms

TAMANO: 1,000,000 elementos, 1,000 claves
BusquedaLineal - Ejecucion 1: 148.1156 ms
BusquedaLineal - Ejecucion 2: 141.7422 ms
BusquedaLineal - Ejecucion 3: 161.5971 ms
PROMEDIO BusquedaLineal: 150.4850 ms
BusquedaBinaria - Ejecucion 1: 0.2311 ms
BusquedaBinaria - Ejecucion 2: 0.1763 ms
BusquedaBinaria - Ejecucion 3: 0.2305 ms
PROMEDIO BusquedaBinaria: 0.2126 ms

---------------------------------------------------------------
 TABLA DE PROMEDIOS
---------------------------------------------------------------
ALGORITMO        ELEMENTOS    PROMEDIO (ms)   COMPARACIONES     
---------------------------------------------------------------
BusquedaLineal   100,000      23.1657         50,915,615        
BusquedaBinaria  100,000      0.1525          15,691            
BusquedaLineal   500,000      73.2746         251,382,390       
BusquedaBinaria  500,000      0.1712          17,953            
BusquedaLineal   1,000,000    150.4850        489,230,498       
BusquedaBinaria  1,000,000    0.2126          18,886            

CUANTAS VECES MAS RAPIDA ES LA BINARIA (tiempo lineal / tiempo binaria):
  n =   100,000: 152 veces
  n =   500,000: 428 veces
  n = 1,000,000: 708 veces

======================================
 COMPARACION FINALIZADA
======================================

Archivos generados:
1. analisis-algoritmos-villarroel\practicas-enClases\c4-linealvsBinaria\resultados.csv
2. analisis-algoritmos-villarroel\practicas-enClases\c4 -linealvsBinaria\resumen.csv
3. analisis-algoritmos-villarroel\practicas-enClases\c4 -linealvsBinaria\conteos.csv

Todos los resultados fueron verificados.