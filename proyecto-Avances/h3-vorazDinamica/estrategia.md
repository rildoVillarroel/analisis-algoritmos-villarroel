# Pablo Rildo Villarroel Fernández

# hitos/h3/estrategia.md — ¿Mi caso admite un voraz o una dinámica?

> Proyecto: asignación óptima de personal técnico a proyectos eléctricos. Datos sintéticos.

## 1. La decisión que se repite y el resultado a optimizar

- **Decisión que se repite:** «¿a qué puesto va el trabajador i?», una vez por cada uno de los n trabajadores (hoy n = 20; peor mes, hipótesis de trabajo, n = 100).
- **Resultado a optimizar:** el **costo total mínimo** C = Σ costo[i, puesto de i], donde costo = 100 − puntuación (puntuación = 0,50·competencia + 0,25·experiencia + 0,15·disponibilidad + 0,10·equilibrio de carga). Cada trabajador ocupa un solo puesto y cada puesto lo cubre una sola persona; los pares que incumplen restricciones obligatorias están prohibidos.

## 2. Las tres preguntas de la clase 12, en orden

### a) ¿Tiene subestructura óptima? **Sí**

Si en la asignación óptima el trabajador T_n va al puesto P, entonces el resto de las asignaciones tiene que ser óptima para los n−1 trabajadores y los n−1 puestos que quedan; si no lo fuera, cambiaría ese resto por uno mejor y bajaría el total, contradiciendo que era óptima. La solución grande se arma con soluciones óptimas de pedazos más chicos.

### b) ¿Alcanza con elegir lo mejor ahora (voraz)? **No**

La elección local es «el par trabajador-puesto más barato que siga disponible». No es segura. **Contraejemplo** (costos, filas = trabajadores, columnas = puestos):

|  | Puesto 1 | Puesto 2 |
|---|---|---|
| Trabajador 1 | **1** | 2 |
| Trabajador 2 | 2 | 100 |

- **Voraz:** toma el 1 (T1-P1) y queda obligado a pagar 100 (T2-P2) → **101**.
- **Óptimo:** T1-P2 y T2-P1 → 2 + 2 = **4**.

Lo mejor ahora dejó al otro trabajador sin alternativa. Por eso el voraz no se puede demostrar correcto en mi caso (y además, con restricciones obligatorias, puede ni siquiera cubrir todos los puestos aunque exista una asignación completa). La evidencia en 200 instancias sintéticas está en `comparacion.md` (`voraz.csv`).

### c) ¿Hay subproblemas que se repiten? **Sí, y es lo que habilita la dinámica**

Un subproblema es «los primeros i trabajadores ya están asignados a un conjunto S de puestos; falta asignar al resto». **Lo único que importa es qué puestos están usados, no el orden en que se usaron.**

Ejemplo con n = 3: T1→P1 y T2→P2 deja usados {P1, P2}; T1→P2 y T2→P1 también deja usados {P1, P2}. Los dos caminos llegan al mismo subproblema («falta asignar a T3 con el puesto P3»), que la fuerza bruta resuelve dos veces y la dinámica una sola.

Conteo: la fuerza bruta recorre n! caminos; los subproblemas distintos son solo 2^n (los subconjuntos de puestos). Con n = 10: 3.628.800 caminos contra 1.024 subproblemas.

## 3. Conclusión: **aplica la programación dinámica**

- **Estados:** `dp[máscara]` = mejor costo de asignar a los primeros popcount(máscara) trabajadores exactamente los puestos de la máscara (cada puesto es un bit).
- **Transición:** `dp[máscara ∪ {j}] = min( dp[máscara] + costo[i, j] )`, con i = popcount(máscara) y j un puesto libre.
- **Costo:** tiempo O(n · 2^n) (exactamente n·2^(n−1) transiciones) y memoria O(2^n); contra O(n · n!) de la fuerza bruta. Se implementa abajo→arriba (tabla) y arriba→abajo (memoización); con memoria las llamadas pasan de ≈ e·n! a 1 + n·2^(n−1).

## 4. La salvedad honesta (panorama NP)

- La dinámica por subconjuntos **sigue siendo exponencial**: con n = 20 son unos 10,5 millones de transiciones (viable), pero con n = 100 serían 2^100 estados (imposible). Por eso **no es la estrategia que usaría en producción**: para mi caso ya elegí el **algoritmo húngaro, O(n³)** (clase 6): con n = 100 son 10⁶ pasos y garantiza el óptimo. El húngaro no es ni voraz ni la dinámica de esta clase: es un algoritmo específico del problema de asignación.
- El problema de asignación **está en P** (se resuelve en tiempo polinomial) aunque su fuerza bruta parezca el agente viajero (n! recorridos). Que la fuerza bruta explote no significa que el problema sea difícil: la estructura permite algo mejor.
- Si el modelo cambiara y **un trabajador pudiera tomar varios puestos según sus horas libres** (asignación generalizada), el problema pasaría a ser **NP-difícil**: sin algoritmo exacto eficiente conocido. Ahí usaría un voraz como heurística, fuerza bruta solo con n muy pequeño, y la mochila 0/1 (peso = horas del puesto, valor = puntuación, capacidad = horas libres) para el subproblema de un solo trabajador.

## 5. Resumen

| Pregunta | Respuesta | Evidencia |
|---|---|---|
| Subestructura óptima | Sí | Argumento de intercambio (sección 2a) |
| Elección voraz segura | **No** | Contraejemplo 2×2: 101 contra 4; `voraz.csv` |
| Subproblemas que se repiten | **Sí** | n! caminos contra 2^n subproblemas |
| Estrategia adecuada del curso | **Programación dinámica** (tabla o memoización) | `comparacion.md`: mismo costo que la fuerza bruta, mucho más rápida |
| Lo que uso en producción | Húngaro O(n³) | n = 100 → 10⁶ pasos; la dinámica no escala |
