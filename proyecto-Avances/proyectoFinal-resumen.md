# Asignación Óptima de Personal Técnico a Proyectos Eléctricos

> **Proyecto de Análisis de Algoritmos** — Modelado matemático, estructuras de datos, optimización combinatoria y teoría de grafos aplicados a la planificación de recursos humanos en proyectos eléctricos e industriales.

---

## Resumen Ejecutivo

Este proyecto aborda el problema de distribuir de manera óptima personal técnico calificado entre múltiples proyectos eléctricos ejecutados de forma simultánea. A lo largo del desarrollo de la asignatura se analizaron distintas estrategias algorítmicas y estructuras de datos para pasar de un proceso manual lento a una solución matemática fundamentada.

* **Núcleo de optimización (v1.0):** **Algoritmo Húngaro** ($O(n^3)$) para el modelo clásico de asignación uno a uno.
* **Gestión de datos:** Estructuras combinadas (`Dictionary`, `HashSet`, `SortedSet` y `List`).
* **Modelado de relaciones:** **Grafo Bipartito** con recorridos **BFS / DFS** para validar elegibilidad y analizar conectividad.
* **Escalabilidad futura:** Si se incorporan asignaciones múltiples y acumulación de horas entre proyectos, el modelo evoluciona hacia **Programación Lineal Entera Mixta (MILP)** o **Flujo de Costo Mínimo**.

---

## Tabla de Contenidos

