# Pablo Rildo Villarroel Fernandez

## Seis casos: que estructura?

	a) El boton atras de un navegador
	b) Los trabajos que esperan turno en una impresora compartida
	c) Un diccionario de 2 millones de palabras con busqueda exacta
	d) El ranking de los 100 mejores jugadores entre 10 millones
	e) Un calendario que responde todos los eventos entre dos fechas
	f) Un planificador que ejecuta primero la tarea mas urgente

## ESTRUCTURA SELECCIONADA PARA CADA CASO: 

a) El botón «atrás» de un navegador
- Estructura: Pila (Stack)
- Costo: O(1) para guardar página (Push) y O(1) para volver (Pop).
- Porqué: Necesita comportamiento LIFO (última página visitada es la primera a la que regresas).

b) Los trabajos que esperan turno en una impresora compartida
- Estructura: Cola (Queue)
- Costo: O(1) para encolar (Enqueue) y O(1) para imprimir el siguiente (Dequeue)
- Porqué: Respeta estrictamente el orden de llegada (FIFO: el primer documento enviado es el primero en imprimirse)

c) Un diccionario de 2 millones de palabras con búsqueda exacta
- Estructura: Tabla Hash (HashSet / Dictionary)
- Costo: O(1) promedio para buscar e insertar
- Porqué: Solo requiere verificar existencia o buscar por clave exacta sin importar el orden alfabético ni rangos

d) El ranking de los 100 mejores jugadores entre 10 millones
- Estructura: Heap Mínimo de tamaño fijo \(k = 100\) (PriorityQueue)
- Costo: O(n log k) en total (con k = 100, cada actualización cuesta apenas log 100)
- Porqué: Permite descartar en O(1) a quienes no superan el puesto 100 y mantener el Top-100 sin ordenar los 10 millones de datos

e) Un calendario que responde «todos los eventos entre dos fechas»
- Estructura: Árbol Binario de Búsqueda Balanceado (SortedSet / SortedDictionary)
- Costo: O(log n + k) para la consulta de rango (donde k son los eventos dentro de esas fechas) y O(log n) para insertar.
- Porqué: Mantiene las fechas ordenadas para saltar rápido al inicio del rango en O(log n) y recorrer solo los \(k\) eventos siguientes.

f) Un planificador que ejecuta primero la tarea más urgente
- Estructura: Cola de Prioridad / Heap (PriorityQueue)
- Costo: O(log n) para insertar tarea, O(log n) para extraer la más urgente y O(1) para consultarla.
- Porqué: Extrae dinámicamente el elemento de mayor prioridad en cada turno sin reordenar todas las tareas pendientes.

