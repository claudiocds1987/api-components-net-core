namespace ApiComponents.DTOs
{
    public class AttributeValidationsDto
    {
        public bool required { get; set; } = false;
        public int? maxLength { get; set; }
        public int? minLength { get; set; }
    }
}
