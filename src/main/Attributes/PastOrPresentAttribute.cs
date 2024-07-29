using System.ComponentModel.DataAnnotations;

namespace iLib.src.main.Attributes
{
    public class PastOrPresentAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime dateTime)
            {
                return dateTime <= DateTime.Now;
            }
            return false;
        }
    }
}
