Pablo Rildo Villarroel Fernandez

# P1.1 — Analizar

## a) Análisis de los cuatro fragmentos

| Fragmento | Costo en el peor caso | Justificación |
|---|---:|---|
| F1 | **O(n)** | El ciclo recorre los `n` elementos una sola vez y en cada iteración realiza una comprobación de paridad y, como máximo, un incremento constante. |
| F2 | **O(n²)** | Hay dos ciclos anidados: para cada uno de los `n` valores de `i`, el ciclo de `j` puede recorrer los `n` elementos. |
| F3 | **O(log n)** | `k` comienza en 1 y se duplica en cada iteración; se necesitan aproximadamente `log₂(n)` duplicaciones para alcanzar o superar `n`. |
| F4 | **O(n log n)** | El ciclo exterior se ejecuta `n` veces y, en cada vuelta, `k` se divide entre 3 hasta llegar a 1, lo que requiere aproximadamente `log₃(n)` iteraciones. |

**Observación:** se analiza el número de operaciones en función de `n`; se omiten constantes y términos de menor orden.

## b) Resolución de recurrencias con el teorema maestro

Se usa la forma:

\[
T(n)=aT(n/b)+\Theta(n^d)
\]

La comparación se realiza entre `a` y `b^d`.

### (i) \(T(n)=4T(n/3)+n\)

- `a = 4`
- `b = 3`
- `d = 1`
- `b^d = 3^1 = 3`
- Comparación: `a = 4 > 3 = b^d`.

Corresponde al caso en que domina el término recursivo:

\[
T(n)=\Theta(n^{\log_b a})=\Theta(n^{\log_3 4})
\]

**Resultado:** \(\boxed{T(n)=\Theta(n^{\log_3 4})}\), aproximadamente \(\Theta(n^{1.262})\).

### (ii) \(T(n)=2T(n/2)+n^2\)

- `a = 2`
- `b = 2`
- `d = 2`
- `b^d = 2^2 = 4`
- Comparación: `a = 2 < 4 = b^d`.

Domina el trabajo no recursivo `n²` (caso 3 del teorema maestro; se cumple la condición de regularidad):

\[
T(n)=\Theta(n^d)=\Theta(n^2)
\]

**Resultado:** \(\boxed{T(n)=\Theta(n^2)}\).

---
