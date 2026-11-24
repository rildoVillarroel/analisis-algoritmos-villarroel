# Medición — Grafos del proyecto de asignación de personal

## 1. Objetivo

Medir cómo cambia el costo de recorrer el mismo grafo bipartito de elegibilidad al usar lista o matriz de adyacencia, aplicando BFS y DFS. La comparación separa el efecto de la representación del grafo del algoritmo de recorrido.

## 2. Escenarios

Cada escenario tiene igual número de trabajadores y puestos. Cada trabajador tiene cuatro aristas de elegibilidad hacia puestos distintos.

| Trabajadores | Puestos | V = trabajadores + puestos | E = relaciones |
|---:|---:|---:|---:|
| 20 | 20 | 40 | 80 |
| 100 | 100 | 200 | 400 |
| 500 | 500 | 1.000 | 2.000 |
| 1.000 | 1.000 | 2.000 | 4.000 |

Los primeros 20 trabajadores y 20 puestos reflejan el tamaño base del modelo académico. Los demás son ampliaciones hipotéticas para evaluar escalabilidad; no representan cantidades reales de la institución.

## 3. Protocolo

Para cada tamaño, representación y recorrido:

1. Construir una sola vez el grafo sintético determinista.
2. Ejecutar una corrida de calentamiento que no se registra.
3. Ejecutar tres corridas medidas.
4. Calcular el promedio de los tres tiempos.
5. Comprobar que el recorrido visita los mismos V nodos en todas las variantes.
6. Registrar tiempos y factores de crecimiento en CSV.

La construcción del grafo queda fuera del cronómetro: se mide únicamente BFS o DFS sobre la representación ya construida. Ejecutar en modo Release:

```bash
dotnet run -c Release
```

## 4. Archivos generados

- `grafo_detalle.csv`: una fila por repetición, con representación, algoritmo, tamaño, cantidad de nodos/aristas, tiempo y nodos visitados.
- `grafo_resumen.csv`: promedio de las tres repeticiones y factor respecto del tamaño anterior para la misma representación y algoritmo.

Factor de crecimiento:

`factor = promedio_actual / promedio_tamano_anterior`

## 5. Tabla para completar con mediciones reales

Copiar los promedios de `grafo_resumen.csv`. No inventar ni reemplazar tiempos por valores teóricos.

| Trabajadores | V | E | Lista BFS (ms) | Lista DFS (ms) | Matriz BFS (ms) | Matriz DFS (ms) |
|---:|---:|---:|---:|---:|---:|---:|
| 20 | 40 | 80 | Pendiente | Pendiente | Pendiente | Pendiente |
| 100 | 200 | 400 | Pendiente | Pendiente | Pendiente | Pendiente |
| 500 | 1.000 | 2.000 | Pendiente | Pendiente | Pendiente | Pendiente |
| 1.000 | 2.000 | 4.000 | Pendiente | Pendiente | Pendiente | Pendiente |

## 6. Conclusión teórica que se contrastará con los datos

Con lista de adyacencia, BFS y DFS deben crecer aproximadamente como O(V + E), porque visitan cada nodo y cada arista una vez. Con matriz, los recorridos examinan una fila de V posiciones por nodo y su costo es O(V²). Como el grafo de elegibilidad es disperso (E = 4n y V = 2n), se espera que la lista use menos memoria y escale mejor. Los tiempos reales pueden fluctuar por ruido del sistema operativo, calentamiento, caché y resolución del cronómetro; por eso se promedian tres corridas y se interpreta la tendencia, no una medición aislada.

**Importante:** esta medición evalúa representación y recorridos de grafos. No mide la calidad ni el tiempo de la asignación óptima ponderada; esa etapa requiere un algoritmo de emparejamiento/asignación.
