using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIGS.Helper;
using AIGS.Common;
using System.Threading;

namespace TIDALDL_UI.Else
{
    public class ThreadTool
    {
        private static ThreadPoolManager Pool;

        public static void SetThreadNum(int iNum)
        {
            if (Pool == null)
                Pool = new ThreadPoolManager(iNum);
            if (iNum < 1)
                iNum = 1;
            Pool.SetPoolSize(iNum);
        }

        public static int GetThreadNum()
        {
            if (Pool == null)
                return 0;
            return Pool.GetPoolSize();
        }

        public static bool AddWork(ThreadPoolManager.EventFunc Func, object[] data = null)
        {
            if (Pool == null)
                Pool = new ThreadPoolManager(Settings.Read().ThreadNum);
            Pool.AddWork(Func, data);
            return true;
        }

        public static void Close()
        {
            if (Pool == null)
                return;
            Pool.CloseAll(true);
            Pool = null;
        }
    }

}
