# Pablo Rildo Villarroel Fernandez

# BST, árboles balanceados y consultas por rango
**Proyecto:** Asignación óptima de personal técnico a proyectos eléctricos  
**Tema:** 

> Los 20 trabajadores, 20 puestos y cinco proyectos corresponden al modelo académico sintético trabajado. No representan personal real.

## 1. Diferencia con el inventario de la clase 8

El inventario de operaciones identifica qué datos administra el proceso, qué operaciones realiza, con qué frecuencia y qué estructuras pueden resolverlas eficientemente. Es un análisis del problema, no una implementación.

En la clase 8 se implementó una propuesta con `Dictionary`, `HashSet`, `List` y estructuras ordenadas. En esta clase se retoma el mismo proceso para estudiar qué operaciones se benefician de un árbol de búsqueda balanceado. No se repite el inventario: se amplía y compara la decisión de diseño.

## 2. Aplicación en el proyecto

El sistema necesita buscar trabajadores por código, ordenar candidatos por experiencia, identificar la menor o mayor experiencia y obtener personas dentro de un intervalo de años. La búsqueda exacta por código sigue siendo conveniente con hash; los recorridos ordenados y rangos justifican un árbol balanceado.

## 3. Estructuras elegidas

| Necesidad | Estructura | Costo esperado |
|---|---|---|
| Buscar trabajador por código | `Dictionary<string, Trabajador>` | O(1) promedio |
| Insertar/eliminar índice por código | `Dictionary` | O(1) promedio amortizado para insertar; O(1) promedio para eliminar |
| Mantener orden por experiencia | `SortedSet<Trabajador>` | O(log n) insertar/eliminar |
| Obtener extremos | `SortedSet.Min` / `SortedSet.Max` | Acceso al extremo del árbol |
| Recorrer por experiencia | `SortedSet` (inorden) | O(n) |
| Consultar intervalo de experiencia | `SortedSet.GetViewBetween` | O(log n + k), k resultados |

En .NET, `SortedSet<T>` utiliza un árbol rojinegro auto-balanceado. Así se evita que las claves ordenadas de llegada formen un árbol degenerado.

## 4. Clave de ordenamiento

El árbol se ordena primero por años de experiencia y después por código. El código desempata y permite conservar trabajadores que tienen los mismos años de experiencia.

Un árbol ordenado por experiencia permite rangos de experiencia, pero no rangos por especialidad o fecha. Para consultar eficientemente otro atributo podría ser necesario otro índice.

## 5. Conceptos de la guía

- **Regla BST:** las claves menores quedan a la izquierda y las mayores a la derecha.
- **Buscar, insertar y borrar:** cuestan O(altura). En un árbol balanceado la altura es O(log n).
- **Inorden:** izquierda → nodo → derecha; devuelve las claves ordenadas en O(n).
- **Degeneración:** si un BST simple recibe claves ya ordenadas, puede alcanzar altura n; las operaciones pasan a O(n) y construirlo puede costar O(n²).
- **Balanceo:** AVL y rojinegros ajustan la estructura para conservar altura logarítmica. En C# se puede usar `SortedSet<T>` o `SortedDictionary<TKey,TValue>`.
- **Rangos:** un árbol balanceado localiza el comienzo del rango y recorre k resultados: O(log n + k). Una tabla hash debe revisar los elementos porque no conserva el orden.

## 6. Frecuencia de operaciones

| Operación | Frecuencia estimada | Motivo |
|---|---|---|
| Buscar por código | Muchas | Consultas de perfiles y validaciones |
| Comprobar disponibilidad/compatibilidad | Muchas | Se realiza durante la asignación |
| Consultar rango de experiencia | Media | Filtrar candidatos |
| Recorrer personal ordenado | Media | Reportes y revisión administrativa |
| Insertar o desactivar trabajador | Pocas | Altas y bajas |
| Obtener menor/mayor experiencia | Pocas | Reportes y selección de extremos |

Son frecuencias estimadas para el diseño, no estadísticas medidas de un sistema desplegado.

## 7. Código de demostración

`Program.cs` utiliza 20 trabajadores y cinco puestos sintéticos. Demuestra búsqueda exacta por código con `Dictionary`, índice por experiencia con `SortedSet`, recorrido inorden, extremos, rango de 5 a 8 años y eliminación sincronizada de ambos índices.

Para ejecutarlo con .NET 8, crear una aplicación de consola o usar una carpeta de consola existente, reemplazar `Program.cs` y ejecutar:

```
bash
dotnet run
```

## 8. Conclusión

La clase 9 complementa la clase 8. Hash es adecuado para búsquedas exactas por código, pero no conserva el orden ni resuelve eficientemente rangos. Para ordenar candidatos por experiencia, obtener extremos y consultar intervalos, conviene un árbol balanceado. La propuesta combina ambos índices en lugar de reemplazar una estructura por otra.

## 9. Resultados de Ejecucion en Consola

$ dotnet run
=== ÁRBOLES BALANCEADOS APLICADOS EN EL PROYECTO ===
Trabajadores: 20; puestos: 5

1) Búsqueda exacta por hash:
T18: Trabajador 18, 3 años

2) Recorrido inorden:
Trabajadores ordenados por experiencia:
 2 años | T11 | Trabajador 11 | Media tensión
 3 años | T07 | Trabajador 07 | Instalaciones hospitalarias
 3 años | T18 | Trabajador 18 | Automatización
 4 años | T05 | Trabajador 05 | Líneas eléctricas
 4 años | T13 | Trabajador 13 | Automatización
 4 años | T20 | Trabajador 20 | Líneas eléctricas
 5 años | T03 | Trabajador 03 | Automatización
 5 años | T10 | Trabajador 10 | Líneas eléctricas
 5 años | T17 | Trabajador 17 | Instalaciones hospitalarias
 6 años | T02 | Trabajador 02 | Instalaciones hospitalarias
 6 años | T08 | Trabajador 08 | Automatización
 6 años | T15 | Trabajador 15 | Líneas eléctricas
 7 años | T06 | Trabajador 06 | Media tensión
 7 años | T14 | Trabajador 14 | Mantenimiento eléctrico
 8 años | T01 | Trabajador 01 | Media tensión
 8 años | T12 | Trabajador 12 | Instalaciones hospitalarias
 8 años | T19 | Trabajador 19 | Mantenimiento eléctrico
 9 años | T04 | Trabajador 04 | Mantenimiento eléctrico
 9 años | T16 | Trabajador 16 | Media tensión
10 años | T09 | Trabajador 09 | Mantenimiento eléctrico

3) Menor y mayor:
Menor experiencia: 2 años (T11)
Mayor experiencia: 10 años (T09)

4) Rango de experiencia 5 a 8:
Trabajadores entre 5 y 8 años:
5 años | T03 | Trabajador 03
5 años | T10 | Trabajador 10
5 años | T17 | Trabajador 17
6 años | T02 | Trabajador 02
6 años | T08 | Trabajador 08
6 años | T15 | Trabajador 15
7 años | T06 | Trabajador 06
7 años | T14 | Trabajador 14
8 años | T01 | Trabajador 01
8 años | T12 | Trabajador 12
8 años | T19 | Trabajador 19
Cantidad encontrada: 11

5) Eliminar T18 de ambos índices:
Eliminado

6) Comprobar búsqueda posterior:
T18 ya no existe

Resumen final:
Trabajadores: 19; puestos: 5