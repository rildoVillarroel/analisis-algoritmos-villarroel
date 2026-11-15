Proyecto: HITO 1 – ALGORITMO DE ASIGNACION OPTIMA PERSONAL DE EMPRESA

1. Planteamiento

Las empresas en la que trabajo son empresa de montaje de proyectos industriales con mayor enfoque al proyectos del sector electrico, esta empresa casi normalmente manejan de 3 a 5 proyectos casi simultaneos, los cuales demandan personal necesario para el cumplimiento de cada uno de sus proyectos con eficiencia y eficacia.
Por tanto, el problema del proceso lento planteado tratara de la asignación de personal técnico a varios proyectos eléctricos que se ejecutan casi simultáneamente, cada uno con diferentes especialidades, responsabilidades, exigencias y cargas de trabajo. Algo muy impportante es que no se trata simplemente de asignar al trabajador que tenga más experiencia. Hay que encontrar una distribución que considere conjuntamente:
• La competencia técnica de cada persona.
• Su experiencia en proyectos similares.
• Sus certificaciones y habilitaciones.
• Su disponibilidad de horas.
• Su carga de trabajo actual.
• El nivel de especialización que exige cada puesto.
La propuesta es enfocar el proyecto como un problema de optimización de asignación de recursos humanos técnicos en diferentes proyectos eléctricos de ejecucion simultanea.

Un título propuesto seria:
Sistema de asignación óptima de personal técnico a proyectos eléctricos mediante algoritmos de optimización.

Ampliando definicion de modelo
Se tiene identificado dos conjuntos principales:
• Trabajadores disponibles.
• Puestos que necesitan ser cubiertos en los proyectos.

Para un escenario cuasi actual:
Elemento                        Cantidad
Proyectos eléctricos                5
Trabajadores disponibles            20
Puestos requeridos                  20
Asignaciones esperadas              20

Los proyectos actuales que se maneja son:
1. Tendido de línea de media tensión rural.
2. Instalación eléctrica de hospital de segundo nivel.
3. Instalación eléctrica de centro de eventos y coliseo.
4. Montaje electromecánico de línea de envasado.
5. Mantenimiento de celdas de media tensión.

Cada proyecto tiene puestos específicos. Por ejemplo, el proyecto de media tensión rural puede requerir:
• Un responsable de proyecto.
• Un especialista en media tensión.
• Dos linieros.
• Un técnico de seguridad industrial.
El sistema debe determinar qué trabajador es más adecuado para cada puesto.

¿Qué significa óptimo?
No significa simplemente seleccionar a los trabajadores con mayor experiencia.
Significa encontrar una combinación de asignaciones que:
• Cumpla los requisitos técnicos.
• Respete las certificaciones obligatorias.
• No exceda la disponibilidad de horas.
• Evite sobrecargas innecesarias.
• Minimice el costo global de asignación.

Un trabajador puede tener varias habilidades
No debemos representar a una persona únicamente con un perfil principal.
Por ejemplo:
Trabajador 2 tiene las siguientes caracteristicas:
Especialidad                Nivel               Experiencia
Media tensión               5                   10 años
Protecciones eléctricas     4                   6 años
Mantenimiento de celdas     4                   7 años
Instalaciones BT            2                   2 años
Capacidad semanal: 40 horas.
Carga comprometida: 16 horas.
Disponibilidad restante: 24 horas.
Se observa que puede ser candidato para distintos puestos, aunque no necesariamente tendrá la misma puntuación para todos.
Por ello es necesario separar las habilidades de los trabajadores en una tabla independiente.

Tabla TRABAJADORES
id          nombre        capacidad_horas carga_actual
T01         Técnico 01          40              10
T02         Técnico 02          40              16
T03         Técnico 03          40              8

Tabla HABILIDADES
trabajador         especialidad         nivel       experiencia
T02                Media tensión        5               10
T02                Protecciones         4               6
T02             Mantenimiento de celdas 4               7
T02                Instalaciones BT     2               2

También tendremos una tabla de certificaciones:
trabajador                      certificación
T02                             Trabajo en instalaciones MT
T02                             Seguridad eléctrica

H1 - Descripcion del proceso a optimizar
Por tanto esta es una representación más cercana a lo que sucede en la empresa eléctrica, una descripcion del proceso lento real seria:
El proceso seleccionado es la asignacion de personal tecnico a proyectos de instalaciones electricas y electromecanicas
1. Este proceso se realiza durante la planificacion y ejecucion de varios proyectos que pueden desarrollarse casi simultaneamente.
2. Cada proyecto requiere diferentes cantidades y perfiles de personal segun su alcance y complejidad.
3. Entre los proyectos considerados se encuentran instalaciones hospitalarias, lineas de media tension, edificaciones y sistemas de automatizacion industrial.
4. La asignacion comprende responsables de proyecto, supervisores, especialistas y trabajadores tecnicos.
5. Actualmente, la distribucion del personal puede realizarse mediante revision manual de hojas de vida, experiencia y disponibilidad.
6. El responsable de planificacion debe identificar que trabajadores cumplen los requisitos de cada puesto.
7. Tambien debe verificar las especialidades, certificaciones, experiencia y disponibilidad horaria.
8. Otro factor importante es evitar que determinados trabajadores concentren demasiadas responsabilidades.
9. Este procedimiento puede resultar lento cuando existen varios proyectos y numerosos candidatos disponibles.
10. La situacion afecta principalmente a responsables de proyectos, jefes tecnicos y encargados de asignar recursos humanos.
11. Una asignacion inadecuada puede provocar desequilibrios de carga laboral, retrasos y puestos cubiertos por personal poco adecuado.
12. Para el caso de estudio se consideran cinco proyectos electricos y electromecanicos simultaneos.
13. Se plantea una base inicial de 20 trabajadores con diferentes especialidades, niveles de competencia y experiencia.
14. Los cinco proyectos generan aproximadamente 20 puestos de trabajo que requieren perfiles tecnicos especificos.
15. La evaluacion de cada candidato debe considerar competencia tecnica, experiencia, disponibilidad y carga laboral.
16. Como estimacion inicial del escenario de estudio, se considera que una asignacion manual puede requerir entre 6 y 8 horas de revision y coordinacion.
17. Este tiempo es una hipotesis de trabajo y debera contrastarse mediante entrevistas o mediciones cuando exista acceso a una organizacion.
18. Se propone desarrollar una aplicacion que construya una matriz de costos entre trabajadores y puestos requeridos.
19. La aplicacion comparara los resultados de los algoritmos de fuerza bruta, voraz y Húngaro en los escenarios que correspondan.
20. Se mediran los tiempos de ejecucion, el costo total de las asignaciones y el cumplimiento de las restricciones.
21. El objetivo es reducir el esfuerzo de planificacion y obtener una distribucion de personal mas equilibrada y adecuada a los requisitos tecnicos.

Conclusion preliminar
Por todo lo expuesto, puede notarse que una mala o deficiente asignacion de recursos humanos en cada proyecto, retrasaria este y todo su coste asociado, teniendo como impacto directo en cada uno de los responsables de cada proyecto asi como de gerencia de la empresa y por ende en ganancia de la empresa.