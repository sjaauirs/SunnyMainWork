using Microsoft.Extensions.Logging;
using Sunny.Benefits.Bff.Core.Domain.Dtos;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using System.Reflection;

namespace Sunny.Benefits.Bff.Infrastructure.Services
{
    public class DynamicQueryProcessor : IDynamicQueryProcessor
    {
        private readonly ILogger<DynamicQueryProcessor> _logger;

        public DynamicQueryProcessor(ILogger<DynamicQueryProcessor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Dynamically resolves a property value from the <see cref="DynamicFilterContext"/> 
        /// based on the given key, ignoring case sensitivity.
        /// </summary>
        /// <param name="filterContext">The dynamic context containing various filter objects (e.g., Consumer, Task, Cohort).</param>
        /// <param name="key">
        /// The name of the property to retrieve. 
        /// This can be provided in any casing (e.g., "consumer", "Consumer", "CONSUMER").
        /// </param>
        /// <returns>
        /// The value of the matching property from <paramref name="filterContext"/>, 
        /// or <c>null</c> if no matching property exists.
        /// </returns>
        public object? GetFilterObject(DynamicFilterContext filterContext, string key)
        {
            try
            {
                if (filterContext == null || string.IsNullOrWhiteSpace(key))
                    return null;

                var prop = filterContext
                    .GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(p => string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase));

                return prop?.GetValue(filterContext);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error retrieving filter object for key: {Key}", key);
                return null;
            }
        }

        /// <summary>
        /// Evaluates multiple contexts, each with their own list of conditions, against the provided filter context.
        /// </summary>
        /// <param name="contexts"></param>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        public bool EvaluateConditionsForAllContexts(Dictionary<string, List<Condition>> contexts, DynamicFilterContext filterContext)
        {
            try
            {
                bool overallResult = false;

                foreach (var kvp in contexts)
                {
                    var key = kvp.Key.ToLowerInvariant().Trim();
                    var conditions = kvp.Value;

                    if (conditions == null || conditions.Count == 0)
                        continue;

                    var contextObj = GetFilterObject(filterContext, key);
                    if (contextObj == null)
                        continue;

                    bool contextResult = EvaluateConditionsForContext(contextObj, conditions);
                    overallResult |= contextResult;
                }

                return overallResult;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error evaluating conditions for all contexts");
                return false;
            }
        }

        /// <summary>
        /// Evaluates a list of suppression <see cref="Condition"/> objects against a given context instance.
        /// </summary>
        /// <param name="context">
        /// The object to evaluate conditions against (e.g., <c>filterContext.Consumer</c>).
        /// </param>
        /// <param name="conditions">
        /// A list of suppression conditions that describe attribute-based logical checks 
        /// (e.g., <c>EligibilityMonths &lt; 6</c> AND <c>onboardingState = VERIFIED</c>).
        /// </param>
        /// <returns>
        /// <c>true</c> if all conditions (when combined based on their <c>criteria</c>) 
        /// evaluate to <c>true</c>; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Conditions are evaluated **left to right**, and each condition’s 
        /// <c>criteria</c> field determines how it combines with the next condition:
        /// <list type="bullet">
        /// <item><description><c>AND</c> → both must be true</description></item>
        /// <item><description><c>OR</c> → either can be true</description></item>
        /// </list>
        /// The last condition in the list should not have a <c>criteria</c> value; 
        /// it is ignored when combining results.<br/>
        /// Unknown criteria defaults to <c>AND</c> to prevent false positives.<br/>
        /// This design allows the condition chain to behave predictably, similar 
        /// to SQL WHERE clause evaluation.
        /// </remarks>
        public bool EvaluateConditionsForContext(object context, List<Condition> conditions)
        {
            try
            {
                if (conditions.Count == 0)
                    return false;

                var results = conditions.Select(c => EvaluateCondition(c, context)).ToList();
                bool overall = results[0];

                for (int i = 0; i < results.Count - 1; i++)
                {
                    var criteria = (conditions[i].Criteria ?? "AND").Trim().ToUpperInvariant();
                    var next = results[i + 1];

                    if (criteria == "AND")
                        overall = overall && next;
                    else if (criteria == "OR")
                        overall = overall || next;
                }

                return overall;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error evaluating conditions for context of type {Type}", context.GetType().Name);
                return false;
            }
        }

