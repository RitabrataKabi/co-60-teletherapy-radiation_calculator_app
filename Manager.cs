using System;
using RadApp.Input;

namespace ManagerNM
{
    public class Manager
    {
        HandleInput handleInput = new HandleInput();

        public void Start(string[]? args = null)
        {
            //runs at start of the problem
            printf("this is the start of the program");

            handleInput.TakeInput();
        }

        public void Update()
        {
            //runs every cycle of the program
            // printf("update");
        }
    }
}