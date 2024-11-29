using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using gs;
using Logger.Contract;

namespace LayerSource.GCode.Parser
{
    internal class Macro : IMacro
    {
        #region Public Methods

        public int Parse(IList<GCodeLine> gcode,
            string macro,
            IList<(string, Action<double>)> intParameters,
            IList<(string, Action<string>)> stringParameters,
            (string parameterName, string value)? parameterConstraint,
            int startIndex)
        {
            var gcodeLine = gcode[startIndex].orig_string.ToLower();
            if (gcodeLine.StartsWith(macro.ToLower()))
            {
                if (IsParameterConstraintSatisfied(macro, parameterConstraint, gcodeLine))
                {
                    startIndex++;

                    ParseNumericParameters(macro, intParameters, gcodeLine);

                    ParseStringParameters(macro, stringParameters, gcodeLine);
                }
            }
            else
            {
                Logger.Warn($"Could not find expected macro {macro} in section {SectionId}");
            }

            return startIndex;
        }

        #endregion

        private bool IsParameterConstraintSatisfied(string macro,
            (string parameterName, string value)? parameterConstraint,
            string gcodeLine)
        {
            var parameterConstraintSatisfied = true;

            if (parameterConstraint != null)
            {
                var matcher =
                    new Regex($@"\s{parameterConstraint.Value.Item1}\s*=\s*{parameterConstraint.Value.Item2}");
                var match = matcher.Match(gcodeLine);

                if (!match.Success)
                {
                    parameterConstraintSatisfied = false;
                    Logger.Warn(
                        $"Could not verify parameter constraint {parameterConstraint.Value.Item1}={parameterConstraint.Value.Item2} in macro {macro} of section  {SectionId}");
                }
            }

            return parameterConstraintSatisfied;
        }

        private void ParseStringParameters(string macro, IList<(string, Action<string>)> stringParameters,
            string gcodeLine)
        {
            if (stringParameters != null)
            {
                foreach (var stringParameter in stringParameters)
                {
                    var matcher = new Regex($@"\s{stringParameter.Item1}\s*=.+");
                    var match = matcher.Match(gcodeLine);

                    if (match.Success)
                    {
                        if (!ParseSingleQuotedStringParameter(stringParameter, match.Value))
                        {
                            ParseNonQuotedStringParameter(match.Value, stringParameter);
                        }
                    }
                    else
                    {
                        Logger.Warn(
                            $"Could not find expected string parameter {stringParameter.Item1} in macro {macro} of section {SectionId}");
                    }
                }
            }
        }

        private void ParseNonQuotedStringParameter(string gcodeLine, (string, Action<string>) stringParameter)
        {
            var matcher = new Regex($@"\s{stringParameter.Item1}\s*=\s*\w+");
            var match = matcher.Match(gcodeLine);

            if (match.Success)
            {
                matcher = new Regex(@"=\s*\w+");
                match = matcher.Match(match.Value);

                matcher = new Regex(@"\w+");
                match = matcher.Match(match.Value);

                stringParameter.Item2.Invoke(match.Value);
            }
        }

        private bool ParseSingleQuotedStringParameter((string, Action<string>) stringParameter, string gcodeLine)
        {
            var success = false;

            var matcher = new Regex($@"\s{stringParameter.Item1}\s*='.+'");

            var match = matcher.Match(gcodeLine);

            if (match.Success)
            {
                success = true;

                matcher = new Regex(@"='.+'");
                match = matcher.Match(match.Value);

                matcher = new Regex(@"[\w\s]+");
                match = matcher.Match(match.Value);

                stringParameter.Item2.Invoke(match.Value);
            }

            return success;
        }

        private void ParseNumericParameters(string macro, IList<(string, Action<double>)> intParameters,
            string gcodeLine)
        {
            if (intParameters != null)
            {
                foreach (var intParameter in intParameters)
                {
                    var matcher = new Regex($@"\s{intParameter.Item1}\s*=\s*\d+.?\d*");
                    var match = matcher.Match(gcodeLine);

                    if (match.Success)
                    {
                        matcher = new Regex(@"\d+.?\d*");
                        match = matcher.Match(match.Value);

                        intParameter.Item2.Invoke(double.Parse(match.Value, CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        Logger.Warn(
                            $"Could not find expected int parameter {intParameter.Item1} in macro {macro} of section {SectionId}");
                    }
                }
            }
        }

        internal ILogger Logger { private get; set; }

        internal string SectionId { private get; set; }
    }
}
