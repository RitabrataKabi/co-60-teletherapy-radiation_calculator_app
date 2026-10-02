namespace RadApp.Utilities
{
    public static class MakeMyLifeEasier
    {
        public static void printf(string s = "")
        {
            Console.WriteLine(s);
        }

        public static void ExitProgram()
        {
            System.Environment.Exit(0);
        }

        public static float Clamp(float input, float minValue = 0, float maxValue = 1)
        {
            if (input < minValue)
            {
                return minValue;
            }
            else if (input > maxValue)
            {
                return maxValue;
            }

            return input;
        }
    }
}