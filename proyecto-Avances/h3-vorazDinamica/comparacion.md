# H3 — Comparación: fuerza bruta frente a programación dinámica

## 1. Problema experimental

Se comparan dos algoritmos para asignar `n` trabajadores a `n` puestos distintos, maximizando la suma de puntuaciones sintéticas trabajador–puesto:

1. **Fuerza bruta:** enumera todas las permutaciones de puestos y conserva el mayor puntaje.
2. **Programación dinámica:** tabulación con máscara de bits; guarda el mejor puntaje para cada conjunto de puestos ocupados.

Ambos reciben exactamente la misma matriz de puntuaciones. El programa comprueba que devuelvan el mismo óptimo.

## 2. Datos y tamaños

Los puntajes son deterministas y sintéticos; no son datos reales. Se usan cuatro tamaños: 7, 8, 9 y 10. Se aumenta de uno en uno porque la fuerza bruta crece factorialmente y se quiere mantener el experimento razonable en una computadora personal.

| n | Permutaciones n! |
|---:|---:|
| 7 | 5.040 |
| 8 | 40.320 |
| 9 | 362.880 |
| 10 | 3.628.800 |

## 3. Protocolo

Para cada tamaño y algoritmo:
1. Una corrida de calentamiento, no contabilizada.
2. Tres corridas medidas.
3. Promedio de las tres.
4. Registro del tiempo y puntaje óptimo en CSV.

Ejecutar en modo Release:

```bash
dotnet run -c Release
```

## 4. Tabla de resultados

Completar con los valores reales de `h3_resumen.csv`. No inventar tiempos: dependen del equipo, carga, caché y ruido de medición.

| n | Fuerza bruta promedio (ms) | Factor vs. anterior | Dinámica promedio (ms) | Factor vs. anterior | Óptimo coincidente |
|---:|---:|---:|---:|---:|:---:|
| 7 | Pendiente | — | Pendiente | — | Sí/No |
| 8 | Pendiente | Pendiente | Pendiente | Pendiente | Sí/No |
| 9 | Pendiente | Pendiente | Pendiente | Pendiente | Sí/No |
| 10 | Pendiente | Pendiente | Pendiente | Pendiente | Sí/No |

`Factor(n) = Tiempo(n) / Tiempo(n anterior)`.

## 5. CSV

- `h3_detalle.csv`: algoritmo, tamaño, repetición, tiempo en ms y puntaje óptimo.
- `h3_resumen.csv`: promedio de tres corridas y factor respecto al tamaño anterior.

## 6. Conclusión para completar tras medir

Fuerza bruta debe crecer factorialmente porque enumera las `n!` asignaciones posibles. La programación dinámica evita recalcular estados y tiene costo O(n · 2^n), por lo que se espera que crezca más lentamente en estos tamaños. Si ambos algoritmos reportan el mismo puntaje, la prueba confirma que la dinámica conserva el óptimo para los casos medidos. Los factores no serán exactos: en tamaños pequeños el ruido del reloj, el calentamiento, la caché y otras tareas del equipo pueden alterar las proporciones; se debe interpretar la tendencia y no una sola medición.
