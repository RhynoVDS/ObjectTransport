using System;
using System.Collections.Generic;
using System.Text;
using OTransport.tests;
using System.Threading;

namespace Test
{
    class Utilities
    { 
        public static void WaitFor(ref string test)
        {
            while(test == string.Empty || test == null) { }
        }

        internal static void WaitFor(ref bool executed)
        {
            while(executed == false) { }
        }

        internal static void WaitFor<T>(ref T Object)
        {
            while(Object == null) { }
        }

        internal static void Wait()
        {
            Thread.Sleep(10);
        }

        internal static void WaitFor(Func<bool> check)
        {

            while (!check.Invoke()) { }
        }
    }
}
