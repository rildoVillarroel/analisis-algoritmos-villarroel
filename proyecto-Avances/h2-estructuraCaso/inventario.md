# Pablo Rildo Villarroel Fernández

# inventario.md — Los datos de mi caso y las operaciones que hago

> Proyecto: asignación óptima de personal técnico a proyectos eléctricos

## 1. Mis datos

**Que se guardo en el inventario:** el registro del personal técnico. Cada trabajador tiene:

| Campo | Ejemplo | Para qué sirve |
|---|---|---|
| Id | T07 | Clave única; mis trabajadores llegan con el Id ya en orden (T01, T02, ...) |
| Perfil principal | Especialista MT, Liniero MT, Instalador BT, Calidad, ... (14 perfiles) | Un puesto solo lo cubre alguien de su perfil |
| Nivel (1-5) | 4 | Restricción: nivel mínimo del puesto |
| Experiencia (años) | 8 | Restricción: experiencia mínima del puesto |
| Carga actual (h/sem) | 16 | Equilibrio de carga; horas libres = 40 − carga |

**Tamaños:**

- **Hoy:** 20 trabajadores y 20 puestos repartidos en 5 proyectos simultáneos (línea MT rural, hospital, centro de eventos, línea de envasado, celdas MT).
- **Peor mes (hipótesis de trabajo, no medición):** 100 trabajadores y 100 puestos (25 proyectos simultáneos).
- **En la medición** uso inventarios de 20 y 100 (los reales) y de 10.000 a 80.000 trabajadores (de estrés, que se duplican), con **n/4 puestos** por corrida. Elegí n/4 para que la mayoría de los puestos se pueda cubrir al escalar; con n puestos y perfiles al azar, muchos quedarían vacíos y la comparación sería menos clara. Es una decisión de diseño del experimento, distinta del caso 20/20 de la clase 6. Ojo: con n = 20 y 100 mi generador casi no cubre puestos (0 de 5 y 10 de 25, según `cobertura.csv`) porque hay pocos trabajadores por perfil; con n ≥ 40,000 se cubren todos.

**Como llegan los datos:** sin orden por carga ni por experiencia; sí con el Id ya ordenado. La carga de todos cambia cada semana.

## 2. Qué hago con el inventario (por planificación)

En cada planificación aplico la versión voraz del proyecto: para cada puesto, busco a quienes lo cumplen (perfil, nivel mínimo, experiencia mínima, horas libres), elijo al de **menor carga** y lo retiro del inventario.

| Operación | Dónde aparece en mi proyecto | Veces por planificación (p = puestos) | ¿Cuánto? |
|---|---|---|---|
| **Un rango** (candidatos por umbral) | Quienes tienen horas libres suficientes ⇔ carga ≤ 40 − horas del puesto, dentro del perfil | p (hoy 20; peor mes 100) | **Muy frecuente** |
| **El menor y el siguiente** | El de menor carga entre los candidatos; si no cumple nivel o experiencia, el siguiente | p veces el menor y unos 2-3 «siguientes» por puesto (estimado con una simulación propia, la medición lo confirma) | **La más frecuente** |
| **Borrar** | El trabajador asignado sale del inventario | ≈ p (uno por puesto cubierto) | **Muy frecuente** |
| **Buscar por clave** | Cargar la ficha de un Id y actualizar su carga tras asignarlo | ≈ n (hoy 20; peor mes 100), más consultas sueltas | Frecuente |
| **Insertar** | Alta de un trabajador; armar el inventario al iniciar la corrida (n inserciones) | Altas: pocas por semana (hipótesis); armar: 1 vez por corrida | Poco frecuente |
| **Recorrer en orden** | Reporte del personal por carga o por perfil | 1 por corrida | Rara |

**Las que más hago:** el menor/siguiente, el rango y el borrar (cada una del orden de p veces por planificación, y p crece con el tamaño del inventario). Buscar por clave es frecuente pero barata con un hash.

## 3. Que se ajusta despues de la clase 9:

- **Antes:** pensaba que lo principal era buscar un trabajador por su clave. Con la clase 9 vi que eso es solo una parte: casi todo lo que hago pide **orden** (el de menor carga) y **rangos** (umbral de horas libres). Una tabla hash no sirve para eso: tendría que revisar todos los trabajadores en cada puesto.
- **Los Id llegan ordenados:** un árbol de búsqueda simple se degeneraría en una lista (armarlo costaría ~n²/2). Necesito un árbol **balanceado** (en C#, `SortedSet`, que es rojinegro).
- **Competencia mínima como restricción dura:** ningún puesto del proyecto exige menos de nivel 3 ni de 2 años de experiencia. Eso me permite indexar solo al personal habilitado y evitar que los demás se acumulen al principio del árbol (ver `estructura.md`).
