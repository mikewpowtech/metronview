using System;

namespace Metron2Parser
{
    public class Variable<T> : IComparable where T: IComparable
    {
        public string VariableName { get; }
        public T Value { get; }
        public bool IsVariable => null != VariableName;

        public Variable(T value)
        {
            Value = value;
        }

        public Variable(T value, string variableName)
        {
            VariableName = variableName;
            Value = value;
        }

        int IComparable.CompareTo(object other)
        {
            if (null == other)
                return 1;
            Variable<T> oth = other as Variable<T>;
            if (null == oth)
                throw new Exception("Variable<T> only knows how to compare with other Variable<T>");
            return Value.CompareTo(oth.Value);
        }
    }
}
