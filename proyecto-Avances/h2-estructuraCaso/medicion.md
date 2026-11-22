Pablo Rildo Villarroel Fernández

# Medicion : Estructura ingenua contra estructura adecuada en mi proyecto

> Tiempos medidos en mi computadora, modo Release (`resumen.csv` y `resultados.csv` en esta carpeta).

## 1. El problema (el mismo del proyecto)

Asignación voraz con equilibrio de carga: hay un inventario de n trabajadores y n/4 puestos. Para cada puesto, en orden: buscar a quienes lo cumplen (perfil, nivel mínimo, experiencia mínima y horas libres suficientes), elegir al de **menor carga** (desempate: menor Id) y **retirarlo** del inventario. Si nadie cumple, el puesto queda sin cubrir.

| Contendiente | Rol | Qué es |
|---|---|---|
| `Lista` | **Ingenua** | `List<Trabajador>` sin orden; cada puesto recorre todo el inventario |
| `ArbolPorPerfil` | **Adecuada** | `Dictionary` perfil → `SortedSet` ordenado por carga, solo con personal habilitado (ver `estructura.md`) |
| `HashPorPerfil` | Control | `Dictionary` perfil → lista; cada puesto recorre solo su perfil |
| `ArbolSinFiltro` | Control | Igual que el adecuado pero sin el filtro de habilitados |

Las cuatro dan **exactamente la misma asignación** (se verifica en cada tamaño). El tiempo medido incluye **armar la estructura** y resolver todos los puestos.

## 2. Protocolo

Modo Release. **3 calentamientos** por contendiente, **3 mediciones** por contendiente y tamaño, **promedio = suma de tiempos / 3**, `Stopwatch` solo alrededor de armar + resolver. Verificación fuera del tiempo medido: la asignación es válida (nadie repetido, todos cumplen su puesto), las cuatro estructuras coinciden y el recorrido inorden de cada árbol sale ordenado. Archivos: `resultados.csv` (cada corrida), `resumen.csv` (promedios, factor, mínimo, máximo y dispersión), `cobertura.csv` y `tabla_medicion.md`.

**Tamaños:** cuatro que se duplican (10.000, 20.000, 40.000 y 80.000 trabajadores, con n/4 puestos) y, aparte, los dos reales (20 y 100).

## 3. Hipótesis (escritas antes de medir) y resultado

| Estructura | Costo total | Factor esperado al duplicar n | Factor medido (20.000 → 40.000 → 80.000) |
|---|---|---|---|
| `Lista` | O(p·n), cuadrático | ≈ ×4 | ×3.62 · ×4.24 · ×4.25 |
| `HashPorPerfil` | O(p·n/14), cuadrático | ≈ ×4 | ×4.05 · ×4.87 · ×3.98 |
| `ArbolSinFiltro` | s crece con n | entre ×2 y ×4 | ×3.33 · ×3.97 · ×4.22 |
| `ArbolPorPerfil` | O(n log n) | ≈ ×2 a ×2.2 | ×1.28 · ×2.51 · ×2.49 |

## 4. Resultados

**Tamaños que se duplican** (promedio de 3 corridas en ms; × = veces que tarda más que la fila anterior):

| n | Puestos | Lista (ms) | × | Árbol por perfil (ms) | × | Lista / Árbol | Dispersión Lista | Dispersión Árbol |
|---|---|---|---|---|---|---|---|---|
| 10,000 | 2,500 | 58.9381 | — | 2.6892 | — | 21.9 veces | 11.3 % | 128.0 % |
| 20,000 | 5,000 | 213.4085 | ×3.62 | 3.4327 | ×1.28 | 62.2 veces | 6.0 % | 41.2 % |
| 40,000 | 10,000 | 904.3484 | ×4.24 | 8.6189 | ×2.51 | 104.9 veces | 12.8 % | 33.7 % |
| 80,000 | 20,000 | 3844.8921 | ×4.25 | 21.4679 | ×2.49 | 179.1 veces | 4.0 % | 18.6 % |

**Controles** (mismos tamaños):

| n | Hash por perfil (ms) | × | Árbol sin filtro (ms) | × |
|---|---|---|---|---|
| 10,000 | 13.9607 | — | 5.7039 | — |
| 20,000 | 56.5062 | ×4.05 | 18.9792 | ×3.33 |
| 40,000 | 274.9573 | ×4.87 | 75.3228 | ×3.97 |
| 80,000 | 1093.1592 | ×3.98 | 317.8673 | ×4.22 |

**Tamaños reales de mi caso** (no se duplican, no llevan factor; tiempos de microsegundos):

| n | Puestos | Lista (ms) | Hash por perfil (ms) | Árbol sin filtro (ms) | Árbol por perfil (ms) |
|---|---|---|---|---|---|
| 20 | 5 | 0.0118 | 0.0148 | 0.0183 | 0.0130 |
| 100 | 25 | 0.0363 | 0.0226 | 0.0244 | 0.0169 |

**Puestos cubiertos** (`cobertura.csv`; las cuatro estructuras dan la misma asignación):