        /// <summary>
        /// Evaluates a single <see cref="Condition"/> against the given context object.
        /// </summary>
        /// <param name="cond">
        /// The condition describing the attribute to check, operator, value, and datatype.
        /// </param>
        /// <param name="context">
        /// The object whose property will be compared (e.g., <c>Consumer</c> filter model).
        /// </param>
        /// <returns>
        /// <c>true</c> if the condition evaluates successfully based on the comparison logic; 
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Uses reflection to dynamically locate the property specified by <c>attribute_name</c>.
        /// Supports data types: <c>int</c>, <c>decimal</c>, <c>datetime</c>, <c>string</c>, and <c>bool</c>.  
        /// Supports flexible operators including: 
        /// <c>&lt;, &lt;=, &gt;, &gt;=, =, !=, IN, NOT IN, CONTAINS, NOT CONTAINS, EMPTY, NOT EMPTY, IS NULL, IS NOT NULL</c>.  
        /// Case-insensitive for both operator and property name.
        /// If the property is missing or an exception occurs during conversion, 
        /// the method logs a warning and safely returns <c>false</c> to avoid disruption.
        /// </remarks>
        private bool EvaluateCondition(Condition cond, object context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cond.AttributeName) ||
                    string.IsNullOrWhiteSpace(cond.Operator) ||
                    string.IsNullOrWhiteSpace(cond.DataType))
                    return false;

                var prop = context.GetType()
                    .GetProperty(cond.AttributeName.Replace("_", string.Empty), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (prop == null)
                    return false;

                var leftValue = prop.GetValue(context);

                var op = NormalizeOperator(cond.Operator);
                var type = cond.DataType.Trim().ToLowerInvariant();

                return type switch
                {
                    "int" => CompareValues(leftValue == null ? (int?)null : Convert.ToInt32(leftValue), cond.AttributeValue!, op),
                    "decimal" => CompareValues(leftValue == null ? (decimal?)null : Convert.ToDecimal(leftValue), cond.AttributeValue!, op),
                    "datetime" => CompareValues(leftValue == null ? (DateTime?)null : Convert.ToDateTime(leftValue), cond.AttributeValue!, op),
                    "string" => CompareValues(leftValue?.ToString(), cond.AttributeValue ?? "", op),
                    "bool" or "boolean" => CompareValues(leftValue == null ? (bool?)null : Convert.ToBoolean(leftValue), cond.AttributeValue!, op),
                    _ when prop.PropertyType.IsEnum => CompareValues(
                        leftValue == null ? (int?)null :
                        (int)leftValue!,
                        ((int)Enum.Parse(prop.PropertyType, cond.AttributeValue!, true)).ToString(),
                        op),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error evaluating suppression condition for {Property}", cond.AttributeName);
                return false;
            }
        }

