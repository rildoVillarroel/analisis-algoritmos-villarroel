# Hito de grafos — Modelo del proyecto de asignación de personal

## 1. ¿Se puede modelar el proyecto como un grafo?

Sí. La asignación de trabajadores a puestos contiene relaciones de muchos-a-muchos: un trabajador puede ser apto para varios puestos y un puesto puede tener varios candidatos. Esta relación se representa mediante un **grafo bipartito no dirigido y ponderado**.

## 2. Nodos y aristas

Para el modelo base hay dos grupos de nodos:

- **Trabajadores:** `T01` a `T20`.
- **Puestos:** `P01` a `P20`, distribuidos entre los cinco proyectos: línea MT rural, hospital de segundo nivel, centro de eventos/coliseo, línea de envasado automatizada y mantenimiento de celdas MT.

Una arista `T_i — P_j` significa que el trabajador `T_i` puede ser considerado para el puesto `P_j`, después de comprobar los requisitos obligatorios. La arista puede llevar un peso de compatibilidad, por ejemplo de 0 a 100, calculado a partir de habilidades, nivel, experiencia, certificaciones y disponibilidad.

No se crea una arista cuando el trabajador no cumple una condición obligatoria, como una certificación indispensable o la disponibilidad mínima. En el código de esta práctica, cada trabajador se conecta con cuatro puestos elegibles generados de forma determinista; esto es una simulación académica y no una evaluación real. Para medir BFS/DFS, las aristas se tratan como conexiones sin peso: el experimento mide conectividad y recorrido, no la puntuación de compatibilidad.

## 3. ¿Y los cinco proyectos?

En esta versión, cada puesto guarda como atributo el proyecto al que pertenece. Así se mantiene el grafo bipartito trabajador–puesto, que es el modelo directo del problema de asignación.

Si se quisiera representar explícitamente la jerarquía de proyectos, se podría ampliar a un grafo de tres tipos de nodos: trabajador, puesto y proyecto, agregando aristas `Puesto — Proyecto`. Esa variante sirve para navegar la estructura y consultar agrupaciones, pero no es necesaria para comparar BFS y DFS en esta práctica.

## 4. Lista y matriz de adyacencia

- **Lista de adyacencia:** cada nodo almacena únicamente sus vecinos. En un grafo disperso como el de elegibilidad, requiere O(V + E) memoria y permite recorrer los vecinos en proporción al grado del nodo.
- **Matriz de adyacencia:** almacena una tabla V × V. Consultar si existe una arista es O(1), pero ocupa O(V²) memoria y examinar los vecinos de un nodo requiere revisar una fila completa.

El programa construye ambas representaciones a partir del mismo conjunto de aristas para que la comparación sea justa.

## 5. ¿Qué pregunta responde cada recorrido?

### BFS — búsqueda en anchura

Utiliza una cola FIFO y explora por niveles. En un grafo no ponderado encuentra el camino con menor cantidad de aristas. En este proyecto puede responder preguntas de conectividad y alcance: desde un trabajador, ¿qué puestos elegibles se pueden alcanzar?, o ¿qué trabajadores/puestos pertenecen al mismo grupo conectado?

En el grafo bipartito directo trabajador–puesto, una arista ya representa elegibilidad directa; por eso la ruta mínima trabajador–puesto conectado es de un solo tramo. BFS es más informativo si se amplía el modelo con nodos intermedios de habilidades, certificaciones o proyectos.

### DFS — búsqueda en profundidad

Utiliza una pila LIFO (o recursión). Explora una rama hasta el fondo y retrocede. Es útil para recorrer todos los componentes, detectar grupos aislados y explorar relaciones. No garantiza caminos mínimos.

Ambos recorridos cuestan O(V + E) con lista de adyacencia. Con matriz, encontrar vecinos obliga a examinar V posibles destinos por cada nodo, por lo que el recorrido completo cuesta O(V²).

## 6. Límite importante: el grafo no resuelve por sí solo la asignación óptima

BFS y DFS recorren y analizan la red; no seleccionan automáticamente la combinación de aristas de mayor peso sin repetir trabajadores ni puestos. La optimización corresponde a un problema de **emparejamiento máximo ponderado / asignación**, que puede resolverse con el algoritmo Húngaro en el caso clásico uno-a-uno, o con modelos de flujo/coste mínimo y optimización entera cuando se agregan restricciones.

Por tanto, el grafo complementa el proyecto: permite representar elegibilidad, relaciones y conectividad; el algoritmo de optimización decide la asignación final.
