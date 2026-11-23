# H3 — Estrategia: programación dinámica para la asignación de personal

## 1. Decisión repetida y objetivo

En el proyecto académico de asignación de personal técnico a cinco proyectos eléctricos (línea MT rural, hospital de segundo nivel, centro de eventos/coliseo, línea de envasado automatizada y mantenimiento de celdas MT) se decide qué trabajador ocupa cada puesto.

Para estudiar el algoritmo se define una versión simplificada:
- Hay `n` trabajadores y `n` puestos.
- Cada trabajador ocupa exactamente un puesto y cada puesto recibe exactamente un trabajador.
- Cada pareja trabajador–puesto tiene una puntuación sintética de compatibilidad, que representa habilidades, experiencia, certificaciones y disponibilidad.
- Se maximiza la suma de las puntuaciones.

Los datos del experimento son sintéticos, reproducibles y no representan evaluaciones reales de personas.

## 2. ¿Voraz o programación dinámica?

Un voraz podría elegir en cada paso la pareja libre con mayor puntuación. Sin embargo, esa decisión local puede bloquear una combinación global mejor.

Contraejemplo:

| | Puesto A | Puesto B |
|---|---:|---:|
| Trabajador 1 | 10 | 9 |
| Trabajador 2 | 9 | 1 |

El voraz elige Trabajador 1 → A (10) y deja Trabajador 2 → B (1): total 11. La solución óptima es Trabajador 1 → B (9) y Trabajador 2 → A (9): total 18. Por tanto, el voraz no garantiza el óptimo.

## 3. Por qué aplica programación dinámica

- **Subestructura óptima:** una asignación óptima completa debe contener una asignación óptima para el estado parcial que deja, porque de lo contrario se podría sustituir esa parte por una mejor y mejorar el total.
- **Subproblemas repetidos:** distintas secuencias de asignaciones pueden llegar al mismo conjunto de puestos ocupados. Si el estado se identifica por ese conjunto, se conserva una sola vez el mejor puntaje alcanzable.

Se aplica tabulación (de abajo hacia arriba) con una máscara de bits que representa los puestos ocupados.

## 4. Estado y recurrencia

Sea `dp[mask]` la máxima puntuación al asignar los primeros `i = popcount(mask)` trabajadores a los puestos marcados en `mask`.

Estado inicial: `dp[0] = 0`.

Si el estado `mask` tiene `i` puestos ocupados, se puede asignar el trabajador `i` a cualquier puesto libre `j`:

`dp[mask | (1 << j)] = max(dp[mask | (1 << j)], dp[mask] + puntuacion[i,j])`.

La respuesta es `dp[(1 << n) - 1]`.

## 5. Costos

- Fuerza bruta: enumera las `n!` permutaciones; costo factorial O(n!) con acumulación incremental.
- Programación dinámica con máscara: `2^n` estados y hasta `n` transiciones por estado; tiempo O(n · 2^n), memoria O(2^n).

## 6. Relación con el proyecto completo y límites

Esta comparación estudia una versión simplificada de asignación uno a uno con puntuaciones aditivas. El proyecto completo puede incorporar certificaciones obligatorias, disponibilidad por horas, carga laboral, múltiples habilidades, incompatibilidades y puestos que requieren equipos. Esas restricciones deben modelarse expresamente; el experimento no afirma resolverlas todas.

Además, para la asignación clásica uno a uno con puntuaciones aditivas existe el algoritmo Húngaro, de tiempo polinomial O(n^3). La dinámica es una estrategia exacta y didáctica para el H3, pero no necesariamente la implementación final más escalable si el modelo sigue siendo asignación clásica. Con restricciones combinatorias adicionales, el problema puede dejar de ser la asignación clásica.

## 7. Conclusión

Para la versión definida, el voraz no garantiza el óptimo: una elección local puede bloquear una combinación global mejor. La programación dinámica sí aplica por la subestructura óptima y los subproblemas repetidos identificados por la máscara de puestos ocupados. Se compara contra fuerza bruta para comprobar igualdad del óptimo y estudiar el crecimiento del tiempo.
