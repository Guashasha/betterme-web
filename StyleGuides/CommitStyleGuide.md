# Estándar de commit
## Objetivo
Para mantener la consistencia en la historia de commits y facilitar la búsqueda y entendimiento de cada commit se establece el actual estándar, el cual deberá ser seguido por todos los miembros del equipo y durante todas las fases en las que se use control de versiones.

## Estructura
Los commits deberán ser escritos en inglés para mantener consistencia con la codificación. Cada commit deberá ser de un solo grupo de cambios, es decir que todos los cambios agregados tengan que ver entre sí, también debe contener un tipo, un resumen y una descripción, en ese orden, y deben específicos de en qué parte de que artefacto o documento se hicieron los cambios.
Para agregar un resumen y una descripción a un commit se usa el comando: 
git commit -m "Resumen" -m "descripción"

### Tipos de commit
El tipo será agregado en el resumen y dependerá de que se haya modificado, se incluirán los siguientes tipos y se podrán añadir más según sea necesario:
- Feat - Funciones nuevas (feature)
- Fix - Arreglos de bugs (bug fixes)
- Docs - Cambios a documentación
- Style - Cambios de estilo, formato o nombres
- Perf - Mejoras del rendimiento
- Test - Pruebas a una función

### Resumen
Deberá ser corto pero abarcar lo que se hizo en los cambios a grandes rasgos.
#### Ejemplos:
- Feat: Agregada y configurada librería xyz en el servidor
- Docs: Arreglados errores de dedo en estándar de codificación
- Feat: Implementado CU-01 en el cliente
- Test: Implementadas pruebas del CU-20

### Descripción
Deberá contener los detalles de las acciones realizadas en los cambios y de ser posible y necesario, el nombre de la actividad a la que pertenece.
#### Ejemplos
- Agregada librería xyz a config.yaml y creada su configuración en el archivo confs/xyz.conf para la tarea “Instalar xyz en el cliente”
- Corregidos errores de dedo en la sección “Nombrado” del estándar de codificación para la tarea “Arreglar estándar”
- Implementadas funciones necesarias para el CU01 en el archivo “operaciones.js” del cliente y creadas las clases Calculadora y TipoOperacion

