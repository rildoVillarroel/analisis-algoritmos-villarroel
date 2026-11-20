# Inventario de operaciones: asignación óptima de personal a proyectos eléctricos

> Los 20 trabajadores, 20 puestos y 5 proyectos son los datos del modelo académico que venimos desarrollando. Las cifras de crecimiento de la tabla son proyecciones para dimensionamiento, no datos reales ni históricos.

## 1. ¿Qué datos guarda el proceso y cuántos?

El proceso administra trabajadores, puestos que deben cubrirse y asignaciones. Para decidir la compatibilidad también consulta habilidades, experiencia, certificaciones, disponibilidad y carga horaria.

| Entidad/dato | Hoy (modelo) | Peor mes proyectado | Base |
|---|---:|---:|---|
| Trabajadores | 20 | 30 | Modelo actual; crecimiento supuesto del 50% |
| Puestos por cubrir | 20 | 40 | Modelo actual; se duplica la demanda |
| Proyectos simultáneos | 5 | 8 | Proyección |
| Asignaciones activas | hasta 20 | hasta 30 | Una asignación principal por trabajador en esta versión |
| Relaciones trabajador-habilidad | aprox. 60 | aprox. 120 | Estimación: varias habilidades por persona |
| Certificaciones | aprox. 40 | aprox. 90 | Estimación de certificados vigentes/históricos |
| Consultas de disponibilidad | variable | 500/día | Supuesto de carga |
| Historial de asignaciones | 20 iniciales | 500 registros | Supuesto acumulado |

Las cifras de habilidades, certificaciones, consultas e historial son estimaciones explícitas que deberán sustituirse por datos reales cuando el sistema entre en operación.

## 2. ¿Qué operaciones realiza?

| Operación | Ejemplo en el proyecto | Frecuencia |
|---|---|---|
| Buscar por clave | Buscar trabajador `T18` o puesto `P03` | Muchas |
| Insertar | Registrar trabajador, puesto o certificación | Pocas |
| Borrar/desactivar | Dar de baja un trabajador o retirar un puesto | Pocas |
| Recorrer en orden | Listar trabajadores por código | Media |
| Menor/mayor | Primer/último código o menor/mayor carga | Pocas |
| Consultar rango | Códigos `T05` a `T12`; certificaciones entre fechas | Media |
| Comprobar pertenencia | Saber si `T18` ya está asignado | Muchas |
| Evaluar compatibilidad | Comparar habilidades, certificados y horas con un puesto | Muchas durante la optimización |

La asignación óptima no es simplemente una búsqueda. Requiere evaluar compatibilidades y restricciones entre trabajadores y puestos. Las estructuras aceleran consultas auxiliares, pero no reemplazan el algoritmo de optimización (por ejemplo, el método Húngaro).

## 3. ¿Qué estructura resuelve cada operación barato?

| Operación | Estructura recomendada | Costo esperado | Estructura que sufre |
|---|---|---|---|
| Buscar clave exacta | `Dictionary` | O(1) promedio; O(n) peor caso | Lista/arreglo sin índice: O(n) |
| Insertar por clave | `Dictionary` | O(1) promedio amortizado | Arreglo fijo: puede requerir desplazamientos |
| Comprobar si está asignado | `HashSet` | O(1) promedio | Lista: O(n) |
| Borrar por clave | `Dictionary` | O(1) promedio | Lista/arreglo: localizar cuesta O(n) |
| Recorrer ordenado | `SortedDictionary` | O(n) recorrido | `Dictionary`/`HashSet` no garantizan orden por clave |
| Menor/mayor clave | `SortedDictionary` | Se obtiene desde el orden; recorrido ordenado O(n) | Hash: O(n) para hallar mínimo/máximo |
| Consultar rango | Árbol ordenado / `SortedDictionary` | Conceptualmente O(log n + k), k resultados | Hash/lista: O(n) |
| Evaluar todos los candidatos | Arreglo o `List` | O(n) recorrido | Hash no aporta ventaja si se recorren todos |

