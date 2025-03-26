# Estandar de codificación C#
Se usará el estandar de [C# de google](https://google.github.io/styleguide/csharp-style.html).
Se especifican a continuación puntos a sobreescribir o que faltan en el estandar anteriormente mencionado.

## Reglas de nombrado
- Se permite usar nombres de variables de una sola letra dentro de ciclos si el proposito de la variable es totalmente claro.

## Manejo de excepciones
- Se usará try/catch, usandose como la sentencia if/else, agregando el catch y finally separado con un espacio de la llave.
- El nombre de la excepción atrapada debe ser "err".
- Deben usarse los tipos de excepción especificos siempre que sea posible.

Ejemplo:
´´´c#
public void ThrowError() {
  try {
    int i = 1/0;
  } catch (DivideByZeroException err) {
    \_logger.err(err);
  }
}
´´´
