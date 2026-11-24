# Algoritmos de grafos aplicados al proyecto

## 1. Representación implementada

El programa construye un grafo bipartito no dirigido:

- Vértices `0` a `n-1`: trabajadores.
- Vértices `n` a `2n-1`: puestos.
- Aristas: relaciones de elegibilidad trabajador–puesto.

Cada trabajador se conecta con cuatro puestos diferentes. Para poder repetir exactamente el experimento, las conexiones se generan mediante una fórmula determinista. El grafo es disperso: para `n` trabajadores hay `V = 2n` nodos y `E = 4n` aristas.

Los datos de 20 trabajadores/20 puestos corresponden al tamaño base de la práctica. Los tamaños 100, 500 y 1.000 son escenarios hipotéticos de crecimiento; no son datos institucionales reales.

## 2. Lista de adyacencia y matriz de adyacencia

El mismo grafo se guarda en dos estructuras:

| Operación | Lista de adyacencia | Matriz de adyacencia |
|---|---:|---:|
| Memoria | O(V + E) | O(V²) |
| Consultar una arista concreta | O(grado(u)) si se busca en la lista de vecinos | O(1) |
| Enumerar vecinos de un nodo | O(grado(u)) | O(V) |
| BFS / DFS completo | O(V + E) | O(V²) |

Se elige lista de adyacencia como representación principal porque las relaciones de elegibilidad suelen ser dispersas: cada trabajador puede ser apto para unos pocos puestos, no necesariamente para todos. La matriz se conserva en el experimento para comparar su costo.

## 3. BFS y DFS

- BFS usa `Queue<int>` y explora por niveles. Es adecuado para conectividad y camino mínimo en número de aristas cuando el grafo no tiene pesos.
- DFS usa `Stack<int>` y explora en profundidad. Es adecuado para recorrer componentes y explorar la estructura.
- El programa ejecuta ambos como un bosque de recorridos: si hay componentes desconectadas, inicia otro recorrido desde el siguiente nodo no visitado. Así visita todos los nodos.

El experimento verifica que las cuatro combinaciones (lista/matriz y BFS/DFS) visiten los mismos V nodos. No compara la ruta de asignación óptima, porque BFS/DFS no resuelven el emparejamiento ponderado.
