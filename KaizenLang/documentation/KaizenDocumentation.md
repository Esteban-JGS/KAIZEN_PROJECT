<!-- markdownlint-disable MD033 -->

<div style="text-align: center; padding-top: 100px;">

<table style="width: 100%; margin: 0 auto; text-align: center; border: none;">
<tr>
<td style="text-align: center; padding: 20px; border: none;">
<img src="UNA Logo2.png" alt="UNA Logo" width="100" style="margin-right: 30px;">
<img src="../Resources/icon.ico" alt="KaizenLang Logo" width="80">
</td>
</tr>
<tr>
<td style="text-align: center; padding: 40px 0 30px 0; border: none;">
<h1 style="font-size: 2.5em; margin: 0; font-weight: bold;">Lenguaje de Programación Kaizen</h1>
</td>
</tr>
<tr>
<td style="text-align: center; padding: 20px 0; font-size: 1.3em; border: none;">
<strong>Estudiantes:</strong><br>
Juan Carlos Camacho Solano<br>
Esteban Granados Sibaja
</td>
</tr>
<tr>
<td style="text-align: center; padding: 20px 0; font-size: 1.3em; border: none;">
<strong>Universidad:</strong><br>
Universidad Nacional (UNA)
</td>
</tr>
<tr>
<td style="text-align: center; padding: 20px 0; font-size: 1.3em; border: none;">
<strong>Profesor:</strong><br>
Josías Ariel Chaves Murillo
</td>
</tr>
<tr>
<td style="text-align: center; padding: 20px 0; font-size: 1.3em; border: none;">
<strong>Curso:</strong><br>
Paradigmas de Programación
</td>
</tr>
<tr>
<td style="text-align: center; padding: 20px 0; font-size: 1.3em; border: none;">
<strong>Ciclo:</strong><br>
II 2025
</td>
</tr>
</table>

</div>

<div style="page-break-after: always;"></div>

## Índice

