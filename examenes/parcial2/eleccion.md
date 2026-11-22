Pablo Rildo Villarroel Fernandez

# Elección de estructuras: turnos de una clínica

## 1. Necesidades del caso

La clínica recibe pacientes con urgencia de 1 (crítico) a 5 (leve). Se atiende primero la urgencia menor. Si dos pacientes tienen la misma urgencia, se atiende primero al que llegó antes.

Carga diaria indicada: 100.000 registros, 100.000 atenciones y 30.000 consultas `EstaEsperando(nombre)`. Los nombres son únicos y no se solicita cancelar pacientes ni consultar rangos.

## 2. Estructuras elegidas

Se combinan dos estructuras porque las operaciones tienen necesidades distintas:

1. `PriorityQueue<Paciente, (int Urgencia, int Llegada)>`: cola de prioridad implementada mediante un heap mínimo. La prioridad compuesta ordena primero por urgencia y después por número de llegada.
2. `HashSet<string>`: mantiene los nombres de los pacientes que continúan esperando y permite consultas exactas por nombre.

La cola de prioridad de .NET extrae la prioridad mínima. La tupla se compara lexicográficamente: primero `Urgencia` y, si empatan, `Llegada`. Así se garantiza el desempate solicitado.

## 3. Operaciones, frecuencias y costos

| Operación | Veces por día | Estructura | Costo |
|---|---:|---|---:|
| `Registrar(nombre, urgencia)` | 100.000 | `PriorityQueue` + `HashSet` | O(log n) por el heap + O(1) promedio por hash; domina O(log n) |
| `AtenderSiguiente()` | 100.000 | `PriorityQueue` + `HashSet` | O(log n) por extraer del heap + O(1) promedio por borrar del hash; domina O(log n) |
| `EstaEsperando(nombre)` | 30.000 | `HashSet` | O(1) promedio |
| `Esperando` | Según consulta | `PriorityQueue.Count` | O(1) |

`n` representa los pacientes que están esperando. El costo del hash es promedio; en el peor caso teórico puede degradarse por colisiones.

## 4. Por qué no sirve la lista simple

En `GestorIngenuo`, registrar cuesta O(1) amortizado. Sin embargo, atender requiere recorrer la lista para encontrar al paciente más urgente (O(n)) y `RemoveAt(indice)` puede desplazar elementos (O(n)). Consultar un nombre también recorre la lista (O(n)); consultar `Count` cuesta O(1).

Con 100.000 pacientes, solo la búsqueda del siguiente paciente durante las 100.000 atenciones puede realizar aproximadamente:

\[
100000 + 99999 + \dots + 1 = \frac{100000(100001)}{2} = 5.000.050.000
\]

comparaciones de pacientes. A eso se agregan desplazamientos de `RemoveAt` y las 30.000 búsquedas por nombre. En el peor caso, estas consultas podrían examinar hasta 3.000.000.000 de elementos adicionales.

Con la solución mejorada, las 100.000 inserciones y 100.000 extracciones cuestan O(log n) cada una; las 30.000 consultas por nombre cuestan O(1) promedio. Como referencia, log2(100000) es aproximadamente 16,6 niveles del heap: se trabaja con la altura, no con todos los pacientes.

Esta es una comparación de orden de crecimiento, no una predicción exacta de segundos: constantes, memoria, procesador y caché afectan los tiempos reales.

## 5. ¿Cuándo cambiaría de elección?

Si el sistema necesitara consultar frecuentemente pacientes dentro de un intervalo de urgencias o recorrerlos en orden completo, un árbol balanceado podría convenir. Si se necesitara atender estrictamente por llegada, bastaría una cola FIFO. Para el caso actual, heap más hash responde directamente a las operaciones solicitadas.

## 6. Relación con los otros algoritmos

- tabla hash para buscar un paciente por nombre.
- árboles balanceados para mantener datos ordenados y resolver rangos; aquí no son la estructura principal porque la operación dominante es extraer al más urgente.
- heap mínimo y cola de prioridad. Insertar y extraer cuestan O(log n), y consultar el elemento prioritario (si se necesitara `Peek`) cuesta O(1). El heap no ordena toda la colección: conserva la propiedad que permite tener el mínimo en la raíz.
