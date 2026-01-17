using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Transform;
using SunnyRewards.Helios.Common.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Constants;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Infrastructure.Helpers.Interface;
using SunnyRewards.Helios.Task.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Task.Infrastructure.Services;
using SunnyRewards.Helios.Task.Infrastructure.Services.Helpers;
using SunnyRewards.Helios.Task.Infrastructure.Services.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ISession = NHibernate.ISession;

namespace SunnyRewards.Helios.Task.Infrastructure.Helpers
{
    public class QueryGeneratorService : IQueryGeneratorService
    {
        private readonly ILogger<QueryGeneratorService> _logger;
        private const string ClassName = nameof(QueryGeneratorService);


        public QueryGeneratorService(ILogger<QueryGeneratorService> logger)
        {
            _logger = logger;
        }

        public async Task<(List<Dictionary<string, object>>, BaseResponseDto)> ExecuteDynamicQueryAsync(
           string baseSql,
           List<SearchAttributeDto> filters,
           IDictionary<string, object> requiredParameters,
           ISession session)
        {
            const string methodName = nameof(QueryGeneratorService);
            try
            {
                if (string.IsNullOrWhiteSpace(baseSql))
                    throw new ArgumentException("Base SQL cannot be null or empty.", nameof(baseSql));

                if (session == null)
                    throw new ArgumentNullException(nameof(session));

                var (whereClause, parameters, response) = BuildDynamicWhereClause(filters);
                if (response.ErrorCode != null)
                {
                    return (new(), response);
                }
                string sql = baseSql.Contains("WHERE", StringComparison.OrdinalIgnoreCase)
                    ? $"{baseSql} {(string.IsNullOrWhiteSpace(whereClause) ? "" : $"AND {whereClause}")}"
                    : $"{baseSql} {(string.IsNullOrWhiteSpace(whereClause) ? "" : $"WHERE {whereClause}")}";

                _logger.LogDebug("Generated SQL: {Sql}", sql);

                var query = session.CreateSQLQuery(sql)
                    .SetResultTransformer(Transformers.AliasToEntityMap);

                // Add required parameters
                if (requiredParameters != null)
                    foreach (var kvp in requiredParameters)
                        query.SetParameter(kvp.Key, kvp.Value);

                // Add dynamic parameters
                if (parameters != null)
                    foreach (var kvp in parameters)
                        query.SetParameter(kvp.Key, kvp.Value);

                var result = await query.ListAsync();

                return (result
                    .Cast<Hashtable>()
                    .Select(row => row.Cast<DictionaryEntry>()
                        .ToDictionary(entry => entry.Key.ToString(), entry => entry.Value))
                    .ToList(), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{className}.{methodName}: API -  ERROR:{msg}, Error Code:{errorCode}", ClassName, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                throw;
            }
        }

        private (string? condition, Dictionary<string, object> parameters, BaseResponseDto? responseDto) BuildDynamicWhereClause(List<SearchAttributeDto> filters)
        {
            BaseResponseDto response = new BaseResponseDto();
            var conditions = new List<string>();
            var parameters = new Dictionary<string, object>();

            var operatorHandlers = new Dictionary<string, Func<SearchAttributeDto, string, Dictionary<string, object>, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["IN"] = HandleInNotIn,
                ["NOT IN"] = HandleInNotIn,
                ["BETWEEN"] = HandleBetween,
                ["NOT BETWEEN"] = HandleBetween,
                ["LIKE"] = HandleLike,
                ["NOT LIKE"] = HandleLike,
                ["IS NULL"] = (attr, paramBase, paramDict) => $"{attr.Column} IS NULL",
                ["IS NOT NULL"] = (attr, paramBase, paramDict) => $"{attr.Column} IS NOT NULL",
                ["="] = HandleEquality,
                ["!="] = HandleEquality
            };

            foreach (var attr in filters.Where(f => !string.IsNullOrWhiteSpace(f.Column)))
            {
                if (Constant.ColumnAliasMap.TryGetValue(attr.Column, out string alias) &&
    !string.IsNullOrEmpty(alias))
                {
                    attr.Column = $"{alias}.{attr.Column}";
                }

                string paramBase = attr.Column.Replace(".", "_");
                string sqlOp = GetSqlOperator(attr.Operator);
                if (string.Equals(sqlOp, Constant.InvalidOperation))
                {
                    response.ErrorCode = StatusCodes.Status400BadRequest;
                    response.ErrorMessage = Constant.InvalidOperation;
                    return (null, parameters, response);
                }
                if (!operatorHandlers.TryGetValue(sqlOp, out var handler))
                { attr.Operator = sqlOp; handler = HandleDefault; }

                string condition = handler(attr, paramBase, parameters);

                if (conditions.Count > 0)
                    conditions.Add(attr.Criteria?.ToUpperInvariant() ?? "AND");

                conditions.Add(condition);
            }

            string where = string.Join(" ", conditions);
            _logger.LogDebug("Generated dynamic WHERE clause: {Where}", where);

            return (where, parameters, response);
        }

