# Pablo Rildo Villarroel Fernández

# estructura.md — La estructura de mi proyecto de asignación óptima de personal

> Proyecto: asignación de personal técnico a proyectos eléctricos.

## 1. Estructuras elegidas

1. **`Dictionary<int perfil, SortedSet<(carga, id, nivel, experiencia)>>`**: un árbol de búsqueda balanceado (rojinegro de .NET) **por perfil**, ordenado por carga. Es la estructura de trabajo del algoritmo voraz.
   - Solo se indexa al personal **habilitado** (nivel ≥ 3 y experiencia ≥ 2 años), porque ningún puesto del proyecto exige menos. Es la restricción obligatoria de competencia mínima aplicada al construir el índice.
2. **`Dictionary<int, Trabajador>` por Id**: para buscar una ficha por clave exacta (medido en la práctica de la clase 9).

**Ingenua (para comparar):** `List<Trabajador>` sin orden; cada puesto recorre todo el inventario.

## 2. Costo O de cada operación que más hago

n = trabajadores, p = puestos (en mi caso p es proporcional a n), s = nodos que se saltan antes de encontrar un candidato válido (en mi simulación, 2-3 por puesto).

| Operación | Lista sin orden (ingenua) | Hash simple | Árbol por perfil (elegida) |
|---|---|---|---|
| Armar con n trabajadores | O(n) | O(n)* | O(n log n) |
| Rango (carga ≤ tope de horas libres) | O(n) | O(n) | **O(log n + k)** |
| El menor | O(n) | O(n) | **O(log n)** |
| El siguiente | O(n) | O(n) | **O(1) amortizado** al recorrer en orden |
| Borrar al asignado | O(n) (desplaza la lista) | O(1)* | **O(log n)** |
| Buscar por clave | O(n) | **O(1)*** | O(log n) |
| Insertar (alta) | O(1) | O(1)* | O(log n) |
| Recorrer en orden | O(n log n) (hay que ordenar) | O(n log n) | **O(n)** |

\* tiempo promedio. k = claves del rango que se recorren.

**Costo total de resolver los p puestos:**

- Lista: p · (n para buscar + n para borrar) = **O(p·n)**. Con p proporcional a n, es **cuadrático**: al duplicar n el tiempo se multiplica por ~4.
- Árbol por perfil: n log n para armar + p · (log n + s + log n para borrar) = **O(n log n)** mientras s se mantenga pequeño: al duplicar n el tiempo se multiplica por ~2 (un poco más).

Con mis tamaños, contando elementos mirados (aproximado, con s ≈ 3):

| Caso | Lista | Árbol por perfil |
|---|---|---|
| Hoy: n = 20, p = 20 | 20 × 20 = 400 | 20·log₂20 + 20·(4,3 + 3) ≈ 86 + 146 ≈ 230 |
| Peor mes: n = 100, p = 100 | 100 × 100 = 10.000 | 100·log₂100 + 100·(6,6 + 3) ≈ 664 + 960 ≈ 1.600 |

## 3. Porque hice esta elección

- **Pido «el de menor carga» y un umbral de horas libres:** son operaciones de **orden y rango**. En un árbol ordenado por carga, «el menor» está en la punta, «el siguiente» es el próximo nodo y el umbral de horas libres (carga ≤ 40 − horas) es un tramo contiguo que empieza en el menor. Una tabla hash no tiene orden, así que cada puesto la obligaría a revisar todo.
- **Ordenar por carga coincide con el criterio de equilibrio de carga** del proyecto (el 10 % de la puntuación).
- **Balanceado y no un BST propio:** mis Id llegan ya ordenados; en un BST simple eso lo degenera en una lista (armarlo costaría ~n²/2, igual que el ejemplo de clase de datos ordenados). `SortedSet` mantiene la altura en ~log n pase lo que pase.
- **Un árbol por perfil:** un puesto solo lo cubre alguien de su perfil, así que cada árbol es ~14 veces más chico y no mezclo perfiles.
- **El filtro de habilitados importa:** sin él, el personal que no cumple ningún puesto tiene poca carga, se queda al principio del árbol y hay que saltarlo en cada consulta; s crece con n y el costo vuelve a ser cuadrático (en una simulación propia, de ~18 nodos por consulta con 2.000 trabajadores a ~64 con 8.000). Por eso la medición incluye una variante **sin filtro** como control.
- **Hash por Id:** para la búsqueda por clave exacta es lo mejor (O(1)); el árbol no aporta nada ahí.

## 4. Lo que esta estructura NO resuelve

- La asignación **óptima** completa (húngaro, O(n³), clase 6) sigue necesitando la matriz de costos n×n. La estructura acelera lo que rodea al algoritmo: encontrar candidatos, equilibrar carga, retirar asignados y recalcular una sola vacante.
- Si algún puesto exigiera menos que nivel 3 o 2 años, habría que reconstruir el índice con ese mínimo menor.

## 5. Cuando cambiaria de opinion

- Con n de 20 a 100 y una sola planificación por día, la estructura no importa en la práctica: en la medición, lista y árbol empataron con n = 20 y el árbol fue unas 2 veces más rápido con n = 100, pero ambos en microsegundos (ver `medicion.md`). Ahí elegiría la lista por simplicidad.
- Si solo necesitara buscar fichas por Id, usaría solo el `Dictionary`.
- El hash por perfil quedó descartado por la medición: con 80,000 trabajadores tardó 1,093 ms contra 21.5 ms del árbol por perfil, porque sigue siendo cuadrático.
