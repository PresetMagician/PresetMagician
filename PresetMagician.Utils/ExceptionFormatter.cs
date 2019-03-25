using System;
using System.Text;
using Catel;

namespace PresetMagician.Utils
{
    public class ExceptionFormatter
    {
        public static string GetFormattedException(Exception e)
        {
            var sb = new StringBuilder();
            if (e is AggregateException ae)
            {
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    sb.AppendLine(GetFormattedException(ex));
                }
            }
            else
            {
                sb.AppendLine(e.Flatten("", true));
            }

            return sb.ToString();
        }
    }
}