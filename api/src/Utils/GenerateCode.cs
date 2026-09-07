namespace api_bora_trampar.src.Utils
{
    public static class GenerateCode
    {
        public static string GenerateCodeNumber()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        public static string GenerateNextCode(string parentCode, long nextSequence)
        {
            if (string.IsNullOrWhiteSpace(parentCode))
            {
                return nextSequence.ToString();
            }

            return $"{parentCode.Trim()}.{nextSequence}";
        }
    }
}