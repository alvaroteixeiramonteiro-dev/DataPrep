using System;
using System.Collections.Generic;
using System.Text;

namespace DataPrep.Core.Validation
{
    public sealed class ValidationIssue
    {
        public required string RuleCode { get; init; }

        public required ValidationSeverity Severity { get; init; }

        public long? RecordNumber { get; init; }

        public long? StartLineNumber { get; init; }

        public long? EndLineNumber { get; init; }

        public int? ColumnNumber { get; init; }

        public string? SourceField { get; init; }

        public string? TargetField { get; init; }

        public string? OriginalValue { get; init; }

        public required string CorrectionCode { get; init; }

        public IReadOnlyDictionary<string, string> Parameters { get; init; }
            = new Dictionary<string, string>();
    }
}