        #region Operator Handlers

        private static string HandleInNotIn(SearchAttributeDto attr, string paramBase, Dictionary<string, object> parameters)
        {
            JArray values = NormalizeToJArray(attr.Value);
            if (!values.Any())
                throw new ArgumentException($"{attr.Operator} operator requires at least one value.");

            var paramNames = values.Select((v, i) =>
            {
                string p = $"{paramBase}_in_{i}";
                parameters[p] = ConvertValue(v, attr.DataType);

                // If data type is JSON/JSONB, cast parameter in SQL
                if (attr.DataType.Equals("json", StringComparison.OrdinalIgnoreCase) ||
                    attr.DataType.Equals("jsonb", StringComparison.OrdinalIgnoreCase))
                {
                    return $"CAST(:{p} AS jsonb)";
                }

                return $":{p}";
            }).ToList();

            return $"{attr.Column} {GetSqlOperator(attr.Operator)} ({string.Join(", ", paramNames)})";
        }


        private static string HandleBetween(SearchAttributeDto attr, string paramBase, Dictionary<string, object> parameters)
        {
            JArray values = NormalizeToJArray(attr.Value);
            if (values.Count != 2)
                throw new ArgumentException($"{attr.Operator} operator requires exactly two values.");

            string startParam = $"{paramBase}_start";
            string endParam = $"{paramBase}_end";

            parameters[startParam] = ConvertValue(values[0], attr.DataType);
            parameters[endParam] = ConvertValue(values[1], attr.DataType);

            return $"{attr.Column} {GetSqlOperator(attr.Operator)} :{startParam} AND :{endParam}";
        }

        private static string HandleLike(SearchAttributeDto attr, string paramBase, Dictionary<string, object> parameters)
        {
            string p = $"{paramBase}_like";
            var value = ConvertValue(attr.Value, attr.DataType)?.ToString() ?? string.Empty;

            value = attr.Operator.ToUpperInvariant() switch
            {
                "CONTAINS" or "NOTCONTAINS" => $"%{value}%",
                "STARTSWITH" => $"{value}%",
                "ENDSWITH" => $"%{value}",
                _ => value
            };

            parameters[p] = value;
            if (attr.DataType.Equals("json", StringComparison.OrdinalIgnoreCase) ||
             attr.DataType.Equals("jsonb", StringComparison.OrdinalIgnoreCase))
            {
                return $"{attr.Column}  {GetSqlOperator(attr.Operator)} CAST(:{p} AS jsonb)";
            }
            return $"{attr.Column} {GetSqlOperator(attr.Operator)} :{p}";
        }

        private static string HandleEquality(SearchAttributeDto attr, string paramBase, Dictionary<string, object> parameters)
        {
            string op = GetSqlOperator(attr.Operator);
            if (attr.Operator.ToUpperInvariant() == "EMPTY")
                return $"{attr.Column} = ''";
            if (attr.Operator.ToUpperInvariant() == "NOTEMPTY")
                return $"{attr.Column} != ''";

            string p = $"{paramBase}_p";
            parameters[p] = ConvertValue(attr.Value, attr.DataType);

            // JSON special handling
            if (attr.DataType.Equals("json", StringComparison.OrdinalIgnoreCase) ||
                attr.DataType.Equals("jsonb", StringComparison.OrdinalIgnoreCase))
            {
                return $"{attr.Column} {op} CAST(:{p} AS jsonb)";
            }

            return $"{attr.Column} {op} :{p}";
        }

        private static string HandleDefault(SearchAttributeDto attr, string paramBase, Dictionary<string, object> parameters)
        {
            string p = $"{paramBase}_p";
            parameters[p] = ConvertValue(attr.Value, attr.DataType);

            return $"{attr.Column} {attr.Operator} :{p}";
        }

        #endregion

        #region Helpers

