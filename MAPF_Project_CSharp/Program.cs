using System;

namespace MAPF_Project_CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Mapf
            var MAPF = new MAPF(new Reader());
            MAPF.StartMAPF();
        }
    }
}
