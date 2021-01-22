using System.IO;
using Diz.Core.util;
using IX.Observable;

namespace Diz.Core.model.parser
{
    public class DizExpressionCollection
    {
        // ROM address -> expression
        public ObservableDictionary<int, DizExpression> Expressions = new ObservableDictionary<int, DizExpression>();
    }

    public class DizExpression
    {
        public string Expression { get; set; }

        private (int, string) Parse(Data data)
        {
            var isConstant = Expression[0] == '#'; 
            
            if (isConstant)
            {
                if (Expression[1] == '$')
                {
                    var constantStr = Expression.Substring(2); // check length/range
                    var parsedValue = ByteUtil.ByteParseHex(constantStr);
                    return (value: (int)parsedValue, final_txt: "");
                }
                else
                {
                    var possibleConstantName = Expression.Substring(1);
                    var constant = data.Constants.Constants[possibleConstantName];
                    return (value: constant.Value, final_txt: possibleConstantName);
                }
            }
            
            throw new InvalidDataException("Parse error");
        }

        public static bool ValidateValue(int valueToValidate, int valueMustEqual)
        {
            return valueToValidate == valueMustEqual;
        }
        
        public static bool ValidateNumBytes(int valueToValidate, int numBytesMustEqual)
        {
            var maxValue = (0x1ul << (8 * numBytesMustEqual))-1;
            return (uint) valueToValidate < maxValue;
        }
        
        // returns true if conditions match and there are no parse errors, else throws exception
        public (int, string) GetExpressionResult(int valueMustEqual, int numBytesMustEqual, Data data)
        {
            // TODO: replace with real and robust parsing. for now, this is fine
            // to demo the concept. it supports a very simple and inflexible grammar
            //
            // Do something like:
            // https://nblumhardt.com/2010/01/building-an-external-dsl-in-c/
            // Sprache is already installed in Diztinguish.

            var parseResult = Parse(data);
            if (!ValidateValue(parseResult.Item1, valueMustEqual))
                throw new InvalidDataException($"Expression must equal {parseResult.Item1} but instead it's {valueMustEqual}");
            
            if (!ValidateNumBytes(parseResult.Item1, numBytesMustEqual))
                throw new InvalidDataException($"Expression not correct number of bytes");

            return parseResult;
        }
    }
}