1. [Planteamiento del Problema Real](#1-planteamiento-del-problema-real)
2. [Hito 1: Formulación y Modelo Matemático](#2-hito-1-formulación-y-modelo-matemático)
3. [Hito 2: Estructuras de Datos](#3-hito-2-estructuras-de-datos)
4. [Hito 3: Estrategias de Optimización Combinatoria](#4-hito-3-estrategias-de-optimización-combinatoria)
5. [El Algoritmo Húngaro: Solución Definitiva](#5-el-algoritmo-húngaro-solución-definitiva)
6. [Grafos Bipartitos, BFS y DFS](#6-grafos-bipartitos-bfs-y-dfs)
7. [Arquitectura e Integración del Sistema](#7-arquitectura-e-integración-del-sistema)
8. [Flujo de Operación en Escenario Real](#8-flujo-de-operación-en-escenario-real)
9. [Métricas de Evaluación y Alcance](#9-métricas-de-evaluación-y-alcance)
10. [Conclusión y Defensa del Proyecto](#10-conclusión-y-defensa-del-proyecto)

---

## 1. Planteamiento del Problema Real

En empresas dedicadas al montaje de instalaciones eléctricas, electromecánicas y proyectos industriales, suelen ejecutarse entre **3 y 5 proyectos de manera simultánea**, cada uno con requerimientos específicos de especialización, certificaciones y disponibilidad horaria:

1. **Línea de media tensión rural:** Linieros, especialistas en media tensión, supervisores y responsables de proyecto.
2. **Hospital de segundo nivel:** Técnicos en instalaciones eléctricas, tableros, sistemas de emergencia y supervisión.
3. **Centro de eventos y coliseo:** Instaladores, técnicos de iluminación, fuerza, canalizaciones y distribución eléctrica.
4. **Línea de envasado automatizada:** Técnicos electromecánicos, automatización, control y mantenimiento.
5. **Mantenimiento de celdas de media tensión:** Especialistas en media tensión, protecciones, maniobras y mantenimiento.

### Riesgos de una asignación empírica o manual
* Personal insuficientemente calificado para tareas críticas.
* Subutilización de especialistas altamente capacitados en puestos básicos.
* Sobrecarga horaria en determinados técnicos e incumplimiento de certificaciones obligatorias.
* Retrasos en cronogramas, sobrecostos y alta carga administrativa (estimada preliminarmente entre **6 y 8 horas** de revisión manual como hipótesis de partida).

### 1.1. ¿Qué significa una asignación óptima?
No basta con elegir al mejor técnico para un puesto de forma aislada; se debe evaluar la **combinación global**.

| Trabajador | Media Tensión | Instalaciones BT | Automatización |
| :--- | :---: | :---: | :---: |
| **T01** | **95** | 60 | 30 |
| **T02** | 80 | **90** | 50 |
| **T03** | 40 | 70 | **95** |

* **Asignación óptima:** `P01 → T01 (95)`, `P02 → T02 (90)`, `P03 → T03 (95)` $\rightarrow$ **Puntuación global: $95 + 90 + 95 = 280$**.

---

## 2. Hito 1: Formulación y Modelo Matemático

### 2.1. Escenario base de experimentación (Datos sintéticos)

| Elemento del Escenario | Cantidad |
| :--- | :---: |
| Proyectos eléctricos simultáneos | 5 |
| Trabajadores disponibles ($T$) | 20 |
| Puestos requeridos ($P$) | 20 |
| Asignaciones esperadas | 20 |

### 2.2. Modelo relacional de perfiles técnicos
Un trabajador posee múltiples habilidades, niveles de dominio, certificaciones y disponibilidad horaria independiente, por lo que la información se desacopla en tres entidades:

* **Trabajadores:** `id`, `nombre`, `capacidad_horas` (ej. 40 h), `carga_actual` (ej. 16 h).
* **Habilidades:** `trabajador`, `especialidad`, `nivel` (1–5), `experiencia` (años).
* **Certificaciones:** `trabajador`, `certificacion` (ej. *Trabajo en instalaciones MT*, *Seguridad eléctrica*).

### 2.3. Función objetivo y matriz de costos
Definimos la variable binaria de decisión:

$$x_{ij} = \begin{cases} 1 & \text{si el trabajador } i \text{ ocupa el puesto } j \\ 0 & \text{en caso contrario} \end{cases}$$

El objetivo es minimizar el costo total de asignación:

$$\min Z = \sum_{i=1}^{n} \sum_{j=1}^{m} C_{ij} x_{ij}$$

Para calcular el costo $C_{ij}$, primero se estima una puntuación de compatibilidad $S_{ij} \in [0, 100]$ (ponderación referencial para el modelo):

$$S_{ij} = 0.50\,S_{\text{tec}} + 0.25\,S_{\text{exp}} + 0.15\,S_{\text{proy}} + 0.10\,S_{\text{carga}}$$

Y se convierte en costo mediante:

$$C_{ij} = 100 - S_{ij}$$

### 2.4. Restricciones del modelo clásico (1 a 1)

1. **Cobertura exacta de puestos:** $\sum_{i} x_{ij} = 1 \quad \forall j \in P$
2. **Asignación máxima por trabajador:** $\sum_{j} x_{ij} \leq 1 \quad \forall i \in T$
3. **Filtro de elegibilidad obligatoria ($E_{ij} \in \{0, 1\}$):** $x_{ij} \leq E_{ij}$ *(las certificaciones y requisitos mínimos se filtran antes de optimizar).*
4. **Naturaleza de las variables:** $x_{ij} \in \{0, 1\}$

---

## 3. Hito 2: Estructuras de Datos

Para gestionar eficientemente los perfiles antes de construir la matriz de costos, se comparó una implementación lineal (`List<Trabajador>`) frente a estructuras indexadas y ordenadas:

| Operación / Necesidad | Estructura Recomendada | Complejidad Típica | Justificación |
| :--- | :--- | :---: | :--- |
| Buscar trabajador por código | `Dictionary<string, Trabajador>` | $O(1)$ promedio | Acceso directo por clave hash (`trabajadores["T02"]`) |
| Buscar puesto por código | `Dictionary<string, Puesto>` | $O(1)$ promedio | Recuperación instantánea de requisitos |
| Evitar códigos duplicados | `HashSet<string>` | $O(1)$ promedio | Validación rápida de unicidad y certificaciones |
| Mantener candidatos ordenados | `SortedSet<Trabajador>` | $O(\log n)$ | Consultas por rango de experiencia o disponibilidad |
| Devolver colecciones filtradas | `List<T>` | $O(1)$ / $O(n)$ | Salida secuencial de reportes y asignaciones |

> **Nota metodológica:** Se diseñaron pruebas de rendimiento con tamaños sintéticos crecientes ($500$, $1.000$, $2.000$ y $4.000$ registros) con fase de calentamiento y promedios de ejecución exportables a CSV.

---

## 4. Hito 3: Estrategias de Optimización Combinatoria

### 4.1. Fuerza Bruta
Evalúa todas las permutaciones posibles ($n!$). Para $n = 10$ existen $3.628.800$ combinaciones.
* **Complejidad:** $O(n \cdot n!)$
* **Uso:** Únicamente como referencia de verificación en instancias muy pequeñas.

### 4.2. Algoritmo Voraz (Greedy)
Selecciona en cada paso la pareja trabajador-puesto de mayor compatibilidad inmediata.
* **Contraejemplo demostrado:**

| Trabajador \ Puesto | Puesto A | Puesto B |
| :--- | :---: | :---: |
| **Trabajador 1** | 10 | 9 |
| **Trabajador 2** | 9 | 1 |

* **Decisión Voraz:** Elige `T1 → Puesto A (10)` y obliga a `T2 → Puesto B (1)` = **11 puntos**.
* **Solución Óptima:** Asigna `T1 → Puesto B (9)` y `T2 → Puesto A (9)` = **18 puntos**.
* **Conclusión:** No garantiza el óptimo global.

### 4.3. Programación Dinámica con Máscara de Bits (Bitmask DP)
Representa mediante una máscara binaria `dp[mask]` el subconjunto de puestos ya asignados.
* **Complejidad temporal:** $O(n \cdot 2^n)$ | **Espacial:** $O(2^n)$
* **Limitación:** Para $n = 20$ genera $2^{20} = 1.048.576$ estados; didácticamente valioso, pero superado por métodos polinómicos especializados.

---

## 5. El Algoritmo Húngaro: Solución Definitiva

El **Algoritmo Húngaro** (método de Kuhn-Munkres) resuelve exactamente el problema de asignación de costo mínimo en tiempo polinómico.

### Funcionamiento conceptual
1. Resta el valor mínimo de cada fila a todos los elementos de esa fila.
2. Resta el valor mínimo de cada columna a todos los elementos de esa columna.
3. Cubre todos los ceros resultantes con el mínimo número de líneas horizontales y verticales.
4. Si el número de líneas es menor que $n$, ajusta la matriz restando el menor elemento no cubierto y sumándolo en las intersecciones, repitiendo hasta encontrar $n$ ceros independientes.

### Comparativa general de algoritmos

| Estrategia | Complejidad Temporal | ¿Garantiza Óptimo? | Rol en el Proyecto |
| :--- | :---: | :---: | :--- |
| **Fuerza Bruta** | $O(n \cdot n!)$ | Sí | Validación en instancias pequeñas |
| **Voraz (Greedy)** | $O(n^2 \log n)$ | No | Heurística rápida de comparación |
| **DP con Máscara** | $O(n \cdot 2^n)$ | Sí | Estudio de subproblemas en H3 |
| **Algoritmo Húngaro** | **$O(n^3)$** | **Sí** | **Motor principal de optimización (v1.0)** |

*Para nuestro escenario base ($n = 20$), el algoritmo húngaro requiere del orden de $20^3 = 8.000$ operaciones elementales.*

---

## 6. Grafos Bipartitos, BFS y DFS

El dominio se modela como un **Grafo Bipartito** $G = (T \cup P, E)$, donde las aristas $e = (t_i, p_j) \in E$ existen **únicamente si el trabajador $t_i$ es elegible para el puesto $p_j$**, con peso igual al costo $C_{ij}$.

```text
  [Trabajadores T]             [Puestos P]
       (T01) ──── costo: 5 ─────▶ (P01)
         │
         └─────── costo: 30 ────▶ (P02)
       (T02) ──── costo: 8 ─────▶ (P02)
       (T03) ──── costo: 6 ─────▶ (P03)
```

* **Representación:** Lista de adyacencia ($O(V + E)$), ideal porque el grafo de elegibilidad es disperso (en el dataset sintético: 20 trabajadores, 20 puestos y 29 aristas válidas).
* **Recorridos BFS y DFS ($O(V + E)$):** No reemplazan al algoritmo húngaro; se emplean para **pre-validación estructural**:
  * Detectar puestos aislados (grado $0$, sin candidatos elegibles).
  * Identificar componentes conexas y cuellos de botella antes de optimizar.

---

## 7. Arquitectura e Integración del Sistema

```text
┌─────────────────────────────────────────────────────────────────┐
│ 1. CAPA DE DATOS (Repositorios en Memoria)                      │
│    Dictionary<string, Trabajador>, HashSet, SortedSet           │
└───────────────────────────────┬─────────────────────────────────┘
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│ 2. VALIDACIÓN DE ELEGIBILIDAD Y GRAFO BIPARTITO                 │
│    Filtro de certificaciones/horas + BFS/DFS de conectividad    │
└───────────────────────────────┬─────────────────────────────────┘
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│ 3. CONSTRUCCIÓN DE MATRIZ DE COSTOS (n × n)                     │
│    C_ij = 100 - S_ij  (Pares no elegibles = ∞ / Penalización)   │
└───────────────────────────────┬─────────────────────────────────┘
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│ 4. MOTOR DE OPTIMIZACIÓN: ALGORITMO HÚNGARO O(n³)               │
│    Cálculo de asignación global de costo mínimo                 │
└───────────────────────────────┬─────────────────────────────────┘
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│ 5. REPORTE Y VALIDACIÓN TÉCNICA                                 │
│    Detalle de asignaciones, costo total y puestos descubiertos  │
└─────────────────────────────────────────────────────────────────┘
```

### Evolución futura: Restricciones de capacidad horaria
Si en una versión posterior un mismo trabajador puede cubrir múltiples puestos repartiendo sus horas disponibles ($D_i$), la restricción pasa a ser:

$$\sum_{j} h_{ij} x_{ij} \leq D_i$$

En ese escenario (Problema de Asignación Generalizada), el modelo deberá migrar hacia **Programación Lineal Entera Mixta (MILP)** o **Flujo de Costo Mínimo**.

---

## 8. Flujo de Operación en Escenario Real

1. **Registro de personal:** Carga de especialidades, niveles, certificaciones vigentes y disponibilidad horaria.
2. **Definición de proyectos y puestos:** Alta de los 5 proyectos eléctricos y sus vacantes técnicas.
3. **Filtrado de elegibilidad:** Descarte automático de combinaciones que incumplan certificaciones o disponibilidad mínima.
4. **Cálculo de costos:** Evaluación de compatibilidad $S_{ij}$ y generación de la matriz $C_{ij}$.
5. **Ejecución del Algoritmo Húngaro:** Obtención del emparejamiento óptimo global.
6. **Revisión por el responsable técnico:** Validación final considerando factores operativos y de seguridad.

---

## 9. Métricas de Evaluación y Alcance

### Indicadores de desempeño
* **Rendimiento algorítmico:** Tiempo de ejecución (ms), costo total obtenido, cumplimiento del 100% de restricciones y brecha frente al óptimo.
* **Impacto operativo:** Tiempo de planificación, porcentaje de cobertura de puestos, equilibrio de carga horaria y tasa de reasignaciones manuales.

### Delimitación del alcance académico vs. empresarial

| Desarrollado en el Modelo Académico | Pendiente de Validación Empresarial |
| :--- | :--- |
| Escenario sintético reproducible (20 trabajadores × 20 puestos) | Integración con base de datos real de RR. HH. |
| Ponderaciones referenciales de compatibilidad ($S_{ij}$) | Calibración estadística de pesos con expertos |
| Modelo clásico de asignación 1 a 1 | Asignaciones múltiples y horas acumuladas |
| Hipótesis base de 6–8 h de gestión manual | Medición cronometrada en entorno real |

---

## 10. Conclusión

> *"No construimos una aplicación para forzar el uso de todos los algoritmos vistos en clase; abordamos un problema real de planificación eléctrica que nos permitió analizar, comparar y seleccionar las estructuras y algoritmos óptimos para cada capa del sistema."*

El proyecto demuestra cómo transformar un problema operativo complejo en un modelo matemático riguroso, respaldado por estructuras de datos de acceso eficiente (`Dictionary`, `HashSet`), modelado relacional mediante grafos bipartitos y resolución exacta en tiempo polinómico mediante el **Algoritmo Húngaro**.