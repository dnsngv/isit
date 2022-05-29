using System;
using System.Collections.Generic;
using System.Linq;

namespace infsystem
{
    enum ValueType
    {
        String, Bool, Number,
    }
    
    enum ExpressionType
    {
        And, Or,
    }
    
    enum ComparisonType
    {
        Equals, NotEquals, LessThan, GreaterThan, Defined, NotDefined, GreaterOrEquals, LessOrEquals,
    }

    struct Rule
    {
        public Condition Condition;
        public Fact[] Assertions;
        public int Salience;
        
        public override string ToString() => $"([{Salience}] {Condition} ==> {string.Join<Fact>("; ",Assertions)})";
    }
    
    // Значение факта или значение, с которым сравнивается факт
    class Value
    {
        public string? StringValue;
        public double? NumberValue;
        public ValueType Type
        {
            get
            {
                if (StringValue != null) return ValueType.String;
                if (NumberValue != null) return ValueType.Number;
                throw new Exception("Value is not defined");
            }
        }

        public override string ToString()
        {
            if (StringValue != null) return StringValue;
            if (NumberValue != null) return NumberValue.ToString();
            throw new Exception("Value is not defined");
        }
    }
    
    // Условие может быть либо простым условием сравнения факта со значением,
    // либо конъюнкцией/дизъюнкцией набора других условий
    abstract class Condition
    {
        // Выполняется ли условие при наличии данного набора фактов
        // В том числе возвращает список конкретных фактов, которых достаточно для выполнения условия
        public abstract (bool CanActivate, List<Fact> Requirements) Evaluate(IEnumerable<Fact> facts);

        // Количество элементарных условий-сравнений, из которых состоит данное условие
        // Используется для разрешения конфликтов
        public abstract int Specificity { get; }
    }
    
    class ConditionExpression : Condition
    {
        public string Operator;
        public Condition[] Conditions;
        
        public ExpressionType Type
        {
            get
            {
                switch (Operator)
                {
                    case "AND":
                    case "And":
                    case "and":
                    case "&&":
                    case "&":
                        return ExpressionType.And;
                    case "OR":
                    case "Or":
                    case "or":
                    case "||":
                    case "|":
                        return ExpressionType.Or;
                    default:
                        throw new FormatException($"Operator ({Operator}) is not supported. Supported operators: AND, OR, or equivalent.");
                }
            }
        }

        public override (bool CanActivate, List<Fact> Requirements) Evaluate(IEnumerable<Fact> facts)
        {
            var results = Conditions.Select(condition => condition.Evaluate(facts));
            if (Type == ExpressionType.And)
            {
                var requirements = results
                    .SelectMany(result => result.Requirements)
                    .ToList();
                return (results.All(result => result.CanActivate), requirements);
            }

            foreach (var result in results)
            {
                if (result.CanActivate) return result;
            }

            return (false, new());
        }

        public override int Specificity => Conditions.Select(condition => condition.Specificity).Sum();
        
        public override string ToString() => $"({String.Join<Condition>(Type == ExpressionType.And ? " && " : " || ", Conditions)})";
    }
    
    // Проверка, определен или не определен факт с заданным именем, или сравнение факта с заданным значением
    class ValueCondition : Condition
    {
        public string FactName;
        public string Comparison;
        public Value? Value;
        
        public ComparisonType? Type =>
            Comparison switch
            {
                "<" when Value is {Type: ValueType.Number} => ComparisonType.LessThan,
                ">" when Value is {Type: ValueType.Number} => ComparisonType.GreaterThan,
                ">=" when Value is {Type: ValueType.Number} => ComparisonType.GreaterOrEquals,
                "<=" when Value is {Type: ValueType.Number} => ComparisonType.LessOrEquals,
                "=" => ComparisonType.Equals,
                "==" => ComparisonType.Equals,
                "!=" => ComparisonType.NotEquals,
                "<>" => ComparisonType.NotEquals,
                "!" when Value is null => ComparisonType.Defined,
                "?" when Value is null => ComparisonType.NotDefined,
                _ => throw new FormatException($"Comparison ({Comparison}) is invalid for given value ({Value}) or not supported")
            };

        public override (bool CanActivate, List<Fact> Requirements) Evaluate(IEnumerable<Fact> facts)
        {
            Fact? foundFact = facts.FirstOrDefault(fact => fact.Name == FactName);

            if (foundFact is null)
            {
                // Если факт с заданным именем не найден и условие - "Факт не определен", то условие выполняется
                return (Type == ComparisonType.NotDefined, new() );
            }
            else if (Type == ComparisonType.Defined)
            {
                // Если факт с заданным именем найден и условие - "Факт определен", то условие выполняется
                return (true, new() { foundFact } );
            }

            var value = foundFact.Value;

            if (Value is null) return (true, new() { foundFact } );

            if (foundFact.Value.Type != Value.Type) throw new ArgumentException($"Fact ({value}) and condition ({Value}) types are incompatible");

            var type = value.Type;
            // Проверка, удовлетворяет ли найденный факт сравнению, описанному в данном условии
            switch (Type)
            {
                case ComparisonType.Equals when value.ToString() == Value.ToString():
                case ComparisonType.NotEquals when value.ToString() != Value.ToString():
                case ComparisonType.LessThan when type == ValueType.Number && value.NumberValue < Value.NumberValue:
                case ComparisonType.GreaterThan when type == ValueType.Number && value.NumberValue > Value.NumberValue:
                case ComparisonType.LessOrEquals when type == ValueType.Number && value.NumberValue <= Value.NumberValue:
                case ComparisonType.GreaterOrEquals when type == ValueType.Number && value.NumberValue >= Value.NumberValue:
                    return (true, new() { foundFact } );
                case ComparisonType.Equals:
                case ComparisonType.NotEquals:
                case ComparisonType.LessThan when type == ValueType.Number:
                case ComparisonType.GreaterThan when type == ValueType.Number:
                case ComparisonType.LessOrEquals when type == ValueType.Number:
                case ComparisonType.GreaterOrEquals when type == ValueType.Number:
                    return (false, new() { foundFact } );
                default:
                    throw new ArgumentException($"Fact ({value}) and condition ({Value}) values cannot be compared");
            }
        }

        public override int Specificity => 1;
        
        public override string ToString() => $"'{FactName}' {Comparison} {Value}";
    }
    
    // Факт может иметь определенное значение, его значение может определяться вопросом к пользователю,
    // либо (при отсутствии значения и вопроса) факт является искомым результатом
    class Fact
    {
        public string Name;
        public Value? Value;
        public Input? Input;

        public bool IsResult => Value == null && Input == null;

        public override string ToString()
        {
            string value = $"'{Name}'";
            if (Input != null) value += $" ?? {Input}";
            if (Value != null) value += $" == {Value}";
            return value;
        }
    }

    // Вопрос к пользователю. Это может быть вопрос с выбором одного из предложенных значений, 
    struct Input
    {
        public string Question;
        public string[]? Values;

        public ValueType Type
        {
            get
            {
                if (Values != null) return ValueType.String;
                return ValueType.Bool;
            }
        }

        public override string ToString()
        {
            string value = $"'{Question}'";
            if (Values != null) value += $" ({string.Join("/", Values)})";
            return value;
        }

    }
}