        /// <summary>
        /// Normalizes operator to a standard uppercase form for consistent matching.
        /// Handles variations like "notin", "NOT IN", "Not In", etc.
        /// </summary>
        private static string NormalizeOperator(string op)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(op)) return string.Empty;

                op = op.Trim().Replace(" ", "").ToUpperInvariant();

                return op switch
                {
                    "EQUALS" or "EQUALSTO" or "EQUAL" or "EQUALTO" or "EQ" => "=",
                    "NOTEQUALS" or "NOTEQUALSTO" or "NOTEQUAL" or "NOTEQUALTO" or "NE" => "!=",
                    "LESSTHAN" or "LT" => "<",
                    "LESSTHANOREQUALS" or "LESSTHANOREQUALTO" or "LESSTHANOREQUALSTO" or "LTE" => "<=",
                    "GREATERTHAN" or "GT" => ">",
                    "GREATERTHANOREQUALS" or "GREATERTHANOREQUALTO" or "GREATERTHANOREQUALSTO" or "GTE" => ">=",
                    "NOTIN" => "NOT IN",
                    "IN" => "IN",
                    "NOTCONTAINS" => "NOT CONTAINS",
                    "CONTAINS" => "CONTAINS",
                    "NOTEMPTY" => "NOT EMPTY",
                    "EMPTY" => "EMPTY",
                    "NOTNULL" => "IS NOT NULL",
                    "NULL" => "IS NULL",
                    "INRANGE" => "IN RANGE",
                    "NOTINRANGE" => "NOT IN RANGE",
                    "STARTSWITH" => "STARTSWITH",
                    "ENDSWITH" => "ENDSWITH",
                    _ => op // For standard operators like <, <=, >, >=, =, !=
                };
            }
            catch
            {
                return op;
            }
        }

        /// <summary>
        /// Compares two values using a wide range of supported operators.
        /// Handles numbers, strings, dates, booleans, and enums.
        /// </summary>
        private bool CompareValues<T>(T? left, string rightValue, string op)
        {
            try
            {
                if (left == null)
                    return op.Equals("IS NULL", StringComparison.OrdinalIgnoreCase);

                op = op.ToUpperInvariant();
                string leftStr = left?.ToString() ?? string.Empty;

                switch (op)
                {
                    case "<":
                    case "<=":
                    case ">":
                    case ">=":
                    case "=":
                    case "==":
                    case "!=":
                    case "<>":
                        if (left is IComparable comparable)
                        {
                            var right = ConvertTypeTo<T>(rightValue);
                            int cmp = comparable.CompareTo(right);
                            return op switch
                            {
                                "<" => cmp < 0,
                                "<=" => cmp <= 0,
                                ">" => cmp > 0,
                                ">=" => cmp >= 0,
                                "=" or "==" => cmp == 0,
                                "!=" or "<>" => cmp != 0,
                                _ => false
                            };
                        }
                        break;

                    case "IN":
                        return rightValue.Split(',')
                            .Select(v => v.Trim().ToLowerInvariant())
                            .Contains(leftStr.ToLowerInvariant());

                    case "NOT IN":
                        return !rightValue.Split(',')
                            .Select(v => v.Trim().ToLowerInvariant())
                            .Contains(leftStr.ToLowerInvariant());

                    case "CONTAINS":
                        return leftStr.Contains(rightValue, StringComparison.OrdinalIgnoreCase);

                    case "NOT CONTAINS":
                        return !leftStr.Contains(rightValue, StringComparison.OrdinalIgnoreCase);

                    case "STARTSWITH":
                        return leftStr.StartsWith(rightValue, StringComparison.OrdinalIgnoreCase);

                    case "ENDSWITH":
                        return leftStr.EndsWith(rightValue, StringComparison.OrdinalIgnoreCase);

                    case "IS NULL":
                        return left == null;

                    case "IS NOT NULL":
                        return left != null;

                    case "EMPTY":
                        return string.IsNullOrWhiteSpace(leftStr);

                    case "NOT EMPTY":
                        return !string.IsNullOrWhiteSpace(leftStr);

                    case "IN RANGE":
                    case "NOT IN RANGE":
                        {
                            var parts = rightValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            if (parts.Length == 2 && left is IComparable cmp)
                            {
                                var lower = (T)Convert.ChangeType(parts[0], typeof(T));
                                var upper = (T)Convert.ChangeType(parts[1], typeof(T));
                                bool inRange = cmp.CompareTo(lower) >= 0 && cmp.CompareTo(upper) <= 0;
                                return op == "IN RANGE" ? inRange : !inRange;
                            }
                            return false;
                        }

                    default:
                        return false;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error comparing values: {Left} {Operator} {Right}", left, op, rightValue);
                return false;
            }
        }

        private T ConvertTypeTo<T>(object rightValue)
        {
            if (rightValue == null || rightValue is DBNull)
                return default;

            Type targetType = typeof(T);

            // If it's a Nullable<T>, get the underlying type
            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            object convertedValue = Convert.ChangeType(rightValue, underlyingType);

            return (T)convertedValue;
        }
    }
}
