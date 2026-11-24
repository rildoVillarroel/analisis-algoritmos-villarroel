# Pablo Rildo Villarroel Fernández

# Fuerza bruta contra programación dinámica en la asignación de personal

> Datos sintéticos. **Pendiente de completar:** las celdas con `___` salen de TU corrida (`tabla_medicion.md`, `resumen.csv`, `voraz.csv`). Los **conteos** de las secciones 2 y 4 son exactos y no dependen de la máquina.

## 1. El problema y las estrategias (código: `Program.cs`)

Asignar n trabajadores a n puestos minimizando el costo total (mismo modelo de costos del proyecto: costo = 100 − puntuación; pares prohibidos con costo 1.000.000 para que todas las estrategias vean la misma matriz).

| Estrategia | Idea | Costo |
|---|---|---|
| Fuerza bruta | Prueba las n! permutaciones | O(n · n!) |
| Dinámica, tabla | `dp[máscara]`: mejor costo de asignar a los primeros popcount(máscara) trabajadores exactamente esos puestos | O(n · 2^n) |
| Dinámica, memoización | La misma idea con recursión y libreta | O(n · 2^n) |
| Húngaro (referencia, clase 6) | Algoritmo específico de asignación | O(n³) |
| Voraz | El par más barato disponible ahora | O(n² log n), **no siempre óptimo** |

## 2. «Sin memoria» contra «con memoria» (conteos exactos, como Fibonacci en la clase 12)

| n | Llamadas sin memoria (suma de n!/k!) | Llamadas con memoria (1 + n·2^(n−1)) | Cociente |
|---|---|---|---|
| 8 | 109.601 | 1.025 | ≈ 107 |
| 9 | 986.410 | 2.305 | ≈ 428 |
| 10 | 9.864.101 | 5.121 | ≈ 1.926 |
| 11 | 108.505.112 | 11.265 | ≈ 9.632 |

El programa verifica en cada tamaño que las llamadas medidas coinciden con estas fórmulas.

## 3. Protocolo

Modo Release. **1 calentamiento + 3 corridas medidas + promedio** (suma de tiempos / 3) por algoritmo y tamaño; `Stopwatch` solo alrededor de la estrategia. Verificación fuera del tiempo medido: **todas las estrategias dan el mismo costo mínimo** (fuerza bruta = dinámica = memoización = húngaro) y los conteos coinciden con las fórmulas. Archivos: `resultados.csv` (cada corrida), `resumen.csv` (promedio, factor, mínimo, máximo, dispersión), `conteos.csv` y `voraz.csv`.

**Tamaños:** 4 para fuerza bruta contra dinámica (n = 8, 9, 10, 11; con más, la fuerza bruta deja de entrar) y 4 para dinámica contra húngaro (n = 16, 18, 20, 22, de 2 en 2 como en la clase 12).

## 4. Lo que espero ver (hipótesis antes de medir)

| Estrategia | Operaciones | Factor esperado al subir n |
|---|---|---|
| Fuerza bruta | ≈ e·n! | ×9, ×10, ×11 por cada trabajador más (n+1 cada vez) |
| Dinámica | n·2^(n−1) | ≈ ×2,2 por cada +1 y ≈ ×4,4 a ×4,5 por cada +2 (16→18: 4,5 · 18→20: 4,4 · 20→22: 4,4) |
| Húngaro | ≤ n³ | ≈ ×1,3 a ×1,4 por cada +2 (casi plano) |

Operaciones exactas de la dinámica: n = 16: 524.288 · n = 18: 2.359.296 · n = 20: 10.485.760 · n = 22: 46.137.344.

## 5. Resultados (pegar aquí `tabla_medicion.md`)

**A) Fuerza bruta contra dinámica** (mismo costo mínimo en cada fila; × = veces más que la fila anterior):

| n | Fuerza bruta (ms) | × | Dinámica (ms) | × | Fuerza bruta / Dinámica |
|---|---|---|---|---|---|
| 8 | ___ | — | ___ | — | ___ veces |
| 9 | ___ | ×___ | ___ | ×___ | ___ veces |
| 10 | ___ | ×___ | ___ | ×___ | ___ veces |
| 11 | ___ | ×___ | ___ | ×___ | ___ veces |

**B) Dinámica contra húngaro** (n sube de 2 en 2):

| n | Dinámica (ms) | × | Húngaro (ms) | × | Dinámica / Húngaro |
|---|---|---|---|---|---|
| 16 | ___ | — | ___ | — | ___ veces |
| 18 | ___ | ×___ | ___ | ×___ | ___ veces |
| 20 | ___ | ×___ | ___ | ×___ | ___ veces |
| 22 | ___ | ×___ | ___ | ×___ | ___ veces |

**Voraz (200 instancias de n = 10, óptimo = dinámica):** no cubrió todos los puestos en ___ de 200; cubrió todo pero no fue óptimo en ___ de ___ instancias completas; sobrecosto medio ___ % (máximo ___ %).

## 6. Conclusión (3 o 4 líneas, con tus números)

1. **Mismo resultado, otro costo:** la fuerza bruta y la dinámica dieron el mismo costo mínimo en los ___ tamaños; con n = 11 la fuerza bruta tardó ___ ms y la dinámica ___ ms (___ veces más). La fuerza bruta creció ×___, ×___ y ×___ (esperaba ×9, ×10, ×11: [coincide / se desvía en ___]) y la dinámica ×___ por cada trabajador más (esperaba ≈ ×2,2).
2. **La dinámica también es exponencial:** de 16 a 22 subió ×___ por cada +2 (esperaba ≈ ×4,4), mientras que el húngaro casi no se movió (×___); con n = 22 la dinámica tardó ___ ms y el húngaro ___ ms. Por eso la dinámica sirve para n pequeño o mediano y el húngaro es el que escala.
3. **Voraz:** [no es confiable en mi caso: ___ de 200 instancias quedaron incompletas o no óptimas].
4. **Ruido:** las tres corridas de cada celda no coinciden; la mayor dispersión fue ___ % (___, n = ___). Los tiempos de microsegundos del húngaro y de la dinámica con n pequeño son los más ruidosos: una diferencia menor que la dispersión no es evidencia. Lo que importa es la FORMA (los factores), no los milisegundos.
