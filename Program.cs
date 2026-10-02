global using static RadApp.Utilities.MakeMyLifeEasier;

using System;

public class Program()
{
    static void Main(string[] arguments)
    {
        ManagerNM.Manager manager = new ManagerNM.Manager();

        //run at start of program & once
        manager.Start();

        while (true)
        {
            //run Update every cycle of the program
            manager.Update();

            //this will run as many times as physically possible by the CPU, hence will pin one CPU at 100% all the time, we do not want that, and need to target a baseline 60 cycles per sec, which comes down to 16.67 ms
            System.Threading.Thread.Sleep(16);
        }


    }
}


