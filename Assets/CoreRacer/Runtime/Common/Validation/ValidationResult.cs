using System.Collections.Generic;
using System.Text;

namespace CoreRacer.Common.Validation
{
    public sealed class ValidationResult
    {
        private readonly List<string> _errors = new List<string>();
        private readonly List<string> _warnings = new List<string>();

        public IReadOnlyList<string> Errors => _errors;
        public IReadOnlyList<string> Warnings => _warnings;
        public bool IsValid => _errors.Count == 0;

        public void Error(string message)
        {
            if (!string.IsNullOrWhiteSpace(message)) _errors.Add(message);
        }

        public void Warning(string message)
        {
            if (!string.IsNullOrWhiteSpace(message)) _warnings.Add(message);
        }

        public void Merge(ValidationResult other)
        {
            if (other == null) return;
            _errors.AddRange(other._errors);
            _warnings.AddRange(other._warnings);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _errors.Count; i++) sb.AppendLine("ERROR: " + _errors[i]);
            for (int i = 0; i < _warnings.Count; i++) sb.AppendLine("WARN: " + _warnings[i]);
            return sb.ToString();
        }
    }

    public interface IValidatableConfig
    {
        ValidationResult ValidateConfig();
    }
}
