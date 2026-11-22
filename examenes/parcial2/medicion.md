# Medición: turnos de una clínica

## 1. Qué se compara

Se comparan dos implementaciones de `IGestorTurnos` que realizan el mismo trabajo:

- `GestorIngenuo`: `List<Paciente>`. Para cada atención recorre la lista, encuentra el menor par `(Urgencia, Llegada)` y lo elimina.
- `GestorMejorado`: `PriorityQueue` (heap mínimo) con prioridad `(Urgencia, Llegada)` y `HashSet<string>` para consultas exactas por nombre.

La carga medida registra primero `n` pacientes y después atiende a los `n`. El patrón de urgencias es determinista e idéntico en las dos implementaciones.

## 2. Protocolo

Para cada tamaño y estructura el programa realiza:

1. Una corrida de calentamiento, no incluida en los resultados.
2. Tres corridas medidas.
3. Promedio de las tres mediciones.
4. Generación de `mediciones_detalle.csv` y `mediciones_resumen.csv`.

Los tamaños predeterminados son 5.000, 10.000 y 20.000 pacientes (n, 2n y 4n). El enunciado pide elegir `n` para que el GestorIngenuo con el mayor tamaño tarde entre 0,5 y 20 segundos. Como depende del equipo, verifica el promedio obtenido. Si queda por debajo de 0,5 s, aumenta N base; si supera 20 s, redúcelo. El programa permite introducir otro N base al comenzar.

## 3. Resultados experimentales

**Completar esta tabla con valores reales de `mediciones_resumen.csv` después de ejecutar el programa.** No sustituirlos por tiempos estimados o inventados.

| N | Ingenuo promedio (ms) | Factor ingenuo | Mejorado promedio (ms) | Factor mejorado |
|---:|---:|---:|---:|---:|
| n | Pendiente | — | Pendiente | — |
| 2n | Pendiente | Pendiente | Pendiente | Pendiente |
| 4n | Pendiente | Pendiente | Pendiente | Pendiente |

\[
\text{Factor} = \frac{\text{promedio del tamaño actual}}{\text{promedio del tamaño anterior}}
\]

Para cada estructura, el primer tamaño no tiene factor porque no existe una fila anterior.

## 4. Archivos CSV

- `mediciones_detalle.csv`: una fila por repetición, con estructura, N, número de repetición, tiempo en milisegundos y valor de control.
- `mediciones_resumen.csv`: promedio de las tres repeticiones y factor respecto del tamaño anterior.

El valor de control permite comprobar que ambas implementaciones atendieron la misma cantidad de pacientes y sumaron la misma longitud de nombres. El guion de prueba comprueba el orden de atención y el desempate.

## 5. Conclusión

La lista simple debe mostrar un crecimiento aproximadamente cuadrático en la fase de atención, porque en cada extracción vuelve a buscar el mínimo entre los pacientes restantes y además elimina un elemento de la lista. Al duplicar N, el tiempo puede acercarse a cuatro veces, aunque no tiene por qué ser exactamente cuatro por el ruido de medición, calentamiento, recolector de basura y caché.

La solución con heap debe crecer aproximadamente como O(n log n) para registrar y atender todos los pacientes, por lo que al duplicar N se espera un aumento menor que el de la lista ingenua. Los resultados deben interpretarse con cautela: para tamaños pequeños el ruido puede ocultar la tendencia y las constantes de implementación también influyen.
.
