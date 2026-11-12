# 🧪 Guía de Testing para KaizenLang

## 📋 Suite Completa de Pruebas

Esta carpeta contiene 15 archivos de prueba diseñados para validar todas las funcionalidades del lenguaje KaizenLang.

---

## 📁 Estructura de Tests

### ✅ Tests Funcionales (Deben Compilar y Ejecutar Correctamente)

#### **Test 01: Variables Básicas** (`test-01-variables-basicas.txt`)
- Declaración de los 5 tipos simples
- Tipos: `gear`, `shikai`, `bankai`, `shin`, `grimoire`
- Caracteres individuales con grimoire
- **Resultado esperado:** Imprime todos los valores correctamente

#### **Test 02: Operaciones Aritméticas** (`test-02-operaciones-aritmeticas.txt`)
- Suma, resta, multiplicación, división
- Operaciones con integers y floats
- Promoción numérica (gear → shikai)
- Expresiones complejas con paréntesis
- **Resultado esperado:** Cálculos matemáticos correctos

#### **Test 03: Operaciones Lógicas** (`test-03-operaciones-logicas.txt`)
- Comparación: `>`, `<`, `>=`, `<=`, `==`, `!=`
- Lógicos: `&&` (AND), `||` (OR), `!` (NOT)
- Expresiones lógicas complejas
- **Resultado esperado:** Resultados booleanos correctos

#### **Test 04: Estructura IF** (`test-04-estructuras-if.txt`)
- `if` simple
- `if-else`
- Condicionales con expresiones lógicas
- IF anidados
- **Resultado esperado:** Flujo de control correcto

#### **Test 05: Estructura WHILE** (`test-05-estructuras-while.txt`)
- Bucles básicos con contador
- While con condiciones complejas
- Decremento y cuenta regresiva
- While con booleanos
- **Resultado esperado:** Iteraciones correctas

#### **Test 06: Estructura FOR** (`test-06-estructuras-for.txt`)
- For básico con incremento
- Cuenta regresiva
- Saltos (incremento de 2, 5, etc.)
- For con operaciones acumuladas
- **Resultado esperado:** Bucles funcionan correctamente

#### **Test 07: Chainsaw Básicos** (`test-07-arrays-basicos.txt`)
- Declaración: `chainsaw<gear>`, `chainsaw<grimoire>`, etc.
- Acceso por índice
- Función `length()`
- Recorrido con for
- Búsqueda en chainsaws
- **Resultado esperado:** Operaciones con chainsaws correctas

#### **Test 08: Hogyokus** (`test-08-matrices.txt`)
- Declaración: `hogyoku<gear>`
- Acceso bidimensional `[i][j]`
- For anidado para recorrer
- Suma de elementos
- Búsqueda en hogyoku
- **Resultado esperado:** Operaciones 2D correctas

#### **Test 09: Funciones Básicas** (`test-09-funciones-basicas.txt`)
- Funciones `void` sin parámetros
- Funciones `void` con parámetros
- Funciones con retorno (`gear`, `shikai`, `shin`, `grimoire`)
- Múltiples parámetros
- **Resultado esperado:** Llamadas y retornos correctos

#### **Test 10: Funciones Avanzadas** (`test-10-funciones-avanzadas.txt`)
- Factorial recursivo
- Fibonacci recursivo
- Funciones con lógica compleja
- Funciones con bucles internos
- Potencia con while
- **Resultado esperado:** Recursión y cálculos correctos

#### **Test 11: Input/Output** (`test-11-input-output.txt`)
⚠️ **REQUIERE INTERACCIÓN DEL USUARIO**
- Función `input()` para leer datos
- Función `output()` para mostrar datos
- Concatenación de strings
- Para pruebas automatizadas se puede usar `inputs/test-11-input.txt`
- **Resultado esperado:** Solicita y muestra datos del usuario

#### **Test 12: Programa Completo** (`test-12-programa-completo.txt`)
- Sistema de gestión de estudiantes
- Integra: chainsaw, funciones, bucles, condicionales
- Cálculo de estadísticas (promedio, máximo, mínimo)
- **Resultado esperado:** Programa completo funcional

#### **Test 15: Casos Límite** (`test-15-casos-limite.txt`)
- Valores extremos (máx/mín integers)
- Strings vacíos
- Chainsaws vacíos y de 1 elemento
- Operaciones con cero
- Recursión con caso base
- Anidamiento profundo
- **Resultado esperado:** Maneja edge cases correctamente

