# Pablo Rildo Villarroel Fernandez

# P1.2 — Curar y medir

## 1. Cura

La función `HayComprobantesRepetidosCurado` utiliza `HashSet<int>` para registrar los valores ya vistos. Devuelve `true` cuando encuentra uno que ya estaba registrado y `false` si termina el recorrido.

- **Tiempo esperado:** `O(n)`.
- **Espacio auxiliar:** `O(n)`.
- **No modifica** el arreglo recibido.
- Los cinco casos de prueba se verificaron antes de medir.

## 2. Protocolo

Medición realizada en configuración Release/equivalente. Para cada tamaño y cada función se ejecutó una corrida de calentamiento y tres corridas medidas. Se informa el promedio. El peor caso utilizado es una lista sin repetidos.

## 3. Tabla de resultados

| n | Por pares promedio (ms) | Veces vs. fila anterior | Curado promedio (ms) | Veces vs. fila anterior |
|---:|---:|---:|---:|---:|
| 8000 | 29,019 | — | 1,178 | — |
| 16000 | 118,357 | 4.08 | 0,489 | 0.42 |
| 32000 | 465,359 | 3.93 | 1,034 | 2.11 |
| 64000 | 1845,947 | 3.97 | 2,762 | 2.67 |

## 4. Conclusión

El peor tiempo promedio medido para la función por pares fue **1845,947 ms**, con **n = 64.000**.

La función por pares tiene costo teórico `O(n²)`: al duplicar el tamaño se espera un cociente cercano a 4. La cura tiene costo esperado `O(n)`: se espera un cociente cercano a 2. Las diferencias respecto de esos valores pueden deberse a JIT, caché, recolección de basura, planificación del sistema operativo y resolución del cronómetro. Se deben interpretar tendencias, no exigir cocientes exactos.

Los tiempos corresponden al equipo donde se ejecutó este programa; pueden variar en otro equipo.
