using IX.Observable;

namespace Diz.Core.model.parser
{
    public class DizConstant
    {
        public int Value { get; set; }
    }

    public class DizConstantCollection
    {
        // constant names -> constant values
        public ObservableDictionary<string, DizConstant> Constants { get; set; } = new ObservableDictionary<string, DizConstant>();
    }
}