## 4. Aplicación de tablas hash

Una función hash transforma una clave en un casillero. La búsqueda y la inserción usan esa misma transformación, por eso el costo promedio es O(1). Dos claves pueden producir el mismo casillero: eso es una colisión y la estructura debe resolverla.

El factor de carga es:

`α = cantidad de elementos / cantidad de casilleros`

Si la tabla crece y no aumenta sus casilleros, las colisiones pueden acumularse y el rendimiento degradarse hacia O(n). El rehashing redimensiona la tabla y redistribuye las claves; tiene un costo puntual O(n), pero mantiene el costo promedio de las operaciones cercano a O(1).

La desventaja fundamental es que hash no mantiene orden: no es adecuado para obtener directamente el menor, el mayor o un rango.

## 5. Decisión para el proyecto

Se recomienda combinar estructuras:

1. `Dictionary<string, Trabajador>` para localizar trabajadores por código.
2. `Dictionary<string, Puesto>` para localizar puestos por código.
3. `HashSet<string>` para comprobar rápidamente si un trabajador ya tiene asignación.
4. `SortedDictionary<string, Trabajador>` para listados ordenados y consultas por rangos.
5. `List<Asignacion>` para recorrer las asignaciones y alimentar procesos que necesiten evaluar el conjunto.

Con solo 20 trabajadores y 20 puestos, una lista podría ser suficiente y las diferencias de tiempo serían pequeñas. La elección de hash se justifica por el acceso por clave y por el crecimiento previsto, no por una optimización prematura.

## 6. Código de demostración y ejecucion consola

dotnet new console -n InventarioAsignacion
```

Copia este `Program.cs` sobre el archivo generado y ejecuta:

```bash
dotnet run
```

## 7. Conclusión

En el proyecto se realizan muchas búsquedas exactas y comprobaciones de disponibilidad; por ello `Dictionary` y `HashSet` son adecuados, con O(1) promedio. Para recorrer en orden y consultar rangos se requiere una estructura ordenada, porque una tabla hash pierde el orden. La mejor solución es combinar estructuras según las operaciones y no intentar resolver todo con una sola.

## Resultados de Ejecucion en Consola

Registrada

Intentar borrar T18 asignado:
No se puede borrar: tiene asignación activa

Recorrido ordenado:
T01 | Trabajador 01 | Media tensión
T02 | Trabajador 02 | Instalaciones hospitalarias
T03 | Trabajador 03 | Automatización
T04 | Trabajador 04 | Mantenimiento eléctrico
T05 | Trabajador 05 | Líneas eléctricas
T06 | Trabajador 06 | Media tensión
T07 | Trabajador 07 | Instalaciones hospitalarias
T08 | Trabajador 08 | Automatización
T09 | Trabajador 09 | Mantenimiento eléctrico
T10 | Trabajador 10 | Líneas eléctricas
T11 | Trabajador 11 | Media tensión
T12 | Trabajador 12 | Instalaciones hospitalarias
T13 | Trabajador 13 | Automatización
T14 | Trabajador 14 | Mantenimiento eléctrico
T15 | Trabajador 15 | Líneas eléctricas
T16 | Trabajador 16 | Media tensión
T17 | Trabajador 17 | Instalaciones hospitalarias
T18 | Trabajador 18 | Automatización
T19 | Trabajador 19 | Mantenimiento eléctrico
T20 | Trabajador 20 | Líneas eléctricas

Menor y mayor:
Menor código: T01
Mayor código: T20

Rango T05 a T10:
T05 | Trabajador 05
T06 | Trabajador 06
T07 | Trabajador 07
T08 | Trabajador 08
T09 | Trabajador 09
T10 | Trabajador 10

Liberar T18 y borrarlo:
Trabajador eliminado

Resumen final:
Trabajadores: 19; Puestos: 5; Asignaciones: 0