1. [Sobre el Lenguaje](#1-sobre-el-lenguaje)
   - [Problemática Planteada](#11-problemática-planteada)
   - [Solución Propuesta](#12-solución-propuesta)

2. [Descripción del Lenguaje](#2-descripción-del-lenguaje)
   - [Filosofía del Proyecto](#21-filosofía-del-proyecto)
   - [Particularidades del Lenguaje](#22-particularidades-del-lenguaje)
     - [Anidación Profunda](#221-anidación-profunda)
     - [Delimitadores de Scope Únicos](#222-delimitadores-de-scope-únicos)
     - [Sistema de Tipos Inspirado en la Cultura Popular Japonesa](#223-sistema-de-tipos-inspirado-en-la-cultura-popular-japonesa)
     - [Convenciones Sintácticas](#224-convenciones-sintácticas)

3. [Problemáticas de Desarrollo](#3-problemáticas-de-desarrollo)
   - [Gestión de Entrada/Salida](#31-gestión-de-entradasalida)
   - [Control de Flujo y Recursión](#32-control-de-flujo-y-recursión)
   - [Manejo de Colecciones (Chainsaw y Hogyoku)](#33-manejo-de-colecciones-chainsaw-y-hogyoku)
   - [Análisis Semántico](#34-análisis-semántico)
   - [Parser y Análisis Sintáctico](#35-parser-y-análisis-sintáctico)
   - [Sistema de Funciones](#36-sistema-de-funciones)
   - [Anidación Profunda de Bloques](#37-anidación-profunda-de-bloques)
   - [Interfaz Gráfica del IDE](#38-interfaz-gráfica-del-ide)

4. [Estructura del Proyecto](#4-estructura-del-proyecto)
   - [Características Principales](#41-características-principales)
   - [IDE Integrado](#42-ide-integrado)
   - [Arquitectura del Sistema](#43-arquitectura-del-sistema)

5. [Innovaciones del Lenguaje](#5-innovaciones-del-lenguaje)
   - [Temática Cultural Japonesa](#51-temática-cultural-japonesa)
   - [Sintaxis Clara y Pedagógica](#52-sintaxis-clara-y-pedagógica)
   - [IDE Personalizado Integrado](#53-ide-personalizado-integrado)
   - [Recursión y Anidación Avanzada](#54-recursión-y-anidación-avanzada)
   - [Tema Visual Coherente](#55-tema-visual-coherente)
   - [Identidad Visual Propia](#56-identidad-visual-propia)
   - [Sistema de Tipos Estricto pero Expresivo](#57-sistema-de-tipos-estricto-pero-expresivo)
   - [Mensajes de Error Pedagógicos](#58-mensajes-de-error-pedagógicos)

6. [Guía de Uso](#6-guía-de-uso)
   - [Ejecución del IDE](#61-ejecución-del-ide)
   - [Herramientas de Desarrollo](#62-herramientas-de-desarrollo)
   - [Ejemplos de Uso del IDE](#63-ejemplos-de-uso-del-ide)
   - [Casos de Uso Comunes](#64-casos-de-uso-comunes)

7. [Referencia de Sintaxis](#7-referencia-de-sintaxis)
   - [Declaración de Variables](#71-declaración-de-variables)
   - [Estructuras de Control](#72-estructuras-de-control)
   - [Funciones](#73-funciones)
   - [Chainsaw (Arrays Unidimensionales)](#74-chainsaw-arrays-unidimensionales)
   - [Hogyoku (Matrices Bidimensionales)](#75-hogyoku-matrices-bidimensionales)
   - [Entrada y Salida](#76-entrada-y-salida)
   - [Operaciones](#77-operaciones)
   - [Comentarios](#78-comentarios)
   - [Ejemplo Completo: Programa Integrado](#79-ejemplo-completo-programa-integrado)

8. [Ejemplos Prácticos Avanzados](#8-ejemplos-prácticos-avanzados)
   - [Ejemplo 1: Búsqueda en Chainsaw](#81-ejemplo-1-búsqueda-en-chainsaw)
   - [Ejemplo 2: Suma de Matriz (Hogyoku)](#82-ejemplo-2-suma-de-matriz-hogyoku)
   - [Ejemplo 3: Números Primos](#83-ejemplo-3-números-primos)
   - [Ejemplo 4: Ordenamiento Burbuja](#84-ejemplo-4-ordenamiento-burbuja)

9. [Validación de Errores en KaizenLang](#9-validación-de-errores-en-kaizenlang)
   - [Errores Léxicos](#91-errores-léxicos)
   - [Errores Sintácticos](#92-errores-sintácticos)
   - [Errores Semánticos](#93-errores-semánticos)
   - [Errores de Runtime](#94-errores-de-runtime)

10. [Mejores Prácticas de Programación](#10-mejores-prácticas-de-programación)

11. [Conclusión](#11-conclusión)

12. [Información del Proyecto](#12-información-del-proyecto)
    - [Contexto Académico](#121-contexto-académico)
    - [Estudiantes](#122-estudiantes)
    - [Tecnologías Utilizadas](#123-tecnologías-utilizadas)

<div style="page-break-after: always;"></div>

## 1. Sobre el Lenguaje

### 1.1 Problemática Planteada

El objetivo del proyecto es desarrollar un lenguaje de programación que se distinga de los demás, con una identidad y esencia propias. Para lograr esto, se ha concebido un lenguaje basado en la temática de la cultura japonesa, incorporando elementos que reflejan los valores y conceptos de esta tradición milenaria.

Asimismo, se busca integrar la familiaridad estructural de lenguajes consolidados como C++, combinando esta base sólida con elementos innovadores que faciliten su adopción por parte de programadores de diferentes niveles de experiencia.

### 1.2 Solución Propuesta

La solución implementada consiste en crear un lenguaje con identidad propia mediante el uso de palabras reservadas y elementos sintácticos distintivos, alejándose de la convencionalidad de lenguajes como C++ para ofrecer una experiencia de programación renovada y atractiva.

El lenguaje se complementa con un Entorno de Desarrollo Integrado (IDE) personalizado que incluye múltiples ejemplos de código, documentación interactiva y herramientas de generación automática de snippets, permitiendo a los usuarios comenzar a programar de manera inmediata y eficiente.

## 2. Descripción del Lenguaje

**KaizenLang** es un lenguaje de programación educativo desarrollado como proyecto académico para el curso de Paradigmas de Programación de la Universidad Nacional de Costa Rica. El proyecto implementa un compilador e intérprete completo con un IDE integrado que proporciona una experiencia de desarrollo moderna y profesional.

### 2.1 Filosofía del Proyecto

El nombre "Kaizen" (改善) proviene del japonés y significa "mejora continua". Este concepto filosófico, ampliamente aplicado en metodologías de desarrollo y gestión de calidad, inspira la esencia del lenguaje. KaizenLang fue diseñado para enseñar conceptos fundamentales de programación mediante una sintaxis clara, intuitiva y un sistema de tipado estricto que previene errores comunes desde las primeras etapas del desarrollo.

La filosofía de mejora continua se refleja en cada aspecto del lenguaje: desde su sintaxis equilibrada (representada por los delimitadores `ying` y `yang`) hasta su enfoque en la prevención de errores mediante validación exhaustiva en tiempo de compilación.

### 2.2 Particularidades del Lenguaje

KaizenLang presenta características únicas que lo diferencian de otros lenguajes educativos:

#### 2.2.1 Anidación Profunda

El lenguaje soporta hasta **512 niveles teóricos de anidación**, permitiendo la creación de estructuras de código complejas sin limitaciones artificiales. Esta característica está pensada para que los desarrolladores puedan expresar algoritmos sofisticados sin preocuparse por restricciones de profundidad.

#### 2.2.2 Delimitadores de Scope Únicos

Utiliza las palabras reservadas **`ying`** y **`yang`** para delimitar bloques de código, inspiradas en el concepto de dualidad y balance de la filosofía oriental. Esta elección sintáctica reemplaza las llaves tradicionales `{}` con una notación más expresiva y significativa.

#### 2.2.3 Sistema de Tipos Inspirado en la Cultura Popular Japonesa
Los tipos de datos están nombrados en honor a elementos icónicos del anime y manga japonés, creando una experiencia de aprendizaje memorable:

**Tipos de datos simples:**
- **`gear`** - Números enteros (inspirado en One Piece)
- **`shikai`** - Números decimales de precisión simple (inspirado en Bleach)
- **`bankai`** - Números decimales de doble precisión (inspirado en Bleach)
- **`grimoire`** - Cadenas de texto (inspirado en Black Clover)
- **`shin`** - Valores booleanos (true/false)

**Tipos de datos compuestos:**
- **`chainsaw<T>`** - Array o lista dinámica unidimensional (inspirado en Chainsaw Man)
- **`hogyoku<T>`** - Matriz bidimensional (inspirado en Bleach)

#### 2.2.4 Convenciones Sintácticas

El resto de la sintaxis del lenguaje mantiene convenciones estándar del idioma inglés para estructuras de control (`if`, `else`, `while`, `for`), operadores y funciones, facilitando la transición desde y hacia otros lenguajes de programación mainstream.

## 3. Problemáticas de Desarrollo

Durante el desarrollo de KaizenLang, el equipo enfrentó diversos desafíos técnicos que requirieron soluciones innovadoras y múltiples iteraciones de refinamiento. A continuación, se detallan los principales obstáculos encontrados y las estrategias empleadas para superarlos.

### 3.1 Gestión de Entrada/Salida

Uno de los mayores desafíos fue la implementación del sistema de entrada y salida (`input()` y `output()`). La integración de estas funciones built-in con el intérprete requirió un diseño cuidadoso para manejar correctamente los tipos de datos, la conversión de cadenas y la sincronización con la interfaz gráfica del IDE.

### 3.2 Control de Flujo y Recursión

Se presentaron dificultades significativas con:

- **Límites en bucles**: La definición precisa de condiciones de terminación para bucles `for` y `while` requirió validación exhaustiva para evitar bucles infinitos.
- **Recursión infinita**: Fue necesario implementar mecanismos de protección para detectar y prevenir recursión infinita sin límites de profundidad razonables.
- **Stack overflow**: Se diseñó un sistema de seguimiento de profundidad de llamadas para alertar al usuario antes de desbordar la pila.

### 3.3 Manejo de Colecciones (Chainsaw y Hogyoku)

El sistema de referencias para colecciones presentó complejidades particulares:

- **Acceso a elementos**: Inicialmente, al intentar acceder a una posición específica de un array (ej. `arr[i]`), el intérprete devolvía la colección completa en lugar del elemento individual. Este problema requirió una refactorización profunda del sistema de resolución de expresiones indexadas.
- **Diferenciación entre shikai y bankai**: El manejo adecuado de las diferencias entre números de precisión simple (`shikai`/`float`) y doble precisión (`bankai`/`double`) requirió implementar validaciones específicas de tipo y conversiones explícitas.

### 3.4 Análisis Semántico

El desarrollo del analizador semántico presentó desafíos continuos:

- **Detección de errores**: La identificación completa de todos los casos de error posibles requirió múltiples ciclos de testing y refinamiento. Aunque se ha logrado cubrir la mayoría de los casos, es posible que aún existan escenarios edge cases pendientes de tratar.
- **Refactorización del entorno**: La estructura del proyecto requirió ser refactorizada en múltiples ocasiones para mejorar la modularidad, mantenibilidad y comprensión del código.

### 3.5 Parser y Análisis Sintáctico

El proceso de parsing atravesó varias etapas:

- **Generación de lexemas**: Esta fase se completó con relativa facilidad gracias a un diseño claro del tokenizador.
- **Construcción del AST**: La complejidad surgió al definir cómo procesar los lexemas para construir el Árbol de Sintaxis Abstracta (AST) y validar la sintaxis completa del código.
- **Transición parser-semántica**: Una vez estructurado correctamente el parser, la transición hacia el análisis semántico se realizó de manera fluida.

### 3.6 Sistema de Funciones

El subsistema de funciones requirió varias iteraciones:

- **Validación de retornos**: Inicialmente, el sistema no validaba correctamente que las funciones no-void tuvieran sentencias `return` en todos los caminos de ejecución.
- **Sistema de tipos**: La verificación de compatibilidad entre tipos de parámetros y argumentos requirió un diseño cuidadoso.
- **Solución adoptada**: Se implementó un enfoque similar al de C++, con validación estricta de tipos y verificación de flujo de control para garantizar que todas las rutas de ejecución retornen valores apropiados.

### 3.7 Anidación Profunda de Bloques

Uno de los desafíos más complejos fue implementar soporte para anidación arbitraria (hasta 512 niveles):

- **Identificación de límites**: Determinar con precisión dónde termina cada bloque `ying...yang` cuando están profundamente anidados requirió un algoritmo sofisticado.
- **Bloques dentro de bloques**: Garantizar que las estructuras de control anidadas (ej. `if` dentro de `while` dentro de `for`) se procesaran correctamente como entidades independientes pero relacionadas.
- **Algoritmo de solución**: Se diseñó un enfoque que identifica el bloque más interno (aquel que no contiene otros bloques en su interior) y luego procesa recursivamente hacia afuera, construyendo una lista ordenada de bloques que respeta la jerarquía de anidación. Este algoritmo garantiza que cada bloque se evalúe en el orden correcto, respetando el scope y las dependencias entre niveles.

### 3.8 Interfaz Gráfica del IDE

El desarrollo del IDE presentó desafíos de diseño y usabilidad:

- **Estética y funcionalidad**: Balancear una interfaz visualmente atractiva con funcionalidad robusta requirió múltiples iteraciones de diseño.
- **Syntax highlighting**: Implementar resaltado de sintaxis en tiempo real para todos los elementos del lenguaje (palabras clave, tipos, operadores, literales) fue técnicamente desafiante.
- **Tema personalizado**: Crear un tema oscuro coherente inspirado en la filosofía Kaizen con paleta verde y dorado requirió atención meticulosa a los detalles visuales.

## 4. Estructura del Proyecto

KaizenLang está organizado como una solución modular de .NET que separa claramente las responsabilidades entre compilación, interpretación e interfaz de usuario.

### 4.1 Características Principales

| Característica                | Descripción                                                                                     |
| ----------------------------- | ----------------------------------------------------------------------------------------------- |
| **Tipado estricto**           | Sistema de tipos robusto sin conversiones implícitas peligrosas; todas las variables deben declararse explícitamente con su tipo |
| **Sintaxis única**            | Usa `ying` y `yang` como delimitadores de bloques, inspirados en el concepto de dualidad y balance |
| **Tipos de datos simples**    | Cinco tipos primitivos: `gear` (int), `shikai` (float), `bankai` (double), `shin` (bool), `grimoire` (string) |
| **Tipos de datos compuestos** | `chainsaw<T>` para arrays unidimensionales y `hogyoku<T>` para matrices bidimensionales con soporte de generics |
| **Estructuras de control**    | Condicionales `if-else`, bucles `while` y `for` con sintaxis clara y validación exhaustiva |
| **Funciones**                 | Soporte completo para funciones con retorno de valores tipados y funciones `void` sin retorno |
| **Operaciones**               | Aritméticas (`+`, `-`, `*`, `/`, `%`), lógicas (`&&`, `||`, `!`), comparación (`>`, `<`, `==`, `!=`, `>=`, `<=`) |
| **Entrada/Salida**            | Funciones built-in `input()` para lectura y `output()` para escritura, integradas nativamente en el lenguaje |
| **Funciones auxiliares**      | Función `length()` para obtener el tamaño de colecciones `chainsaw` y `hogyoku` |

### 4.2 IDE Integrado

El Entorno de Desarrollo Integrado de KaizenLang ofrece una experiencia profesional y moderna:

- **Editor con syntax highlighting**: Resaltado de sintaxis en tiempo real para palabras clave, tipos de datos, operadores, literales numéricos y cadenas de texto
- **Compilación en tiempo real**: Detecta y reporta errores de sintaxis y semánticos con información precisa de línea y columna
- **Ejecución interactiva**: Interpreta y ejecuta código directamente desde el IDE con manejo de entrada/salida interactivo
- **Generación automática de código**: Menú contextual con snippets predefinidos para todas las estructuras del lenguaje (variables, funciones, bucles, condicionales)
- **Tema oscuro moderno**: Paleta de colores verde y dorado inspirada en la filosofía Kaizen, optimizada para reducir la fatiga visual
- **Barra de estado inteligente**: Muestra posición del cursor (línea/columna), estado de compilación en tiempo real y reloj
- **Gestión de archivos**: Capacidad para abrir, editar, guardar y crear nuevos archivos `.kaizen` con diálogos nativos del sistema operativo

### 4.3 Arquitectura del Sistema

KaizenLang implementa un pipeline de compilación clásico con las siguientes etapas:

#### 4.3.1 Flujo de Compilación

```
┌─────────────────────────────────────────────────────────────┐
│                  Pipeline de Compilación                     │
├─────────────────────────────────────────────────────────────┤
│  Código Fuente (.kaizen)                                    │
│       ↓                                                      │
│  Lexer (Análisis Léxico)    → Tokens con metadatos         │
│       ↓                                                      │
│  Parser (Análisis Sintáctico) → AST (Árbol Sintáctico)     │
│       ↓                                                      │
│  Semantic Analyzer          → Validación y tabla símbolos   │
│       ↓                                                      │
│  Interpreter                → Ejecución del AST             │
│       ↓                                                      │
│  Output / Errores          → Resultados o mensajes          │
└─────────────────────────────────────────────────────────────┘
```

| Etapa                    | Descripción                                     | Entrada             | Salida                     |
| ------------------------ | ----------------------------------------------- | ------------------- | -------------------------- |
| **Código Fuente**        | Archivo `.kaizen` escrito por el usuario        | -                   | Texto plano (código)       |
| **Lexer (Tokenización)** | Convierte caracteres en tokens con ubicación    | Código fuente       | Lista de tokens            |
| **Parser (Sintáctico)**  | Construye el AST validando la sintaxis          | Lista de tokens     | AST (Árbol Sintáctico)     |
| **Semantic Analyzer**    | Valida tipos, scope y existencia de símbolos    | AST                 | AST validado + tabla símbolos |
| **Interpreter**          | Ejecuta el AST nodo por nodo                    | AST validado        | Resultado de ejecución     |
| **Output / Errores**     | Presenta resultados o mensajes de error         | Resultado ejecución | Salida o diagnósticos      |

#### 4.3.2 Componentes Modulares

El proyecto está dividido en los siguientes módulos principales:

- **`KaizenLang.Core`**: Núcleo del compilador e intérprete
  - `Lexeme/`: Tokenizador y análisis léxico
  - `Syntax/`: Parser y construcción del AST
  - `Semantic/`: Analizador semántico, tabla de símbolos, validación de tipos
  - `Interpreter/`: Motor de ejecución del código
  - `ATS/`: Definiciones del Árbol de Sintaxis Abstracta
  - `Tokens/`: Definiciones de tokens y tipos

- **`KaizenLang.UI`**: Interfaz gráfica del IDE
  - `Components/`: Controles personalizados del editor
  - `Services/`: Servicios de integración con el compilador
  - `Theming/`: Sistema de temas y colores

- **`KaizenLang.App`**: Punto de entrada de la aplicación

- **`tools/`**: Herramientas de desarrollo y testing
  - `Tests/`: Suite de tests unitarios (xUnit)
  - `TestRunner/`: Ejecutor de tests de integración
  - `CompilationTester/`: Testing end-to-end desde consola
  - `IDERunner/`: Ejecutor de archivos `.kaizen` desde CLI
  - `QuickRunner/`: Herramienta para pruebas rápidas de código

## 5. Innovaciones del Lenguaje

KaizenLang introduce múltiples innovaciones que lo distinguen de otros lenguajes de programación educativos y convencionales, combinando elementos culturales únicos con características técnicas avanzadas.

### 5.1 Temática Cultural Japonesa

La integración profunda de términos y conceptos de la cultura japonesa no solo aporta una identidad única al lenguaje, sino que también hace que el aprendizaje de la programación sea más atractivo, memorable y significativo para los estudiantes.

**Delimitadores de bloques:**
- **`ying`/`yang`** - Define inicio y fin de bloques de código (equivalente a las llaves `{` `}` en C/C++)
  - Representa el concepto de dualidad y balance en la filosofía oriental
  - Hace que la estructura del código sea más expresiva y semánticamente significativa

**Tipos de datos primitivos:**
- **`gear`** - Números enteros (inspirado en las transformaciones Gear de Luffy en One Piece)
- **`shikai`** - Números decimales de precisión simple (primera liberación de la zanpakutō en Bleach)
- **`bankai`** - Números decimales de doble precisión (liberación final de la zanpakutō en Bleach)
- **`grimoire`** - Cadenas de texto (libros mágicos de hechizos en Black Clover)
- **`shin`** - Valores booleanos true/false (神, "dios" o "espíritu" en japonés)

**Tipos de datos compuestos:**
- **`chainsaw<T>`** - Array/lista dinámica unidimensional (inspirado en Chainsaw Man)
- **`hogyoku<T>`** - Matriz bidimensional (la gema omnipotente de Bleach)

### 5.2 Sintaxis Clara y Pedagógica

La sintaxis de KaizenLang está meticulosamente diseñada para ser intuitiva, expresiva y fácil de comprender:

- **Conjunto reducido de palabras reservadas**: Aproximadamente 20 keywords, facilitando la memorización y reduciendo la carga cognitiva para principiantes
- **Nomenclatura consistente**: Convenciones de nomenclatura coherentes en todo el lenguaje
- **Expresividad semántica**: Los nombres de tipos y estructuras reflejan claramente su propósito y comportamiento
- **Curva de aprendizaje suave**: Diseñado específicamente para facilitar la transición desde pseudocódigo hacia lenguajes de producción

### 5.3 IDE Personalizado Integrado

El Entorno de Desarrollo Integrado de KaizenLang va más allá de ser una simple herramienta; es una experiencia de aprendizaje completa:

- **Resaltado de sintaxis avanzado**: Colorización contextual de todos los elementos del lenguaje con paleta inspirada en la filosofía Kaizen
- **Generación automática de código**: Menú interactivo con snippets para todas las estructuras, permitiendo a los estudiantes aprender por exploración
- **Compilación y validación en tiempo real**: Feedback inmediato sobre errores sintácticos y semánticos con mensajes descriptivos
- **Documentación integrada**: Ayuda contextual accesible desde el menú principal que explica palabras reservadas, sintaxis y semántica
- **Depuración visual**: Mensajes de error con línea y columna exactas para facilitar la corrección
- **Interfaz intuitiva**: Diseño minimalista que no distrae del proceso de aprendizaje

### 5.4 Recursión y Anidación Avanzada

KaizenLang soporta estructuras de código significativamente complejas:

- **Hasta 512 niveles de anidación teóricos**: Permite a los desarrolladores crear algoritmos sofisticados sin limitaciones artificiales
- **Validación de profundidad**: Sistema de protección contra stack overflow con alertas tempranas
- **Recursión profunda**: Soporte robusto para algoritmos recursivos complejos con mecanismos de detección de recursión infinita
- **Manejo eficiente de scope**: Sistema de gestión de ámbitos que mantiene el rendimiento incluso con anidación profunda

### 5.5 Tema Visual Coherente

El IDE presenta una identidad visual distintiva y profesional:

- **Paleta de colores Kaizen**: Tema oscuro con tonos verde y dorado que reflejan la filosofía de mejora continua y la estética japonesa
- **Optimización para legibilidad**: Contraste cuidadosamente calibrado para reducir la fatiga visual durante sesiones prolongadas de programación
- **Iconografía cultural**: Elementos visuales inspirados en símbolos tradicionales japoneses
- **Experiencia cohesiva**: Cada elemento de la interfaz refuerza la identidad cultural del lenguaje

### 5.6 Identidad Visual Propia

KaizenLang cuenta con un logotipo diseñado profesionalmente que encapsula su filosofía:

<p align="center">
    <img src="../Resources/icon.ico" alt="KaizenLang Logo" width="80">
</p>

**Elementos del diseño:**
- Incorpora el símbolo del yin-yang para representar los delimitadores `ying` y `yang`
- Colores verde y dorado que reflejan crecimiento (verde) y excelencia (dorado)
- Formas circulares que simbolizan el ciclo continuo de mejora
- Estética moderna que combina tradición japonesa con tecnología contemporánea

### 5.7 Sistema de Tipos Estricto pero Expresivo

KaizenLang implementa un sistema de tipos que balancea seguridad con facilidad de uso:

- **Tipado estático obligatorio**: Todas las variables deben declararse explícitamente con su tipo
- **Sin conversiones implícitas peligrosas**: Previene errores comunes de conversión automática entre tipos incompatibles
- **Generics en colecciones**: `chainsaw<T>` y `hogyoku<T>` soportan tipos genéricos para mayor flexibilidad
- **Validación en tiempo de compilación**: Los errores de tipo se detectan antes de la ejecución, acelerando el ciclo de desarrollo-testing

### 5.8 Mensajes de Error Pedagógicos

Los mensajes de error están diseñados para enseñar, no solo reportar problemas:

- **Ubicación precisa**: Cada error indica la línea y columna exactas donde ocurrió
- **Mensajes descriptivos**: Explicaciones claras de qué salió mal y por qué
- **Sugerencias de corrección**: Cuando es posible, el compilador sugiere cómo resolver el problema
- **Categorización clara**: Distinción entre errores léxicos, sintácticos, semánticos y de runtime

## 6. Guía de Uso

Esta sección proporciona instrucciones detalladas para compilar, ejecutar y utilizar KaizenLang, tanto desde el IDE integrado como desde las herramientas de línea de comandos.

### 6.1 Ejecución del IDE

#### 6.1.1 Método 1: Usando .NET CLI (Recomendado)

```bash
dotnet run --project src/KaizenLang.App/KaizenLang.App.csproj
```

#### 6.1.2 Método 2: Ejecutable Compilado

Después de compilar el proyecto, puedes ejecutar directamente el binario:

```powershell
# Windows PowerShell
.\src\KaizenLang.App\bin\Debug\net9.0-windows\KaizenLang.App.exe

# Windows CMD
src\KaizenLang.App\bin\Debug\net9.0-windows\KaizenLang.App.exe
```

```bash
# Linux/macOS
./src/KaizenLang.App/bin/Debug/net9.0-windows/KaizenLang.App
```

### 6.2 Herramientas de Desarrollo

KaizenLang incluye varias herramientas de línea de comandos diseñadas para facilitar el desarrollo, testing y depuración.

#### 6.2.1 CompilationTester - Testing End-to-End

Herramienta interactiva para probar el pipeline completo de compilación con snippets de código.

```bash
dotnet run --project tools/CompilationTester/CompilationTester.csproj
```

**Funcionalidades:**
- Muestra todos los tokens generados por el lexer con su ubicación
- Imprime el AST completo en formato legible
- Reporta errores semánticos con detalles precisos
- Permite testing rápido de fragmentos de código sin crear archivos

**Uso recomendado:** Ideal para verificar cómo el compilador procesa construcciones sintácticas específicas.

#### 6.2.2 AstDump - Visualización del AST

Herramienta especializada para examinar la estructura del Árbol de Sintaxis Abstracta generado por el parser.

```bash
dotnet run --project tools/AstDump/AstDump.csproj
```

**Funcionalidades:**
- Imprime el AST en formato jerárquico indentado
- Muestra todos los nodos y sus relaciones padre-hijo
- Útil para debugging del parser y entender la estructura interna

**Uso recomendado:** Debugging avanzado del compilador y comprensión profunda del AST.

#### 6.2.3 IDERunner - Ejecutor de Archivos `.kaizen`

Ejecuta archivos de código KaizenLang desde la línea de comandos sin necesidad de abrir el IDE.

```bash
dotnet run --project tools/IDERunner/IDERunner.csproj -- sample.kaizen
```

**Funcionalidades:**
- Compilación e interpretación completa de archivos `.kaizen`
- Manejo de entrada/salida interactivo desde la consola
- Reporta errores con formato similar al IDE
- Soporte para rutas relativas y absolutas

**Uso recomendado:** Ejecución de scripts, automatización, integración en pipelines.

#### 6.2.4 QuickRunner - Pruebas Rápidas

Ejecutor minimalista para probar snippets de código de manera inmediata.

```bash
dotnet run --project tools/QuickRunner/QuickRunner.csproj
```

**Funcionalidades:**
- Interfaz de línea de comandos simple
- Entrada de código en línea o desde stdin
- Tiempo de inicio mínimo para pruebas rápidas

**Uso recomendado:** Testing rápido de expresiones o pequeños fragmentos de código.

#### 6.2.5 TestRunner - Suite de Tests Automatizados

Ejecuta la suite completa de tests del proyecto.

```bash
# Ejecutar todos los tests
dotnet test tools/Tests/KaizenLang.Tests.csproj

# Ejecutar con salida detallada
dotnet test tools/Tests/KaizenLang.Tests.csproj --logger "console;verbosity=detailed"

# Ejecutar un test específico
dotnet test tools/Tests/KaizenLang.Tests.csproj --filter "FullyQualifiedName~FunctionTests"
```

**Funcionalidades:**
- Tests unitarios con xUnit
- Cobertura de lexer, parser, análisis semántico e intérprete
- Reportes detallados de fallos

**Uso recomendado:** Validación de cambios, desarrollo guiado por tests (TDD), CI/CD.

### 6.3 Ejemplos de Uso del IDE

#### 6.3.1 Crear un Nuevo Programa

1. **Abrir el IDE** ejecutando `KaizenLang.App.exe`
2. **Escribir código** en el editor principal (o usar snippets del menú)
3. **Compilar** presionando el botón "Compilar" o `Ctrl+B`
4. **Ejecutar** presionando el botón "Ejecutar" o `F5`
5. **Ver resultados** en el panel de salida

#### 6.3.2 Guardar y Abrir Archivos

- **Nuevo archivo**: `Archivo → Nuevo` o `Ctrl+N`
- **Abrir archivo**: `Archivo → Abrir` o `Ctrl+O`
- **Guardar**: `Archivo → Guardar` o `Ctrl+S`
- **Guardar como**: `Archivo → Guardar Como` o `Ctrl+Shift+S`

#### 6.3.3 Usar Snippets de Código

1. Ir al menú **Generar Código**
2. Seleccionar la categoría deseada (Sintaxis, Tipos de Datos, etc.)
3. Elegir el snippet específico
4. El código se insertará automáticamente en la posición del cursor

#### 6.3.4 Interpretar Errores

Los errores se muestran en el panel de salida con el siguiente formato:

```
Error Semántico en línea 5, columna 10: Variable 'x' no declarada
Error Sintáctico en línea 8, columna 1: Se esperaba 'yang' para cerrar el bloque
```

Cada error indica:
- **Tipo de error**: Léxico, Sintáctico, Semántico o Runtime
- **Ubicación exacta**: Línea y columna
- **Descripción**: Qué salió mal y por qué

### 6.4 Casos de Uso Comunes

#### 6.4.1 Testing Rápido de una Expresión

```bash
# Usar QuickRunner para probar una expresión rápida
echo "gear x = 10 + 5; output(x);" | dotnet run --project tools/QuickRunner/QuickRunner.csproj
```

#### 6.4.2 Ejecutar Todos los Ejemplos

```bash
# Windows PowerShell
Get-ChildItem examples\*.txt | ForEach-Object { dotnet run --project tools/IDERunner/IDERunner.csproj -- $_.FullName }
```

#### 6.4.3 Validar Cambios con Tests

```bash
# Después de modificar el compilador
dotnet test tools/Tests/KaizenLang.Tests.csproj --logger "console;verbosity=normal"
```

## 7. Referencia de Sintaxis

Esta sección documenta la sintaxis completa del lenguaje KaizenLang con ejemplos prácticos.

### 7.1 Declaración de Variables

En KaizenLang, todas las variables deben declararse explícitamente con su tipo. No se permiten conversiones implícitas.

```kaizen
// Tipos primitivos con inicialización
gear edad = 25;                    // Número entero
shikai altura = 1.75;              // Decimal de precisión simple
bankai temperatura = 36.6;         // Decimal de doble precisión
shin esEstudiante = true;          // Booleano
grimoire nombre = "Ana";           // Cadena de texto

// Declaración sin inicialización (valor por defecto)
gear contador;                     // Se inicializa en 0
shin activo;                       // Se inicializa en false
grimoire mensaje;                  // Se inicializa en ""
```

### 7.2 Estructuras de Control

#### 7.2.1 Condicional if-else

```kaizen
// Condicional simple
if (edad > 18) ying
    output("Mayor de edad");
yang

// Condicional con else
if (edad >= 18) ying
    output("Mayor de edad");
yang else ying
    output("Menor de edad");
yang

// Condicionales anidados
if (edad < 13) ying
    output("Niño");
yang else ying
    if (edad < 18) ying
        output("Adolescente");
    yang else ying
        output("Adulto");
    yang
yang
```

#### 7.2.2 Bucle while

```kaizen
// Bucle while básico
gear contador = 0;
while (contador < 5) ying
    output("Iteración: " + contador);
    contador = contador + 1;
yang

// Bucle while con condición compleja
gear x = 10;
gear y = 20;
while (x < 100 && y > 0) ying
    x = x * 2;
    y = y - 1;
    output("x: " + x + ", y: " + y);
yang
```

#### 7.2.3 Bucle for

```kaizen
// Bucle for clásico
for (gear i = 0; i < 10; i = i + 1) ying
    output("Número: " + i);
yang

// Bucle for con paso diferente
for (gear i = 0; i < 100; i = i + 10) ying
    output("Múltiplo de 10: " + i);
yang

// Bucle for descendente
for (gear i = 10; i > 0; i = i - 1) ying
    output("Cuenta regresiva: " + i);
yang
```

### 7.3 Funciones

#### 7.3.1 Funciones con Retorno

```kaizen
// Función que retorna un valor
gear suma(gear a, gear b) ying
    return a + b;
yang

// Función con múltiples parámetros
shikai promedio(shikai a, shikai b, shikai c) ying
    return (a + b + c) / 3.0;
yang

// Función booleana
shin esPar(gear numero) ying
    return (numero % 2) == 0;
yang

// Uso de funciones
gear resultado = suma(10, 5);              // resultado = 15
shikai prom = promedio(8.5, 9.0, 7.5);     // prom = 8.333...
shin par = esPar(8);                        // par = true
```

#### 7.3.2 Funciones void

```kaizen
// Función sin retorno
void saludar(grimoire nombre) ying
    output("Hola " + nombre + "!");
yang

// Función void con múltiples parámetros
void imprimirRectangulo(gear ancho, gear alto) ying
    for (gear i = 0; i < alto; i = i + 1) ying
        for (gear j = 0; j < ancho; j = j + 1) ying
            output("* ");
        yang
        output("\n");
    yang
yang

// Llamadas a funciones void
saludar("KaizenLang");
imprimirRectangulo(5, 3);
```

#### 7.3.3 Funciones Recursivas

```kaizen
// Factorial recursivo
gear factorial(gear n) ying
    if (n <= 1) ying
        return 1;
    yang
    return n * factorial(n - 1);
yang

// Fibonacci recursivo
gear fibonacci(gear n) ying
    if (n <= 1) ying
        return n;
    yang
    return fibonacci(n - 1) + fibonacci(n - 2);
yang

// Uso
gear fact5 = factorial(5);      // fact5 = 120
gear fib7 = fibonacci(7);       // fib7 = 13
```

### 7.4 Chainsaw (Arrays Unidimensionales)

```kaizen
// Declaración e inicialización de chainsaw
chainsaw<gear> numeros = [1, 2, 3, 4, 5];
chainsaw<grimoire> nombres = ["Ana", "Luis", "Pedro", "María"];
chainsaw<shin> flags = [true, false, true, true];

// Acceso por índice (base 0)
gear primero = numeros[0];           // primero = 1
grimoire persona = nombres[1];       // persona = "Luis"
shin flag = flags[2];                // flag = true

// Modificación de elementos
numeros[0] = 10;                     // numeros = [10, 2, 3, 4, 5]
nombres[2] = "Juan";                 // nombres = ["Ana", "Luis", "Juan", "María"]

// Obtener longitud
gear tam = length(numeros);          // tam = 5

// Iterar sobre chainsaw
for (gear i = 0; i < length(nombres); i = i + 1) ying
    output("Nombre " + i + ": " + nombres[i]);
yang

// Chainsaw de tipos decimales
chainsaw<shikai> temperaturas = [36.5, 37.2, 36.8, 38.1];
shikai primera = temperaturas[0];
```

### 7.5 Hogyoku (Matrices Bidimensionales)

```kaizen
// Declaración e inicialización de hogyoku
hogyoku<gear> matriz = [
    [1, 2, 3],
    [4, 5, 6],
    [7, 8, 9]
];

// Acceso a elementos (fila, columna)
gear elemento = matriz[0][1];        // elemento = 2 (fila 0, columna 1)
gear centro = matriz[1][1];          // centro = 5

// Modificación de elementos
matriz[0][0] = 10;                   // Primera fila, primera columna = 10
matriz[2][2] = 99;                   // Última fila, última columna = 99

// Hogyoku de diferentes tipos
hogyoku<shikai> decimales = [
    [1.1, 2.2, 3.3],
    [4.4, 5.5, 6.6]
];

hogyoku<grimoire> palabras = [
    ["hola", "mundo"],
    ["adios", "mundo"]
];

// Iterar sobre hogyoku
gear filas = 3;
gear columnas = 3;

for (gear i = 0; i < filas; i = i + 1) ying
    for (gear j = 0; j < columnas; j = j + 1) ying
        output("matriz[" + i + "][" + j + "] = " + matriz[i][j]);
    yang
yang
```

### 7.6 Entrada y Salida

```kaizen
// Función output() - Imprime en consola
output("Hola Mundo");
output("La suma es: " + 10);

// Función input() - Lee entrada del usuario
grimoire nombre = input();           // Lee una línea de texto
output("Hola " + nombre);

// Ejemplo interactivo completo
output("Ingrese su edad:");
grimoire edad_texto = input();
// Nota: En la versión actual, la conversión de grimoire a gear
// para inputs numéricos se maneja internamente por el runtime

output("Ingrese su nombre:");
grimoire nombre_usuario = input();
output("Hola " + nombre_usuario + ", tienes " + edad_texto + " años");

// Función length() - Obtiene longitud de colecciones
chainsaw<gear> datos = [10, 20, 30, 40];
gear tamaño = length(datos);
output("El chainsaw tiene " + tamaño + " elementos");

hogyoku<gear> tabla = [[1, 2], [3, 4], [5, 6]];
gear filas = length(tabla);          // Número de filas
output("La matriz tiene " + filas + " filas");
```

### 7.7 Operaciones

#### 7.7.1 Operaciones Aritméticas

```kaizen
gear a = 10;
gear b = 3;

gear suma = a + b;                   // 13
gear resta = a - b;                  // 7
gear multiplicacion = a * b;         // 30
gear division = a / b;               // 3 (división entera)
gear modulo = a % b;                 // 1 (resto)

// Operaciones con decimales
shikai x = 10.0;
shikai y = 3.0;
shikai div_decimal = x / y;          // 3.333...

// Prioridad de operadores (igual que en matemáticas)
gear resultado = 10 + 5 * 2;         // 20 (primero *, luego +)
gear resultado2 = (10 + 5) * 2;      // 30 (paréntesis primero)
```

#### 7.7.2 Operaciones de Comparación

```kaizen
gear x = 10;
gear y = 20;

shin mayor = (x > y);                // false
shin menor = (x < y);                // true
shin mayorIgual = (x >= 10);         // true
shin menorIgual = (y <= 15);         // false
shin igual = (x == y);               // false
shin diferente = (x != y);           // true

// Comparación con decimales
shikai a = 3.14;
shikai b = 3.14;
shin iguales = (a == b);             // true
```

#### 7.7.3 Operaciones Lógicas

```kaizen
shin p = true;
shin q = false;

// AND lógico
shin and_result = p && q;            // false

// OR lógico
shin or_result = p || q;             // true

// NOT lógico
shin not_p = !p;                     // false
shin not_q = !q;                     // true

// Combinaciones complejas
gear edad = 25;
shin esMayor = edad >= 18;
shin esJoven = edad < 30;
shin adultoJoven = esMayor && esJoven;  // true

// Cortocircuito en evaluación
shin resultado = (edad > 18) && (edad < 65);
// Si (edad > 18) es false, (edad < 65) no se evalúa
```

### 7.8 Comentarios

```kaizen
// Este es un comentario de una línea

// Los comentarios son ignorados por el compilador
gear x = 10;  // También puedes poner comentarios al final de líneas

// Comentarios multilinea no están soportados en la versión actual
// Para comentar varias líneas, usa // al inicio de cada una
```

### 7.9 Ejemplo Completo: Programa Integrado

```kaizen
// Programa que calcula el factorial de un número
// solicitado al usuario

// Función recursiva para calcular factorial
gear factorial(gear n) ying
    if (n <= 1) ying
        return 1;
    yang
    return n * factorial(n - 1);
yang

// Función principal (punto de entrada)
void main() ying
    output("Calculadora de Factorial");
    output("Ingrese un número:");

    grimoire input_texto = input();
    // El runtime maneja la conversión automáticamente

    gear numero = 5;  // Por simplicidad, usamos un valor fijo

    if (numero < 0) ying
        output("Error: El factorial no está definido para números negativos");
    yang else ying
        gear resultado = factorial(numero);
        output("El factorial de " + numero + " es: " + resultado);
    yang
yang

// Llamada a la función principal
main();
```

## 8. Ejemplos Prácticos Avanzados

Esta sección presenta ejemplos más complejos que demuestran las capacidades avanzadas de KaizenLang.

### 8.1 Ejemplo 1: Búsqueda en Chainsaw

```kaizen
// Función que busca un elemento en un chainsaw
shin buscar(chainsaw<gear> arr, gear objetivo) ying
    gear tam = length(arr);
    for (gear i = 0; i < tam; i = i + 1) ying
        if (arr[i] == objetivo) ying
            return true;
        yang
    yang
    return false;
yang

// Uso
chainsaw<gear> numeros = [10, 20, 30, 40, 50];
shin encontrado = buscar(numeros, 30);

if (encontrado) ying
    output("El número está en el chainsaw");
yang else ying
    output("El número no está en el chainsaw");
yang
```

### 8.2 Ejemplo 2: Suma de Matriz (Hogyoku)

```kaizen
// Función que suma todos los elementos de una matriz
gear sumaMatriz(hogyoku<gear> matriz) ying
    gear suma = 0;
    gear filas = length(matriz);

    for (gear i = 0; i < filas; i = i + 1) ying
        gear columnas = length(matriz[i]);
        for (gear j = 0; j < columnas; j = j + 1) ying
            suma = suma + matriz[i][j];
        yang
    yang

    return suma;
yang

// Uso
hogyoku<gear> tabla = [
    [1, 2, 3],
    [4, 5, 6],
    [7, 8, 9]
];

gear total = sumaMatriz(tabla);
output("La suma total es: " + total);  // Output: 45
```

### 8.3 Ejemplo 3: Números Primos

```kaizen
// Función que verifica si un número es primo
shin esPrimo(gear n) ying
    if (n <= 1) ying
        return false;
    yang

    if (n == 2) ying
        return true;
    yang

    for (gear i = 2; i < n; i = i + 1) ying
        if (n % i == 0) ying
            return false;
        yang
    yang

    return true;
yang

// Función que encuentra los primeros N números primos
void imprimirPrimos(gear cantidad) ying
    gear contador = 0;
    gear numero = 2;

    output("Los primeros " + cantidad + " números primos son:");

    while (contador < cantidad) ying
        if (esPrimo(numero)) ying
            output(numero);
            contador = contador + 1;
        yang
        numero = numero + 1;
    yang
yang

// Uso
imprimirPrimos(10);  // Imprime los primeros 10 números primos
```

### 8.4 Ejemplo 4: Ordenamiento Burbuja

```kaizen
// Función que ordena un chainsaw usando bubble sort
void ordenarBurbuja(chainsaw<gear> arr) ying
    gear n = length(arr);

    for (gear i = 0; i < n - 1; i = i + 1) ying
        for (gear j = 0; j < n - i - 1; j = j + 1) ying
            if (arr[j] > arr[j + 1]) ying
                // Intercambiar elementos
                gear temp = arr[j];
                arr[j] = arr[j + 1];
                arr[j + 1] = temp;
            yang
        yang
    yang
yang

// Función que imprime un chainsaw
void imprimirChainsaw(chainsaw<gear> arr) ying
    gear tam = length(arr);
    for (gear i = 0; i < tam; i = i + 1) ying
        output(arr[i]);
    yang
yang

// Uso
chainsaw<gear> numeros = [64, 34, 25, 12, 22, 11, 90];
output("Chainsaw original:");
imprimirChainsaw(numeros);

ordenarBurbuja(numeros);

output("Chainsaw ordenado:");
imprimirChainsaw(numeros);
```

## 9. Validación de Errores en KaizenLang

KaizenLang implementa un sistema exhaustivo de detección de errores en todas las fases de compilación e interpretación.

### 9.1 Errores Léxicos

Ocurren cuando el tokenizador encuentra caracteres o secuencias no reconocidas.

```kaizen
gear x = 123abc;           // Error: Token inválido '123abc'
grimoire y = "texto;       // Error: Cadena sin cerrar
gear z = @invalido;        // Error: Carácter '@' no reconocido
```

### 9.2 Errores Sintácticos

Ocurren cuando la estructura del código no sigue las reglas gramaticales del lenguaje.

```kaizen
gear x = 10                // Error: Falta punto y coma
if x > 5 ying              // Error: Faltan paréntesis en condición '(x > 5)'
chainsaw<gear> = [1, 2];   // Error: Falta identificador de variable
gear suma(gear a, gear b   // Error: Falta paréntesis de cierre ')'
while (true) ying          // Error: Falta 'yang' para cerrar bloque
```

### 9.3 Errores Semánticos

Ocurren cuando el código es sintácticamente correcto pero viola las reglas de tipo o scope.

```kaizen
// Error de tipo
gear x = "texto";                    // Tipo incompatible: esperado gear, recibido grimoire

// Variable no declarada
gear y = z + 10;                     // Variable 'z' no declarada

// Función sin retorno
gear suma(gear a, gear b) ying       // Función no-void debe retornar valor en todos los caminos
    gear resultado = a + b;
yang

// Tipos incompatibles en operaciones
gear a = 10;
grimoire b = "texto";
gear c = a + b;                      // No se puede sumar gear y grimoire

// Elementos heterogéneos en chainsaw
chainsaw<gear> nums = [1, "dos", 3]; // Todos los elementos deben ser gear

// Hogyoku no rectangular
hogyoku<gear> mat = [
    [1, 2, 3],
    [4, 5]                           // Todas las filas deben tener la misma longitud
];
```

### 9.4 Errores de Runtime

Ocurren durante la ejecución del programa.

```kaizen
// División por cero
gear x = 10;
gear y = 0;
gear resultado = x / y;              // Runtime Error: División por cero

// Índice fuera de rango en chainsaw
chainsaw<gear> arr = [1, 2, 3];
gear elemento = arr[10];             // Runtime Error: Índice 10 fuera de rango [0-2]

// Índice fuera de rango en hogyoku
hogyoku<gear> matriz = [[1, 2], [3, 4]];
gear valor = matriz[5][5];           // Runtime Error: Índice de fila fuera de rango
```

## 10. Mejores Prácticas de Programación

### 10.1 Nombres Descriptivos

```kaizen
// Bien
gear edadEstudiante = 20;
grimoire nombreCompleto = "Ana García";

// Mal
gear e = 20;
grimoire n = "Ana García";
```

### 10.2 Validación de Entradas

```kaizen
// Bien
gear dividir(gear dividendo, gear divisor) ying
    if (divisor == 0) ying
        output("Error: No se puede dividir por cero");
        return 0;
    yang
    return dividendo / divisor;
yang
```

## 11. Conclusión

KaizenLang es un lenguaje de programación educativo que combina una sintaxis única inspirada en la cultura japonesa con características técnicas robustas. Diseñado bajo la filosofía de "mejora continua", el lenguaje proporciona:

- **Sistema de tipos estricto** que previene errores comunes
- **Sintaxis expresiva y memorable** con delimitadores `ying`/`yang` y tipos de datos con nombres culturales
- **IDE integrado profesional** con syntax highlighting, compilación en tiempo real y herramientas de debugging
- **Validación exhaustiva** en todas las fases: léxica, sintáctica, semántica y runtime
- **Soporte para programación avanzada** con recursión profunda y anidación hasta 512 niveles
- **Herramientas de desarrollo completas** para testing, depuración y ejecución

El proyecto demuestra la implementación completa de un compilador e intérprete funcional, desde el análisis léxico hasta la ejecución de código, cumpliendo con todos los requisitos académicos del curso de Paradigmas de Programación de la Universidad Nacional de Costa Rica.

---

## 12. Información del Proyecto

### 12.1 Contexto Académico

- **Universidad**: Universidad Nacional de Costa Rica (UNA)
- **Sede**: Sede Regional Brunca - Campus Pérez Zeledón
- **Curso**: Paradigmas de Programación
- **Profesor**: MSc. Josías Ariel Chaves Murillo
- **Ciclo Académico**: II Ciclo 2025

### 12.2 Estudiantes

- Juan Carlos Camacho Solano
- Esteban Granados Sibaja

### 12.3 Tecnologías Utilizadas

- **Lenguaje de Implementación**: C# 12.0
- **Framework**: .NET 9.0
- **UI Framework**: Windows Forms
- **Testing Framework**: xUnit
- **Control de Versiones**: Git / GitHub
- **IDE de Desarrollo**: Visual Studio 2022

---

<div align="center">

**🌟 KaizenLang - Mejora Continua en Programación 🌟**

*"El aprendizaje es un tesoro que seguirá a su dueño a todas partes"*

**改善** (Kaizen) - Cambio para mejorar

Hecho con ❤️ para la Universidad Nacional de Costa Rica

</div>
