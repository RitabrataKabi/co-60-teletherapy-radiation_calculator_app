using System;
using System.Numerics;

namespace RadApp.Input
{
    public static class ParsingVector2
    {
        public static Vector2 parseToVector2(string input)
        {
            string newInput = input.Trim('(', '{', '[', ')', '}', ']');
            string[] tokens = newInput.Split(new char[] { ' ', ':', ',', 'X', 'x', 'Y', 'y' }, StringSplitOptions.RemoveEmptyEntries);

            Vector2 vector = Vector2.Zero;

            if (tokens.Length >= 2)
            {
                //means vector 2 is possible
                //also need to set a minimum value, clamping here
                float.TryParse(tokens[0], out vector.X);
                float.TryParse(tokens[1], out vector.Y);
            }

            return vector;
        }

    }

    public class HandleInput()
    {
        /*seperating out the main input layer into a seperate function to run it time and again in one process of the program*/
        public void TakeInput()
        {
            //tackling the field size input
            Console.WriteLine("Type in field size as (X, Y) (in cm) : ");

            string inputStream = Console.ReadLine() ?? "(10, 10)";

            Vector2 fieldSize = ParsingVector2.parseToVector2(inputStream);

            //tackling depth (in cm)
            Console.WriteLine("Type in Depth (in cm) : ");

            float depth = 7f;
            float.TryParse(Console.ReadLine() ?? "7", out depth);

            //tackling dosage (in cGy)
            Console.WriteLine("Type in required dosage (in cGy): ");

            float dosage = 100f;
            float.TryParse(Console.ReadLine() ?? "100", out dosage);

            #region DebugLines
            Console.WriteLine("field size (in cm) = {0}; dosage (cGy) = {1}; depth (in cm) = {2}", fieldSize, dosage, depth);
            #endregion

            //exiting the program with exit code 0, i.e. safely
            ExitProgram();
        }
    }
}