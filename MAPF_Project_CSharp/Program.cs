using System;

namespace MAPF_Project_CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            /*/
            IReader reader = new Reader();
            reader.Open("Berlin_1_3.scen");
            reader.ReadAgents();
            reader.ReadMap();

            var a = reader.Map;
            TimeExpGraphBothSide b = new TimeExpGraphBothSide(a);
            b.SetEnumerator();
            a.AddAgent(2);
            foreach (var VARIABLE in b)
            {
                var c = VARIABLE;
            }
            b.AddDistance();
            foreach (var VARIABLE in b)
            {
                var c = VARIABLE;
            }
            
            /**/
            var MAPF = new MAPF(new Reader());
            MAPF.StartMAPF();
        }
    }
}
