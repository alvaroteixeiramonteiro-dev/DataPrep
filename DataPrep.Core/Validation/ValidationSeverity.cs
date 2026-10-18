using System;
using System.Collections.Generic;
using System.Text;

namespace DataPrep.Core.Validation;

/// <summary>
/// Define a gravidade de um problema identificado durante a validação.
/// A interface traduz os nomes apresentados ao utilizador.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// Problema que exige atenção, mas não bloqueia a importação.
    /// </summary>
    Warning,
    /// <summary>
    /// Problema que bloqueia a importação até ser resolvido.
    /// </summary>
    Error
}
