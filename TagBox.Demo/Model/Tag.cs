
namespace TagBox.Demo.Model
{
    internal class Tag
    {
        public string Value { get; set; }

        public override string ToString()
        {
            return $"Contact: {Value}";
        }
    }
}
