Pablo Rildo Villarroel Fernandez

# I1 — Análisis y elección

## a) Costo de CompradorIngenuo
Hay `n` útiles y cada uno se compra o no se compra; existen `2^n` subconjuntos. El código recorre los `n` útiles para calcular costo y utilidad de cada máscara. Por ello, el tiempo es **O(n·2^n)**. La lista final requiere O(n) memoria auxiliar.

## b) Recurrencia y teorema maestro
La recurrencia `T(n)=2T(n-1)+1` no tiene la forma clásica `T(n)=aT(n/b)+f(n)`: el tamaño se reduce restando uno, no dividiéndose por una constante. Al expandir:
`T(n)=2^nT(0)+(2^n-1)`, así que `T(n)=O(2^n)`. En el código concreto, además se recorre la lista de n útiles para evaluar cada subconjunto, dando O(n·2^n).

## c) Voraz por utilidad por Bs
No garantiza el óptimo. En el primer caso:

| Útil | Costo | Utilidad | Utilidad/costo |
|---|---:|---:|---:|
| U1 | 20 Bs | 120 | 6 |
| U2 | 40 Bs | 200 | 5 |
| U3 | 60 Bs | 240 | 4 |

Con presupuesto 100 Bs, el voraz elige U1 y U2: gasta 60 Bs y obtiene 320 puntos; U3 ya no cabe. La solución óptima es U2+U3: cuesta 100 Bs y suma 440 puntos.

**Estrategia elegida: programación dinámica 0/1.** Defino `dp[i,b]` como la máxima utilidad con los primeros `i` útiles y presupuesto disponible `b`. Para el útil i comparo no comprarlo (`dp[i-1,b]`) con comprarlo (`dp[i-1,b-costo]+utilidad`) si cabe. La transición es:

`dp[i,b] = max(dp[i-1,b], dp[i-1,b-c[i]] + u[i])` si `c[i] <= b`; en caso contrario se copia `dp[i-1,b]`.

Se reutilizan soluciones óptimas de subproblemas y cada útil se considera una sola vez. Llenar la tabla cuesta **O(n·B)**, reconstruir los códigos O(n) y la tabla ocupa O(n·B) memoria. B es el presupuesto entero.

## d) Consulta de pertenencia
Mantengo los códigos elegidos en un `HashSet<string>`. `Contains` cuesta O(1) promedio. `List.Contains` cuesta O(k) en el peor caso, siendo k la cantidad de útiles elegidos. La lista se devuelve por contrato y el conjunto sirve como índice para las consultas repetidas.

## Límite del enfoque
O(n·B) es pseudopolinómico: depende del valor numérico del presupuesto. Con B=10.000 es manejable; con B=1.000.000.000 la tabla sería inviable. Para ese caso se necesitaría otra formulación, como DP por suma de utilidades si esta es pequeña, aproximación o programación lineal entera.
