# Práctica C4: búsqueda lineal contra búsqueda binaria

## Relación con el proyecto

El caso de estudio considera 20 trabajadores técnicos identificados como T01 a T20 y 20 puestos distribuidos en cinco proyectos eléctricos. Esta práctica compara la búsqueda de un trabajador por su identificador único.

Los registros y perfiles son los datos sintéticos definidos para el caso de estudio académico; no representan trabajadores reales.

## Algoritmos

- **Búsqueda lineal:** revisa los registros desde el primero hasta encontrar el identificador o terminar el arreglo. Complejidad: O(n).
- **Búsqueda binaria:** compara con el elemento central y descarta la mitad del intervalo en cada paso. Complejidad: O(log n). Requiere que los registros estén ordenados por la misma clave utilizada para buscar.

## Protocolo de medición

1. Se preparan 20 registros ordenados por identificador.
2. Se generan 1.000.000 de consultas reproducibles y se usan las mismas claves para ambos algoritmos.
3. Se ejecuta una corrida de calentamiento que no se incluye en el promedio.
4. Se realizan tres corridas medidas por algoritmo.
5. Se calcula el promedio de cada algoritmo y el cociente `promedio lineal / promedio binaria`.

Los tiempos deben obtenerse ejecutando el programa en el equipo del estudiante. No se deben copiar tiempos de otra computadora.

## Ejecución

Desde la terminal de VS Code, dentro de esta carpeta:

```bash
dotnet run -c Release
```

## Tabla de resultados

Copiar los valores reales que aparezcan en la consola:

| Algoritmo | Tiempo promedio (ms) |
|---|---:|
| Búsqueda lineal | completar |
| Búsqueda binaria | completar |
| Cociente lineal / binaria | completar |

## Análisis: ¿conviene ordenar primero en nuestro proceso?

En nuestro caso inicial hay solamente 20 trabajadores y 20 puestos; por ello, ordenar los registros únicamente para realizar unas pocas búsquedas no compensa necesariamente el costo adicional, y una búsqueda lineal puede ser suficiente por su simplicidad.

Si el sistema crece y debe realizar muchas consultas, conviene mantener una estructura ordenada por identificador o un índice actualizado, ordenar una sola vez al cargar o modificar los datos y reutilizarla. Para localizar candidatos por especialidad se necesitarían índices adecuados por especialidad; ordenar solamente por identificador no resuelve directamente esa consulta.

## Alcance de la práctica

Esta comparación estudia la localización de trabajadores por identificador. No constituye todavía el algoritmo de asignación óptima de trabajadores a puestos; esa parte se desarrollará posteriormente con los algoritmos de optimización del proyecto.
