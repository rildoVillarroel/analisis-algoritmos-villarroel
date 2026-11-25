Pablo Rildo Villarroel Fernandez

# I3 — Medición

## Protocolo
El programa ejecuta una corrida de calentamiento y tres corridas medidas por cada tamaño. Genera los mismos datos deterministas para ambos algoritmos, verifica que las utilidades coincidan y calcula el promedio. Se cronometra `Elegir`; la generación del lote queda fuera del cronómetro. Tamaños predeterminados: 18, 20, 22 y 24 útiles. 

## Tabla
Completar con `integradora_resumen.csv`, generado al ejecutar:

| n | Ingenua promedio (ms) | Factor vs. fila anterior | Mejorada promedio (ms) | Factor vs. fila anterior | Utilidad igual |
|---:|---:|---:|---:|---:|:---:|
| 18 | 14.1879 | | 1.1823 | | 637 | true
| 20 | 53.0375 | 3.738 | 0.0868 | 0.073 | 517 | true
| 22 | 225.5117 | 4.252 | 0.0780 | 0.898 | 634 | true
| 24 | 904.0590 | 4.009 | 0.0686 | 0.879 | 705 | true

| 18 |14.1879| — | Pendiente | — | — |
| 20 | Pendiente | Pendiente | Pendiente | Pendiente | — |
| 22 | Pendiente | Pendiente | Pendiente | Pendiente | — |
| 24 | Pendiente | Pendiente | Pendiente | Pendiente | — |

Factor = promedio actual / promedio de la fila anterior. La ingenua debería crecer aproximadamente por un factor cercano a cuatro al sumar dos útiles, porque duplica dos veces la cantidad de subconjuntos. El factor medido puede diferir por el costo de recorrer cada subconjunto y el ruido del equipo.

## 1.000 útiles y presupuesto 10.000 Bs
El programa mide la solución mejorada con calentamiento y tres corridas, y escribe `integradora_1000.csv`. Anotar aquí el promedio real que imprima el equipo: **pendiente de ejecutar**. La tabla tiene 1.001 × 10.001 = 10.011.001 enteros, aproximadamente 40 MB de datos.

## Presupuesto de 1.000.000.000 Bs
La tabla tendría (n+1) × 1.000.000.001 celdas. Su memoria y tiempo serían impracticables. Esto confirma que O(n·B) es pseudopolinómico, no polinómico respecto al tamaño de la representación binaria de B. Se debe cambiar de estrategia, por ejemplo DP indexada por utilidad si la suma de utilidades es pequeña, aproximación o un modelo de optimización entera.

## Conclusión
Completar tras medir: la ingenua crece exponencialmente y la mejorada depende de n·B. Los tiempos varían por carga del sistema operativo, procesos de fondo, caché, calentamiento del procesador y modo Release/Debug. Por eso se informa el promedio de tres corridas y no se presentan tiempos no medidos.