#### **Test 16: Stress Integral** (`test-16-stress-integral.txt`)
- Tres niveles de bucles anidados con validaciones internas
- Recursión profunda y recursión acumulativa
- Chainsaws y hogyokus con iteraciones y sumatorias
- Condiciones anidadas que combinan aritmética y lógica
- **Resultado esperado:** Todas las operaciones completan sin errores y muestran los acumuladores finales

---

### ❌ Tests de Validación de Errores (Deben Fallar)

#### **Test 13: Errores Sintácticos** (`test-13-errores-sintacticos.txt`)
⚠️ **CÓDIGO COMENTADO** - Descomentar para probar errores

Errores a detectar:
- Falta punto y coma `;`
- Falta paréntesis en condiciones
- Falta `ying` o `yang` en bloques
- Operadores inválidos
- Paréntesis/corchetes no balanceados
- **Resultado esperado:** Error de sintaxis con línea/columna

#### **Test 14: Errores Semánticos** (`test-14-errores-semanticos.txt`)
⚠️ **CÓDIGO COMENTADO** - Descomentar para probar errores

Errores a detectar:
- Variable no declarada
- Tipo incompatible en asignación
- Operación entre tipos incompatibles
- Función no definida
- Aridad incorrecta (parámetros)
- Tipo de retorno incorrecto
- Falta `return` en función
- Chainsaw heterogéneo
- Hogyoku no rectangular
- Variable duplicada
- **Resultado esperado:** Error semántico con descripción

---

## 🚀 Cómo Ejecutar los Tests

### Opción 1: Desde el IDE

1. Abrir KaizenLang IDE
2. Cargar archivo de test (copiar/pegar o usar menú)
3. Click en **Compilar** 🔧
4. Si compila OK, click en **Ejecutar** ▶️
5. Ver resultados en panel de output

### Opción 2: Desde Herramientas de Consola

#### TestRunner (compila y ejecuta un archivo)
```powershell
dotnet run --project tools/TestRunner/TestRunner.csproj -- examples/test-01-variables-basicas.txt
```

#### IDERunner (cualquier archivo de código)
```powershell
# Acepta cualquier extensión (.txt, .kaizen, .kz)
dotnet run --project tools/IDERunner/IDERunner.csproj -- examples/test-01-variables-basicas.txt
```

### Opción 3: Testing Automatizado

```powershell
# Ejecutar suite de tests xUnit
dotnet test tools/Tests/KaizenLang.Tests.csproj --logger "console;verbosity=detailed"
```

---

## 📊 Checklist de Testing

### Tests Funcionales
- [ ] **Test 01**: Variables básicas - Todos los tipos simples
- [ ] **Test 02**: Operaciones aritméticas - Cálculos correctos
- [ ] **Test 03**: Operaciones lógicas - Booleanos correctos
- [ ] **Test 04**: Estructura IF - Condicionales funcionan
- [ ] **Test 05**: Estructura WHILE - Bucles iteran correctamente
- [ ] **Test 06**: Estructura FOR - Bucles con incremento/decremento
- [ ] **Test 07**: Chainsaws básicos - Declaración, acceso, length
- [ ] **Test 08**: Hogyokus - Acceso 2D, recorrido
- [ ] **Test 09**: Funciones básicas - Void y con retorno
- [ ] **Test 10**: Funciones avanzadas - Recursión
- [ ] **Test 11**: Input/Output - Interacción con usuario
- [ ] **Test 12**: Programa completo - Integración
- [ ] **Test 15**: Casos límite - Edge cases
- [ ] **Test 16**: Stress integral - Control y recursión intensivos

### Tests de Validación
- [ ] **Test 13**: Errores sintácticos detectados
- [ ] **Test 14**: Errores semánticos detectados

---

## 🎯 Resultados Esperados por Categoría

### ✅ Compilación Exitosa
Los tests 01-12 y 15 deben:
1. **Compilar sin errores**
2. **Ejecutar completamente**
3. **Producir output esperado**

### ❌ Detección de Errores
Los tests 13-14 deben:
1. **Fallar en compilación** (sintácticos) o semántica
2. **Reportar línea y columna del error**
3. **Mostrar mensaje descriptivo**

---

## 🔍 Qué Validar en Cada Test