| n | Puestos | Cubiertos | Sin cubrir |
|---|---|---|---|
| 20 | 5 | 0 | 5 |
| 100 | 25 | 10 | 15 |
| 10,000 | 2,500 | 2,497 | 3 |
| 20,000 | 5,000 | 4,992 | 8 |
| 40,000 | 10,000 | 10,000 | 0 |
| 80,000 | 20,000 | 20,000 | 0 |

Con n = 20 y 100 hay pocos trabajadores por perfil (~1.4 y ~7) y casi nadie cumple los umbrales, así que esos tiempos miden sobre todo búsquedas sin resultado (casi sin borrados). Con n ≥ 10,000 casi todos los puestos se cubren.

## 5. Conclusión

1. **Cada una se comporta como su O, con una salvedad.** La lista creció ×3.62, ×4.24 y ×4.25 al duplicar n: lo esperado para O(p·n) con p proporcional a n (×4). El árbol por perfil creció ×2.51 y ×2.49: claramente casi lineal y no cuadrático, pero algo por encima del ×2 a ×2.2 que esperaba para O(n log n); lo más probable es que influyan la memoria y el recolector de basura al tener más nodos, aunque no lo verifiqué. El primer factor del árbol (×1.28) no es confiable: el promedio de n = 10,000 quedó inflado por una corrida atípica (mínimo 1.38 ms, máximo 4.82 ms). Con la mediana de las tres corridas en lugar del promedio, los factores del árbol serían ×1.79, ×2.82 y ×2.16: oscilan demasiado para dar una cifra exacta, pero el último queda dentro de lo esperado.
2. **La diferencia crece con n.** Con n = 80,000 la lista tardó 3,844.9 ms (el peor tiempo medido, ~3.8 s) y el árbol 21.5 ms, es decir 179 veces menos; el cociente fue 21.9, 62.2, 104.9 y 179.1 en las cuatro filas, porque una crece ×4 y la otra ~×2.5 por cada duplicación. Los controles confirman la causa: el árbol sin filtro también creció ×4 (317.9 ms con 80,000; el filtro de habilitados lo hace 15 veces más rápido) y el hash por perfil, aunque unas 3.5 veces más rápido que la lista, también es cuadrático (×4).
3. **Ruido.** Las tres corridas de una misma celda no coinciden: la dispersión fue de 128 % en el árbol con n = 10,000 y de 41 % con n = 20,000, contra 4 % a 13 % en la lista; los tiempos del árbol con pocos milisegundos son los más ruidosos. Con n = 20 y 100 los tiempos son de 0.01 a 0.04 ms con dispersiones de 16 % a 114 %: con n = 20 la lista (0.0118) y el árbol (0.0130) están dentro del ruido (empate); con n = 100 los rangos de las tres corridas no se solapan (lista 0.0313-0.0438 ms, árbol 0.0156-0.0188 ms) y el árbol rinde ~2 veces más, pero son ~20 microsegundos.
4. **Para mi caso.** Con 20 a 100 trabajadores la estructura es irrelevante en la práctica (microsegundos); la lista sirve y es más simple. Además, con mi generador sintético a ese tamaño casi no se cubren puestos (0 de 5 y 10 de 25), así que esa comparación es de búsquedas con poco trabajo y no representa bien la planificación completa. El árbol por perfil compensa claramente desde miles de trabajadores, donde la lista ya cuesta segundos. Esto no cambia el cuello de botella del proyecto: el húngaro sigue siendo O(n³).

# Resultados ontenidos en consola:

### Tamaños que se duplican (promedio de 3 corridas en ms; × = veces que tarda más que la fila anterior)

| n | Puestos | Lista (ms) | × | Árbol por perfil (ms) | × | Lista / Árbol | Dispersión Lista | Dispersión Árbol |
|---|---|---|---|---|---|---|---|---|
| 10,000 | 2,500 | 52.6023 | — | 2.7644 | — | 19.0 veces | 2.1 % | 146.0 % |
| 20,000 | 5,000 | 212.5401 | ×4.04 | 3.0562 | ×1.11 | 69.5 veces | 1.6 % | 10.8 % |
| 40,000 | 10,000 | 935.0599 | ×4.40 | 10.0457 | ×3.29 | 93.1 veces | 8.2 % | 60.5 % |
| 80,000 | 20,000 | 4080.0336 | ×4.36 | 26.0226 | ×2.59 | 156.8 veces | 29.4 % | 11.2 % |

### Controles (mismos tamaños)

| n | Hash por perfil (ms) | × | Árbol sin filtro (ms) | × |
|---|---|---|---|---|
| 10,000 | 13.8221 | — | 5.0619 | — |
| 20,000 | 55.9346 | ×4.05 | 20.0798 | ×3.97 |
| 40,000 | 244.6961 | ×4.37 | 91.6823 | ×4.57 |
| 80,000 | 964.4747 | ×3.94 | 312.7177 | ×3.41 |

### Tamaños reales de mi caso (no se duplican, no llevan factor)

| n | Puestos | Lista (ms) | Hash por perfil (ms) | Árbol sin filtro (ms) | Árbol por perfil (ms) |
|---|---|---|---|---|---|
| 20 | 5 | 0.0155 | 0.0240 | 0.0217 | 0.0156 |
| 100 | 25 | 0.0412 | 0.0299 | 0.0221 | 0.0130 |