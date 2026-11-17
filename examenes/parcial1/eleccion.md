Pablo Rildo Villarroel Fernandez

# P1.3 — Elegir

## Datos del problema

- Cantidad de códigos: `n = 250.000`.
- Consultas diarias: `q = 8.000`.
- El listado llega desordenado.
- La memoria es limitada y no se puede duplicar el listado.

Para comparar, se considera el peor caso de una búsqueda lineal (el código no está o aparece al final), una ordenación eficiente de orden `n log₂ n` y una búsqueda binaria de orden `log₂ n`. Los valores son aproximaciones de operaciones/comparaciones, no tiempos de reloj.

## 1. Opción 1: recorrer el listado en cada consulta

Cada consulta puede revisar los 250.000 códigos:

\[
q\cdot n=8.000\cdot250.000=2.000.000.000
\]

**Costo aproximado total: 2.000 millones de comparaciones.**

No requiere una estructura auxiliar importante, pero repite el recorrido completo para cada consulta.

## 2. Opción 2: ordenar una vez con un algoritmo n log n y buscar en binario

Tomando `log₂(250.000) ≈ 17,93`:

Ordenación aproximada:

\[
n\log_2 n=250.000\cdot17,93\approx4.482.500
\]

Búsquedas binarias:

\[
q\log_2 n=8.000\cdot17,93\approx143.440
\]

Total aproximado:

\[
4.482.500+143.440=4.625.940
\]

**Costo aproximado total: 4,63 millones de operaciones/comparaciones**, usando el modelo simplificado `n log₂ n` para ordenar. Una implementación real puede tener constantes diferentes.

Para respetar la restricción de memoria, se puede ordenar el propio arreglo/listado **en el lugar** (in-place), utilizando un algoritmo de ordenación eficiente como heapsort o introsort, en vez de crear una segunda copia completa. Después, las 8.000 consultas se resuelven con búsqueda binaria.

## 3. Opción 3: ordenar con un algoritmo cuadrático y buscar en binario

Una ordenación cuadrática requiere del orden de:

\[
n^2=250.000^2=62.500.000.000
\]

Si se usa como referencia el número de comparaciones de selección o burbuja en el caso típico de recorrido completo:

\[
\frac{n(n-1)}{2}=\frac{250.000\cdot249.999}{2}
=31.249.875.000
\]

A esto se suman aproximadamente `143.440` comparaciones de las búsquedas binarias.

**Costo aproximado total (modelo de n²/2): 31.250.018.440 operaciones.**

Aunque luego las consultas son rápidas, el costo inicial de ordenar domina ampliamente. No es una opción razonable para este volumen.

## Comparación

| Opción | Costo aproximado total |
|---|---:|
| 1. Búsqueda lineal en cada consulta | 2.000.000.000 |
| 2. Ordenación eficiente + búsqueda binaria | 4.625.940 |
| 3. Ordenación cuadrática + búsqueda binaria | 31.250.018.440 |

## Elección

**Elijo la opción 2: ordenar una sola vez el listado original con un algoritmo `O(n log n)` in-place y realizar las 8.000 consultas mediante búsqueda binaria.**

Su costo aproximado es unas 432 veces menor que la opción 1 en este escenario (`2.000.000.000 / 4.625.940 ≈ 432`). Además, evita duplicar el listado si se utiliza una ordenación in-place. La opción 3 se descarta porque su costo cuadrático es incluso mayor que el de responder todas las consultas con búsqueda lineal.

La medición de P1.2 apoya esta decisión desde el comportamiento de los algoritmos: al crecer el tamaño, una solución cuadrática aumenta mucho más rápido que una solución lineal. En este problema, ordenar eficientemente tiene un costo inicial que se amortiza con las numerosas consultas.

## ¿A partir de cuántas consultas cambiaría de opinión?

Se compara el costo de la búsqueda lineal repetida con el costo de ordenar una vez y luego buscar en binario:

\[
q\cdot n=n\log_2 n+q\log_2 n
\]

Despejando `q`:

\[
q=\frac{n\log_2 n}{n-\log_2 n}
\]

Con `n = 250.000` y `log₂ n ≈ 17,93`:

\[
q\approx\frac{4.482.500}{250.000-17,93}
\approx18,2
\]

Por tanto, con este modelo simplificado, **a partir de aproximadamente 19 consultas** conviene ordenar y buscar en binario; con menos consultas podría convenir recorrer linealmente el listado, porque se evita pagar el costo inicial de ordenación. En la práctica, el umbral exacto depende de las constantes de la implementación, del costo real de ordenar y de la distribución de las consultas.

---