        private static JArray NormalizeToJArray(object value)
        {
            if (value == null)
                return new JArray();

            // Convert System.Text.Json.JsonElement to .NET object
            if (value is JsonElement json)
            {
                value = json.ValueKind switch
                {
                    JsonValueKind.String => json.GetString(),
                    JsonValueKind.Number => json.TryGetInt64(out var l) ? (object)l : json.GetDecimal(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Array => new JArray(json.EnumerateArray().Select(x => ConvertJsonElementToObject(x))),
                    JsonValueKind.Object => json.GetRawText(),
                    JsonValueKind.Null => null,
                    _ => json.GetRawText()
                };
            }

            switch (value)
            {
                case JArray jArray: return jArray;
                case string str when str.TrimStart().StartsWith("[") && str.TrimEnd().EndsWith("]"): return JArray.Parse(str);
                case IEnumerable<object> list: return new JArray(list);
                default: return new JArray(value);
            }
        }

        // Helper to convert each JsonElement in an array
        private static object ConvertJsonElementToObject(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out var l) ? (object)l : element.GetDecimal(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => element.GetRawText()
            };
        }



        private static object ConvertValue(object value, string dataType)
        {
            if (value == null)
                return null;

            // Step 1: unwrap JsonElement (if any)
            if (value is JsonElement json)
            {
                value = json.ValueKind switch
                {
                    JsonValueKind.String => json.GetString(),
                    JsonValueKind.Number => json.TryGetInt64(out var l)
                                                ? l
                                                : json.TryGetDouble(out var d)
                                                    ? d
                                                    : json.GetRawText(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Object or JsonValueKind.Array => json.GetRawText(),
                    JsonValueKind.Null => null,
                    _ => json.ToString()
                };
            }

            // Step 2: handle JSON/JSONB types
            if (dataType.Equals("json", StringComparison.OrdinalIgnoreCase) ||
                dataType.Equals("jsonb", StringComparison.OrdinalIgnoreCase))
            {
                return value?.ToString();
            }

            // Step 3: normalize string before conversion
            if (value is string s)
                s = s.Trim();

            // Step 4: convert based on declared data type
            switch (dataType.Trim().ToLowerInvariant())
            {
                case "int":
                case "integer":
                    if (value is int i)
                        return i;
                    if (int.TryParse(value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var intVal))
                        return intVal;
                    break;

                case "decimal":
                    if (value is decimal d)
                        return d;
                    if (decimal.TryParse(value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var decVal))
                        return decVal;
                    break;

                case "datetime":
                    if (value is DateTime dt)
                        return dt;
                    if (DateTime.TryParse(value?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                        return parsedDate;
                    break;

                case "bool":
                case "boolean":
                    if (value is bool b)
                        return b;
                    if (bool.TryParse(value?.ToString(), out var boolVal))
                        return boolVal;
                    // Handle numeric bools (e.g. "1" or "0")
                    if (value?.ToString() == "1") return true;
                    if (value?.ToString() == "0") return false;
                    break;
            }

            // Default: return as string
            return value?.ToString()!;
        }




        public static string GetSqlOperator(string op)
        {
            if (string.IsNullOrWhiteSpace(op))
                return "=";

            return op.Trim().ToUpperInvariant() switch
            {
                "EQUALS" or "EQUALSTO" or "EQUAL" or "EQUALTO" or "EQ" or "=" => "=",
                "NOTEQUALS" or "NOTEQUALSTO" or "NOTEQUAL" or "NOTEQUALTO" or "NE" or "!=" => "!=",
                "LESSTHAN" or "LT" or "<" => "<",
                "LESSTHANOREQUALS" or "LESSTHANOREQUALTO" or "LESSTHANOREQUALSTO" or "LTE" or "<=" => "<=",
                "GREATERTHAN" or "GT" or ">" => ">",
                "GREATERTHANOREQUALS" or "GREATERTHANOREQUALTO" or "GREATERTHANOREQUALSTO" or "GTE" or ">=" => ">=",
                "IN" => "IN",
                "NOTIN" => "NOT IN",
                "CONTAINS" => "LIKE",
                "NOTCONTAINS" => "NOT LIKE",
                "STARTSWITH" => "LIKE",
                "ENDSWITH" => "LIKE",
                "NULL" => "IS NULL",
                "NOTNULL" => "IS NOT NULL",
                "EMPTY" => "=",
                "NOTEMPTY" => "!=",
                "INRANGE" => "BETWEEN",
                "NOTINRANGE" => "NOT BETWEEN",
                _ => Constant.InvalidOperation
            };
        }

        #endregion
    }
}
