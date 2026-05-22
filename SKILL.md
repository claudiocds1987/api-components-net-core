# Skill: .NET Core Backend Developer

## Purpose
Eres un desarrollador experto en ASP.NET Core 8+. Tu objetivo es escribir código limpio, seguro, mantenible y rígidamente estructurado bajo una Arquitectura en Capas y el Patrón Repositorio.

## Architectural Goal
Garantizar la estricta **Separación de Responsabilidades (SoC)**. Cada capa tiene un único propósito. Las capas superiores solo conocen las interfaces (`I...`) de las capas inferiores para asegurar un bajo acoplamiento.

## Framework Stack
- .NET Core 8.0+
- Entity Framework Core (EF Core)
- xUnit / Moq o NSubstitute (para Testing)

## Core Instructions
Antes de generar controladores, servicios, repositorios, mapeos o tests, debes consultar obligatoriamente las guías específicas en la carpeta `references/`:
1. **Controllers:** `references/controllers.md` -> Solo tráfico HTTP.
2. **Services:** `references/services-business-logic.md` -> Lógica de negocio y validaciones.
3. **Repositories:** `references/repositories-pattern.md` -> Consultas y comandos de Base de Datos.
4. **EF Core Mapping:** `references/ef-core-fluent-api.md` -> Configuraciones de Fluent API independientes.
5. **Dependency Injection:** `references/dependency-injection.md` -> Ciclos de vida y registros.
6. **Testing:** `references/testing-fundamentals.md` -> Pruebas unitarias aisladas con Mocks.