### Test 01 - Variables
- ✅ Todos los tipos se declaran correctamente
- ✅ Valores se asignan e imprimen
- ✅ Strings manejan caracteres individuales

### Test 02 - Aritmética
- ✅ Operaciones básicas (+, -, *, /)
- ✅ Promoción numérica (gear → shikai)
- ✅ Precedencia de operadores
- ✅ Expresiones con paréntesis

### Test 03 - Lógica
- ✅ Comparaciones funcionan
- ✅ AND, OR, NOT correctos
- ✅ Expresiones complejas evalúan bien

### Test 04-06 - Control de Flujo
- ✅ Bloques ying/yang parsean
- ✅ Condiciones evalúan correctamente
- ✅ Bucles iteran el número correcto de veces
- ✅ Anidamiento funciona

- ✅ Chainsaws/hogyokus se declaran con sintaxis genérica
- ✅ Acceso por índice funciona
- ✅ `length()` retorna tamaño correcto
- ✅ Validación de tipos homogéneos
- ✅ Hogyokus son rectangulares

### Test 09-10 - Funciones
- ✅ Declaración y llamada funcionan
- ✅ Parámetros se pasan correctamente
- ✅ Return devuelve valores
- ✅ Recursión funciona sin stack overflow

### Test 11 - I/O
- ✅ `input()` solicita entrada
- ✅ `output()` muestra en consola
- ✅ Concatenación funciona

### Test 12 - Integración
- ✅ Programa completo ejecuta
- ✅ Todas las features funcionan juntas
- ✅ Lógica compleja correcta

- ✅ Valores extremos no causan overflow
- ✅ Strings/chainsaws vacíos funcionan
- ✅ Operaciones con cero correctas
- ✅ Recursión termina correctamente

### Test 13-14 - Errores
- ✅ Errores son detectados
- ✅ Mensajes son claros
- ✅ Línea/columna reportadas

---

## 📈 Métricas de Éxito

### Compilación
- **Target**: 13 de 15 tests compilan (excluir 13-14)
- **Crítico**: Tests 01-12 deben compilar

### Ejecución
- **Target**: 13 de 15 tests ejecutan completamente
- **Crítico**: Test 12 (programa completo) funciona

### Validación
- **Target**: Tests 13-14 reportan errores correctamente
- **Crítico**: Al menos 5 tipos de error detectados por categoría

---

## 🐛 Troubleshooting

### Error: "Variable no declarada"
- Verificar que todas las variables tengan declaración de tipo
- Revisar alcance (scope) de variables

### Error: "Tipo incompatible"
- Verificar que operaciones usen tipos compatibles
- Recordar: gear + shikai = shikai (promoción)

### Error: "Falta return"
- Todas las funciones no-void deben tener `return`
- Revisar todos los caminos de ejecución

### Error: "Chainsaw heterogéneo"
- Chainsaws deben tener elementos del mismo tipo
- `chainsaw<gear>` solo acepta integers

### Error: "Hogyoku no rectangular"
- Todas las filas deben tener igual número de columnas
- Ejemplo: `[[1,2], [3,4,5]]` ❌ es inválido

---

## 📝 Notas Adicionales

### Tests Interactivos
- **Test 11** requiere entrada del usuario
- Probar con diferentes tipos de datos
- Validar manejo de strings vacíos

### Performance
- Tests con recursión (Test 10) pueden tardar más
- Factorial de números grandes puede ser lento
- Fibonacci > 10 puede tomar tiempo

### Debugging
- Usar `output()` para debug intermedio
- Verificar valores en cada paso
- Confirmar flujo de control con mensajes

---

## ✅ Checklist de Entrega

Antes de entregar el proyecto, verificar:

- [ ] Los 13 tests funcionales ejecutan sin errores
- [ ] Test 12 (programa completo) funciona correctamente
- [ ] Tests 13-14 detectan al menos 10 tipos de errores diferentes
- [ ] Mensajes de error son claros y útiles
- [ ] Línea/columna se reportan en errores
- [ ] No hay crashes o excepciones no manejadas
- [ ] Output es legible y correcto
- [ ] Syntax highlighting funciona en el IDE
- [ ] Stress test (`test-16-stress-integral.txt`) ejecuta sin degradar el rendimiento

---

**Generado:** 19 de Octubre, 2025
**Versión:** 1.0
**Suite de Tests:** Completa (15 archivos)
