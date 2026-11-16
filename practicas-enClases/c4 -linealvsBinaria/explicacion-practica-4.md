# Práctica C4: búsqueda lineal contra búsqueda binaria

## Relación con el proyecto

El caso de estudio considera 20 trabajadores técnicos identificados como T01 a T20 y 20 puestos distribuidos en cinco proyectos eléctricos. Esta práctica compara la búsqueda de un trabajador por su identificador único.

Los registros y perfiles son los datos sintéticos definidos para el caso de estudio académico; no representan trabajadores reales.

## Algoritmos

- **Búsqueda lineal:** revisa los registros desde el primero hasta encontrar el identificador o terminar el arreglo. Complejidad: O(n).
- **Búsqueda binaria:** compara con el elemento central y descarta la mitad del intervalo en cada paso. Complejidad: O(log n). Requiere que los registros estén ordenados por la misma clave utilizada para buscar.

## Protocolo de medición

1. Se preparan 20 registros ordenados por identificador.
2. Se generan 1.000.000 de consultas reproducibles y se usan las mismas claves para ambos algoritmos.
3. Se ejecuta una corrida de calentamiento que no se incluye en el promedio.
4. Se realizan tres corridas medidas por algoritmo.
5. Se calcula el promedio de cada algoritmo y el cociente `promedio lineal / promedio binaria`.

Los tiempos deben obtenerse ejecutando el programa en el equipo del estudiante. No se deben copiar tiempos de otra computadora.

## Ejecución

Desde la terminal de VS Code, dentro de esta carpeta:

```bash
dotnet run -c Release
```

## Tabla de resultados

Copiar los valores reales que aparezcan en la consola:

| Algoritmo | Tiempo promedio (ms) |
|---|---:|
| Búsqueda lineal | completar |
| Búsqueda binaria | completar |
| Cociente lineal / binaria | completar |

## Análisis: ¿conviene ordenar primero en nuestro proceso?

En nuestro caso inicial hay solamente 20 trabajadores y 20 puestos; por ello, ordenar los registros únicamente para realizar unas pocas búsquedas no compensa necesariamente el costo adicional, y una búsqueda lineal puede ser suficiente por su simplicidad.

Si el sistema crece y debe realizar muchas consultas, conviene mantener una estructura ordenada por identificador o un índice actualizado, ordenar una sola vez al cargar o modificar los datos y reutilizarla. Para localizar candidatos por especialidad se necesitarían índices adecuados por especialidad; ordenar solamente por identificador no resuelve directamente esa consulta.

## Alcance de la práctica

Esta comparación estudia la localización de trabajadores por identificador. No constituye todavía el algoritmo de asignación óptima de trabajadores a puestos; esa parte se desarrollará posteriormente con los algoritmos de optimización del proyecto.

----------------------------------------------------------------------

# Algoritmo de búsqueda binaria a mano y en c#

datos = [3, 8, 12, 19, 25, 31, 40, 47]
- busca el 31 con la binaria. anota cada medio que miras ¿cuantos pasos?
- busca el 5 (no esta) : ¿cuantos pasos da hasta darse cuenta?
- ¿cuantos pasos como maximo para 1.000 datos? ¿y para 1.000.000?

# SOLUCION

La búsqueda binaria es un algoritmo que permite encontrar un elemento dentro de un conjunto de datos ordenados. Su funcionamiento consiste en dividir repetidamente el conjunto en dos mitades, descartando la mitad donde no puede encontrarse el elemento

Importante: Los datos deben estar ordenados de menor a mayor para que funcione correctamente.

## Desarrollo manual del algoritmo

a) Buscar el número 31
Datos iniciales:

[3, 8, 12, 19, 25, 31, 40, 47]

Tenemos 8 elementos.
Paso	Inicio	Fin	Medio (posición)	Valor medio	Decisión
1	       0     7	    3	                19	     31 es mayor, buscar derecha
2	       4     7	    5	                31	     Encontrado

Explicación:
1. Primero calculamos la mitad: (0 + 7) / 2 = 3. El valor es 19.
2. Como 31 es mayor que 19, descartamos la mitad izquierda.
3. Buscamos entre las posiciones 4 y 7.
4. Calculamos nuevamente la mitad: (4 + 7) / 2 = 5. El valor es 31.
5. ¡Encontrado!
Resultado
 2 pasos para encontrar el número 31

b) Buscar el número 5
Datos iniciales:

[3, 8, 12, 19, 25, 31, 40, 47]
Paso	Inicio	Fin	Medio (posición)	Valor medio	Decisión
1	       0	 7	    3	                19	     5 es menor, buscar izquierda
2	       0	 2	    1	                 8	     5 es menor, buscar izquierda
3	       0	 0	    0	                 3	     5 es mayor, buscar derecha

Explicación:
1. Comparamos 5 con 19. Como es menor, buscamos a la izquierda.
2. Comparamos 5 con 8. Como es menor, seguimos a la izquierda.
3. Comparamos 5 con 3. Como es mayor, buscamos a la derecha.
4. Ya no quedan posiciones por revisar (inicio = 1 y fin = 0). Por tanto, el número no existe en el arreglo.
Resultado
 3 pasos para determinar que el número 5 no existe

c) ¿Cuántos pasos como máximo se necesitan para 1.000 y 1.000.000 de datos?
Para calcular el máximo de pasos de la búsqueda binaria utilizamos:

    Pmax = |log_2(n)| + 1

Donde:
- Pmax : máximo número de pasos.
- n : cantidad de elementos.
- log_2 : logaritmo en base 2.

Para 1.000 datos:
   P_max = |log_2(1.000)| + 1
   P_max = 9 + 1 = 10 pasos

Para 1.000.000 de datos:
   P_max = |log_2(1.000.000)| + 1
   P_max = 19 + 1 = 20 pasos

## Análisis de complejidad algorítmica

La búsqueda binaria tiene una complejidad temporal de:
   O (log_2 n)

Esto significa que, aunque aumente considerablemente la cantidad de datos, el número de comparaciones crece muy lentamente.

Por ejemplo:
- Con 1.000 elementos necesitamos como máximo 10 pasos.
- Con 1.000.000 elementos necesitamos como máximo 20 pasos.
- Con 1.000.000.000 elementos necesitamos como máximo 30 pasos.

Conclusión: La búsqueda binaria es mucho más eficiente que la búsqueda lineal para conjuntos de datos grandes y ordenados. La búsqueda lineal puede necesitar revisar todos los elementos, mientras que la binaria reduce el espacio de búsqueda a la mitad en cada iteración.