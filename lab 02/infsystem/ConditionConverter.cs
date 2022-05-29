using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace infsystem
{
    // JSON-конвертер условий. Нужен для правильной полиморфической десериализации
    // классов-наследников от абстрактного класса Condition
    internal class ConditionConverter : JsonConverter<Condition>
    {
        
        public override bool CanConvert(Type typeToConvert) => typeof(Condition).IsAssignableFrom(typeToConvert);

        public override Condition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Cannot convert JSON to type {typeToConvert}: invalid JSON structure");
            }
            
            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Cannot convert JSON to type {typeToConvert}: invalid JSON structure");
            }
            
            var propertyName = reader.GetString();

            Condition condition;
            switch (propertyName)
            {
                case nameof(ConditionExpression.Operator):
                case nameof(ConditionExpression.Conditions):
                    condition = new ConditionExpression();
                    break;
                case nameof(ValueCondition.FactName):
                case nameof(ValueCondition.Comparison):
                case nameof(ValueCondition.Value):
                    condition = new ValueCondition();
                    break;
                default:
                    throw new JsonException($"Cannot convert JSON to type {typeToConvert}: property name {propertyName} not recognized");
            }

            do
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return condition;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case nameof(ConditionExpression.Operator)
                            when condition is ConditionExpression conditionExpression:
                            var operatorString = reader.GetString();
                            if (operatorString is null) throw new JsonException($"Cannot convert JSON to type {typeToConvert}: property {propertyName} is not nullable");
                            conditionExpression.Operator = operatorString;
                            break;
                        case nameof(ConditionExpression.Conditions)
                            when condition is ConditionExpression conditionExpression:
                            var conditions = JsonSerializer.Deserialize<Condition[]>(ref reader, options);
                            if (conditions is null) throw new JsonException($"Cannot convert JSON to type {typeToConvert}: property {propertyName} is not nullable");
                            conditionExpression.Conditions = conditions;
                            break;
                        case nameof(ValueCondition.FactName) when condition is ValueCondition valueCondition:
                            var factName = reader.GetString();
                            if (factName is null) throw new JsonException($"Cannot convert JSON to type {typeToConvert}: property {propertyName} is not nullable");
                            valueCondition.FactName = factName;
                            break;
                        case nameof(ValueCondition.Comparison) when condition is ValueCondition valueCondition:
                            var comparison = reader.GetString();
                            valueCondition.Comparison = comparison;
                            break;
                        case nameof(ValueCondition.Value) when condition is ValueCondition valueCondition:
                            var value = JsonSerializer.Deserialize<Value>(ref reader, options);
                            valueCondition.Value = value;
                            break;
                        default:
                            throw new JsonException($"Cannot convert JSON to type {typeToConvert}: property name {propertyName} not recognized");
                    }
                }
            } while (reader.Read());

            throw new JsonException($"Cannot convert JSON to type {typeToConvert}: invalid JSON structure");
        }

        public override void Write(Utf8JsonWriter writer, Condition condition, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            switch (condition)
            {
                case ConditionExpression expression:
                    writer.WriteString(nameof(expression.Operator), expression.Operator);
                    writer.WritePropertyName(nameof(expression.Conditions));
                    JsonSerializer.Serialize(writer, expression.Conditions, options);
                    break;
                case ValueCondition value:
                    writer.WriteString(nameof(value.FactName), value.FactName);
                    if (value.Comparison is not null) writer.WriteString(nameof(value.Comparison), value.Comparison);
                    if (value.Value is not null)
                    {
                        writer.WritePropertyName(nameof(value.Value));
                        JsonSerializer.Serialize(writer, value.Value, options);
                    }
                    break;
                default:
                    throw new JsonException($"Unknown condition type {condition.GetType()}");
            }
            writer.WriteEndObject();
        }
    }
}